
using UnityEngine;

namespace FR8.Runtime.Train
{
    [CreateAssetMenu(menuName = "Config/Train/Carriage Connector Settings")]
    public class CarriageConnectorSettings : ScriptableObject
    {
        public float forceScale;
        public float forceRange = 0.8f;
        public AnimationCurve forceFalloff = AnimationCurve.EaseInOut(0.0f, 1.0f, 1.0f, 0.0f);
        public float connectionDistance = 0.03f;
        public float connectionForce = 300.0f;
        public float connectionDamping = 20.0f;
    }
}