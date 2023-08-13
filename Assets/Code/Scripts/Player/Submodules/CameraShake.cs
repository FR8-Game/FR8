using System;
using UnityEngine;

namespace FR8.Player.Submodules
{
    [Serializable]
    public class CameraShake
    {
        [SerializeField] private Vector3 amplitude = new Vector3(2.0f, 0.0f, 2.0f);
        [SerializeField] private float frequency = 1.0f;
        [SerializeField] private float slope = 0.2f;

        private float d;

        public void GetOffsets(PlayerGroundedCamera playerGroundedCamera, out Vector3 translationalOffset, out Quaternion rotationalOffset)
        {
            translationalOffset = Vector3.zero;
            rotationalOffset = Quaternion.identity;

            if (!playerGroundedCamera.Controller) return;
            if (!playerGroundedCamera.Controller.CurrentAvatar) return;
            if (playerGroundedCamera.Controller.CurrentAvatar is not PlayerGroundedAvatar avatar) return;

            if (!avatar.IsOnGround) return;
            
            var moveSpeed = avatar.MoveSpeed / avatar.MaxSpeed;
            var shakeAmount = -1.0f / (moveSpeed / slope + 1.0f) + 1.0f;

            d += moveSpeed * frequency * Time.deltaTime;

            var x = Mathf.Abs(Mathf.Cos(d)) * shakeAmount;
            var y = Mathf.Sin(d * Mathf.PI) * shakeAmount;
            rotationalOffset = Quaternion.Euler(amplitude.x * x, amplitude.y * y, amplitude.z * y);
        }
    }
}