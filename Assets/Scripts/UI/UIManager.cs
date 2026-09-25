using TMPro;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Shows and hides the in-game screens and keeps the HUD values current (GDD section 5).
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;

        private void OnEnable()
        {
            GameManager.Instance.ScoreChanged += ShowScore;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ScoreChanged -= ShowScore;
        }

        private void Start()
        {
            ShowScore(GameManager.Instance.Score);
        }

        private void ShowScore(int score)
        {
            scoreText.text = $"SCORE  {score}";
        }
    }
}
