using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This class defines and sets the criteria the player needs to score to earn a certain Rank after they complete a level.
    /// </summary>
    [CreateAssetMenu(fileName = "RankCriteriaSO", menuName = "CharonsCorner/Scoring/RankCriteriaSO")]
    public class RankCriteriaSO : ScriptableObject
    {
        [Header("Order Ranks from Best to Worst")]
        [SerializeField] private List<RankCriteria> _rankCriteria = new List<RankCriteria>();

        public IReadOnlyList<RankCriteria> Criteria => _rankCriteria;

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

            [Tooltip("Points given when completing in time")]
            public int timeBonusPoints;

            [Tooltip("Points required to earn this rank")]
            public int requiredPoints;
        }
    }
}
