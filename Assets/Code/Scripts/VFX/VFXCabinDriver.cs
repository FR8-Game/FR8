using FR8.Runtime.Train;
using FR8.Runtime.Train.Engine;
using UnityEngine;
using UnityEngine.VFX;

namespace FR8.Runtime.VFX
{
    [RequireComponent(typeof(VisualEffect))]
    public class VFXCabinDriver : MonoBehaviour
    {
        [Header("Cabin")]
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 scale = Vector3.one;

        private Locomotive locomotive;
        private TrainEngine engine;
        private VisualEffect vfx;

        private void Update()
        {
            if (!locomotive) locomotive = FindObjectOfType<Locomotive>();
            if (!engine) engine = locomotive.GetComponent<TrainEngine>();
            if (!vfx) vfx = GetComponent<VisualEffect>();

            if (vfx.HasTransform("Cabin"))
            {
                var localRotation = Quaternion.Euler(localEulerAngles);
                vfx.SetTransform("Cabin", locomotive.transform.TransformPoint(localPosition), locomotive.transform.rotation * localRotation, scale);
            }

            if (vfx.HasFloat("TrainSpeed"))
            {
                var speed = engine.GetNormalizedForwardSpeed();
                vfx.SetFloat("TrainSpeed", speed);
            }
        }

        private void OnDrawGizmosSelected() { }
    }
}