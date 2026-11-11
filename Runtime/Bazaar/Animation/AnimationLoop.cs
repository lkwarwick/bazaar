using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bazaar
{
    /// <summary>
    /// Animates a sequence of sprites on a SpriteRenderer or UI Image.
    /// </summary>
    [AddComponentMenu("Bazaar/Animation Loop")]
    public class AnimationLoop : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("SpriteRenderer or UI Image to animate. If empty, one is searched for on this GameObject.")]
        public Component visualTarget;

        [Tooltip("The sequence of sprites to animate.")]
        public Sprite[] sprites;

        [Header("Settings")]
        [Tooltip("Animation playback speed in frames per second.")]
        [Min(0.01f)]
        public float framesPerSecond = 8f;

        [Tooltip("Destroy this GameObject when the animation finishes.")]
        public bool destroyOnFinish;

        [Tooltip("Automatically play the animation on start.")]
        public bool playOnStart;

        [Tooltip("Repeat the animation indefinitely.")]
        public bool loop;

        [Tooltip("Use unscaled time, ignoring Time.timeScale.")]
        public bool useUnscaledTime;

        public event Action OnFrameChange;
        public event Action OnAnimationFinished;

        private Coroutine _play;

        /// <summary>
        /// Gets the SpriteRenderer or Image being animated.
        /// </summary>
        public Component Target => visualTarget;

        private void Start()
        {
            if (playOnStart)
                Play();
        }

        /// <summary>
        /// Starts the animation coroutine.
        /// </summary>
        [ContextMenu("Play")]
        public void Play()
        {
            Stop();
            if (sprites == null || sprites.Length == 0 || !ResolveTarget())
                return;

            object waitInstruction = SpriteAnimationUtility.CreateWaitInstruction(framesPerSecond, useUnscaledTime);
            _play = StartCoroutine(PlayCoroutine(waitInstruction));
        }

        /// <summary>
        /// Configures and starts this player from an animation definition.
        /// </summary>
        public void Play(SpriteAnimationDefinition definition)
        {
            if (definition == null)
            {
                Debug.LogError("Cannot play a null sprite animation definition.", this);
                return;
            }

            Stop();
            visualTarget = null;
            sprites = definition.Sprites;
            framesPerSecond = definition.FramesPerSecond;
            destroyOnFinish = definition.DestroyOnFinish;
            loop = definition.Loop;
            useUnscaledTime = definition.UseUnscaledTime;
            Play();
        }

        /// <summary>
        /// Stops the animation coroutine.
        /// </summary>
        public void Stop()
        {
            if (_play == null)
                return;

            StopCoroutine(_play);
            _play = null;
        }

        private IEnumerator PlayCoroutine(object waitInstruction)
        {
            do
            {
                foreach (Sprite sprite in sprites)
                {
                    SpriteAnimationUtility.SetSprite(visualTarget, sprite);
                    OnFrameChange?.Invoke();
                    yield return waitInstruction;
                }

                OnAnimationFinished?.Invoke();
                if (destroyOnFinish)
                {
                    _play = null;
                    Destroy(gameObject);
                    yield break;
                }
            } while (loop);

            _play = null;
        }

        private bool ResolveTarget()
        {
            if (SpriteAnimationUtility.TryResolveTarget(visualTarget, gameObject, out Component resolvedTarget))
            {
                visualTarget = resolvedTarget;
                return true;
            }

            Debug.LogError("AnimationLoop requires a SpriteRenderer or UI Image target.", this);
            return false;
        }
    }
}