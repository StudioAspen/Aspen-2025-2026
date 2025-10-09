// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer
{
    /// <summary>Attribute for static methods which try to create a transition from an object.</summary>
    /// <remarks>
    /// The method signature must be:
    /// <c>static ITransitionDetailed TryCreateTransition(Object target)</c>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/TryCreateTransitionAttribute
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TryCreateTransitionAttribute : Attribute
    {
        /************************************************************************************************************************/

        /// <summary>The base type of object which the attributed method can handle.</summary>
        public Type ObjectType { get; private set; }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="TryCreateTransitionAttribute"/>.</summary>
        public TryCreateTransitionAttribute(Type objectType)
        {
            ObjectType = objectType;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private static List<Func<Object, ITransitionDetailed>> _Methods;
        private static List<Type> _TargetTypes;

        /// <summary>[Editor-Only] Ensures that all methods with this attribute have been gathered.</summary>
        private static void InitializeMethods()
        {
            if (_Methods != null)
                return;

            _Methods = new();
            _TargetTypes = new();

            foreach (var method in TypeCache.GetMethodsWithAttribute<TryCreateTransitionAttribute>())
            {
                try
                {
                    var attributes = method.GetCustomAttributes(typeof(TryCreateTransitionAttribute), true);
                    if (!attributes.IsNullOrEmpty())
                    {
                        var attribute = attributes[0] as TryCreateTransitionAttribute;
                        if (attribute?.ObjectType != null)
                            _TargetTypes.Add(attribute.ObjectType);
                    }

                    var func = Delegate.CreateDelegate(typeof(Func<Object, ITransitionDetailed>), method);
                    _Methods.Add((Func<Object, ITransitionDetailed>)func);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"Failed to create delegate for" +
                        $" {method.DeclaringType.GetNameCS()}.{method.Name}," +
                        $" it must take one {typeof(Object).FullName} parameter" +
                        $" and return {typeof(ITransition).FullName}" +
                        $": {exception}");
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Validates that the `mainAsset` is actually an asset.</summary>
        public static bool CanCreateAndSave(Object mainAsset)
        {
            if (TransitionAssetBase.CreateInstance == null)
                return false;

            if (!CanCreate(mainAsset.GetType()))
                return false;

            var path = AssetDatabase.GetAssetPath(mainAsset);
            return !string.IsNullOrEmpty(path);
        }

        /// <summary>[Editor-Only] Validates that the `mainAsset` is actually an asset.</summary>
        public static bool CanCreate(Type targetType)
        {
            InitializeMethods();
            for (int i = 0; i < _TargetTypes.Count; i++)
                if (_TargetTypes[i].IsAssignableFrom(targetType))
                    return true;

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>[Editor-Only] Tries to create an asset containing an appropriate transition for the `target`.</summary>
        public static TransitionAssetBase TryCreateTransitionAsset(Object target, bool saveNextToTarget = false)
        {
            if (target is TransitionAssetBase asset)
                return asset;

            var assetType = TransitionAssetBase.CreateInstance;
            if (assetType == null)
                return null;

            InitializeMethods();

            for (int i = 0; i < _Methods.Count; i++)
            {
                var transition = _Methods[i](target);
                if (transition is not null)
                {
                    var created = TransitionAssetBase.CreateInstance(transition);
                    created.name = target.name;

                    if (saveNextToTarget)
                    {
                        var path = AssetDatabase.GetAssetPath(target);
                        if (string.IsNullOrEmpty(path))
                        {
                            Debug.LogError(
                                $"Can't create TransitionAsset for '{target}' because it isn't an asset.",
                                target);

                            return created;
                        }

                        path = System.IO.Path.GetDirectoryName(path);
                        path = System.IO.Path.Combine(path, $"{target.name}.asset");
                        path = AssetDatabase.GenerateUniqueAssetPath(path);

                        AssetDatabase.CreateAsset(created, path);
                        Selection.activeObject = created;
                    }

                    return created;
                }
            }

            return null;
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

