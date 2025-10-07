// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A utility for manually drawing a <see cref="UnityEditor.Editor"/>.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/CachedEditor
    public class CachedEditor : IDisposable
    {
        /************************************************************************************************************************/

        [NonSerialized] private Object[] _Targets = Array.Empty<Object>();
        [NonSerialized] private UnityEditor.Editor _Editor;
        [NonSerialized] private bool _WillCleanup;

        /************************************************************************************************************************/

        /// <summary>
        /// Creates a <see cref="UnityEditor.Editor"/> for the `target`
        /// and caches it to be returned by subsequent calls with the same `target`.
        /// </summary>
        public UnityEditor.Editor GetEditor(Object target)
        {
            if (_Targets.Length == 1 &&
                _Targets[0] == target &&
                _Editor != null)
                return _Editor;

            Dispose();

            AnimancerUtilities.SetLength(ref _Targets, 1);
            _Targets[0] = target;
            _Editor = UnityEditor.Editor.CreateEditor(target);
            EnsureCleanup();
            return _Editor;
        }

        /// <summary>
        /// Creates a <see cref="UnityEditor.Editor"/> for the `targets`
        /// and caches it to be returned by subsequent calls with the same `targets`.
        /// </summary>
        public UnityEditor.Editor GetEditor(Object[] targets)
        {
            if (AnimancerUtilities.ContentsAreEqual(targets, _Targets) &&
                _Editor != null)
                return _Editor;

            Dispose();

            _Targets = targets;
            _Editor = UnityEditor.Editor.CreateEditor(targets);
            EnsureCleanup();
            return _Editor;
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the cached <see cref="UnityEditor.Editor"/>.</summary>
        public void Dispose()
            => Object.DestroyImmediate(_Editor);

        /************************************************************************************************************************/

        /// <summary>Ensures that <see cref="Dispose"/> will be called before assemblies are reloaded.</summary>
        private void EnsureCleanup()
        {
            if (_WillCleanup)
                return;

            _WillCleanup = true;

            AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        }

        /************************************************************************************************************************/
    }
}

#endif

