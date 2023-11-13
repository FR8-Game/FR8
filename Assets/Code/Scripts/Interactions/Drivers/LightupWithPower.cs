using FR8.Runtime.Train.Electrics;
using UnityEngine;

namespace FR8.Runtime.Interactions.Drivers
{
    [RequireComponent(typeof(MeshRenderer))]
    public class LightupWithPower : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField] private float brightness = 2.0f;
        [SerializeField] private int materialIndex = 0;

        private new Renderer renderer;
        private Material material;
        private DriverNetwork driverNetwork;

        private void Awake()
        {
            renderer = GetComponent<Renderer>();
            driverNetwork = GetComponentInParent<DriverNetwork>();

            material = renderer.materials[materialIndex];
        }

        private void Update()
        {
            var on = driverNetwork.GetValue(TrainElectricsController.MainFuse) > 0.5f;

            var color = this.color * brightness;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", on ? color : Color.clear);
        }

        private void OnValidate()
        {
            renderer = GetComponent<Renderer>();
            if (!renderer) return;
            
            materialIndex = Mathf.Clamp(materialIndex, 0, renderer.sharedMaterials.Length - 1);
        }
    }
}
