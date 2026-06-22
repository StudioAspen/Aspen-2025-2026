using UnityEngine;
using MoreMountains.Feedbacks;
using CharonsCorner.Runtime;

public class BalloonBunch : MonoBehaviour
{
    [SerializeField] private MMF_Player _mmfPlayer;
    [SerializeField] private float _triggerRadius = 5f;
    [SerializeField] private Vector3 _triggerOffset = Vector3.zero;
    [SerializeField] private bool _playOnce = true;

    [Header("Balloons")]
    [SerializeField] private GameObject[] _balloonModels; // Expects 4 models
    [SerializeField] private Material[] _balloonMaterials; // Expects 3 materials
    
    private bool _hasPlayed = false;
    private GameplayPlayerController _player;

    void Start()
    {
        _player = FindFirstObjectByType<GameplayPlayerController>();
        if (_mmfPlayer != null)
        {
            _mmfPlayer.Initialization();
        }

        SetupBalloons();
    }

    private void SetupBalloons()
    {
        if (_balloonModels == null || _balloonModels.Length < 4) return;
        if (_balloonMaterials == null || _balloonMaterials.Length < 3) return;

        // Randomly determine between model three and four (indices 2 and 3) deactivate its gameobject
        int modelToDeactivate = Random.Range(2, 4);
        _balloonModels[modelToDeactivate].SetActive(false);

        // Collect remaining 3 active balloons
        System.Collections.Generic.List<GameObject> activeBalloons = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            if (i != modelToDeactivate)
            {
                activeBalloons.Add(_balloonModels[i]);
            }
        }

        // Randomly set their materials using the three assigned ones, such that each balloon is using a different material
        // Shuffle the materials
        System.Collections.Generic.List<Material> shuffledMaterials = new System.Collections.Generic.List<Material>(_balloonMaterials);
        for (int i = 0; i < shuffledMaterials.Count; i++)
        {
            Material temp = shuffledMaterials[i];
            int randomIndex = Random.Range(i, shuffledMaterials.Count);
            shuffledMaterials[i] = shuffledMaterials[randomIndex];
            shuffledMaterials[randomIndex] = temp;
        }

        // Assign materials to the 3 active balloons
        for (int i = 0; i < 3; i++)
        {
            Renderer renderer = activeBalloons[i].GetComponent<Renderer>();
            if (renderer == null)
            {
                // In case renderer is on a child object
                renderer = activeBalloons[i].GetComponentInChildren<Renderer>();
            }

            if (renderer != null)
            {
                renderer.material = shuffledMaterials[i];
            }
        }
    }

    void Update()
    {
        if (_mmfPlayer == null || _player == null) return;
        
        Vector3 triggerPosition = transform.position + transform.TransformDirection(_triggerOffset);
        float distance = Vector3.Distance(triggerPosition, _player.transform.position);
        bool inRange = distance <= _triggerRadius;

        if (inRange)
        {
            if (!_hasPlayed)
            {
                _mmfPlayer.PlayFeedbacks();
                _hasPlayed = true;
            }
        }
        else
        {
            if (!_playOnce)
            {
                _hasPlayed = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 triggerPosition = transform.position + transform.TransformDirection(_triggerOffset);
        Gizmos.DrawWireSphere(triggerPosition, _triggerRadius);
    }
}
