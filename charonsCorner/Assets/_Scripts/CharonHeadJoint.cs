using System;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using CharonsCorner.Runtime;

public class CharonHeadJoint : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [Serializable]
    public class StaringItem
    {
        public string startEventName;
        public Transform targetTransform;

        [Button("Test Start")]
        private void TestStart()
        {
            if (Application.isPlaying)
            {
                MMGameEvent.Trigger(startEventName);
            }
            else
            {
                Debug.LogWarning("Test buttons only work in Play Mode.");
            }
        }
    }

    [SerializeField] private List<StaringItem> staringItems;
    [SerializeField] private string stopEventName;
    [SerializeField] private JawOverride jawOverrider;
    [SerializeField] private AnimationStringsSO animationStrings;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Vector3 defaultRotation;
    public Transform headObject;

    [Button("Update Scriptable Object List")]
    private void UpdateScriptableObjectList()
    {
        if (animationStrings == null)
        {
            Debug.LogWarning("[CharonHeadJoint] No AnimationStringsSO assigned!");
            return;
        }

        bool changed = false;
        foreach (var item in staringItems)
        {
            if (string.IsNullOrEmpty(item.startEventName)) continue;

            if (!animationStrings.AnimationEvents.Contains(item.startEventName))
            {
                animationStrings.AnimationEvents.Add(item.startEventName);
                changed = true;
                Debug.Log($"[CharonHeadJoint] Added '{item.startEventName}' to {animationStrings.name}");
            }
        }

        if (changed)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(animationStrings);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }
        else
        {
            Debug.Log("[CharonHeadJoint] No new animation events to add.");
        }
    }

    private Quaternion _defaultQuaternion;

    private void Awake()
    {
        _defaultQuaternion = Quaternion.Euler(defaultRotation);
    }

    [Button("Set Current As Default")]
    private void SetCurrentAsDefault()
    {
        if (headObject != null)
        {
            defaultRotation = headObject.localEulerAngles;
            _defaultQuaternion = headObject.localRotation;
        }
    }

    [Button("Test Stop")]
    private void TestStop()
    {
        if (Application.isPlaying)
        {
            MMGameEvent.Trigger(stopEventName);
        }
        else
        {
            Debug.LogWarning("Test buttons only work in Play Mode.");
        }
    }


    private bool _staring = false;
    private StaringItem _currentItem;

    public bool Staring => _staring;

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == stopEventName)
        {
            StopStaring();
            return;
        }

        foreach (var item in staringItems)
        {
            if (gameEvent.EventName == item.startEventName)
            {
                StartStaring(item);
                break;
            }
        }
    }

    private void StartStaring(StaringItem item)
    {
        _currentItem = item;
        _staring = true;

        if (jawOverrider != null)
        {
            jawOverrider.enabled = true;
        }
    }

    private void StopStaring()
    {
        _staring = false;
        _currentItem = null;

        if (jawOverrider != null)
        {
            jawOverrider.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (headObject == null) return;

        if (_staring && _currentItem != null && _currentItem.targetTransform != null)
        {
            Vector3 direction = _currentItem.targetTransform.position - headObject.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                headObject.rotation = Quaternion.Slerp(headObject.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            headObject.localRotation = Quaternion.Slerp(headObject.localRotation, _defaultQuaternion, Time.deltaTime * rotationSpeed);
        }
    }
}
