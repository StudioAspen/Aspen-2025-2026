using UnityEngine;
using TMPro;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorCore.Typing;
using CharonsCorner.Runtime;
using UnityEngine.InputSystem;
using MoreMountains.Tools;

public class FlashbackText : MonoBehaviour
{
    [SerializeField] private TypewriterComponent typewriter;
    [SerializeField] private TMP_Text textComponent;

    [Header("Input Prompt")]
    [SerializeField] private GameObject inputPromptObject;
    [SerializeField] private TMP_Text inputPromptText;
    [SerializeField] private InputActionReference interactAction;

    private string[] _lines;
    private int _currentLineIndex = -1;
    private bool _isTyping = false;
    private bool _isActive = false;
    private bool _skipFirstFrameInput = false;
    private bool _isUnskippable = false;

    public static bool IsDialogueActive { get; private set; }

    public static bool CanStartDialogue => !IsDialogueActive && Time.frameCount > _instanceLastFinishedFrame;
    private static int _instanceLastFinishedFrame = -1;

    public delegate void DialogueRequestAction(string[] lines);
    public static event DialogueRequestAction OnDialogueRequested;

    public delegate void DialogueAction();
    public static event DialogueAction OnDialogueFinished;
    public static event DialogueAction OnNextLineRequested;

    public static void RequestDialogue(string[] lines)
    {
        OnDialogueRequested?.Invoke(lines);
    }

    public static void RequestDialogue(FlashbackDialogueData data)
    {
        if (data != null && data.lines != null && data.lines.Length > 0)
        {
            RequestDialogue(data.lines);
        }
    }

    private void OnEnable()
    {
        OnDialogueRequested += ActivateText;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact += HandleInput;
            InputManager.Instance.OnControlSchemeChanged += UpdateInputPrompt;
        }
    }

    private void OnDisable()
    {
        OnDialogueRequested -= ActivateText;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact -= HandleInput;
            InputManager.Instance.OnControlSchemeChanged -= UpdateInputPrompt;
        }
    }

    private void Awake()
    {
        if (typewriter == null) typewriter = GetComponent<TypewriterComponent>();
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();

        if (typewriter != null)
        {
            typewriter.onTextShowed.AddListener(OnTextShowed);
            typewriter.onMessage.AddListener(OnMessage);
        }

        if (inputPromptObject != null)
        {
            inputPromptObject.SetActive(false);
        }
    }

    private void OnTextShowed()
    {
        _isTyping = false;
        MMGameEvent.Trigger("StopTalk");
        ShowInputPrompt();
    }

    private void OnMessage(EventMarker marker)
    {
        if (marker.name == "unskippable")
        {
            _isUnskippable = true;
        }
    }

    public void ActivateText(string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        _lines = lines;
        _currentLineIndex = 0;
        _isActive = true;
        IsDialogueActive = true;
        _skipFirstFrameInput = true;
        
        DisplayCurrentLine();
    }

    private void Update()
    {
        if (!_isActive) return;

        if (_skipFirstFrameInput)
        {
            _skipFirstFrameInput = false;
        }
    }

    private void HandleInput()
    {
        if (!_isActive || _skipFirstFrameInput) return;

        if (_isTyping)
        {
            if (_isUnskippable) return;
            typewriter.SkipTypewriter();
        }
        else
        {
            if (_isUnskippable) return;
            OnNextLineRequested?.Invoke();
            _currentLineIndex++;
            if (_currentLineIndex < _lines.Length)
            {
                DisplayCurrentLine();
            }
            else
            {
                FinishSequence();
            }
        }
    }

    private void DisplayCurrentLine()
    {
        _isTyping = true;
        _isUnskippable = false;
        HideInputPrompt();
        string line = _lines[_currentLineIndex];
        
        typewriter.ShowText(line);
    }

    private void FinishSequence()
    {
        _isActive = false;
        IsDialogueActive = false;
        _instanceLastFinishedFrame = Time.frameCount;
        if (textComponent != null) textComponent.text = string.Empty;
        HideInputPrompt();
        OnDialogueFinished?.Invoke();
    }

    private void ShowInputPrompt()
    {
        if (inputPromptObject != null)
        {
            UpdateInputPrompt(InputManager.Instance.CurrentControlScheme);
            inputPromptObject.SetActive(true);
        }
    }

    private void HideInputPrompt()
    {
        if (inputPromptObject != null)
        {
            inputPromptObject.SetActive(false);
        }
    }

    private void UpdateInputPrompt(InputManager.ControlScheme controlScheme)
    {
        if (inputPromptText != null && interactAction != null)
        {
            inputPromptText.text = InputDisplayer.GetInputDisplayString(interactAction, controlScheme);
        }
    }
}
