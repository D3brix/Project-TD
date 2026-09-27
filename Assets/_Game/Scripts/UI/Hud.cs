using System;
using System.Globalization;
using ProjectTD.Core;
using ProjectTD.Placement;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTD.UI
{
    /// <summary>
    /// Shows lives, currency and waves; the build and Start Wave buttons; the selected tower's stats with
    /// Upgrade and Sell; and VICTORY/DEFEAT with Restart. It only displays state and forwards clicks:
    /// the rules live in <see cref="GameController"/>, <see cref="TowerBuilder"/> and <see cref="TowerInteraction"/>.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        [SerializeField] GameController game;
        [SerializeField] TowerBuilder builder;
        [SerializeField] TowerInteraction interaction;

        [Header("Top bar")]
        [SerializeField] Text livesText;
        [SerializeField] Text currencyText;
        [SerializeField] Text waveText;
        [SerializeField] Button restartButton;

        [Header("Bottom bar")]
        [SerializeField] Button buildButton;
        [SerializeField] Text buildLabel;
        [SerializeField] Button startWaveButton;
        [SerializeField] Text startWaveLabel;
        [SerializeField] Text hintText;

        [Header("Selected tower")]
        [SerializeField] GameObject towerPanel;
        [SerializeField] Text towerTitle;
        [SerializeField] Text towerStats;
        [SerializeField] Button upgradeButton;
        [SerializeField] Text upgradeLabel;
        [SerializeField] Button sellButton;
        [SerializeField] Text sellLabel;

        [Header("End of game")]
        [SerializeField] GameObject endPanel;
        [SerializeField] Text outcomeText;
        [SerializeField] Text outcomeDetail;
        [SerializeField] Button endRestartButton;

        Tower BuildOption => builder.AvailableTowers.Count > 0 ? builder.AvailableTowers[0] : null;

        void Awake()
        {
            buildButton.onClick.AddListener(OnBuildClicked);
            startWaveButton.onClick.AddListener(() => game.StartNextWave());
            upgradeButton.onClick.AddListener(() => builder.TryUpgrade(interaction.Selected));
            sellButton.onClick.AddListener(OnSellClicked);
            restartButton.onClick.AddListener(game.Restart);
            endRestartButton.onClick.AddListener(game.Restart);
        }

        void OnBuildClicked()
        {
            if (interaction.IsPlacing)
                interaction.CancelPlacement();
            else if (BuildOption != null)
                interaction.BeginPlacement(BuildOption);
        }

        void OnSellClicked()
        {
            if (builder.TrySell(interaction.Selected))
                interaction.ClearSelection();
        }

        void Update()
        {
            GameSession session = game.Session;
            GamePhase phase = game.Phase;
            bool over = session.IsOver;

            livesText.text = $"Lives: {session.Lives}";
            currencyText.text = $"Gold: {session.Currency}";
            int wave = game.Waves.CurrentWaveNumber;
            waveText.text = wave == 0 ? $"Wave: - / {game.Waves.WaveCount}" : $"Wave: {wave} / {game.Waves.WaveCount}";

            Tower option = BuildOption;
            buildButton.interactable = !over && option != null && (interaction.IsPlacing || builder.CanAfford(option));
            buildLabel.text = option == null ? "-" : interaction.IsPlacing ? "Cancel build" : $"Build {option.DisplayName}\n{option.BuildCost} gold";

            startWaveButton.interactable = game.CanStartNextWave;
            startWaveLabel.text = over ? "Game over"
                : phase == GamePhase.WaveRunning ? $"Wave {wave} running..."
                : game.Waves.CurrentWaveNumber >= game.Waves.WaveCount ? "No more waves"
                : $"Start Wave {wave + 1}";

            UpdateTowerPanel(over);
            hintText.text = Hint(phase, option);

            endPanel.SetActive(over);
            if (over)
            {
                bool victory = session.Outcome == GameOutcome.Victory;
                outcomeText.text = victory ? "VICTORY" : "DEFEAT";
                outcomeText.color = victory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
                outcomeDetail.text = victory
                    ? $"All {game.Waves.WaveCount} waves survived with {session.Lives} lives left."
                    : $"The enemy broke through on wave {wave} of {game.Waves.WaveCount}.";
            }
        }

        void UpdateTowerPanel(bool over)
        {
            Tower tower = interaction.Selected;
            bool show = tower != null && !over;
            towerPanel.SetActive(show);
            if (!show)
                return;

            TowerProgression progression = tower.Progression;
            TowerLevel stats = progression.Current;
            towerTitle.text = $"{tower.DisplayName}  -  Level {progression.LevelIndex + 1}/{progression.LevelCount}  ({stats.label})";
            towerStats.text = Invariant($"Damage {stats.damage:0.#}   Attack rate {stats.AttacksPerSecond:0.0}/s   Range {stats.range:0.0}");

            if (progression.IsMaxLevel)
            {
                upgradeLabel.text = "Max level";
                upgradeButton.interactable = false;
            }
            else
            {
                TowerLevel next = progression.Next;
                upgradeLabel.text = $"Upgrade: {next.label} ({next.cost} gold)\n{UpgradeSummary(stats, next)}";
                upgradeButton.interactable = builder.CanUpgrade(tower);
            }

            sellLabel.text = $"Sell\n+{builder.SellValue(tower)} gold";
            sellButton.interactable = true;
        }

        string Hint(GamePhase phase, Tower option)
        {
            if (phase == GamePhase.Victory || phase == GamePhase.Defeat)
                return "";
            if (interaction.IsPlacing)
                return $"{PlacementRules.Describe(interaction.PlacementState)}   (right-click or Esc to cancel)";
            if (interaction.Selected != null)
                return "";
            if (option != null && !builder.CanAfford(option) && builder.Towers.Count == 0)
                return "Not enough gold to build.";
            if (phase == GamePhase.WaveRunning)
                return "Click a tower to upgrade or sell it.";
            if (builder.Towers.Count == 0)
                return "Build your first tower: choose a spot where it can cover the road.";
            return "Build, upgrade or sell, then start the next wave when ready.";
        }

        static string UpgradeSummary(TowerLevel current, TowerLevel next)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!Mathf.Approximately(next.damage, current.damage))
                parts.Add(Invariant($"dmg {current.damage:0.#} → {next.damage:0.#}"));
            if (!Mathf.Approximately(next.attackInterval, current.attackInterval))
                parts.Add(Invariant($"rate {current.AttacksPerSecond:0.0} → {next.AttacksPerSecond:0.0}"));
            if (!Mathf.Approximately(next.range, current.range))
                parts.Add(Invariant($"range {current.range:0.0} → {next.range:0.0}"));
            return string.Join(", ", parts);
        }

        static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
    }
}
