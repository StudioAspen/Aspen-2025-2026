using log4net.Core;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ScoringSystem : MonoBehaviour
    {
        [Header("Parameters Needed to Achieve Each Rank")]
        [SerializeField] private float _sCompletionTimeInSeconds;
        [SerializeField] private int _sNumPinsHit;
        [SerializeField] private int _sCollectiblesCollected;
        [Space(15)]
        [SerializeField] private float _aCompletionTimeInSeconds;
        [SerializeField] private int _aNumPinsHit;
        [SerializeField] private int _aCollectiblesCollected;
        [Space(15)]
        [SerializeField] private float _bCompletionTimeInSeconds;
        [SerializeField] private int _bNumPinsHit;
        [SerializeField] private int _bCollectiblesCollected;
        [Space(15)]
        [SerializeField] private float _cCompletionTimeInSeconds;
        [SerializeField] private int _cNumPinsHit;
        [SerializeField] private int _cCollectiblesCollected;
        [Space(15)]
        [SerializeField] private float _dCompletionTimeInSeconds;
        [SerializeField] private int _dNumPinsHit;
        [SerializeField] private int _dCollectiblesCollected;
        [Space(15)]
        [SerializeField] private float _fCompletionTimeInSeconds;
        [SerializeField] private int _fNumPinsHit;
        [SerializeField] private int _fCollectiblesCollected;

        private float _levelTimer;
        private int _timeInSeconds;
        private int _numPinsHit;
        private int _numCollectibles;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            _levelTimer += Time.deltaTime;
            _timeInSeconds = (int) _levelTimer % 60;
        }
    }
}
