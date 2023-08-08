using UnityEngine;

namespace FR8.Pickups
{
    [CreateAssetMenu(menuName = "Config/Pickup Pose")]
    public class PickupPose : ScriptableObject
    {
        public Vector3 holdTranslation;
        public Vector3 holdRotation;        
    }
}