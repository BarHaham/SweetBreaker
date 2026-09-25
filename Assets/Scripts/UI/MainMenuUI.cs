using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SweetBreaker
{
    /// <summary>
    /// The main menu: PLAY starts a fresh run, QUIT exits the application (GDD section 5).
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            playButton.onClick.AddListener(() => GameManager.Instance.StartNewRun());
            quitButton.onClick.AddListener(Quit);
        }

        private void Start()
        {
            // Selected so Enter confirms it without touching the mouse (GDD section 4).
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }

        private static void Quit()
        {
            // Does nothing in the editor, as GDD section 5 notes.
            Application.Quit();
        }
    }
}
