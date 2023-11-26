using System.Linq;
using Mirror;
using UnityEngine;
using ZSTUnity.Avocode.Exceptions;
using ZSTUnity.Avocode.Utils;
using System;
using ZSTUnity.QoL;

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
        [field: SerializeField, Min(0)] public int SampleRate { get; private set; } = 44100;
        [field: Tooltip("Voice recording duration in seconds (not recommended to modify)")]
        [field: SerializeField, Min(0)] public int RecordDuration { get; private set; } = 60;
        [field: Tooltip("Scale of sample packet that will be send to clients (not recommended to modify)")]
        [field: SerializeField, Range(0, 1)] public float SamplePacketScale { get; private set; } = 0.1f;

        [field: Header("Reading")]
        [field: Tooltip("TODO: Tooltip (not recommended to modify)")]
        [field: SerializeField, Min(1)] public int VoiceClipScale { get; private set; } = 5;

        [field: Header("Info")]
        [ReadOnly, SerializeField] private int _samplePacketSize;
        [ReadOnly, SerializeField] private int _voiceClipSize;
        [ReadOnly, SerializeField] private bool _recording;

        private AudioClip _recordClip;
        private bool _failed;
        private int _recordHead;
        private int _readHead;

        // Unity freezes when you switch inspector mode to debug, HideInInspector attribute prevents those massive arrays from being shown
        [HideInInspector] private float[] _recordSampleBuffer;
        [HideInInspector] private float[] _readSampleBuffer;
        [HideInInspector] private float[] _samplePacket;

        protected override void OnValidate()
        {
            base.OnValidate();
            DeviceName = DeviceName.Trim();
            _samplePacketSize = (int)(SampleRate * SamplePacketScale);
            _voiceClipSize = _samplePacketSize * VoiceClipScale;

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

        private void Update()
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

            static int GetDataLength(int bufferLength, int head, int tail) =>
                head < tail ? tail - head : bufferLength - head + tail;

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

                AudioReady();
                _recordHead += _samplePacket.Length;
                if (_recordHead > _recordSampleBuffer.Length)
                {
                    _recordHead -= _recordSampleBuffer.Length;
                }
            }
        }

        private void AudioReady()
        {
            byte[] compressedSamplePacket = AudioCompression.Compress(_samplePacket);
            CmdSendAudio(compressedSamplePacket);
        }

        [Command]
        private void CmdSendAudio(byte[] compressedSamplePacket)
        {
            RpcReceiveAudio(compressedSamplePacket);
        }

        [ClientRpc]
        private void RpcReceiveAudio(byte[] compressedSamplePacket)
        {
            if (!HearSelf && isOwned) return;

            float[] samplePacket = AudioCompression.Decompress(compressedSamplePacket);
            ReadAudio(samplePacket);
        }

        private void ReadAudio(float[] samplePacket)
        {
            if (!VoiceSource.clip)
            {
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

            _readHead %= _voiceClipSize;
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