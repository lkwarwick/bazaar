using System.Collections.Generic;
using System;
using UnityEngine;

namespace Bazaar2D
{
    /// <summary>
    /// Handles movement for any entity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Movement2D : MonoBehaviour
    {
        [Header("Movement Settings")]

        /// <summary>
        /// The movement speed of the player.
        /// </summary>
        [Tooltip("The movement speed of the player.")] public float speed = 4f;

        /// <summary>
        /// Whether this entity can be affected by knockback.
        /// </summary>
        [Tooltip("Whether this entity can be affected by knockback.")]
        public bool canReceiveKnockback = true;

        /// <summary>
        /// Current direction of movement.
        /// </summary>
        public Vector2 Direction { protected set; get; }

        /// <summary>
        /// Last non-zero direction.
        /// </summary>
        public Vector2 LastDirection { protected set; get; }

        /// <summary>
        /// Current movement direction snapped to the nearest cardinal direction, or zero while idle.
        /// </summary>
        public Vector2Int FacingDirection { protected set; get; }

        /// <summary>
        /// Last non-zero movement direction snapped to the nearest cardinal direction.
        /// </summary>
        public Vector2Int LastFacingDirection { protected set; get; }

        /// <summary>
        /// Invoked when the last cardinal facing direction changes, with the new direction.
        /// </summary>
        public event Action<Vector2Int> OnLastFacingDirectionChange;

        /// <summary>
        /// Any objects in this list prevent movement.
        /// </summary>
        public List<object> FreezeLocks { get; } = new List<object>();

        /// <summary>
        /// Rigidbody2D component.
        /// </summary>
        public Rigidbody2D Rigidbody2D { protected set; get; }

        private float knockbackTimer = 0f;

        /// <inheritdoc/>
        protected virtual void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();  
        }

        /// <inheritdoc/>
        protected virtual void FixedUpdate()
        {
            Direction = ReceiveInput();

            if (FreezeLocks.Count > 0) Direction = Vector2.zero;

            FacingDirection = ToCardinalDirection(Direction);
            if (Direction != Vector2.zero)
            {
                LastDirection = Direction;
                if (LastFacingDirection != FacingDirection)
                {
                    LastFacingDirection = FacingDirection;
                    OnLastFacingDirectionChange?.Invoke(LastFacingDirection);
                }
            }

            // Only control movement when NOT in knockback
            if (knockbackTimer <= 0f)
            {
                Rigidbody2D.linearVelocity = Direction * GetSpeed();
            }
            else
            {
                knockbackTimer -= Time.fixedDeltaTime;
                
                // Let physics handle the knockback velocity naturally
                // Optionally add drag to slow down faster
                Rigidbody2D.linearVelocity *= 0.92f; // Damping
            }
        }

        /// <summary>
        /// Applies knockback to the entity.
        /// </summary>
        /// <param name="direction">Normalized direction vector for knockback.</param>
        /// <param name="intensity">Force intensity of the knockback.</param>
        /// <param name="duration">Duration of the knockback effect in seconds.</param>
        public void Knockback(Vector2 direction, float intensity = 20f, float duration = 0.35f)
        {
            if (!canReceiveKnockback) return;

            knockbackTimer = duration;
            
            // Apply force once - Unity handles the rest
            Rigidbody2D.AddForce(direction.normalized * intensity, ForceMode2D.Impulse);
        }

        /// <summary>
        /// Sets the current and last cardinal facing direction without changing movement.
        /// </summary>
        public void SetFacingDirection(Vector2 direction)
        {
            Vector2Int facingDirection = ToCardinalDirection(direction);
            if (facingDirection == Vector2Int.zero)
                return;

            FacingDirection = facingDirection;
            if (LastFacingDirection == facingDirection)
                return;

            LastFacingDirection = facingDirection;
            OnLastFacingDirectionChange?.Invoke(facingDirection);
        }

        /// <summary>
        /// Receives input for movement.
        /// </summary>
        /// <returns>Direction vector based on input.</returns>
        protected abstract Vector2 ReceiveInput();

        private static Vector2Int ToCardinalDirection(Vector2 direction)
        {
            if (direction == Vector2.zero)
                return Vector2Int.zero;

            if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
                return direction.x >= 0f ? Vector2Int.right : Vector2Int.left;

            return direction.y >= 0f ? Vector2Int.up : Vector2Int.down;
        }

        /// <summary>
        /// Get the speed for the entity
        /// </summary>
        /// <returns></returns>
        protected virtual float GetSpeed()
        {
            return speed;
        }
    }
}
