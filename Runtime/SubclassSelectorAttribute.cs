using System;
using UnityEngine;

namespace Bazaar
{
    /// <summary>Selects a concrete subclass of the specified base type in the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubclassSelectorAttribute : PropertyAttribute
    {
        public Type BaseType { get; }

        public SubclassSelectorAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}