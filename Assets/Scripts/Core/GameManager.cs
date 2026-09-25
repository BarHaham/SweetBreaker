using System;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// The one object every system asks about the run: the score, the lives and the current state.
    /// A singleton kept alive with DontDestroyOnLoad (GDD section 7). It runs before other scripts,
    /// so GameManager.Instance is already set when their OnEnable runs.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        public int Score { get; private set; }

        public event Action<int> ScoreChanged;

        private void Awake()
        {
            // A copy is placed in each scene so any scene can be played on its own in the editor;
            // whichever arrives second removes itself.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>Score only ever goes up (GDD section 3).</summary>
        public void AddScore(int points)
        {
            if (points <= 0)
                return;

            Score += points;
            ScoreChanged?.Invoke(Score);
        }
    }
}
