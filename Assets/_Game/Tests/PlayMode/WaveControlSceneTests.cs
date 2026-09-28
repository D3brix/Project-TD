using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectTD.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectTD.Tests
{
    /// <summary>Manual waves, Auto Wave with its countdown, and Send Now, on the real level.</summary>
    public class WaveControlSceneTests : LevelSceneFixture
    {
        const float TimeScale = 8f;

        readonly List<(int wave, float time)> starts = new List<(int, float)>();
        readonly List<float> completions = new List<float>();

        [UnitySetUp]
        public IEnumerator RecordWaves()
        {
            starts.Clear();
            completions.Clear();
            game.Waves.WaveStarted += wave => starts.Add((wave, Time.time));
            game.Waves.WaveCompleted += _ => completions.Add(Time.time);
            yield break;
        }

        /// <summary>Enough defence to clear the early waves without losing (test setup, not the real economy).</summary>
        void BuildStrongDefence()
        {
            game.Session.AddCurrency(300);
            Place(InsideHorseshoeSpot);
            Place(HorseshoeTopSpot);
            Place(WestBankSpot);
            Place(NearSpawnSpot);
        }

        [UnityTest]
        public IEnumerator AutoOff_TheGameWaitsIndefinitely()
        {
            Assert.IsFalse(game.AutoWave.AutoEnabled, "Auto Wave is off when the level starts.");
            Time.timeScale = TimeScale;
            yield return new WaitForSeconds(60f);

            Assert.IsEmpty(starts);
            Assert.IsFalse(game.AutoWave.IsCounting);
            Assert.AreEqual(GamePhase.Preparation, game.Phase);
        }

        [UnityTest]
        public IEnumerator AutoOn_CountsDown_ThenLaunchesEachWaveOnce_OnlyAfterThePreviousOneResolved()
        {
            BuildStrongDefence();
            Time.timeScale = TimeScale;
            float enabledAt = Time.time;
            game.SetAutoWave(true);
            yield return null;
            Assert.IsTrue(game.AutoWave.IsCounting, "Turning Auto on during preparation starts the countdown.");
            Assert.IsEmpty(starts);

            yield return WaitFor(() => starts.Count >= 3, 300f);

            Assert.AreEqual(3f, starts[0].time - enabledAt, 0.3f, "Wave 1 launched when the countdown ran out.");
            for (int i = 0; i < starts.Count; i++)
                Assert.AreEqual(i + 1, starts[i].wave, "Each wave launched once, in order.");
            for (int i = 1; i < starts.Count; i++)
            {
                Assert.GreaterOrEqual(completions.Count, i);
                float pause = starts[i].time - completions[i - 1];
                Assert.AreEqual(3f, pause, 0.3f, $"Wave {i + 1} started a countdown after wave {i} resolved, never overlapping it.");
            }
            Assert.AreEqual(GameOutcome.InProgress, game.Session.Outcome);
        }

        [UnityTest]
        public IEnumerator SendNow_SkipsTheRestOfTheCountdown_AndDoesNotLaunchTwice()
        {
            BuildStrongDefence();
            Time.timeScale = TimeScale;
            game.SetAutoWave(true);
            yield return WaitFor(() => game.AutoWave.IsCounting && game.AutoWave.Remaining < 2.5f, 5f);

            Button waveButton = GameObject.Find("HUD/TopRight/WaveButton").GetComponent<Button>();
            yield return null; // let the HUD show the countdown
            Assert.AreEqual("Send Now", HudText("TopRight/WaveButton/Label"));
            Assert.IsTrue(waveButton.interactable);

            waveButton.onClick.Invoke();
            waveButton.onClick.Invoke(); // a hurried double click
            Assert.AreEqual(1, starts.Count, "Send Now launched the wave at once, and only once.");
            Assert.IsFalse(game.AutoWave.IsCounting);
            Assert.IsFalse(game.StartNextWave(), "No second wave while one is running.");

            yield return new WaitForSeconds(4f);
            Assert.AreEqual(1, starts.Count, "The cancelled countdown does not fire later.");
        }

        [UnityTest]
        public IEnumerator TurningAutoOff_DuringTheCountdown_CancelsTheLaunch()
        {
            Time.timeScale = TimeScale;
            game.SetAutoWave(true);
            yield return WaitFor(() => game.AutoWave.IsCounting, 1f);
            yield return new WaitForSeconds(1.5f);

            GameObject.Find("HUD/TopRight/AutoWaveButton").GetComponent<Button>().onClick.Invoke();

            Assert.IsFalse(game.AutoWave.AutoEnabled);
            Assert.IsFalse(game.AutoWave.IsCounting);
            yield return new WaitForSeconds(20f);
            Assert.IsEmpty(starts);
            Assert.AreEqual("Start Wave", HudText("TopRight/WaveButton/Label"));
        }

        [UnityTest]
        public IEnumerator Defeat_CancelsThePendingCountdown_AndNoCountdownStartsAfterwards()
        {
            game.Session.LoseLives(9);
            Time.timeScale = TimeScale;
            game.SetAutoWave(true);
            yield return WaitFor(() => game.Session.IsOver, 120f);

            Assert.AreEqual(GameOutcome.Defeat, game.Session.Outcome);
            Assert.IsFalse(game.AutoWave.IsCounting);
            int launched = starts.Count;
            game.SetAutoWave(true);
            yield return new WaitForSeconds(10f);
            Assert.IsFalse(game.AutoWave.IsCounting, "No countdown once the game is over.");
            Assert.AreEqual(launched, starts.Count);
        }

        [UnityTest]
        public IEnumerator Victory_LeavesNoCountdown()
        {
            BuildStrongDefence();
            game.Session.AddCurrency(1000); // test setup: upgrade everything so the scripted defence surely wins
            foreach (var tower in builder.Towers)
            {
                builder.TryUpgrade(tower, Rapid);
                builder.TryUpgrade(tower, Rapid);
            }
            Place(RidgeBendSpot);
            Time.timeScale = TimeScale;
            game.SetAutoWave(true);
            yield return WaitFor(() => game.Session.IsOver, 600f);

            Assert.AreEqual(GameOutcome.Victory, game.Session.Outcome);
            Assert.AreEqual(game.Waves.WaveCount, starts.Count, "Auto Wave ran every wave once.");
            yield return new WaitForSeconds(10f);
            Assert.IsFalse(game.AutoWave.IsCounting);
            Assert.AreEqual(game.Waves.WaveCount, starts.Count);
        }

        [UnityTest]
        public IEnumerator Restart_ResetsAutoWaveToOff()
        {
            game.SetAutoWave(true);
            yield return null;
            GameController oldGame = game;

            game.Restart();
            yield return WaitFor(() => Object.FindAnyObjectByType<GameController>() != null && Object.FindAnyObjectByType<GameController>() != oldGame, 10f);
            yield return null;
            FindLevelObjects();

            Assert.IsFalse(game.AutoWave.AutoEnabled);
            Assert.IsFalse(game.AutoWave.IsCounting);
            Assert.AreEqual("Auto: OFF", HudText("TopRight/AutoWaveButton/Label"));
        }

        [UnityTest]
        public IEnumerator RapidStartClicks_StartOneWave()
        {
            Button waveButton = GameObject.Find("HUD/TopRight/WaveButton").GetComponent<Button>();
            for (int i = 0; i < 5; i++)
                waveButton.onClick.Invoke();
            yield return null;
            waveButton.onClick.Invoke();

            Assert.AreEqual(1, starts.Count);
            Assert.AreEqual(1, game.Waves.CurrentWaveNumber);
            Assert.IsFalse(waveButton.interactable, "The button is disabled while the wave runs.");
        }
    }
}
