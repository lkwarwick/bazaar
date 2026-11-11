using System;
using UnityEngine;

namespace Bazaar
{
    /// <summary>
    /// Selects and plays named sprite animations on a SpriteRenderer or UI Image.
    /// </summary>
    [AddComponentMenu("Bazaar/Animation Group")]
    public class AnimationGroup : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("SpriteRenderer or UI Image to animate. If empty, one is searched for on this GameObject.")]
        public Component visualTarget;

        [Tooltip("Name of the animation to use on start.")]
        public string defaultAnimation;

        [Tooltip("The available named animations.")]
        public AnimationState[] animationStates;

        [Header("Settings")]
        [Tooltip("Animation playback speed in frames per second.")]
        [Min(0.01f)]
        public float framesPerSecond = 8f;

        [Tooltip("Automatically play the animation on start.")]
        public bool playOnStart = true;

        [Tooltip("Use unscaled time, ignoring Time.timeScale.")]
        public bool useUnscaledTime;

        public event Action OnFrameChange;
        public event Action OnAnimationFinished;

        private bool _isPlaying;
        private float _timer;

        /// <summary>
        /// The current animation that is running.
        /// </summary>
        public AnimationState CurrentAnimation { get; private set; }

        /// <summary>
        /// The current frame that the animation is on.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Gets the SpriteRenderer or Image being animated.
        /// </summary>
        public Component Target => visualTarget;

        private void Start()
        {
            if (!string.IsNullOrEmpty(defaultAnimation))
                Set(defaultAnimation);

            if (playOnStart)
                Play();
        }

        /// <summary>
        /// Changes to a named animation. Does nothing if it is already selected unless ignoreNameMatch is true.
        /// </summary>
        public void Set(string animationName, bool ignoreNameMatch = false)
        {
            if (!ignoreNameMatch && CurrentAnimation.name == animationName)
                return;

            if (animationStates != null)
            {
                for (int i = 0; i < animationStates.Length; i++)
                {
                    AnimationState state = animationStates[i];
                    if (state.name != animationName)
                        continue;

                    CurrentAnimation = state;
                    CurrentFrame = 0;
                    _timer = 0f;
                    ApplyCurrentFrame();
                    return;
                }
            }

            Debug.LogWarning("Attempted to set animation to '" + animationName + "' but it does not exist in this animation set.", this);
        }

        /// <summary>
        /// Starts or restarts the current animation.
        /// </summary>
        [ContextMenu("Play")]
        public void Play()
        {
            if (!HasFrames())
                return;

            if (!ResolveTarget())
            {
                Debug.LogError("AnimationGroup requires a SpriteRenderer or UI Image target.", this);
                return;
            }

            _isPlaying = true;
            _timer = 0f;
            CurrentFrame = 0;
            ApplyCurrentFrame();
            OnFrameChange?.Invoke();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            _timer = 0f;
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timer += deltaTime;

            float frameDuration = SpriteAnimationUtility.GetFrameDuration(framesPerSecond);
            if (_timer < frameDuration)
                return;

            _timer %= frameDuration;
            NextFrame();
        }

        private void NextFrame()
        {
            if (!HasFrames())
            {
                Stop();
                return;
            }

            if (CurrentFrame + 1 >= CurrentAnimation.sprites.Length)
            {
                CurrentFrame = 0;
                OnAnimationFinished?.Invoke();
            }
            else
            {
                CurrentFrame++;
            }

            ApplyCurrentFrame();
            OnFrameChange?.Invoke();
        }

        private bool HasFrames()
        {
            return CurrentAnimation.sprites != null && CurrentAnimation.sprites.Length > 0;
        }

        private bool ResolveTarget()
        {
            if (SpriteAnimationUtility.TryResolveTarget(visualTarget, gameObject, out Component resolvedTarget))
            {
                visualTarget = resolvedTarget;
                return true;
            }

            return false;
        }

        private void ApplyCurrentFrame()
        {
            if (!HasFrames())
                return;

            if (ResolveTarget())
                SpriteAnimationUtility.SetSprite(visualTarget, CurrentAnimation.sprites[CurrentFrame]);
        }
    }

    /// <summary>
    /// Defines a named sequence of animation sprites.
    /// </summary>
    [Serializable]
    public struct AnimationState
    {
        public string name;
        public Sprite[] sprites;
    }
}