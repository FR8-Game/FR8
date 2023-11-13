
using UnityEngine;

namespace FR8.Runtime.Train
{
    [CreateAssetMenu(menuName = "Config/Train/Locomotive Settings")]
    public class LocomotiveSettings : ScriptableObject
    {
        [Header("Locomotive")]
        public float handbrakeConstant = 0.5f;
        
        [Header("Train Engine")]
        public float maxSpeedKmpH = 120.0f;
        public float acceleration = 8.0f;
        public float maxPowerConsumption = 75.0f;
        [Range(0.0f, 1.0f)] public float throttleLoadPowerSplit;
        [Range(0.0f, 1.0f)] public float throttleSmoothing;
        public float loadScaling = 0.02f;
        public AnimationCurve loadCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 0.0f);
        
        [Header("Train Electrics Controller")]
        public float baselineGeneration = 5.0f;
        
        [Header("Train Gas Turbine")]
        public float idleRpm = 1000.0f;
        public float maxRpm = 10000.0f;
        public float stallRpm = 700.0f;
        public float rpmSmoothTime = 0.1f;

        [Space]
        public float maxPowerProduction = 200.0f;

        [Space]
        public float fuelCapacity = 20.0f;
    }
}