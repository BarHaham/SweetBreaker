using System.Collections;
using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Makes breaking a brick feel like a hit (GDD section 6). Every effect here runs on unscaled
    /// time, and the time scale always goes through GameManager, never Time.timeScale directly.
    /// </summary>
    public class ImpactFeedback : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private Transform cameraTransform;

        private Vector3 cameraRestPosition;
        private Coroutine hitStopRoutine;
        private Coroutine shakeRoutine;
        private bool isHitStopped;

        private void Awake()
        {
            cameraRestPosition = cameraTransform.localPosition;
        }

        private void OnEnable()
        {
            levelManager.BrickDestroyed += PlayBrickBreak;
        }

        private void OnDisable()
        {
            levelManager.BrickDestroyed -= PlayBrickBreak;
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
