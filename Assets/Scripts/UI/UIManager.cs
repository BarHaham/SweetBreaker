using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SweetBreaker
{
    /// <summary>
    /// Shows and hides the in-game screens and keeps the HUD values current (GDD section 5).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // Stops the panicked click that follows a lost ball from pressing a button (GDD section 4).
        private const float EndScreenInputLockout = 0.75f;

        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField, Tooltip("One candy per life, removed from the last one down.")]
        private Image[] lifeIcons;
        [SerializeField] private GameObject servePrompt;
        [SerializeField, Tooltip("LEVEL n CLEAR: a beat, not a screen. No buttons, no input.")]
        private TMP_Text levelClearBanner;

        [Header("Pause")]
        [SerializeField] private GameObject pauseScreen;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartRunButton;
        [SerializeField] private Button pauseMainMenuButton;

        [Header("Game Over / Victory (one layout, two headings)")]
        [SerializeField] private GameObject endScreen;
        [SerializeField] private TMP_Text endHeading;
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text storedHighScoreText;
        [SerializeField] private GameObject newHighScoreLine;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button endMainMenuButton;

        private void Awake()
        {
            resumeButton.onClick.AddListener(() => GameManager.Instance.Resume());
            restartRunButton.onClick.AddListener(() => GameManager.Instance.StartNewRun());
            pauseMainMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
            playAgainButton.onClick.AddListener(() => GameManager.Instance.StartNewRun());
            endMainMenuButton.onClick.AddListener(() => GameManager.Instance.ReturnToMainMenu());
        }

        private void OnEnable()
        {
            GameManager.Instance.StateChanged += HandleStateChanged;
            GameManager.Instance.ScoreChanged += ShowScore;
            GameManager.Instance.LivesChanged += ShowLives;
            GameManager.Instance.LevelClearBannerShown += ShowLevelClearBanner;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.StateChanged -= HandleStateChanged;
            GameManager.Instance.ScoreChanged -= ShowScore;
            GameManager.Instance.LivesChanged -= ShowLives;
            GameManager.Instance.LevelClearBannerShown -= ShowLevelClearBanner;
        }

        private void Start()
        {
            GameManager game = GameManager.Instance;
            ShowScore(game.Score);
            ShowLives(game.Lives);
            highScoreText.text = $"HIGH  {game.HighScore}";
            HandleStateChanged(game.State);
        }

        private void Update()
        {
            if (pauseScreen.activeSelf)
                MenuFocus.RestoreOnKeyboardInput(resumeButton.gameObject);
            else if (endScreen.activeSelf && playAgainButton.interactable)
                MenuFocus.RestoreOnKeyboardInput(playAgainButton.gameObject);
        }

        private void HandleStateChanged(GameState state)
        {
            servePrompt.SetActive(state == GameState.Serve);
            ShowPauseScreen(state == GameState.Paused);

            // The banner waits for the last-brick slow motion; any other state takes it down.
            if (state != GameState.LevelClear)
                levelClearBanner.gameObject.SetActive(false);

            if (state == GameState.GameOver)
                ShowEndScreen("GAME OVER");
            else if (state == GameState.Victory)
                ShowEndScreen("YOU WIN!");
            else
                endScreen.SetActive(false);
        }

        private void ShowLevelClearBanner()
        {
            levelClearBanner.text = $"LEVEL {GameManager.Instance.LevelIndex + 1} CLEAR";
            levelClearBanner.gameObject.SetActive(true);
        }

        private void ShowScore(int score)
        {
            scoreText.text = $"SCORE  {score}";
        }

        private void ShowLives(int lives)
        {
            // Deactivated rather than hidden, so the layout re-centres the candies that are left.
            for (int i = 0; i < lifeIcons.Length; i++)
                lifeIcons[i].gameObject.SetActive(i < lives);
        }

        /// <summary>The level stays visible behind the dimmed overlay (GDD section 5).</summary>
        private void ShowPauseScreen(bool show)
        {
            if (pauseScreen.activeSelf == show)
                return;

            pauseScreen.SetActive(show);

            // Selecting RESUME lets Enter confirm it; nothing stays selected once play resumes.
            EventSystem.current.SetSelectedGameObject(show ? resumeButton.gameObject : null);
        }

        private void ShowEndScreen(string heading)
        {
            GameManager game = GameManager.Instance;
            endHeading.text = heading;
            finalScoreText.text = $"SCORE  {game.Score}";
            storedHighScoreText.text = $"HIGH SCORE  {game.HighScore}";
            newHighScoreLine.SetActive(game.IsNewHighScore);
            highScoreText.text = $"HIGH  {game.HighScore}";
            endScreen.SetActive(true);
            StartCoroutine(UnlockEndScreenButtons());
        }

        private IEnumerator UnlockEndScreenButtons()
        {
            playAgainButton.interactable = false;
            endMainMenuButton.interactable = false;
            EventSystem.current.SetSelectedGameObject(null);

            yield return new WaitForSecondsRealtime(EndScreenInputLockout);

            playAgainButton.interactable = true;
            endMainMenuButton.interactable = true;
            EventSystem.current.SetSelectedGameObject(playAgainButton.gameObject);
        }
    }
}
