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

        public float LevelTimeSeconds => _levelTimer;

        /// <summary>
        /// Called by Unity once before the first Update on this MonoBehaviour.
        /// </summary>
        void Start()
        {
        
        }

        /// <summary>
        /// Advances the internal level timer by the time elapsed since the previous frame.
        /// </summary>
        void Update()
        {
            _levelTimer += Time.deltaTime; // will probably need to be adjusted in the future
        }

        /// <summary>
        /// Adds a certain amount of points to the player's score. 
        /// To be used when player accomplishes something, and receives points for it.
        /// </summary>
        /// <summary>
/// Increments the player's accumulated score by the specified number of points.
/// </summary>
/// <param name="amount">The number of points to add to the current score; a negative value will deduct points.</param>
        public void AddPoints(int amount) => _score += amount;

        /// <summary>
        /// Calculates the Rank the player receives based on scoring criteria.
        /// </summary>
        /// <summary>
        /// Determines the player's rank by evaluating the configured rank criteria in order.
        /// </summary>
        /// <remarks>
        /// For each criterion, the method computes a local time-bonus-adjusted score when the level time is within the criterion's maxTimeSeconds, but the rank decision uses the actual accumulated score (_score) to compare against the criterion's requiredPoints. The first criterion whose requiredPoints is satisfied by the accumulated score is returned.
        /// </remarks>
        /// <returns>The rank from the first matching criterion, or <c>RankCriteriaSO.Rank.F</c> if no criteria are met.</returns>
        public RankCriteriaSO.Rank CalculateRank()
        {
            for (int i = 0; i < _rankData.Criteria.Count; i++)
            {
                var criteria = _rankData.Criteria[i];
                int playerScore = _score;

                if (_levelTimer <= criteria.maxTimeSeconds)
                {
                    playerScore += criteria.timeBonusPoints;
                }

                if (_score >= criteria.requiredPoints)
                {
                    return criteria.rank;
                }
            }

            return RankCriteriaSO.Rank.F;
        }
    }
}