using System;
using System.Collections.Specialized;
using UnityEngine;

namespace CharonsCorner.Runtime
{

    /// <summary>
    /// Handles the player's score when knocking pins.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; } // Singleton
        public static event Action<int> OnScoreChanged;

        public int Score { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Awake()
        {
            if (Instance != null && Instance == this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Notify();
        }

        /// <summary>
        /// Adds an integer amount to the player's score.
        /// </summary>
        /// <param name="amount"></param>
        public void AddScore(int amount)
        {
            if (amount == 0) return;
            Score += amount;
            Notify();
        }

        /// <summary>
        /// Resets the player's score to zero.
        /// </summary>
        public void ResetScore()
        {
            Score = 0;
            Notify();
        }

        // Invoke the action when the score is changed
        private void Notify() => OnScoreChanged?.Invoke(Score);
    }
}