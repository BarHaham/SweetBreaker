namespace SweetBreaker
{
    /// <summary>The run's states, as drawn in the GDD section 3 state diagram.</summary>
    public enum GameState
    {
        MainMenu,
        Serve,
        Playing,
        Paused,
        LevelClear,
        GameOver,
        Victory,
    }
}
