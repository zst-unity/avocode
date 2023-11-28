namespace ZSTUnity.Avocode.Utils
{
    public static class AvocodeProcessing
    {
        public static void NoiseReduceSamples(float[] input)
        {

        }

        public static void NormalizeSamples(float[] input)
        {

        }

        public static void GateSamples(float[] input, float threshold)
        {
            var volume = AvocodeMainUtils.GetRMS(input);
            for (int i = 0; i < input.Length; i++)
            {
                if (volume < threshold) input[i] = 0;
            }
        }
    }
}
