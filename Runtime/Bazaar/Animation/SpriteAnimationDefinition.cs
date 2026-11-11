using UnityEngine;

namespace Bazaar
{
    /// <summary>
    /// Reusable sprite frames and playback settings for an AnimationLoop.
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteAnimation", menuName = "Bazaar/Animation/Sprite Animation")]
    public sealed class SpriteAnimationDefinition : ScriptableObject
    {
        [SerializeField] private Sprite[] sprites;

        [Min(0.01f)]
        [SerializeField] private float framesPerSecond = 8f;

        [SerializeField] private bool loop;

        [Tooltip("Destroy the spawned GameObject when a non-looping animation finishes.")]
        [SerializeField] private bool destroyOnFinish = true;

        [SerializeField] private bool useUnscaledTime;

        public Sprite[] Sprites => sprites;
        public float FramesPerSecond => framesPerSecond;
        public bool Loop => loop;
        public bool DestroyOnFinish => destroyOnFinish;
        public bool UseUnscaledTime => useUnscaledTime;
    }
}
