using System.Collections;
using CharonsCorner.Runtime;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;

public class CheckpointIndicator : MonoBehaviour
{
    [SerializeField] private CheckpointManager _checkpointManager;
    [SerializeField] private TypewriterComponent _typewriter;
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private string _checkpointDisplayText = "Checkpoint!";
    [SerializeField] private float _displayDuration = 3f;

    private Coroutine _displayCoroutine;

    private void Awake()
    {
        if (_checkpointManager == null)
            _checkpointManager = FindAnyObjectByType<CheckpointManager>();
        
        if (_typewriter == null)
            _typewriter = GetComponent<TypewriterComponent>();
            
        if (_tmpText == null && _typewriter != null)
            _tmpText = _typewriter.GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (_checkpointManager != null)
        {
            _checkpointManager.OnCheckpointProgressed.AddListener(OnCheckpointProgressed);
        }
    }

    private void OnDisable()
    {
        if (_checkpointManager != null)
        {
            _checkpointManager.OnCheckpointProgressed.RemoveListener(OnCheckpointProgressed);
        }
    }

    public void DisplayCheckpoint()
    {
        DisplayCheckpoint(_checkpointDisplayText);
    }

    public void DisplayCheckpoint(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (_displayCoroutine != null)
        {
            StopCoroutine(_displayCoroutine);
        }

        _displayCoroutine = StartCoroutine(ShowCheckpointRoutine(text));
    }

    private void OnCheckpointProgressed(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;
        DisplayCheckpoint();
    }

    private IEnumerator ShowCheckpointRoutine(string checkpointName)
    {
        if (_tmpText != null && !string.IsNullOrEmpty(checkpointName))
        {
            _typewriter.ShowText(checkpointName);
            // ShowText usually starts the reveal.
            
            yield return new WaitForSeconds(_displayDuration);
            
            _typewriter.StartDisappearingText();
        }
        
        _displayCoroutine = null;
    }
}
