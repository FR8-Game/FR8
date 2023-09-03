using UnityEngine;

namespace FR8Runtime.Player.Submodules
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerAvatar))]
    [RequireComponent(typeof(PlayerGroundedMovement))]
    [RequireComponent(typeof(PlayerFlight))]
    public class PlayerMovementController : MonoBehaviour
    {
        
    }
}