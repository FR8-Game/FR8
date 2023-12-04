using UnityEngine;

namespace FR8.Runtime.Train.Electrics
{
    [CreateAssetMenu(menuName = "Config/Light Driver Settings")]
    public class LightDriverSettings : ScriptableObject
    {
        public float warmUpTime = 0.5f;
        public float cooldownTime = 1.0f;
        public AnimationCurve smoothingCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float delayMin;
        public float delayMax;

        public ColorFlickerinator emergencyLightColor = Color.red;
    }
}
