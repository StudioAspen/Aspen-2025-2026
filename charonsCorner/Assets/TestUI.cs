using UnityEngine;

public class TestUI : MonoBehaviour
{
    GameObject Player;
    PlayerController3D playerController3D;

    void Awake()
    {
        Player = GameObject.Find("Player");
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (playerController3D != null)
        {
            var textMesh = GetComponent<TMPro.TMP_Text>();
            if (textMesh != null)
            {
                textMesh.text = "Speed: " + playerController3D.GetCurrentForwardSpeed().ToString("F2") +
                    "\n Speed Increases: " + playerController3D.numSpeedIncreases.ToString() +
                    "\n Brake Turn Speed: " + playerController3D.brakeAngleAdjustSpeed.ToString() +
                    "\n BrakeHoldElapsed: " + playerController3D.GetBrakeHoldElapsed().ToString() +
                    "\n BrakeHoldTime: " + playerController3D.brakeHoldTime.ToString();
            }
        }
    }
}