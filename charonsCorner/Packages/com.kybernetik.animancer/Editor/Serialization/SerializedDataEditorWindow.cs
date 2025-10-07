// Animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only]
    /// A window for managing a copy of some serialized data and applying or reverting it.
    /// </summary>
    /// <remarks>
    /// This system assumes the implementation of <see cref="IEquatable{T}"/>
    /// compares the values of all fields in <typeparamref name="TData"/>.
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/SerializedDataEditorWindow_2
    public abstract class SerializedDataEditorWindow<TObject, TData> : EditorWindow
        where TObject : Object
        where TData : class, ICopyable<TData>, IEquatable<TData>, new()
    {
        /************************************************************************************************************************/

        [SerializeField]
        private TObject _SourceObject;

        /// <summary>The object which contains the data this class manages.</summary>
        /// <remarks><see cref="SetAndCaptureSource"/> should generally be used instead of setting this property directly.</remarks>
        public virtual TObject SourceObject
        {
            get => _SourceObject;
            protected set => _SourceObject = value;
        }

        /************************************************************************************************************************/

        /// <summary>The <see cref="Data"/> field of the <see cref="SourceObject"/>.</summary>
        public abstract TData SourceData { get; set; }

        /************************************************************************************************************************/

        [SerializeField]
        private TData _Data;

        /// <summary>A copy of the <see cref="SourceData"/> being managed by this window.</summary>
        public ref TData Data
            => ref _Data;

        /************************************************************************************************************************/

        /// <summary>Is the <see cref="Data"/> managed by this window different to the <see cref="SourceData"/>.</summary>
        public bool HasDataChanged
        {
            get
            {
                try
                {
                    return _Data != null && !_Data.Equals(SourceData);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                    return false;
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Initializes this window.</summary>
        protected virtual void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.wantsToQuit += OnTryCloseEditor;
            Undo.undoRedoPerformed += Repaint;
        }

        /// <summary>Cleans up this window.</summary>
        protected virtual void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.wantsToQuit -= OnTryCloseEditor;
            Undo.undoRedoPerformed -= Repaint;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Prompts the user to <see cref="Apply"/> or <see cref="Revert"/>
        /// if there are changes in the <see cref="Data"/> when this window is closed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            var sourceObject = SourceObject;
            if (sourceObject == null ||
                !HasDataChanged ||
                titleContent == null)
                return;

            if (EditorUtility.DisplayDialog(
                titleContent.text,
                $"Apply unsaved changes to '{sourceObject.name}'?",
                "Apply",
                "Revert"))
            {
                Apply();
            }
        }

        /************************************************************************************************************************/

        /// <summary>Called before closing the Unity Editor to confirm that un-saved data is applied.</summary>
        private bool OnTryCloseEditor()
        {
            var sourceObject = SourceObject;
            if (sourceObject == null ||
                !HasDataChanged ||
                titleContent == null)
                return true;

            var option = EditorUtility.DisplayDialogComplex(
                titleContent.text,
                $"Apply unsaved changes to '{sourceObject.name}'?",
                "Apply",
                "Cancel",
                "Revert");

            switch (option)
            {
                case 0:// Apply.
                    Apply();
                    return true;

                case 2:// Revert.
                    Revert();
                    return true;

                case 1:// Cancel.
                default:
                    return false;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the <see cref="SourceObject"/> and captures the <see cref="Data"/>
        /// as a copy of its <see cref="SourceData"/>.
        /// </summary>
        protected void SetAndCaptureSource(TObject sourceObject)
        {
            _SourceObject = sourceObject;
            CaptureData();
            Repaint();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Override this to return <c>true</c> if the <see cref="SourceObject"/> could be part of a prefab
        /// to ensure that modifications are serialized properly.
        /// </summary>
        public virtual bool SourceObjectMightBePrefab
            => false;

        /************************************************************************************************************************/

        /// <summary>Saves the edited <see cref="Data"/> into the <see cref="SourceObject"/>.</summary>
        public virtual void Apply()
        {
            var sourceObject = SourceObject;
            if (sourceObject == null)
                return;

            using (new ModifySerializedField(sourceObject, name, SourceObjectMightBePrefab))
            {
                SourceData = _Data.CopyableClone();

                if (EditorUtility.IsPersistent(SourceObject))
                {
                    var objects = SetPool.Acquire<Object>();
                    GatherObjectReferences(sourceObject, objects);

                    foreach (var obj in objects)
                        if (!EditorUtility.IsPersistent(obj))
                            AssetDatabase.AddObjectToAsset(obj, SourceObject);

                    SetPool.Release(objects);
                }
            }

            Repaint();
            AssetDatabase.SaveAssets();
        }

        /************************************************************************************************************************/

        /// <summary>Gathers all objects referenced by the `root`.</summary>
        public static void GatherObjectReferences(Object root, HashSet<Object> objects)
        {
            using var serializedObject = new SerializedObject(root);
            var property = serializedObject.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType == SerializedPropertyType.ObjectReference)
                {
                    var value = property.objectReferenceValue;
                    if (value != null)
                        objects.Add(value);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>Restores the <see cref="Data"/> to the original values from the <see cref="SourceData"/>.</summary>
        public virtual void Revert()
        {
            RecordUndo();
            CaptureData();
        }

        /************************************************************************************************************************/

        /// <summary>Stores a copy of the <see cref="SourceData"/> in the <see cref="Data"/>.</summary>
        protected virtual void CaptureData()
        {
            _Data = SourceData?.CopyableClone() ?? new();
            AnimancerReflection.TryInvoke(_Data, "OnValidate");
        }

        /************************************************************************************************************************/

        /// <summary>Records the current state of this window so it can be undone later.</summary>
        public TData RecordUndo()
            => RecordUndo(titleContent.text);

        /// <summary>Records the current state of this window so it can be undone later.</summary>
        public virtual TData RecordUndo(string name)
        {
            Undo.RecordObject(this, name);
            Repaint();
            return _Data;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Opens a new <typeparamref name="TWindow"/> for the `sourceObject`
        /// or gives focus to an existing window that was already displaying it.
        /// </summary>
        public static TWindow Open<TWindow>(
            TObject sourceObject,
            bool onlyOneWindow = false,
            params Type[] desiredDockNextTo)
            where TWindow : SerializedDataEditorWindow<TObject, TData>
        {
            if (!onlyOneWindow)
            {
                foreach (var window in Resources.FindObjectsOfTypeAll<TWindow>())
                {
                    if (window.SourceObject == sourceObject)
                    {
                        window.Show();
                        window.SetAndCaptureSource(sourceObject);
                        window.Focus();
                        return window;
                    }
                }
            }

            var newWindow = onlyOneWindow
                ? GetWindow<TWindow>(desiredDockNextTo ?? Type.EmptyTypes)
                : CreateInstance<TWindow>();
            newWindow.Show();
            newWindow.SetAndCaptureSource(sourceObject);
            return newWindow;
        }

        /************************************************************************************************************************/
        #region Auto Apply
        /************************************************************************************************************************/

        /// <summary>The <see cref="EditorPrefs"/> key for <see cref="AutoApply"/>.</summary>
        protected virtual string AutoApplyPref
            => $"{titleContent.text}.{nameof(AutoApply)}";

        /************************************************************************************************************************/

        private bool _HasLoadedAutoApply;
        private bool _AutoApply;
        private bool _EnabledAutoApplyInPlayMode;

        /// <summary>Is the "Auto Apply" toggle currently enabled?</summary>
        public bool AutoApply
        {
            get
            {
                if (!_HasLoadedAutoApply)
                {
                    _HasLoadedAutoApply = true;
                    _AutoApply = EditorPrefs.GetBool(AutoApplyPref);
                }

                return _AutoApply;
            }
            set
            {
                _HasLoadedAutoApply = true;
                _AutoApply = value;
                _EnabledAutoApplyInPlayMode = _AutoApply && EditorApplication.isPlayingOrWillChangePlaymode;
                EditorPrefs.SetBool(AutoApplyPref, value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>Handles entering and exiting Play Mode.</summary>
        protected virtual void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    if (HasDataChanged && focusedWindow != null)
                        focusedWindow.ShowNotification(new($"{titleContent.text} window has un-applied changes"));
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    if (_EnabledAutoApplyInPlayMode)
                        AutoApply = false;
                    break;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region GUI
        /************************************************************************************************************************/

        private static readonly GUIContent
            RevertLabel = new(
                "Revert",
                "Undo all changes made in this window"),
            ApplyLabel = new(
                "Apply",
                "Apply all changes made in this window to the source object"),
            AutoApplyLabel = new(
                "Auto",
                "Immediately apply all changes made in this window to the source object?" +
                "\n\nIf enabled in Play Mode, this toggle will be disabled when returning to Edit Mode.");

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the pixel width required for
        /// <see cref="DoApplyRevertGUI(Rect, Rect, Rect, ButtonGroupStyles)"/>.
        /// </summary>
        public float CalculateApplyRevertWidth(ButtonGroupStyles styles = default)
        {
            styles.CopyMissingStyles(ButtonGroupStyles.Button);
            return
                styles.left.CalculateWidth(RevertLabel) + 1 +
                styles.middle.CalculateWidth(ApplyLabel) + 1 +
                styles.right.CalculateWidth(AutoApplyLabel) + 1;
        }

        /************************************************************************************************************************/

        /// <summary>Draws GUI controls for <see cref="Revert"/>, <see cref="Apply"/>, and <see cref="AutoApply"/>.</summary>
        public void DoApplyRevertGUI(ButtonGroupStyles styles = default)
        {
            styles.CopyMissingStyles(ButtonGroupStyles.Button);

            GUILayout.BeginHorizontal();

            var leftArea = GUILayoutUtility.GetRect(RevertLabel, styles.left);
            var middleArea = GUILayoutUtility.GetRect(ApplyLabel, styles.middle);
            var rightArea = GUILayoutUtility.GetRect(AutoApplyLabel, styles.right);

            DoApplyRevertGUI(leftArea, middleArea, rightArea, styles);

            GUILayout.EndHorizontal();
        }

        /************************************************************************************************************************/

        /// <summary>Draws GUI controls for <see cref="Revert"/>, <see cref="Apply"/>, and <see cref="AutoApply"/>.</summary>
        public void DoApplyRevertGUI(Rect area, ButtonGroupStyles styles = default)
        {
            styles.CopyMissingStyles(ButtonGroupStyles.Button);

            var leftArea = AnimancerGUI.StealFromLeft(ref area, styles.left.CalculateWidth(RevertLabel) + 1);
            var middleArea = AnimancerGUI.StealFromLeft(ref area, styles.middle.CalculateWidth(ApplyLabel) + 1);

            DoApplyRevertGUI(leftArea, middleArea, area, styles);
        }

        /************************************************************************************************************************/

        /// <summary>Draws GUI controls for <see cref="Revert"/>, <see cref="Apply"/>, and <see cref="AutoApply"/>.</summary>
        public void DoApplyRevertGUI(
            Rect leftArea,
            Rect middleArea,
            Rect rightArea,
            ButtonGroupStyles styles = default)
        {
            styles.CopyMissingStyles(ButtonGroupStyles.Button);

            var enabled = GUI.enabled;
            GUI.enabled = SourceObject != null && HasDataChanged;

            // Revert.
            if (GUI.Button(leftArea, RevertLabel, styles.left))
                Revert();

            // Apply.
            if (GUI.Button(middleArea, ApplyLabel, styles.middle))
                Apply();

            // Auto Apply.
            var autoApply = AutoApply;
            if (autoApply && GUI.enabled)
                Apply();

            GUI.enabled = enabled;

            if (autoApply != GUI.Toggle(rightArea, autoApply, AutoApplyLabel, styles.right))
                AutoApply = !autoApply;
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

#endif
