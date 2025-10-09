// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

#if UNITY_EDITOR

using UnityEngine;

namespace Animancer.Editor
{
    /// <summary>[Editor-Only] Utility for implementing IMGUI controls.</summary>
    /// <remarks>
    /// <strong>Example:</strong>
    /// <code>
    /// private static readonly int ControlHash = "ControlName".GetHashCode();
    /// 
    /// void OnGUI(Rect area)
    /// {
    ///     var control = new GUIControl(area, ControlHash);
    ///     
    ///     switch (control.EventType)
    ///     {
    ///         case EventType.MouseDown:
    ///             if (control.TryUseMouseDown())
    ///             {
    ///             }
    ///             break;
    ///     
    ///         case EventType.MouseUp:
    ///             if (control.TryUseMouseUp())
    ///             {
    ///             }
    ///             break;
    ///     
    ///         case EventType.MouseDrag:
    ///             if (control.TryUseHotControl())
    ///             {
    ///             }
    ///             break;
    ///     }
    /// }
    /// </code></remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer.Editor/GUIControl
    public readonly struct GUIControl
    {
        /************************************************************************************************************************/

        /// <summary>The position and size of this control</summary>
        public readonly Rect Area;

        /// <summary>The Control ID of this control.</summary>
        public readonly int ID;

        /// <summary>The event being processed by this control.</summary>
        public readonly Event Event;

        /************************************************************************************************************************/

        /// <summary>The type of the <see cref="Event"/> in relation to this control.</summary>
        public EventType EventType
            => Event.GetTypeForControl(ID);

        /// <summary>Does the <see cref="Area"/> contain the <see cref="Event.mousePosition"/>?</summary>
        public bool ContainsMousePosition
            => Area.Contains(Event.mousePosition);

        /************************************************************************************************************************/

        /// <summary>Creaates a new <see cref="GUIControl"/>.</summary>
        public GUIControl(Rect area, Event currentEvent, int idHint, FocusType focusType = FocusType.Passive)
        {
            Area = area;
            Event = currentEvent;
            ID = GUIUtility.GetControlID(idHint, focusType, area);
        }

        /// <summary>Creaates a new <see cref="GUIControl"/> with the <see cref="Event.current"/>.</summary>
        public GUIControl(Rect area, int idHint, FocusType focusType = FocusType.Passive)
            : this(area, Event.current, idHint, focusType)
        {
        }

        /************************************************************************************************************************/

        /// <summary><see cref="AnimancerGUI.TryUseMouseDown"/></summary>
        public bool TryUseMouseDown()
            => AnimancerGUI.TryUseMouseDown(Area, Event, ID);

        /// <summary><see cref="AnimancerGUI.TryUseMouseUp"/></summary>
        public bool TryUseMouseUp(bool guiChanged = false)
            => AnimancerGUI.TryUseMouseUp(Event, ID, guiChanged);

        /// <summary><see cref="AnimancerGUI.TryUseHotControl"/></summary>
        public bool TryUseHotControl(bool guiChanged = true)
            => AnimancerGUI.TryUseHotControl(Event, ID, guiChanged);

        /// <summary><see cref="AnimancerGUI.TryUseKey"/></summary>
        public bool TryUseKey(KeyCode key = KeyCode.None)
            => AnimancerGUI.TryUseKey(Event, ID, key);

        /************************************************************************************************************************/
    }
}

#endif

