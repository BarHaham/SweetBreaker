using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SweetBreaker
{
    /// <summary>
    /// The one object every system asks about the run: the state, the score and the lives.
    /// A singleton kept alive with DontDestroyOnLoad (GDD section 7). It runs before other scripts,
    /// so GameManager.Instance is already set when their OnEnable runs.
    /// Gameplay calls up into it; it tells the scene what happened through its events.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class GameManager : MonoBehaviour
    {
        public const string GameSceneName = "Game";

        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        private Coroutine serveRoutine;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int Lives { get; private set; }

        /// <summary>True in the two states where the paddle and ball are live.</summary>
        public bool IsInPlay => State == GameState.Serve || State == GameState.Playing;

        public event Action<GameState> StateChanged;
        public event Action<int> ScoreChanged;
        public event Action<int> LivesChanged;

        /// <summary>The ball reached the dead zone; raised before the serve delay starts.</summary>
        public event Action LifeLost;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
            ResetRun();
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
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

        public void NotifyBallLaunched()
        {
            if (State == GameState.Serve)
                SetState(GameState.Playing);
        }

        /// <summary>
        /// The ball is gone: one life less. Score, bricks and level are untouched. The next serve
        /// comes after ServeDelay, which also locks out the panicked launch press (GDD sections 3, 4).
        /// </summary>
        public void LoseLife()
        {
            if (State != GameState.Playing || serveRoutine != null)
                return;

            Lives--;
            LivesChanged?.Invoke(Lives);
            LifeLost?.Invoke();

            if (Lives <= 0)
                SetState(GameState.GameOver);
            else
                serveRoutine = StartCoroutine(ServeAfterDelay());
        }

        private IEnumerator ServeAfterDelay()
        {
            yield return new WaitForSeconds(config.ServeDelay);
            serveRoutine = null;
            SetState(GameState.Serve);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetState(scene.name == GameSceneName ? GameState.Serve : GameState.MainMenu);
        }

        private void ResetRun()
        {
            Score = 0;
            Lives = config.StartingLives;
        }

        private void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
