using UnityEngine;

namespace FR8Runtime.SceneUtility
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