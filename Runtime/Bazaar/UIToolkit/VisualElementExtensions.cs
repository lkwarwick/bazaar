using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace Bazaar
{
    /// <summary>
    /// Extension methods for the VisualElement class.
    /// </summary>
    public static class VisualElementExtensions
    {
        private static readonly ConditionalWeakTable<VisualElement, List<Action>> RegisteredCleanups = new();

        /// <summary>
        /// Add callbacks across platforms for buttons.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="onEnter"></param>
        /// <param name="onExit"></param>
        /// <param name="onSubmit"></param>
        public static void AddCallbacks(
            this VisualElement element,
            Action onEnter,
            Action onExit,
            Action onSubmit
        )
        {
            if (element == null)
            {
                return;
            }

            EventCallback<MouseEnterEvent> handleMouseEnter = _ => onEnter?.Invoke();
            EventCallback<FocusEvent> handleFocus = _ => onEnter?.Invoke();
            EventCallback<MouseLeaveEvent> handleMouseLeave = _ => onExit?.Invoke();
            EventCallback<BlurEvent> handleBlur = _ => onExit?.Invoke();
            EventCallback<ClickEvent> handleClick = _ => onSubmit?.Invoke();
            EventCallback<NavigationSubmitEvent> handleSubmit = _ => onSubmit?.Invoke();

            element.RegisterCallback(handleMouseEnter);
            element.RegisterCallback(handleFocus);
            element.RegisterCallback(handleMouseLeave);
            element.RegisterCallback(handleBlur);
            element.RegisterCallback(handleClick);
            element.RegisterCallback(handleSubmit);

            List<Action> cleanups = RegisteredCleanups.GetOrCreateValue(element);
            cleanups.Add(() =>
            {
                element.UnregisterCallback(handleMouseEnter);
                element.UnregisterCallback(handleFocus);
                element.UnregisterCallback(handleMouseLeave);
                element.UnregisterCallback(handleBlur);
                element.UnregisterCallback(handleClick);
                element.UnregisterCallback(handleSubmit);
            });
        }

        public static void RemoveCallbacks(this VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            if (RegisteredCleanups.TryGetValue(element, out List<Action> cleanups))
            {
                for (int i = cleanups.Count - 1; i >= 0; i--)
                {
                    cleanups[i].Invoke();
                }

                cleanups.Clear();
            }
        }
    }
}