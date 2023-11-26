using System.Diagnostics;

namespace ZSTUnity.QoL
{
    public class TimeMeasure
    {
        private readonly Stopwatch stopwatch = new();

        public TimeMeasure()
        {
            stopwatch.Start();
        }

        public void LogResult(string message)
        {
            stopwatch.Stop();
            string withSeconds = message.Replace("{Seconds}", ((float)stopwatch.Elapsed.TotalSeconds).ToString());
            UnityEngine.Debug.Log(withSeconds);
        }

        public void LogResult(string message, object value)
        {
            stopwatch.Stop();
            string withSeconds = message.Replace("{Seconds}", ((float)stopwatch.Elapsed.TotalSeconds).ToString());
            string withObject = withSeconds.Replace("{Object}", value.ToString());
            UnityEngine.Debug.Log(withObject);
        }
    }
}
