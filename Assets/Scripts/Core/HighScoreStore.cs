using UnityEngine;

namespace SweetBreaker
{
    /// <summary>
    /// Reads and writes the single high-score integer. One integer of local state is exactly what
    /// PlayerPrefs is for (GDD section 7).
    /// </summary>
    public static class HighScoreStore
    {
        private const string HighScoreKey = "HighScore";

        public static int Load()
        {
            return PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        /// <summary>
        /// Stores the score only if it is strictly greater than the stored one.
        /// Returns true when it did, so the end screen can say "NEW HIGH SCORE!".
        /// </summary>
        public static bool TrySave(int score)
        {
            if (score <= Load())
                return false;

            PlayerPrefs.SetInt(HighScoreKey, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}
