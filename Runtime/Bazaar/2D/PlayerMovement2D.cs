using UnityEngine;
using UnityEngine.InputSystem;

namespace Bazaar2D {
    public class PlayerMovement2D : Movement2D
    {
        [Header("Player Settings")]

        /// <summary>
        /// Input action reference used to read the player's movement direction.
        /// </summary>
        [Tooltip("Input action used to read the player's movement direction.")]
        public InputActionReference moveAction;

        protected override Vector2 ReceiveInput()
        {
            return moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        }
    }
}
