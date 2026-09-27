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
    [SerializeField] private GameObject[] _balloonModels;
    [SerializeField] private Material[] _balloonMaterials;
    [SerializeField] private bool _deactivateModel = true;
    
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
        if (_balloonModels == null || _balloonModels.Length == 0) return;
        if (_balloonMaterials == null || _balloonMaterials.Length == 0) return;

        int modelToDeactivate = -1;
        if (_deactivateModel && _balloonModels.Length > 0)
        {
            // Randomly determine a model to deactivate
            modelToDeactivate = Random.Range(0, _balloonModels.Length);
            _balloonModels[modelToDeactivate].SetActive(false);
        }

        // Collect active balloons
        System.Collections.Generic.List<GameObject> activeBalloons = new System.Collections.Generic.List<GameObject>();
        for (int i = 0; i < _balloonModels.Length; i++)
        {
            if (i != modelToDeactivate)
            {
                activeBalloons.Add(_balloonModels[i]);
            }
        }

        // Randomly set their materials using the assigned ones
        // Shuffle the materials
        System.Collections.Generic.List<Material> shuffledMaterials = new System.Collections.Generic.List<Material>(_balloonMaterials);
        for (int i = 0; i < shuffledMaterials.Count; i++)
        {
            Material temp = shuffledMaterials[i];
            int randomIndex = Random.Range(i, shuffledMaterials.Count);
            shuffledMaterials[i] = shuffledMaterials[randomIndex];
            shuffledMaterials[randomIndex] = temp;
        }

        // Assign materials to the active balloons
        for (int i = 0; i < activeBalloons.Count; i++)
        {
            Renderer renderer = activeBalloons[i].GetComponent<Renderer>();
            if (renderer == null)
            {
                // In case renderer is on a child object
                renderer = activeBalloons[i].GetComponentInChildren<Renderer>();
            }

            if (renderer != null)
            {
                // Use modulo to wrap around if there are more balloons than materials
                renderer.material = shuffledMaterials[i % shuffledMaterials.Count];
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
