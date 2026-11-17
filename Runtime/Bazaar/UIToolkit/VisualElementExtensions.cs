using System;
using UnityEngine.UIElements;

namespace Bazaar {

    /// <summary>
    /// Extension methods for the VisualElement class.
    /// </summary>
    public static class VisualElementExtensions
    {
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
            // Enter
            element.RegisterCallback<MouseEnterEvent>(_ => onEnter?.Invoke());
            element.RegisterCallback<FocusEvent>(_ => onEnter?.Invoke());

            // Exit
            element.RegisterCallback<MouseLeaveEvent>(_ => onExit?.Invoke());
            element.RegisterCallback<BlurEvent>(_ => onExit?.Invoke());

            // Submit
            element.RegisterCallback<PointerUpEvent>(_ => onSubmit?.Invoke());
            element.RegisterCallback<PointerCaptureOutEvent>(_ => onSubmit?.Invoke());
        }
    }
}
