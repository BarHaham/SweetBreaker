using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Sizes the orthographic camera so the whole play field, both side walls included, is visible
    /// at any aspect ratio (GDD section 5). Refits whenever the window size changes.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private Camera cam;
        private int fittedWidth;
        private int fittedHeight;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            Fit();
        }

        private void LateUpdate()
        {
            if (Screen.width != fittedWidth || Screen.height != fittedHeight)
                Fit();
        }

        private void Fit()
        {
            fittedWidth = Screen.width;
            fittedHeight = Screen.height;
            float aspect = (float)fittedWidth / fittedHeight;

            // The screen-shake bound is added as a margin, so the field stays fully visible while shaking.
            Vector2 halfExtents = config.PlayFieldSize * 0.5f + Vector2.one * config.ScreenShakeMagnitude;
            cam.orthographicSize = Mathf.Max(halfExtents.y, halfExtents.x / aspect);
        }
    }
}
