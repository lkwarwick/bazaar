using UnityEngine;
using UnityEngine.UI;

namespace Bazaar
{
    internal static class SpriteAnimationUtility
    {
        public static bool TryResolveTarget(Component target, GameObject owner, out Component resolvedTarget)
        {
            resolvedTarget = target;
            if (resolvedTarget == null && owner != null)
            {
                resolvedTarget = owner.GetComponent<SpriteRenderer>();
                if (resolvedTarget == null)
                    resolvedTarget = owner.GetComponent<Image>();
            }

            return resolvedTarget is SpriteRenderer || resolvedTarget is Image;
        }

        public static void SetSprite(Component target, Sprite sprite)
        {
            if (target is SpriteRenderer spriteRenderer)
                spriteRenderer.sprite = sprite;
            else if (target is Image image)
                image.sprite = sprite;
        }

        public static float GetFrameDuration(float framesPerSecond)
        {
            if (float.IsNaN(framesPerSecond) || float.IsInfinity(framesPerSecond) || framesPerSecond <= 0f)
                framesPerSecond = 8f;

            return 1f / framesPerSecond;
        }

        public static object CreateWaitInstruction(float framesPerSecond, bool useUnscaledTime)
        {
            float frameDuration = GetFrameDuration(framesPerSecond);
            return useUnscaledTime
                ? (object)new WaitForSecondsRealtime(frameDuration)
                : new WaitForSeconds(frameDuration);
        }
    }
}