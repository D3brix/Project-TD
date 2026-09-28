using System;

namespace ProjectTD.Waves
{
    /// <summary>
    /// Auto Wave: while it is on and a wave may start, counts down and then asks for the next wave exactly once.
    /// Whenever a wave may not start (one is running, the game is over, no waves left) or Auto is off,
    /// there is no countdown, so a pending launch is cancelled by any of those. Pure logic, no Unity lifecycle.
    /// </summary>
    public class WaveCountdown
    {
        public float Duration { get; }
        public bool AutoEnabled { get; private set; }
        public bool IsCounting { get; private set; }

        /// <summary>Seconds left before the automatic launch; 0 when not counting.</summary>
        public float Remaining { get; private set; }

        public WaveCountdown(float duration, bool autoEnabled = false)
        {
            if (duration <= 0f)
                throw new ArgumentOutOfRangeException(nameof(duration), "The countdown must last some time.");

            Duration = duration;
            AutoEnabled = autoEnabled;
        }

        /// <summary>Turning Auto off cancels a running countdown. Turning it on starts one on the next <see cref="Tick"/> if a wave may start.</summary>
        public void SetAuto(bool enabled)
        {
            AutoEnabled = enabled;
            if (!enabled)
                Cancel();
        }

        public void Cancel()
        {
            IsCounting = false;
            Remaining = 0f;
        }

        /// <summary>
        /// Advances the countdown by <paramref name="deltaTime"/>. <paramref name="canStartWave"/> says whether a wave may start right now.
        /// Returns true on the one tick the countdown runs out; the countdown is then over, and the caller launches the wave.
        /// </summary>
        public bool Tick(float deltaTime, bool canStartWave)
        {
            if (!AutoEnabled || !canStartWave)
            {
                Cancel();
                return false;
            }

            if (!IsCounting)
            {
                IsCounting = true;
                Remaining = Duration;
                return false;
            }

            Remaining -= deltaTime;
            if (Remaining > 0f)
                return false;

            Cancel();
            return true;
        }
    }
}
