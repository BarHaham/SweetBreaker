using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SweetBreaker
{
    /// <summary>
    /// Shows and hides the in-game screens and keeps the HUD values current (GDD section 5).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField, Tooltip("One candy per life, removed from the last one down.")]
        private Image[] lifeIcons;
        [SerializeField] private GameObject servePrompt;

        private void OnEnable()
        {
            GameManager.Instance.StateChanged += HandleStateChanged;
            GameManager.Instance.ScoreChanged += ShowScore;
            GameManager.Instance.LivesChanged += ShowLives;
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.StateChanged -= HandleStateChanged;
            GameManager.Instance.ScoreChanged -= ShowScore;
            GameManager.Instance.LivesChanged -= ShowLives;
        }

        private void Start()
        {
            GameManager game = GameManager.Instance;
            ShowScore(game.Score);
            ShowLives(game.Lives);
            HandleStateChanged(game.State);
        }

        private void HandleStateChanged(GameState state)
        {
            servePrompt.SetActive(state == GameState.Serve);
        }

        private void ShowScore(int score)
        {
            scoreText.text = $"SCORE  {score}";
        }

        private void ShowLives(int lives)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
                lifeIcons[i].enabled = i < lives;
        }
    }
}
