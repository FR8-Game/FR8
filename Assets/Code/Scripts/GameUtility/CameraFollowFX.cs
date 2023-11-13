using UnityEngine;

namespace FR8.Runtime.GameUtility
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