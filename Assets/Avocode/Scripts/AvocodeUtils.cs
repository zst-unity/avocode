using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace ZSTUnity.Avocode.Utils
{
    public static class AvocodeUtils
    {
        public static float ToAmplitude(float dB) => Mathf.Pow(10, dB / 20);

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

        public static byte[] Compress(float[] input)
        {
            var compressedSamples = new byte[input.Length * 4];
            Buffer.BlockCopy(input, 0, compressedSamples, 0, compressedSamples.Length);

            using var memoryStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true/*msは*/))
            {
                deflateStream.Write(compressedSamples, 0, compressedSamples.Length);
            }

            memoryStream.Position = 0;
            var compressed = new byte[memoryStream.Length];
            memoryStream.Read(compressed, 0, compressed.Length);
            return compressed;
        }

        public static float[] Decompress(byte[] input)
        {
            using var memoryStream = new MemoryStream(input);
            using var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress);
            using var targetStream = new MemoryStream();
            deflateStream.CopyTo(targetStream);

            targetStream.Position = 0;
            var decompressed = new byte[targetStream.Length];
            targetStream.Read(decompressed, 0, decompressed.Length);
            return ToFloatArray(decompressed);
        }

        public static float[] ToFloatArray(byte[] byteArray)
        {
            var len = byteArray.Length / 4;
            var floatArray = new float[len];
            for (int i = 0; i < byteArray.Length; i += 4)
            {
                floatArray[i / 4] = BitConverter.ToSingle(byteArray, i);
            }
            return floatArray;
        }
    }
}
