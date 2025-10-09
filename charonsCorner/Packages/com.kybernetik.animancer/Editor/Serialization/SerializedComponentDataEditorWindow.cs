// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A <see cref="SerializedDataEditorWindow{TObject, TData}"/> for <see cref="Component"/>s.
    /// </summary>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializedComponentDataEditorWindow_2
    public abstract class SerializedComponentDataEditorWindow<TObject, TData> :
        SerializedDataEditorWindow<TObject, TData>
        where TObject : Component
        where TData : class, ICopyable<TData>, IEquatable<TData>, new()
    {
        /************************************************************************************************************************/

        [SerializeField] private GameObject _SourceGameObject;
        [SerializeField] private int _SourceComponentInstanceID;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override TObject SourceObject
        {
            get
            {
                // For whatever reason, component references in an EditorWindow can't survive entering Play Mode but
                // a GameObject or Transform reference can so we use that to recover the component.

                // Storing the Instance ID also works, but seems to also survive restarting the Unity Editor which is
                // bad because the scene references inside the data don't survive that which would leave us
                // with an open window full of empty references. Working around that isn't worth the effort.

                // So if the GameObject still exists, we use the Component's Instance ID to find it.

                var source = base.SourceObject;

                if (source == null && _SourceGameObject != null)
                    source = base.SourceObject = EditorUtility.InstanceIDToObject(_SourceComponentInstanceID) as TObject;

                return source;
            }
        }

        /************************************************************************************************************************/

        /// <inheritdoc/>
        public override bool SourceObjectMightBePrefab
            => true;

        /************************************************************************************************************************/

        /// <inheritdoc/>
        protected override void CaptureData()
        {
            base.CaptureData();

            if (SourceObject != null)
            {
                _SourceGameObject = SourceObject.gameObject;
                _SourceComponentInstanceID = SourceObject.GetInstanceID();
            }
        }

        /************************************************************************************************************************/
    }
}

#endif
