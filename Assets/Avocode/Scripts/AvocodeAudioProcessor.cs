using UnityEngine;
using ZSTUnity.Avocode.Utils;

namespace ZSTUnity.Avocode.Processing
{
    public class AvocodeAudioProcessor
    {
        public readonly int sampleRate;
        public readonly float sampleDelta;

        private float _gateAttackFactor;
        private float _gateReleaseFactor;

        public AvocodeAudioProcessor(int sampleRate)
        {
            this.sampleRate = sampleRate;
            sampleDelta = 1 / sampleRate;
        }

        public void NoiseReduceSamples(float[] input)
        {

        }

        public void NormalizeSamples(float[] input, Vector2 range)
        {
            var volume = AvocodeUtils.GetRMS(input);
            var max = Mathf.Max(input);

            for (int i = 0; i < input.Length; i++)
            {
                if (volume >= range.x) input[i] /= max;
            }
        }

        public void GateSamples(float[] input, float threshold, float attack, float release)
        {
            var attackFactorDelta = 1 / (attack * sampleRate);
            var releaseFactorDelta = 1 / (release * sampleRate);
            var volume = AvocodeUtils.GetRMS(input);

            for (int i = 0; i < input.Length; i++)
            {
                if (volume < threshold)
                {
                    if (float.IsFinite(releaseFactorDelta) && _gateReleaseFactor > 0f)
                    {
                        _gateReleaseFactor -= releaseFactorDelta;
                        input[i] *= _gateReleaseFactor;
                    }
                    else input[i] = 0;

                    _gateAttackFactor = 0f;
                }
                else if (float.IsFinite(attackFactorDelta) && _gateAttackFactor < 1f)
                {
                    _gateReleaseFactor = 1f;
                    _gateAttackFactor += attackFactorDelta;
                    input[i] *= _gateAttackFactor;
                }
            }
        }
    }
}
