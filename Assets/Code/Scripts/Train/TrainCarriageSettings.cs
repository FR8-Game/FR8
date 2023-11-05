
using UnityEngine;

namespace FR8Runtime.Train
{
    [CreateAssetMenu(menuName = "Config/Train/Train Carriage Settings")]
    public class TrainCarriageSettings : ScriptableObject
    {
        public float drag = 12.0f;
        public float cornerLean = 0.6f;

        [Space]
        public float handbrakeConstant = 40.0f;
        public AnimationCurve handbrakeEfficiencyCurve = new
        (
            new Keyframe(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            new Keyframe(0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            new Keyframe(4.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        );

        [Space]
        public float retentionSpring = 800.0f;
        public float retentionDamping = 60.0f;
        public float retentionTorqueConstant = 0.1f;

    }
}