using Mirror;
using UnityEngine;

namespace ZSTUnity.Avocode.Demo
{
    public class AvocodeDemoPlayer : NetworkBehaviour
    {
        [SerializeField] private Avocode _avocode;

        public override void OnStartLocalPlayer()
        {
            _avocode.SetRecording(true);
        }

        /*
        PUSH TO TALK
        private void Update()
        {
            if (!isLocalPlayer) return;
            _avocode.SetRecording(Input.GetKey(KeyCode.T));
        }
        */
    }
}
