using UnityEngine;

namespace FR8Runtime.Pickups
{
    [CreateAssetMenu(menuName = "Config/Pickup Pose")]
    public class PickupPose : ScriptableObject
    {
        public Vector3 holdTranslation;
        public Vector3 holdRotation;        
    }
}