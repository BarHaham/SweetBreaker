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
        public const string MainMenuSceneName = "MainMenu";
        public const string GameSceneName = "Game";

        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig config;

        private Coroutine serveRoutine;
        private Coroutine levelClearRoutine;
        private GameState stateBeforePause;

        /// <summary>One-shot sound effects; the AudioManager travels with this object.</summary>
        public AudioManager Audio { get; private set; }

        public GameState State { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int Lives { get; private set; }

        /// <summary>Zero-based index of the current level.</summary>
        public int LevelIndex { get; private set; }

        /// <summary>The stored best score. It only changes when a run ends.</summary>
        public int HighScore { get; private set; }

        /// <summary>Whether the run that just ended beat the stored high score.</summary>
        public bool IsNewHighScore { get; private set; }

        /// <summary>True in the two states where the paddle and ball are live.</summary>
        public bool IsInPlay => State == GameState.Serve || State == GameState.Playing;

        public event Action<GameState> StateChanged;
        public event Action<int> ScoreChanged;
        public event Action<int> LivesChanged;

        /// <summary>The ball reached the dead zone; raised before the serve delay starts.</summary>
        public event Action LifeLost;

        /// <summary>The run moved on to the level with this index; raised before its first serve.</summary>
        public event Action<int> LevelAdvanced;

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
            Audio = GetComponent<AudioManager>();
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                TogglePause();
        }

        // Alt-tabbing away must never cost a life (GDD section 4).
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsInPlay)
                Pause();
        }

        /// <summary>Esc or P. Ignored outside the Serve and Playing states (GDD section 3).</summary>
        public void TogglePause()
        {
            if (State == GameState.Paused)
                Resume();
            else if (IsInPlay)
                Pause();
        }

        public void Resume()
        {
            if (State != GameState.Paused)
                return;

            AudioListener.pause = false;
            SetState(stateBeforePause);
            ApplyTimeScale();
        }

        /// <summary>PLAY, PLAY AGAIN and RESTART RUN: score 0, full lives, level 1.</summary>
        public void StartNewRun()
        {
            ResetRun();
            SceneManager.LoadScene(GameSceneName);
        }

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
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
            {
                Audio.Play(Sfx.GameOver);
                EndRun(GameState.GameOver);
            }
            else
            {
                Audio.Play(Sfx.LifeLost);
                serveRoutine = StartCoroutine(ServeAfterDelay());
            }
        }

        /// <summary>The last breakable brick is gone: show the banner, then the next level or victory.</summary>
        public void CompleteLevel(bool wasFinalLevel)
        {
            if (State == GameState.Playing && levelClearRoutine == null)
                levelClearRoutine = StartCoroutine(LevelClearSequence(wasFinalLevel));
        }

        /// <summary>The high score is written only here, when a run ends (GDD section 3).</summary>
        private void EndRun(GameState ending)
        {
            IsNewHighScore = HighScoreStore.TrySave(Score);
            HighScore = HighScoreStore.Load();
            SetState(ending);
        }

        private IEnumerator ServeAfterDelay()
        {
            yield return new WaitForSeconds(config.ServeDelay);
            serveRoutine = null;
            SetState(GameState.Serve);
        }

        /// <summary>
        /// The LEVEL CLEAR beat. Score and lives carry over to the next level; lives are not refilled
        /// (GDD section 3).
        /// </summary>
        private IEnumerator LevelClearSequence(bool wasFinalLevel)
        {
            SetState(GameState.LevelClear);
            Audio.Play(Sfx.LevelClear);
            yield return new WaitForSeconds(config.LevelClearDelay);
            levelClearRoutine = null;

            if (wasFinalLevel)
            {
                EndRun(GameState.Victory);
                yield break;
            }

            LevelIndex++;
            LevelAdvanced?.Invoke(LevelIndex);
            SetState(GameState.Serve);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Nothing timed may carry over from the scene that was just unloaded.
            StopAllCoroutines();
            serveRoutine = null;
            levelClearRoutine = null;
            AudioListener.pause = false;
            SetState(scene.name == GameSceneName ? GameState.Serve : GameState.MainMenu);
            ApplyTimeScale();
        }

        private void Pause()
        {
            stateBeforePause = State;
            AudioListener.pause = true;
            SetState(GameState.Paused);
            ApplyTimeScale();
        }

        // The only place in the game that writes Time.timeScale.
        private void ApplyTimeScale()
        {
            Time.timeScale = State == GameState.Paused ? 0f : 1f;
        }

        private void ResetRun()
        {
            Score = 0;
            Lives = config.StartingLives;
            LevelIndex = 0;
            HighScore = HighScoreStore.Load();
            IsNewHighScore = false;
        }

        private void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
