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
        [SerializeField, Tooltip("The main menu's buttons, taken out of keyboard navigation while How to Play covers them.")]
        private CanvasGroup menuButtons;
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
            ShowHowToPlay(false);

            // Selected so Enter confirms it without touching the mouse (GDD section 4).
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private void Update()
        {
            GameObject defaultButton = howToPlayPanel.activeSelf ? backButton.gameObject : playButton.gameObject;
            MenuFocus.RestoreOnKeyboardInput(defaultButton);
        }

        private void ShowHowToPlay(bool show)
        {
            howToPlayPanel.SetActive(show);

            // Otherwise the arrow keys could walk from BACK onto the menu buttons hidden behind the panel.
            menuButtons.interactable = !show;
            EventSystem.current.SetSelectedGameObject(show ? backButton.gameObject : howToPlayButton.gameObject);
        }

        private static void Quit()
        {
            // Does nothing in the editor, as GDD section 5 notes.
            Application.Quit();
        }
    }
}
