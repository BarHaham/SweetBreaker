using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SweetBreaker
{
    /// <summary>
    /// The main menu: PLAY starts a fresh run, HOW TO PLAY opens the controls and scoring panel,
    /// QUIT exits the application (GDD section 5).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToPlayButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TMP_Text highScoreText;

        [Header("How to Play")]
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            playButton.onClick.AddListener(() => GameManager.Instance.StartNewRun());
            howToPlayButton.onClick.AddListener(() => ShowHowToPlay(true));
            backButton.onClick.AddListener(() => ShowHowToPlay(false));
            quitButton.onClick.AddListener(Quit);
        }

        private void Start()
        {
            highScoreText.text = $"HIGH SCORE  {HighScoreStore.Load()}";
            howToPlayPanel.SetActive(false);

            // Selected so Enter confirms it without touching the mouse (GDD section 4).
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private void ShowHowToPlay(bool show)
        {
            howToPlayPanel.SetActive(show);
            EventSystem.current.SetSelectedGameObject(show ? backButton.gameObject : howToPlayButton.gameObject);
        }

        private static void Quit()
        {
            // Does nothing in the editor, as GDD section 5 notes.
            Application.Quit();
        }
    }
}
