using UnityEngine;

public class FeelTestScript : MonoBehaviour
{

    [SerializeField] private LocalFeelManager playerFeelManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump();
        }
    }

    public void jump()
    {
        playerFeelManager.PlayUnified("Jump");
    }
}
