using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ZSTUnity.QoL
{
    public static class ZEnumerableExtensions
    {
        public static T GetRandomValue<T>(this IEnumerable<T> collection)
        {
            return collection.ElementAt(UnityEngine.Random.Range(0, collection.Count()));
        }

        public static T GetNext<T>(this IEnumerable<T> collection, int current)
        {
            return current == collection.Count() - 1 ? collection.First() : collection.ElementAt(current + 1);
        }

        public static T GetNext<T>(this IEnumerable<T> collection, T current)
        {
            return current.Equals(collection.Last()) ? collection.First() : collection.ElementAt(collection.IndexOf(current) + 1);
        }

        public static T GetPrevious<T>(this IEnumerable<T> collection, int current)
        {
            return current == 0 ? collection.Last() : collection.ElementAt(current - 1);
        }

        public static T GetPrevious<T>(this IEnumerable<T> collection, T current)
        {
            return current.Equals(collection.First()) ? collection.Last() : collection.ElementAt(collection.IndexOf(current) - 1);
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            int index = 0;
            var comparer = EqualityComparer<T>.Default;
            foreach (T item in source)
            {
                if (comparer.Equals(item, value)) return index;
                index++;
            }
            return -1;
        }
    }

    public static class ZNoise
    {
        public static float PerlinNoise2D(float x, float y, params PerlinNoise2DSettings[] settings)
        {
            float output = 0f;

            foreach (var setting in settings)
            {
                Vector2 noiseVector = new Vector2
                (
                    x / setting.Space.x,
                    y / setting.Space.y
                ) * setting.Frequency;

                float noiseHeight = Mathf.PerlinNoise(setting.Seed + noiseVector.x, setting.Seed + noiseVector.y);

                output += Mathf.Pow(noiseHeight, setting.Power).Remap(Span.ZeroPositive, setting.RemapTo) * setting.Amplitude;
            }

            return output;
        }

        public static float PerlinNoise1D(float position, params PerlinNoise1DSettings[] settings)
        {
            float output = 0f;

            foreach (var setting in settings)
            {
                float noiseValue = position / setting.Space * setting.Frequency;

                float noiseHeight = Mathf.PerlinNoise1D(setting.Seed + noiseValue);

                output += Mathf.Pow(noiseHeight, setting.Power).Remap(Span.ZeroPositive, setting.RemapTo) * setting.Amplitude;
            }

            return output;
        }
    }

    public abstract class NoiseSettings
    {
        public readonly int Seed = 1 * UnityEngine.Random.Range(10000, 99999);
        public Span RemapTo = Span.ZeroPositive;
        public float Amplitude = 1f;
        public float Frequency = 1f;
        public float Power = 1f;
    }

    public abstract class Noise1DSettings : NoiseSettings
    {
        public float Space;
    }

    public abstract class Noise2DSettings : NoiseSettings
    {
        public Vector2 Space;
    }

    public class PerlinNoise2DSettings : Noise2DSettings { }
    public class PerlinNoise1DSettings : Noise1DSettings { }

    public static class ZRandom
    {
        public static bool Chance(int chance)
        {
            return UnityEngine.Random.Range(0, 101) <= chance;
        }

        public static int Range(int minimum, int maximum)
        {
            return UnityEngine.Random.Range(minimum, maximum + 1);
        }

        public static T Between<T>(params T[] objects)
        {
            return objects.GetRandomValue();
        }
    }

    public static class ZMath
    {
        public static float Remap(this float value, Span from, Span to)
        {
            var fromAbs = value - from.Min;
            var fromMaxAbs = from.Max - from.Min;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = to.Max - to.Min;
            var toAbs = toMaxAbs * normal;

            var result = toAbs + to.Min;

            return result;
        }

        public static float WhatIsHigher(float first, float second) => first > second ? first : second;
        public static int WhatIsHigher(int first, int second) => first > second ? first : second;

        public static float WhatIsLower(float first, float second) => first < second ? first : second;
        public static int WhatIsLower(int first, int second) => first < second ? first : second;

        public static float ClampMaximum(this float target, float value) => target > value ? value : target;
        public static float ClampMaximum(this int target, int value) => target > value ? value : target;

        public static float ClampMinimum(this float target, float value) => target < value ? value : target;
        public static float ClampMinimum(this int target, int value) => target < value ? value : target;
    }

    public class Span
    {
        public readonly float Min;
        public readonly float Max;

        public Span(float min, float max)
        {
            Min = min;
            Max = max;
        }

        public static readonly Span ZeroPositive = new Span(0, 1);
        public static readonly Span NegativePositive = new Span(-1, 1);
        public static readonly Span NegativeZero = new Span(-1, 0);
    }

    public static class ZLogic
    {
        public static bool IsInRange(this float input, float min, float max)
        {
            return input >= min && input <= max;
        }

        public static bool IsInRange(this int input, int min, int max)
        {
            return input >= min && input <= max;
        }

        public static bool IsInRange(this Vector2 input, Vector2 min, Vector2 max)
        {
            return input.x >= min.x && input.y >= min.y && input.x <= max.x && input.y <= max.y;
        }

        public static bool IsInRange(this Vector2Int input, Vector2Int min, Vector2Int max)
        {
            return input.x >= min.x && input.y >= min.y && input.x <= max.x && input.y <= max.y;
        }

        public static bool IsInRange(this Vector3 input, Vector3 min, Vector3 max)
        {
            return input.x >= min.x && input.y >= min.y && input.z >= min.z && input.x <= max.x && input.y <= max.y && input.z <= max.z;
        }

        public static bool IsInRange(this Vector3Int input, Vector3Int min, Vector3Int max)
        {
            return input.x >= min.x && input.y >= min.y && input.z >= min.z && input.x <= max.x && input.y <= max.y && input.z <= max.z;
        }
    }

    public static class ZVector2
    {
        public static Vector3 ToVector3(this Vector2 target) => new(target.x, target.y, 0);
        public static Vector2 ToVector2(this Vector2Int target) => new(target.x, target.y);
    }

    public static class ZVector3
    {
        public static Vector2 ToVector2(this Vector3 target) => new(target.x, target.y);
        public static Vector3 ToVector3(this Vector3Int target) => new(target.x, target.y, target.z);
    }

    public static class ZVector2Math
    {
        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static Vector2 Abs(this Vector2 target)
        {
            return new Vector2
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y)
            );
        }

        public static Vector2Int Abs(this Vector2Int target)
        {
            return new Vector2Int
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y)
            );
        }

        public static Vector2 ClampX(this Vector2 target, float min, float max)
        {
            return new Vector2
            (
                Mathf.Clamp(target.x, min, max),
                target.y
            );
        }

        public static Vector2 ClampY(this Vector2 target, float min, float max)
        {
            return new Vector2
            (
                target.x,
                Mathf.Clamp(target.y, min, max)
            );
        }

        public static Vector2 Clamp(this Vector2 target, Vector2 min, Vector2 max)
        {
            return new Vector2
            (
                Mathf.Clamp(target.x, min.x, max.x),
                Mathf.Clamp(target.y, min.y, max.y)
            );
        }

        public static Vector2 ClampMinimum(this Vector2 target, Vector2 min)
        {
            return new Vector2
            (
                target.x < min.x ? min.x : target.x,
                target.y < min.y ? min.y : target.y
            );
        }

        public static Vector2 ClampMinimum(this Vector2 target, float x, float y)
        {
            return new Vector2
            (
                target.x < x ? x : target.x,
                target.y < y ? y : target.y
            );
        }

        public static Vector2 ClampMaximum(this Vector2 target, Vector2 max)
        {
            return new Vector2
            (
                target.x > max.x ? max.x : target.x,
                target.y > max.y ? max.y : target.y
            );
        }

        public static Vector3 ClampMaximum(this Vector3 target, float x, float y)
        {
            return new Vector2
            (
                target.x > x ? x : target.x,
                target.y > y ? y : target.y
            );
        }

        public static Vector2 Round(this Vector2 target, float roundTo = 1f)
        {
            return new Vector2
            (
                Mathf.Round(target.x / roundTo) * roundTo,
                Mathf.Round(target.y / roundTo) * roundTo
            );
        }

        public static Vector2Int RoundToInt(this Vector2 target)
        {
            return new Vector2Int
            (
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y)
            );
        }

        public static Vector2 Floor(this Vector2 target, float roundTo = 1f)
        {
            return new Vector2
            (
                Mathf.Floor(target.x / roundTo) * roundTo,
                Mathf.Floor(target.y / roundTo) * roundTo
            );
        }

        public static Vector2Int FloorToInt(this Vector2 target)
        {
            return new Vector2Int
            (
                Mathf.FloorToInt(target.x),
                Mathf.FloorToInt(target.y)
            );
        }

        public static Vector2 Multiply(this Vector2 target, Vector2 multiplier)
        {
            return new Vector2
            (
                target.x * multiplier.x,
                target.y * multiplier.y
            );
        }

        public static Vector2 Multiply(this Vector2 target, float x, float y)
        {
            return new Vector2
            (
                target.x * x,
                target.y * y
            );
        }

        public static Vector2 Divide(this Vector2 target, Vector2 divider)
        {
            return new Vector2
            (
                target.x / divider.x,
                target.y / divider.y
            );
        }

        public static Vector2 Divide(this Vector2 target, float x, float y)
        {
            return new Vector2
            (
                target.x / x,
                target.y / y
            );
        }

        public static Vector2 ProjectOnPlane(Vector2 vector, Vector2 normal)
        {
            return Vector3.ProjectOnPlane(vector, normal);
        }

        public static Vector2 RandomInCone(this Vector2 dir, float openingAngle)
        {
            float targetAngle = openingAngle * Mathf.Deg2Rad;
            Vector2 deviation = UnityEngine.Random.insideUnitSphere;
            dir = dir.normalized;
            deviation -= Vector3.Dot(deviation, dir) * dir;

            return (dir * Mathf.Cos(targetAngle) + deviation * Mathf.Sin(targetAngle)).normalized;
        }
    }

    public static class ZVector3Math
    {
        public static readonly Vector3 XZ = new(1, 0, 1);
        public static readonly Vector3 XY = new(1, 1, 0);
        public static readonly Vector3 YZ = new(0, 1, 1);

        public static readonly Vector3 mXZ = new(-1, 0, -1);
        public static readonly Vector3 mXY = new(-1, -1, 0);
        public static readonly Vector3 mYZ = new(0, -1, -1);

        public static readonly Vector3 mXYmZ = new(-1, 1, -1);

        public static Vector3 Round(this Vector3 target, float roundTo = 1f)
        {
            return new Vector3
            (
                Mathf.Round(target.x / roundTo) * roundTo,
                Mathf.Round(target.y / roundTo) * roundTo,
                Mathf.Round(target.z / roundTo) * roundTo
            );
        }

        public static Vector3Int RoundToInt(this Vector3 target)
        {
            return new Vector3Int
            (
                Mathf.RoundToInt(target.x),
                Mathf.RoundToInt(target.y),
                Mathf.RoundToInt(target.z)
            );
        }

        public static Vector3 Floor(this Vector3 target, float floorTo = 1f)
        {
            return new Vector3
            (
                Mathf.Floor(target.x / floorTo) * floorTo,
                Mathf.Floor(target.y / floorTo) * floorTo,
                Mathf.Floor(target.z / floorTo) * floorTo
            );
        }

        public static Vector3Int FloorToInt(this Vector3 target)
        {
            return new Vector3Int
            (
                Mathf.FloorToInt(target.x),
                Mathf.FloorToInt(target.y),
                Mathf.FloorToInt(target.z)
            );
        }

        public static Vector3 Clamp(this Vector3 target, Vector3 min, Vector3 max)
        {
            return new Vector3
            (
                Mathf.Clamp(target.x, min.x, max.x),
                Mathf.Clamp(target.y, min.y, max.y),
                Mathf.Clamp(target.z, min.z, max.z)
            );
        }

        public static Vector3 ClampMinimum(this Vector3 target, Vector3 min)
        {
            return new Vector3
            (
                target.x < min.x ? min.x : target.x,
                target.y < min.y ? min.y : target.y,
                target.z < min.z ? min.z : target.z
            );
        }

        public static Vector3 ClampMinimum(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x < x ? x : target.x,
                target.y < y ? y : target.y,
                target.z < z ? z : target.z
            );
        }

        public static Vector3 ClampMaximum(this Vector3 target, Vector3 max)
        {
            return new Vector3
            (
                target.x > max.x ? max.x : target.x,
                target.y > max.y ? max.y : target.y,
                target.z > max.z ? max.z : target.z
            );
        }

        public static Vector3 ClampMaximum(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x > x ? x : target.x,
                target.y > y ? y : target.y,
                target.z > z ? z : target.z
            );
        }

        public static Vector3 Abs(this Vector3 target)
        {
            return new Vector3
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y),
                Mathf.Abs(target.z)
            );
        }

        public static Vector3Int Abs(this Vector3Int target)
        {
            return new Vector3Int
            (
                Mathf.Abs(target.x),
                Mathf.Abs(target.y),
                Mathf.Abs(target.z)
            );
        }

        public static Vector3 Multiply(this Vector3 target, Vector3 multiplier)
        {
            return new Vector3
            (
                target.x * multiplier.x,
                target.y * multiplier.y,
                target.z * multiplier.z
            );
        }

        public static Vector3 Multiply(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x * x,
                target.y * y,
                target.z * z
            );
        }

        public static Vector3 Divide(this Vector3 target, Vector3 divider)
        {
            return new Vector3
            (
                target.x / divider.x,
                target.y / divider.y,
                target.z / divider.z
            );
        }

        public static Vector3 Divide(this Vector3 target, float x, float y, float z)
        {
            return new Vector3
            (
                target.x / x,
                target.y / y,
                target.z / z
            );
        }

        public static bool Equals(this Vector3 target, float x, float y, float z)
        {
            return target.x == x && target.y == y && target.z == z;
        }

        public static Vector3 RandomInCone(this Vector3 dir, float openingAngle)
        {
            float targetAngle = openingAngle * Mathf.Deg2Rad;
            Vector3 deviation = UnityEngine.Random.insideUnitSphere;
            dir = dir.normalized;
            deviation -= Vector3.Dot(deviation, dir) * dir;

            return (dir * Mathf.Cos(targetAngle) + deviation * Mathf.Sin(targetAngle)).normalized;
        }
    }

    public static class Const
    {
        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";

        public const string PLAYER = "Player";
        public const string ENEMY = "Enemy";
        public const string MAP = "Map";
    }

    public static class Keyboard
    {
        public static bool IsPressed(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key)) return true;
            }

            return false;
        }

        public static bool IsReleased(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyUp(key)) return true;
            }

            return false;
        }

        public static bool IsHolding(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetKey(key)) return true;
            }

            return false;
        }

        public static float AxisFrom(KeyCode negative, KeyCode positive, Span remapTo = null)
        {
            float value = 0;

            if (IsHolding(negative) && value != -1) value--;
            if (IsHolding(positive) && value != 1) value++;

            float valueRemaped = remapTo != null ? value.Remap(Span.NegativePositive, remapTo) : value;

            return valueRemaped;
        }
    }

    public static class Mouse
    {
        public static bool IsPressed(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButtonDown((int)key)) return true;
            }

            return false;
        }

        public static bool IsReleased(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButtonUp((int)key)) return true;
            }

            return false;
        }

        public static bool IsHolding(params MouseCode[] keys)
        {
            foreach (var key in keys)
            {
                if (Input.GetMouseButton((int)key)) return true;
            }

            return false;
        }
    }

    public static class ZGameObjectExtensions
    {
        public static GameObject Spawn(this GameObject gameObject, Vector3 position)
        {
            return Object.Instantiate(gameObject, position, Quaternion.identity);
        }

        public static GameObject Spawn(this GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            return Object.Instantiate(gameObject, position, rotation);
        }

        public static GameObject Spawn(this GameObject gameObject, Transform parent)
        {
            return Object.Instantiate(gameObject, parent);
        }
    }

    public static class AudioSystem
    {
        public static void Play(this AudioClip clip, AudioOptions options)
        {
            GameObject source = new(clip.name);
            AudioSource audioSource = source.AddComponent<AudioSource>();

            audioSource.clip = clip;

            audioSource.volume = options.Volume;
            audioSource.pitch = options.Pitch;
            audioSource.loop = options.Loop;

            audioSource.Play();

            if (!options.Loop) Object.Destroy(source, clip.length);
        }

        public static void Play(IEnumerable<AudioClip> clips, AudioOptions options)
        {
            AudioClip clip = clips.GetRandomValue();

            GameObject source = new(clip.name);
            AudioSource audioSource = source.AddComponent<AudioSource>();

            audioSource.clip = clip;

            audioSource.volume = options.Volume;
            audioSource.pitch = options.Pitch;
            audioSource.loop = options.Loop;

            audioSource.Play();

            if (!options.Loop) Object.Destroy(source, clip.length);
        }
    }

    public class AudioOptions
    {
        public float Volume = 1f;
        public float Pitch = 1f;
        public bool Loop = false;

        public static AudioOptions Default = new();

        public static AudioOptions HalfVolume => new() { Volume = 0.5f };
        public static AudioOptions WithVariation => new() { Pitch = UnityEngine.Random.Range(0.9f, 1.1f) };
        public static AudioOptions HalfVolumeWithVariation => new() { Volume = 0.5f, Pitch = UnityEngine.Random.Range(0.9f, 1.1f) };
    }

    public static class LineSystem
    {
        public static LineRenderer Draw2D((Vector2 point, Color color) start, (Vector2 point, Color color) end, float width, Material material, int sortingLayer = 0)
        {
            GameObject source = new("2D Line");
            LineRenderer lineRenderer = source.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(new Vector3[] { start.point, end.point });
            lineRenderer.startColor = start.color;
            lineRenderer.endColor = end.color;
            lineRenderer.widthMultiplier = width;
            lineRenderer.material = material;
            lineRenderer.sortingLayerID = sortingLayer;

            return lineRenderer;
        }

        public static FadingLine DrawFading2D((Vector2 point, Color color) start, (Vector2 point, Color color) end, float width, Material material, int sortingLayer = 0)
        {
            GameObject source = new("2D Line");
            LineRenderer lineRenderer = source.AddComponent<LineRenderer>();

            lineRenderer.SetPositions(new Vector3[] { start.point, end.point });
            lineRenderer.startColor = start.color;
            lineRenderer.endColor = end.color;
            lineRenderer.widthMultiplier = width;
            lineRenderer.material = material;
            lineRenderer.sortingLayerID = sortingLayer;

            return source.AddComponent<FadingLine>();
        }
    }

    public static class ZLayerExtensions
    {
        public static int PlayerLayerMask() => LayerMask.GetMask(Const.PLAYER);
        public static int EnemyLayerMask() => LayerMask.GetMask(Const.ENEMY);
        public static int MapLayerMask() => LayerMask.GetMask(Const.MAP);
    }

    public static class ZCollision2DExtensions
    {
        public static bool TryGetComponent<T>(this Collision2D collision, out T component) where T : Component
        {
            return collision.transform.TryGetComponent(out component);
        }

        public static T GetComponent<T>(this Collision2D collision) where T : Component
        {
            return (T)collision.transform.GetComponent(typeof(T));
        }
    }

    public static class ZColorExtensions
    {
        public static Color HexToColor(this string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        public static string ToHex(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }

    public static class ZObjectExtensions
    {
        public static T Clone<T>(this T obj)
        {
            var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (T)inst?.Invoke(obj, null);
        }
    }
}

public enum MouseCode
{
    Left = 0,
    Middle = 2,
    Right = 1
}
