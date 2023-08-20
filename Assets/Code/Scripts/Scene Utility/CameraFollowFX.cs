using UnityEngine;

namespace FR8.FX
{
    [ExecuteAlways]
    [SelectionBase, DisallowMultipleComponent]
    public sealed class CameraFollowFX : MonoBehaviour
    {
        private Camera mainCamera;

        private void Update()
        {
            if (!mainCamera) mainCamera = Camera.main;
            else transform.position = mainCamera.transform.position;
        }
    }
}