using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A class to calculate the player's score and rank based on parameters achieved in the level.
    /// </summary>
    public class ScoringSystem : MonoBehaviour
    {
        public enum Rank
        {
            S, A, B, C, D, F
        }

        [System.Serializable]
        public struct RankCriteria
        {
            public Rank rank;

            [Tooltip("Complete in <= this many seconds")]
            public float maxTimeSeconds;
            
            [Tooltip("Hit at least this many pins")]
            public float minPins;

            [Tooltip("Collect at least this many collectibles")]
            public float minCollectibles;
        }

        [Header("Order Ranks from Best to Worst")]
        [SerializeField] private RankCriteria[] _rankCriteria; 

        private float _levelTimer;
        private int _pinsHit;
        private int _collectibles;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            _levelTimer += Time.deltaTime;
        }

        /// <summary>
        /// Counts an amount of pins as hit. Defaults at 1.
        /// </summary>
        /// <param name="amount"></param>
        public void AddPin(int amount = 1) => _pinsHit += amount;

        /// <summary>
        /// Counts an amount of collectibles as collected. Defaults at 1.
        /// </summary>
        /// <param name="amount"></param>
        public void AddCollective(int amount = 1) => _collectibles += amount;

        /// <summary>
        /// Calculates the Rank the player receives based on scoring criteria.
        /// </summary>
        /// <returns></returns>
        public Rank CalculateRank()
        {
            for (int i = 0; i < _rankCriteria.Length; i++)
            {
                RankCriteria criteria = _rankCriteria[i];

                if (_levelTimer <= criteria.maxTimeSeconds &&
                    _pinsHit >= criteria.minPins &&
                    _collectibles >= criteria.minCollectibles)
                {
                    return criteria.rank;
                }
            }

            return Rank.F;
        }
    }
}
