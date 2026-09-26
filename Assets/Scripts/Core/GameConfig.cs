using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Every tuning value from the GDD (section 3) in one asset, so the feel of the game can be
    /// re-tuned without touching a prefab or a script.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Sweet Breaker/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [field: Header("Paddle")]
        [field: SerializeField, Tooltip("How fast the paddle crosses the screen (u/s).")]
        public float PaddleSpeed { get; private set; } = 12f;

        [field: SerializeField, Tooltip("How long a held key takes to bring the paddle up to full speed (s). A tap only nudges it.")]
        public float PaddleAccelerationTime { get; private set; } = 0.12f;

        [field: SerializeField, Tooltip("Base paddle width (u).")]
        public float PaddleWidth { get; private set; } = 2.2f;

        [field: SerializeField, Tooltip("How much wider the power-up makes the paddle.")]
        public float PaddleExpandMultiplier { get; private set; } = 1.5f;

        [field: Header("Ball")]
        [field: SerializeField, Tooltip("Constant ball speed (u/s). Re-check PaddleSpeed whenever this changes.")]
        public float BallSpeed { get; private set; } = 8f;

        [field: SerializeField, Tooltip("Angle above horizontal for the serve (degrees).")]
        public float LaunchAngle { get; private set; } = 60f;

        [field: SerializeField, Tooltip("Angle from vertical at a full edge hit on the paddle (degrees).")]
        public float MaxBounceAngle { get; private set; } = 75f;

        [field: SerializeField, Range(0f, 1f), Tooltip("Below this fraction of BallSpeed going up or down, the ball counts as stalling.")]
        public float MinVerticalSpeedFraction { get; private set; } = 0.25f;

        [field: SerializeField, Tooltip("How long a near-horizontal ball is tolerated before it is nudged (s).")]
        public float StallTimeout { get; private set; } = 1.5f;

        [field: Header("Run")]
        [field: SerializeField, Min(1)]
        public int StartingLives { get; private set; } = 3;

        [field: SerializeField, Tooltip("Pause after losing a life before the next serve, also the launch lockout (s).")]
        public float ServeDelay { get; private set; } = 1f;

        [field: SerializeField, Tooltip("How long the LEVEL CLEAR banner holds (s).")]
        public float LevelClearDelay { get; private set; } = 1.5f;

        [field: Header("Scoring")]
        [field: SerializeField] public int ScoreOneHitBreak { get; private set; } = 50;
        [field: SerializeField] public int ScoreTwoHitCrack { get; private set; } = 25;
        [field: SerializeField] public int ScoreTwoHitBreak { get; private set; } = 75;

        [field: Header("Power-up")]
        [field: SerializeField, Range(0f, 1f)]
        public float PowerUpDropChance { get; private set; } = 0.15f;

        [field: SerializeField, Tooltip("How fast the capsule falls (u/s).")]
        public float PowerUpFallSpeed { get; private set; } = 3f;

        [field: SerializeField, Tooltip("How long the expanded paddle lasts (s).")]
        public float PowerUpDuration { get; private set; } = 8f;

        [field: Header("Play field")]
        [field: SerializeField, Tooltip("Width x height of the walled play area, walls included (u).")]
        public Vector2 PlayFieldSize { get; private set; } = new Vector2(16f, 10f);

        [field: SerializeField, Tooltip("Strip at the top of the play field that the HUD sits over (u). No brick may sit in it; LevelManager warns if one does.")]
        public float HudBandHeight { get; private set; } = 1f;

        [field: Header("Impact feedback")]
        [field: SerializeField] public float HitStopDuration { get; private set; } = 0.05f;
        [field: SerializeField] public float ScreenShakeDuration { get; private set; } = 0.12f;

        [field: SerializeField, Tooltip("Largest camera offset while shaking (u). Also the camera fit margin.")]
        public float ScreenShakeMagnitude { get; private set; } = 0.15f;

        [field: SerializeField] public float PaddleSquashDuration { get; private set; } = 0.1f;

        [field: SerializeField, Range(0.05f, 1f)]
        public float LastBrickSlowMoScale { get; private set; } = 0.35f;

        [field: SerializeField, Tooltip("How long the last-brick slow motion lasts, in real seconds.")]
        public float LastBrickSlowMoDuration { get; private set; } = 0.8f;
    }
}
