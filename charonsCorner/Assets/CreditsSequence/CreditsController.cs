using UnityEngine;

public class CreditsController : MonoBehaviour
{    
    [Header("Settings"), Range(10f, 100f)]
    [SerializeField] float _scrollSpeed = 40f;

    RectTransform _rectTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        _rectTransform.anchoredPosition += new Vector2(0f, _scrollSpeed * Time.deltaTime);
    }
}
