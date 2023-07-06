using UnityEngine;

namespace FR8.Player
{
    [SelectionBase, DisallowMultipleComponent]
    public class PlayerZeroGMovement : PlayerMovementModule
    {
        [SerializeField] private float maxSpeed;
        [SerializeField] private float accelerationTime;
        [SerializeField] private float rotationSpeed;

        private Quaternion cameraOrientation;
        
        private void FixedUpdate()
        {
            
        }
    }
}