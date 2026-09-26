using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// How long the wide paddle has left, drawn right under the paddle where the player is already
    /// looking (GDD section 5). It drains from both ends toward the centre, the way the paddle is
    /// about to shrink, and flashes near the end so the shrink never comes as a surprise.
    /// </summary>
    public class ExpansionTimerBar : MonoBehaviour
    {
        [SerializeField] private PaddleController paddle;
        [SerializeField] private SpriteRenderer track;
        [SerializeField] private SpriteRenderer fill;

        [SerializeField, Tooltip("Gap between each end of the bar and the end of the paddle (u).")]
        private float inset = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("The bar flashes once less than this fraction of the time is left.")]
        private float warningFraction = 0.25f;

        [SerializeField, Tooltip("Flashes per second while warning.")]
        private float flashRate = 5f;

        [SerializeField] private Color fillColour = new Color(0.25f, 0.78f, 0.61f);
        [SerializeField] private Color warningColour = new Color(1f, 0.31f, 0.48f);

        // Paused time stops the timer, so it also stops the flashing: scaled time throughout.
        private void LateUpdate()
        {
            float timeLeft = paddle.ExpansionTimeLeft01;
            bool show = timeLeft > 0f;
            track.enabled = show;
            fill.enabled = show;
            if (!show)
                return;

            float height = track.size.y;
            float fullWidth = paddle.Width - 2f * inset;
            track.size = new Vector2(fullWidth, height);

            // Never narrower than it is tall, so the last moment still reads as a dot, not a sliver.
            fill.size = new Vector2(Mathf.Max(fullWidth * timeLeft, height), height);

            bool flashOn = timeLeft < warningFraction && Mathf.Repeat(Time.time * flashRate, 1f) < 0.5f;
            fill.color = flashOn ? warningColour : fillColour;
        }
    }
}
