using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// A class to calculate the player's score and rank based on parameters achieved in the level.
    /// </summary>
    public class ScoringSystem : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private RankCriteriaSO _rankData;

        private float _levelTimer;
        private int _score;
        private bool _bonusApplied = false;

        public float LevelTimeSeconds => _levelTimer;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            _levelTimer += Time.deltaTime; // will probably need to be adjusted in the future
        }

        /// <summary>
        /// Adds a certain amount of points to the player's score. 
        /// To be used when player accomplishes something, and receives points for it.
        /// </summary>
        /// <param name="amount"></param>
        public void AddPoints(int amount) => _score += amount;

        /// <summary>
        /// Calculates the Rank the player receives based on scoring criteria.
        /// </summary>
        /// <returns></returns>
        public RankCriteriaSO.Rank CalculateRank()
        {
            for (int i = 0; i < _rankData.Criteria.Count; i++)
            {
                var criteria = _rankData.Criteria[i];

                if (_levelTimer <= criteria.MaxTimeSeconds && !_bonusApplied)
                {
                    _score += criteria.TimeBonusPoints;
                    _bonusApplied = true;
                }

                if (_score >= criteria.RequiredPoints)
                {
                    return criteria.Rank;
                }
            }

            return RankCriteriaSO.Rank.F;
        }
    }
}
