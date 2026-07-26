using System.Collections.Generic;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

public class SectionManager : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private List<GameObject> sections = new List<GameObject>();
    [SerializeField] private int extraSectionsOnStart = 0;

    private int _currentActiveIndex = 0;

    private void Start()
    {
        if (sections == null || sections.Count == 0)
        {
            Debug.LogWarning($"[SectionManager] {gameObject.name} has no sections assigned!");
            return;
        }
    }

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
        if (gameEvent.EventName == "NextSection")
        {
            ActivateNextSection();
        }
    }

    /// <summary>
    /// Turns on the next section in the list when called.
    /// </summary>
    [Button]
    public void ActivateNextSection()
    {
        _currentActiveIndex++;

        if (_currentActiveIndex < sections.Count)
        {
            if (sections[_currentActiveIndex] != null)
            {
                sections[_currentActiveIndex].SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning($"[SectionManager] {gameObject.name}: No more sections to activate!");
        }
    }
}
