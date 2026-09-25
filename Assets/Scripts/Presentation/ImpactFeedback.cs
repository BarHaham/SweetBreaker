using System.Collections;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Makes breaking a brick and hitting the paddle feel like hits (GDD section 6). Every effect
    /// here runs on unscaled time, and the time scale always goes through GameManager, never
    /// Time.timeScale directly.
    /// </summary>
    public class ImpactFeedback : MonoBehaviour
    {
        // How far the paddle body squashes at the moment of contact, before springing back.
        private static readonly Vector3 SquashedScale = new Vector3(1.12f, 0.7f, 1f);

        [SerializeField] private GameConfig config;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private PaddleController paddle;
        [SerializeField] private Transform cameraTransform;

        private Vector3 cameraRestPosition;
        private Coroutine hitStopRoutine;
        private Coroutine shakeRoutine;
        private Coroutine squashRoutine;
        private bool isHitStopped;

        private void Awake()
        {
            cameraRestPosition = cameraTransform.localPosition;
        }

        private void OnEnable()
        {
            levelManager.BrickDestroyed += PlayBrickBreak;
            paddle.BallHit += SquashPaddle;
        }

        private void OnDisable()
        {
            levelManager.BrickDestroyed -= PlayBrickBreak;
            paddle.BallHit -= SquashPaddle;
        }

        private void SquashPaddle()
        {
            Restart(ref squashRoutine, Squash());
        }

        private void PlayBrickBreak(Brick brick)
        {
            Restart(ref hitStopRoutine, HitStop());
            Restart(ref shakeRoutine, Shake());
        }

        /// <summary>Freezes the game for a moment, so the hit lands instead of the brick just vanishing.</summary>
        private IEnumerator HitStop()
        {
            isHitStopped = true;
            ApplyTimeScale();
            yield return new WaitForSecondsRealtime(config.HitStopDuration);
            isHitStopped = false;
            ApplyTimeScale();
        }

        /// <summary>
        /// A short, decaying camera offset. It never exceeds ScreenShakeMagnitude, which CameraFitter
        /// leaves as margin around the field, so the play field always stays in view (GDD section 1).
        /// </summary>
        private IEnumerator Shake()
        {
            for (float elapsed = 0f; elapsed < config.ScreenShakeDuration; elapsed += Time.unscaledDeltaTime)
            {
                float strength = 1f - elapsed / config.ScreenShakeDuration;
                Vector2 offset = Random.insideUnitCircle * config.ScreenShakeMagnitude * strength;
                cameraTransform.localPosition = cameraRestPosition + (Vector3)offset;
                yield return null;
            }

            cameraTransform.localPosition = cameraRestPosition;
        }

        /// <summary>The paddle flattens on contact and springs back, so the rebound reads as a hit.</summary>
        private IEnumerator Squash()
        {
            Transform body = paddle.Body;
            for (float elapsed = 0f; elapsed < config.PaddleSquashDuration; elapsed += Time.unscaledDeltaTime)
            {
                body.localScale = Vector3.Lerp(SquashedScale, Vector3.one, elapsed / config.PaddleSquashDuration);
                yield return null;
            }

            body.localScale = Vector3.one;
        }

        private void ApplyTimeScale()
        {
            GameManager.Instance.SetFeedbackTimeScale(isHitStopped ? 0f : 1f);
        }

        /// <summary>A new break restarts an effect that is still running instead of stacking a second one.</summary>
        private void Restart(ref Coroutine routine, IEnumerator effect)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(effect);
        }
    }
}
