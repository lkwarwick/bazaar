using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Bazaar
{
    /// <summary>
    /// Loads and plays Addressable sprite animation definitions.
    /// </summary>
    public static class AnimationPlayer
    {
        private static readonly Dictionary<string, AsyncOperationHandle<SpriteAnimationDefinition>> CachedDefinitions = new();

        /// <summary>
        /// Loads an animation definition and plays it in a new GameObject without requiring async code at the call site.
        /// Load failures are logged.
        /// </summary>
        public static void PlayAddressable(string address, Vector3 position)
        {
            PlayAddressableAndLog(address, position);
        }

        /// <summary>
        /// Loads an animation definition and plays it in a new GameObject at the specified position.
        /// </summary>
        public static async Task<GameObject> PlayAddressableAsync(string address, Vector3 position)
        {
            ValidateAddress(address);

            SpriteAnimationDefinition definition = await LoadDefinitionAsync(address);
            if (definition.Sprites == null || definition.Sprites.Length == 0)
                throw new InvalidOperationException(
                    $"Sprite animation definition at address '{address}' does not contain any frames.");

            GameObject animationObject = new($"Animation - {definition.name}");
            animationObject.transform.position = position;
            animationObject.AddComponent<SpriteRenderer>();
            AnimationLoop player = animationObject.AddComponent<AnimationLoop>();
            player.Play(definition);

            return animationObject;
        }

        /// <summary>
        /// Loads and caches an animation definition without starting playback.
        /// </summary>
        public static async Task PreloadAsync(string address)
        {
            ValidateAddress(address);
            await LoadDefinitionAsync(address);
        }

        /// <summary>
        /// Releases all cached animation definitions. Stop animations using these definitions first.
        /// </summary>
        public static void ReleaseCachedDefinitions()
        {
            foreach (AsyncOperationHandle<SpriteAnimationDefinition> handle in CachedDefinitions.Values)
            {
                if (handle.IsValid() && !handle.IsDone)
                    throw new InvalidOperationException("Cannot release cached definitions while they are still loading.");
            }

            foreach (AsyncOperationHandle<SpriteAnimationDefinition> handle in CachedDefinitions.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }

            CachedDefinitions.Clear();
        }

        private static async void PlayAddressableAndLog(string address, Vector3 position)
        {
            try
            {
                await PlayAddressableAsync(address, position);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static async Task<SpriteAnimationDefinition> LoadDefinitionAsync(string address)
        {
            if (!CachedDefinitions.TryGetValue(address, out AsyncOperationHandle<SpriteAnimationDefinition> handle) ||
                !handle.IsValid())
            {
                handle = Addressables.LoadAssetAsync<SpriteAnimationDefinition>(address);
                CachedDefinitions[address] = handle;
            }

            try
            {
                if (!handle.IsDone)
                    await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    Exception cause = handle.OperationException ??
                                      new InvalidOperationException("The loaded asset is not a sprite animation definition.");
                    throw new InvalidOperationException(
                        $"Failed to load sprite animation definition at address '{address}'.",
                        cause);
                }

                return handle.Result;
            }
            catch
            {
                if (CachedDefinitions.TryGetValue(address, out AsyncOperationHandle<SpriteAnimationDefinition> cachedHandle) &&
                    cachedHandle.Equals(handle))
                {
                    CachedDefinitions.Remove(address);
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }

                throw;
            }
        }

        private static void ValidateAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("An Addressables key is required.", nameof(address));
        }

    }
}
