using UnityEngine;

namespace ZSTUnity.Avocode.Demo
{
    public class AvocodeGameController : MonoBehaviour
    {
        public void SetFPSCap(string input)
        {
            Application.targetFrameRate = string.IsNullOrEmpty(input) ? -1 : int.Parse(input);
        }
    }
}
