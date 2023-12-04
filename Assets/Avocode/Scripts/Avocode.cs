using System.Linq;
using Mirror;
using UnityEngine;
using ZSTUnity.Avocode.Exceptions;
using ZSTUnity.Avocode.Utils;
using ZSTUnity.Avocode.Attributes;
using System;
using ZSTUnity.Avocode.Processing;

namespace ZSTUnity.Avocode
{
    public class Avocode : NetworkBehaviour
    {
        [field: SerializeField] public AudioSource VoiceSource { get; private set; }

        [field: Header("Properties")]
        [field: Tooltip("Specify the name of the input device, or leave empty field if you want the input device to be detected automatically")]
        [field: SerializeField] public string DeviceName { get; private set; }
        [field: Tooltip("If true, the player will be able to hear himself talking")]
        [field: SerializeField] public bool HearSelf { get; private set; }

        [field: Header("Recording")]
        [field: Tooltip("Voice recording sample rate in hz\nCommon sample rates: 48000, 44100, 32000, 22050, 11025, 8000")]
        [field: SerializeField, Min(0)] public int SampleRate { get; private set; } = 32000;
        [field: Tooltip("Voice recording duration in seconds (not recommended to modify)")]
        [field: SerializeField, Min(0)] public int RecordDuration { get; private set; } = 45;
        [field: Tooltip("Scale of sample packet that will be send to clients (not recommended to modify)")]
        [field: SerializeField, Range(0, 1)] public float SamplePacketScale { get; private set; } = 0.05f;

        [field: Header("Reading")]
        [field: Tooltip("Scale of voice audio clip that will be player (not recommended to modify)")]
        [field: SerializeField, Min(1)] public int VoiceClipScale { get; private set; } = 5;

        [field: Header("Gate")]
        [field: Tooltip("Toggle noise gate effect")]
        [field: SerializeField] public bool EnableGate { get; private set; } = true;
        [field: Tooltip("Every voice sample quieter than the specified threshold will be silent")]
        [field: SerializeField, Range(-96, 20)] public float GateThreshold { get; private set; } = -45f;
        [field: Tooltip("Sound fade duration in seconds from silent to normal")]
        [field: SerializeField, Range(0, 1)] public float GateAttack { get; private set; } = 0.05f;
        [field: Tooltip("Sound fade duration in seconds from normal to silent")]
        [field: SerializeField, Range(0, 1)] public float GateRelease { get; private set; } = 0.5f;
        [SerializeField, HideInInspector] private float _gateThreshold;

        [field: Header("Info")]
        [AvocodeReadOnly, SerializeField] private int _samplePacketSize;
        [AvocodeReadOnly, SerializeField] private int _voiceClipSize;
        [AvocodeReadOnly, SerializeField] private bool _recording;

        private AudioClip _recordClip;
        private bool _failed;
        private int _recordHead;
        private int _readHead;

        // Unity freezes when you switch inspector mode to debug, HideInInspector attribute prevents those massive arrays from being shown
        [HideInInspector] private float[] _recordSampleBuffer;
        [HideInInspector] private float[] _readSampleBuffer;
        [HideInInspector] private float[] _samplePacket;

        private AvocodeAudioProcessor _avocodeAudioProcessor;

        protected override void OnValidate()
        {
            base.OnValidate();
            DeviceName = DeviceName.Trim();
            _samplePacketSize = (int)(SampleRate * SamplePacketScale);
            _voiceClipSize = _samplePacketSize * VoiceClipScale;
            _gateThreshold = AvocodeUtils.ToAmplitude(GateThreshold);

            if (VoiceSource)
            {
                VoiceSource.loop = true;
            }
        }

        private void Awake()
        {
            if (!VoiceSource)
                Fail($"Audio Source of the voice wasn't assigned");

            if (!string.IsNullOrEmpty(DeviceName) && !Microphone.devices.Contains(DeviceName))
                Fail($"An input device \"{DeviceName}\" was not found. Specify a correct name for the input device, or leave the Device Name field blank so that the input device is automatically detected");

            _avocodeAudioProcessor = new(SampleRate);
            ResetRecording();
            ResetReading();
        }

        private void ResetRecording()
        {
            _recordHead = 0;
            _recordSampleBuffer = new float[RecordDuration * SampleRate];
            _samplePacket = new float[_samplePacketSize];
        }

        private void ResetReading()
        {
            _readSampleBuffer = Array.Empty<float>();
            _readHead = 0;
        }

        public void SetRecording(bool enable)
        {
            if (_recording == enable) return;

            ThrowIfFailed();
            _recording = enable;

            if (enable) _recordClip = Microphone.Start(DeviceName, true, RecordDuration, SampleRate);
            else
            {
                Microphone.End(DeviceName);
                ResetRecording();
                CmdStopReading();
            }
        }

        private void FixedUpdate()
        {
            if (_failed || !isOwned) return;
            Record();
        }

        private void Record()
        {
            if (!_recording) return;

            var position = Microphone.GetPosition(null);
            if (position < 0 || _recordHead == position) return;

            _recordClip.GetData(_recordSampleBuffer, 0);

            while (GetDataLength(_recordSampleBuffer.Length, _recordHead, position) > _samplePacket.Length)
            {
                var remain = _recordSampleBuffer.Length - _recordHead;
                if (remain < _samplePacket.Length)
                {
                    Array.Copy(_recordSampleBuffer, _recordHead, _samplePacket, 0, remain);
                    Array.Copy(_recordSampleBuffer, 0, _samplePacket, 0, _samplePacket.Length - remain);
                }
                else
                {
                    Array.Copy(_recordSampleBuffer, _recordHead, _samplePacket, 0, _samplePacket.Length);
                }

                SamplePacketReady(_samplePacket);
                _recordHead += _samplePacket.Length;
                if (_recordHead > _recordSampleBuffer.Length)
                {
                    _recordHead -= _recordSampleBuffer.Length;
                }
            }
        }

        private int GetDataLength(int bufferLength, int head, int tail) => head < tail ? tail - head : bufferLength - head + tail;

        private void SamplePacketReady(float[] samplePacket)
        {
            var processSamplePacket = new float[_samplePacketSize];
            Array.Copy(samplePacket, processSamplePacket, _samplePacketSize);

            if (EnableGate)
                _avocodeAudioProcessor.GateSamples(processSamplePacket, _gateThreshold, GateAttack, GateRelease);

            var compressedSamplePacket = AvocodeUtils.Compress(processSamplePacket);
            CmdSendAudio(compressedSamplePacket);
        }

        [Command]
        private void CmdSendAudio(byte[] compressedSamplePacket)
        {
            if (HearSelf) RpcReceiveAudioForAll(compressedSamplePacket);
            else RpcReceiveAudioForOthers(compressedSamplePacket);
        }

        [ClientRpc]
        private void RpcReceiveAudioForAll(byte[] compressedSamplePacket)
        {
            var samplePacket = AvocodeUtils.Decompress(compressedSamplePacket);
            ReadAudio(samplePacket);
        }

        [ClientRpc(includeOwner = false)]
        private void RpcReceiveAudioForOthers(byte[] compressedSamplePacket)
        {
            var samplePacket = AvocodeUtils.Decompress(compressedSamplePacket);
            ReadAudio(samplePacket);
        }

        private void ReadAudio(float[] samplePacket)
        {
            if (!VoiceSource.clip)
            {
                Debug.LogError("Voice clip is null, creating new voice clip");
                VoiceSource.clip = AudioClip.Create("Voice", _voiceClipSize, 1, SampleRate, false);
            }

            if (_readSampleBuffer == null || _readSampleBuffer.Length != samplePacket.Length)
            {
                _readSampleBuffer = new float[samplePacket.Length];
            }

            Array.Copy(samplePacket, _readSampleBuffer, samplePacket.Length);
            VoiceSource.clip.SetData(_readSampleBuffer, _readHead);
            _readHead += samplePacket.Length;
            if (!VoiceSource.isPlaying && _readHead > _voiceClipSize / 2)
            {
                VoiceSource.Play();
            }

            Debug.LogError($"Read head before is {_readHead}");
            _readHead %= _voiceClipSize;
            Debug.LogError($"Read head after is {_readHead}");
        }

        [Command]
        private void CmdStopReading()
        {
            RpcStopReading();
        }

        [ClientRpc]
        private void RpcStopReading()
        {
            VoiceSource.Stop();
            VoiceSource.clip = null;
            ResetReading();
        }

        private void Fail(string message)
        {
            _failed = true;
            _recording = false;

            throw new Fail($"{message}. Avocode script will not function.", gameObject);
        }

        private void ThrowIfFailed()
        {
            if (_failed)
                throw new Exception($"Avocode script failed on game object {gameObject.name}");
        }
    }
}
