using System;
using UnityEngine;

namespace FR8.ProcGen
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class Grid : MonoBehaviour
    {
        [SerializeField] private int xi, yi;
        [SerializeField] private float baseSize;
        [SerializeField] private float minHeight;
        [SerializeField] private float maxHeight;
        [SerializeField] private Material material;

        private void Awake()
        {
            Instance((pos, height) =>
            {
                pos.y = height / 2.0f;

                var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                instance.transform.SetParent(transform);
                instance.transform.localPosition = pos;
                instance.transform.localScale = new Vector3(baseSize, height, baseSize);
                instance.GetComponent<Renderer>().sharedMaterial = material;
            });
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Instance((pos, height) =>
            {
                pos.y = height / 2.0f;
                Gizmos.DrawWireCube(pos, new Vector3(baseSize, height, baseSize));
            });
        }

        public void Instance(Action<Vector3, float> buildingCallback)
        {
            for (var x = 0; x < xi; x++)
            for (var y = 0; y < yi; y++)
            {
                var pos = new Vector3(x * baseSize, 0.0f, y * baseSize);
                pos -= new Vector3(xi - 1, 0, yi - 1) * baseSize * 0.5f;

                var height = Mathf.Lerp(minHeight, maxHeight, Mathf.Pow(Mathf.PerlinNoise(pos.x + 0.5f, pos.z + 0.5f), 3.0f));
                buildingCallback(pos, height);
            }
        }
    }
}