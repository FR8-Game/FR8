using System;
using UnityEngine;

namespace FR8Runtime.Player.Submodules
{
    [Serializable]
    public class CameraShake
    {
        [SerializeField] private Vector3 cosAmplitude = new(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 sinAmplitude = new(0.0f, 0.0f, 0.0f);
        [SerializeField] private float frequency = 1.0f;
        [SerializeField] private float slope = 0.2f;

        private float d;

        public void GetOffsets(PlayerCamera playerCamera, out Vector3 translationalOffset, out Quaternion rotationalOffset)
        {
            translationalOffset = Vector3.zero;
            rotationalOffset = Quaternion.identity;

            var avatar = playerCamera.Avatar;
            if (!avatar) return;
            if (!avatar.groundedMovement.IsOnGround) return;
            
            var moveSpeed = avatar.groundedMovement.MoveSpeed / avatar.groundedMovement.maxGroundedSpeed;
            var shakeAmount = -1.0f / (moveSpeed / slope + 1.0f) + 1.0f;

            d += moveSpeed * frequency * Time.deltaTime;

            var c = Mathf.Abs(Mathf.Cos(d * Mathf.PI)) * shakeAmount;
            var s = Mathf.Sin(d * Mathf.PI) * shakeAmount;

            rotationalOffset *= Quaternion.Euler(new Vector3()
            {
                x = c * cosAmplitude.x + s * sinAmplitude.x,
                y = c * cosAmplitude.y + s * sinAmplitude.y,
                z = c * cosAmplitude.z + s * sinAmplitude.z,
            });
        }
    }
}