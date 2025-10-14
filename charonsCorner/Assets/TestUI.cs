using UnityEngine;

public class TestUI : MonoBehaviour
{
    [Header("UI Display Toggles")]
    public bool showSpeed = true;
    public bool showSpeedIncreases = true;
    public bool showBrakeTurnSpeed = true;
    public bool showBrakeHoldElapsed = true;
    public bool showBrakeHoldTime = true;
    public bool showCurrentTurnSpeed = true;
    public bool showTurnHoldTime = true;

    GameObject Player;
    PlayerController3D playerController3D;

    void Awake()
    {
        Player = GameObject.Find("PlayerPrototypeD");
        if (Player == null)
        {
            Debug.LogError("Player object not found!");
        }
        var textMesh = GetComponent<TMPro.TMP_Text>();
        if (textMesh == null)
        {
            Debug.LogError("TMP_Text component not found!");
        }
        playerController3D = Player.GetComponent<PlayerController3D>();
    }

    void Start()
    {

    }

    void Update()
    {
        if (playerController3D != null)
        {
            var textMesh = GetComponent<TMPro.TMP_Text>();
            if (textMesh != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (showSpeed)
                    sb.AppendLine("Speed: " + playerController3D.GetCurrentForwardSpeed().ToString("F2"));
                if (showSpeedIncreases)
                    sb.AppendLine("Speed Increases: " + playerController3D.numSpeedIncreases.ToString());
                if (showBrakeTurnSpeed)
                    sb.AppendLine("Brake Turn Speed: " + playerController3D.brakeAngleAdjustSpeed.ToString());
                if (showBrakeHoldElapsed)
                    sb.AppendLine("BrakeHoldElapsed: " + playerController3D.GetBrakeHoldElapsed().ToString());
                if (showBrakeHoldTime)
                    sb.AppendLine("BrakeHoldTime: " + playerController3D.brakeHoldTime.ToString());
                if (showCurrentTurnSpeed)
                    sb.AppendLine("currentTurnSpeed: " + playerController3D.GetCurrentTurnSpeed());
                if (showTurnHoldTime)
                    sb.AppendLine("turnHoldTime: " + playerController3D.GetTurnHoldTime().ToString());
                textMesh.text = sb.ToString();
            }
        }
    }
}