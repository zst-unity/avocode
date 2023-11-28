using UnityEngine;

namespace ZSTUnity.Avocode.Utils
{
    public static class AvocodeMainUtils
    {
        public static float GetRMS(float[] samples)
        {
            var sqSum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sqSum += samples[i] * samples[i];
            }

            var avg = sqSum / samples.Length;
            return Mathf.Sqrt(avg);
        }
    }
}
