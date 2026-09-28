using NUnit.Framework;
using ProjectTD.Waves;

namespace ProjectTD.Tests
{
    public class WaveCountdownTests
    {
        const float Step = 0.1f;

        /// <summary>Ticks for <paramref name="seconds"/> and returns how many launches were requested.</summary>
        static int Run(WaveCountdown countdown, float seconds, bool canStartWave = true)
        {
            int launches = 0;
            for (float t = 0f; t < seconds; t += Step)
                if (countdown.Tick(Step, canStartWave))
                    launches++;
            return launches;
        }

        [Test]
        public void AutoIsOffByDefault_AndThenNothingEverLaunches()
        {
            var countdown = new WaveCountdown(3f);

            Assert.IsFalse(countdown.AutoEnabled);
            Assert.AreEqual(0, Run(countdown, 600f));
            Assert.IsFalse(countdown.IsCounting);
        }

        [Test]
        public void WithAutoOn_TheCountdownRunsItsDuration_ThenLaunchesExactlyOnce()
        {
            var countdown = new WaveCountdown(3f, autoEnabled: true);

            countdown.Tick(Step, true);
            Assert.IsTrue(countdown.IsCounting);
            Assert.AreEqual(3f, countdown.Remaining, 1e-4f);

            float elapsed = 0f;
            while (!countdown.Tick(Step, true))
            {
                elapsed += Step;
                Assert.Less(elapsed, 10f, "The countdown never ended.");
            }
            Assert.AreEqual(3f, elapsed + Step, Step * 1.5f, "Launches when the countdown runs out, not before.");
            Assert.IsFalse(countdown.IsCounting);
            Assert.AreEqual(0, Run(countdown, 10f, canStartWave: false), "While the launched wave runs, nothing else is launched.");
        }

        [Test]
        public void TurningAutoOff_DuringTheCountdown_CancelsTheLaunch()
        {
            var countdown = new WaveCountdown(3f, autoEnabled: true);
            Run(countdown, 2f);

            countdown.SetAuto(false);

            Assert.IsFalse(countdown.IsCounting);
            Assert.AreEqual(0, Run(countdown, 60f));
        }

        [Test]
        public void TurningAutoOn_DuringPreparation_StartsAFullCountdown()
        {
            var countdown = new WaveCountdown(3f);
            Run(countdown, 5f);

            countdown.SetAuto(true);
            countdown.Tick(Step, true);

            Assert.IsTrue(countdown.IsCounting);
            Assert.AreEqual(3f, countdown.Remaining, 1e-4f);
        }

        [Test]
        public void WhenAWaveMayNotStart_ThereIsNoCountdown_AndAPendingLaunchIsCancelled()
        {
            var countdown = new WaveCountdown(3f, autoEnabled: true);
            Run(countdown, 2f);

            // A wave started some other way, the game ended, or no waves are left: all look the same here.
            Assert.IsFalse(countdown.Tick(Step, canStartWave: false));
            Assert.IsFalse(countdown.IsCounting);
            Assert.AreEqual(0, Run(countdown, 60f, canStartWave: false));
        }

        [Test]
        public void Cancel_ForgetsTheCountdown_AndANewOneStartsFromTheTop()
        {
            var countdown = new WaveCountdown(3f, autoEnabled: true);
            Run(countdown, 2f);

            countdown.Cancel(); // e.g. Send Now launched the wave

            Assert.IsFalse(countdown.IsCounting);
            Assert.AreEqual(0f, countdown.Remaining);
            countdown.Tick(Step, true);
            Assert.AreEqual(3f, countdown.Remaining, 1e-4f);
        }
    }
}
