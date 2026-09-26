using UnityEngine;
using UnityEngine.EventSystems;

namespace SweetBreaker
{
    /// <summary>
    /// Keeps a menu usable from the keyboard. A click on empty space clears the UI selection, which
    /// would leave Enter and the arrow keys with nothing to act on (GDD section 4). The next key
    /// press selects the menu's default button again.
    /// </summary>
    public static class MenuFocus
    {
        public static void RestoreOnKeyboardInput(GameObject defaultButton)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null && selected.activeInHierarchy)
                return;

            bool keyboardUsed = Input.GetButtonDown("Submit")
                || Input.GetAxisRaw("Vertical") != 0f
                || Input.GetAxisRaw("Horizontal") != 0f;
            if (keyboardUsed)
                EventSystem.current.SetSelectedGameObject(defaultButton);
        }
    }
}
