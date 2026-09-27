using System;
using ProjectTD.Waves;

namespace ProjectTD.Core
{
    public enum GameOutcome
    {
        InProgress,
        Victory,
        Defeat
    }

    /// <summary>
    /// Lives, currency and the win/loss result for one play session. Once the game is over,
    /// nothing changes any more.
    /// </summary>
    public class GameSession
    {
        public int Lives { get; private set; }
        public int Currency { get; private set; }
        public GameOutcome Outcome { get; private set; } = GameOutcome.InProgress;
        public bool IsOver => Outcome != GameOutcome.InProgress;

        public GameSession(int startingLives, int startingCurrency)
        {
            if (startingLives <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingLives), "Starting lives must be positive.");
            if (startingCurrency < 0)
                throw new ArgumentOutOfRangeException(nameof(startingCurrency), "Starting currency cannot be negative.");

            Lives = startingLives;
            Currency = startingCurrency;
        }

        public void AddCurrency(int amount)
        {
            if (IsOver || amount <= 0)
                return;

            Currency += amount;
        }

        public void LoseLives(int amount)
        {
            if (IsOver || amount <= 0)
                return;

            Lives = Math.Max(0, Lives - amount);
            if (Lives == 0)
                Outcome = GameOutcome.Defeat;
        }

        /// <summary>Declares victory if every wave is complete (all spawned, none still alive) and the game is not already over.</summary>
        public bool TryDeclareVictory(WaveProgress waves)
        {
            if (IsOver || !waves.AreAllWavesComplete)
                return false;

            Outcome = GameOutcome.Victory;
            return true;
        }
    }
}
