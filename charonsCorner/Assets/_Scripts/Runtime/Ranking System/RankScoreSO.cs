using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRankingScore", menuName = "CharonsCorner/RankScore")]
public class RankScoreSO : ScriptableObject
{
    [SerializedDictionary("Time", "Ranking"), Tooltip("Max of five elements for the five ranks"), DrawWithUnity]
    public SerializedDictionary<float, Ranks> Ranks;

    Ranks _finalRank;

    public Ranks GetFinakRank() => _finalRank;
    public void SetFinalRank(Ranks rank) => _finalRank = rank;
}
