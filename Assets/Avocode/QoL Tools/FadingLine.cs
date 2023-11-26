using System.Collections;
using UnityEngine;

namespace ZSTUnity.QoL
{
    public class FadingLine : MonoBehaviour
    {
        public LineRenderer Target { get; private set; }

        private void Awake()
        {
            Target = GetComponent<LineRenderer>();
        }

        public void Fade(float time)
        {
            StartCoroutine(CO_Fade(time));
        }

        private IEnumerator CO_Fade(float time)
        {
            Color startOrigin = Target.startColor;
            Color endOrigin = Target.endColor;

            float elapsed = 0;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float elapsedRemapped = elapsed.Remap(new Span(0, time), Span.ZeroPositive);

                Target.startColor = Color.Lerp(startOrigin, Color.clear, elapsedRemapped);
                Target.endColor = Color.Lerp(endOrigin, Color.clear, elapsedRemapped);

                yield return null;
            }

            Destroy(gameObject);
        }
    }

}