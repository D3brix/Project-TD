using System;
using System.Globalization;
using ProjectTD.Core;
using ProjectTD.Placement;
using ProjectTD.Towers;
using ProjectTD.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTD.UI
{
    /// <summary>
    /// The gameplay HUD: lives and gold (top left); wave number, Start Wave / Send Now, Auto Wave and its countdown
    /// (top right); the tower roster (bottom left); the selected tower's stats, branch upgrades and Sell (only while a
    /// tower is selected); and VICTORY/DEFEAT with Restart. It only displays state and forwards clicks: the rules live in
    /// <see cref="GameController"/>, <see cref="TowerBuilder"/> and <see cref="TowerInteraction"/>.
    /// </summary>
    public class Hud : MonoBehaviour
    {
        /// <summary>The widgets of one upgrade choice in the selected-tower panel.</summary>
        [Serializable]
        public class UpgradeOption
        {
            public Button button;
            public Image accent;
            public Image icon;
            public Text title;
            public Text traits;
            public Text preview;
            public Text cost;
        }

        static readonly Color Better = new Color(0.55f, 0.9f, 0.45f);
        static readonly Color Worse = new Color(1f, 0.5f, 0.4f);
        static readonly Color Muted = new Color(0.72f, 0.66f, 0.55f);
        static readonly Color Gold = new Color(1f, 0.83f, 0.35f);
        static readonly Color TooExpensive = new Color(1f, 0.45f, 0.4f);

        [SerializeField] GameController game;
        [SerializeField] TowerBuilder builder;
        [SerializeField] TowerInteraction interaction;

        [Header("Resources (top left)")]
        [SerializeField] Text livesText;
        [SerializeField] Text currencyText;

        [Header("Waves (top right)")]
        [SerializeField] Text waveText;
        [SerializeField] Text waveStatusText;
        [SerializeField] Button waveButton;
        [SerializeField] Text waveButtonLabel;
        [SerializeField] Button autoWaveButton;
        [SerializeField] Text autoWaveLabel;
        [SerializeField] Image autoWaveLamp;
        [SerializeField] Image countdownFill;
        [SerializeField] Button restartButton;

        [Header("Roster (bottom left)")]
        [SerializeField] RosterCard rosterCard;
        [SerializeField] Image rosterHighlight;
        [SerializeField] Text rosterName;
        [SerializeField] Text rosterCost;
        [SerializeField] Text hintText;

        [Header("Selected tower")]
        [SerializeField] GameObject towerPanel;
        [SerializeField] Text towerTitle;
        [SerializeField] Image branchBadge;
        [SerializeField] Text branchBadgeText;
        [SerializeField] Text towerStats;
        [SerializeField] UpgradeOption[] upgradeOptions = new UpgradeOption[0];
        [SerializeField] Button sellButton;
        [SerializeField] Text sellLabel;
        [SerializeField] Button closeButton;

        [Header("End of game")]
        [SerializeField] GameObject endPanel;
        [SerializeField] Text outcomeText;
        [SerializeField] Text outcomeDetail;
        [SerializeField] Button endRestartButton;

        Tower RosterTower => builder.AvailableTowers.Count > 0 ? builder.AvailableTowers[0] : null;

        void Awake()
        {
            rosterCard.Pressed += OnRosterPressed;
            waveButton.onClick.AddListener(() => game.StartNextWave());
            autoWaveButton.onClick.AddListener(() => game.SetAutoWave(!game.AutoWave.AutoEnabled));
            for (int i = 0; i < upgradeOptions.Length; i++)
            {
                int branchIndex = i;
                upgradeOptions[i].button.onClick.AddListener(() => builder.TryUpgrade(interaction.Selected, branchIndex));
            }
            sellButton.onClick.AddListener(OnSellClicked);
            closeButton.onClick.AddListener(interaction.ClearSelection);
            restartButton.onClick.AddListener(game.Restart);
            endRestartButton.onClick.AddListener(game.Restart);
        }

        /// <summary>Pressing the card starts placement (click the map, or drag onto it); pressing it again cancels.</summary>
        void OnRosterPressed()
        {
            Tower tower = RosterTower;
            if (tower == null)
                return;
            if (interaction.IsPlacing && interaction.PlacingPrefab == tower)
                interaction.CancelPlacement();
            else
                interaction.BeginPlacement(tower, fromPress: true);
        }

        void OnSellClicked()
        {
            if (builder.TrySell(interaction.Selected))
                interaction.ClearSelection();
        }

        void Update()
        {
            GameSession session = game.Session;
            bool over = session.IsOver;

            livesText.text = session.Lives.ToString(CultureInfo.InvariantCulture);
            currencyText.text = session.Currency.ToString(CultureInfo.InvariantCulture);

            UpdateWaveControls(over);
            UpdateRoster(over);
            UpdateTowerPanel(over);
            UpdateEndPanel(session, over);
        }

        void UpdateWaveControls(bool over)
        {
            WaveSpawner waves = game.Waves;
            WaveCountdown auto = game.AutoWave;
            int wave = waves.CurrentWaveNumber;
            bool running = game.Phase == GamePhase.WaveRunning;

            waveText.text = $"WAVE {(wave == 0 ? "-" : wave.ToString(CultureInfo.InvariantCulture))} / {waves.WaveCount}";

            waveButton.interactable = game.CanStartNextWave;
            if (over)
                waveButtonLabel.text = "Game over";
            else if (running)
                waveButtonLabel.text = "In battle";
            else if (auto.IsCounting)
                waveButtonLabel.text = "Send Now";
            else if (!waves.CanStartNextWave)
                waveButtonLabel.text = "No more waves";
            else
                waveButtonLabel.text = wave == 0 ? "Start Wave" : "Next Wave";

            waveStatusText.text = over ? ""
                : running ? $"Enemies on the road: {waves.ActiveEnemies.Count}"
                : auto.IsCounting ? $"Next wave in {Mathf.CeilToInt(auto.Remaining)}..."
                : auto.AutoEnabled ? ""
                : wave == 0 ? "Prepare your defences" : "Waiting for your order";

            countdownFill.gameObject.SetActive(auto.IsCounting);
            countdownFill.fillAmount = auto.IsCounting ? auto.Remaining / auto.Duration : 0f;

            autoWaveButton.interactable = !over;
            autoWaveLabel.text = auto.AutoEnabled ? "Auto: ON" : "Auto: OFF";
            autoWaveLamp.color = auto.AutoEnabled ? new Color(1f, 0.8f, 0.3f) : new Color(0.3f, 0.26f, 0.2f);
        }

        void UpdateRoster(bool over)
        {
            Tower tower = RosterTower;
            bool placing = interaction.IsPlacing;
            rosterName.text = tower != null ? tower.DisplayName : "-";
            rosterCost.text = tower != null ? tower.BuildCost.ToString(CultureInfo.InvariantCulture) : "";
            rosterCost.color = tower != null && builder.CanAfford(tower) ? Gold : TooExpensive;
            rosterHighlight.enabled = placing;

            if (over)
                hintText.text = "";
            else if (placing)
                hintText.text = $"{PlacementRules.Describe(interaction.PlacementState)}\nRight-click or Esc cancels.";
            else if (interaction.Selected != null)
                hintText.text = "";
            else if (tower != null && !builder.CanAfford(tower))
                hintText.text = builder.Towers.Count == 0 ? "Not enough gold to build." : "Click a tower to upgrade or sell it.";
            else if (builder.Towers.Count == 0)
                hintText.text = "Pick a tower, then click the map to build it.\nOr drag it straight onto the map.";
            else
                hintText.text = "Click a tower to upgrade or sell it.";
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
            TowerBranch branch = progression.Branch;

            towerTitle.text = tower.DisplayName;
            branchBadge.color = branch != null ? branch.color : new Color(0.45f, 0.4f, 0.33f);
            branchBadgeText.text = branch != null ? stats.label.ToUpperInvariant() : "BASIC";
            towerStats.text = Invariant($"Damage  <b>{stats.damage:0.#}</b>\nRate  <b>{stats.AttacksPerSecond:0.0}</b>/s\nRange  <b>{stats.range:0.0}</b>");

            for (int i = 0; i < upgradeOptions.Length; i++)
                UpdateUpgradeOption(upgradeOptions[i], tower, i);

            sellLabel.text = $"Sell\n<color=#FFD35A>+{builder.SellValue(tower)}</color>";
            sellButton.interactable = true;
        }

        /// <summary>
        /// At the base level every branch is offered with its trade-off. Once committed, only that branch's card stays,
        /// showing its next tier (or that it is complete).
        /// </summary>
        void UpdateUpgradeOption(UpgradeOption option, Tower tower, int branchIndex)
        {
            TowerProgression progression = tower.Progression;
            bool exists = branchIndex < progression.Branches.Count;
            bool committedElsewhere = progression.HasBranch && progression.BranchIndex != branchIndex;
            option.button.gameObject.SetActive(exists && !committedElsewhere);
            if (!exists || committedElsewhere)
                return;

            TowerBranch branch = progression.Branches[branchIndex];
            option.accent.color = branch.color;
            option.icon.sprite = branch.icon;
            option.icon.enabled = branch.icon != null;

            TowerLevel next = progression.NextIn(branchIndex);
            if (next == null)
            {
                option.title.text = progression.Current.label.ToUpperInvariant();
                option.traits.text = "Fully upgraded";
                option.preview.text = "";
                option.cost.gameObject.SetActive(false);
                option.button.interactable = false;
                return;
            }

            option.title.text = (progression.HasBranch ? next.label : branch.name).ToUpperInvariant();
            option.traits.text = progression.HasBranch ? $"Next tier of {branch.name}" : $"+ {branch.strength}\n- {branch.weakness}";
            option.preview.text = Preview(progression.Current, next);
            option.cost.gameObject.SetActive(true);
            option.cost.text = next.cost.ToString(CultureInfo.InvariantCulture);
            bool affordable = builder.CanUpgrade(tower, branchIndex);
            option.cost.color = affordable ? Gold : TooExpensive;
            option.button.interactable = affordable;
        }

        void UpdateEndPanel(GameSession session, bool over)
        {
            endPanel.SetActive(over);
            if (!over)
                return;

            bool victory = session.Outcome == GameOutcome.Victory;
            outcomeText.text = victory ? "VICTORY" : "DEFEAT";
            outcomeText.color = victory ? new Color(1f, 0.85f, 0.35f) : new Color(1f, 0.4f, 0.35f);
            outcomeDetail.text = victory
                ? $"All {game.Waves.WaveCount} waves survived with {session.Lives} lives left."
                : $"The enemy broke through on wave {game.Waves.CurrentWaveNumber} of {game.Waves.WaveCount}.";
        }

        /// <summary>Every stat with its change, coloured by whether it gets better or worse.</summary>
        internal static string Preview(TowerLevel current, TowerLevel next)
        {
            return string.Join("\n",
                StatLine("Damage", current.damage, next.damage, "0.#", ""),
                StatLine("Rate", current.AttacksPerSecond, next.AttacksPerSecond, "0.0", "/s"),
                StatLine("Range", current.range, next.range, "0.0", ""));
        }

        static string StatLine(string name, float from, float to, string format, string unit)
        {
            string a = from.ToString(format, CultureInfo.InvariantCulture);
            string b = to.ToString(format, CultureInfo.InvariantCulture);
            if (a == b)
                return $"<color=#{ColorUtility.ToHtmlStringRGB(Muted)}>{name} {a}{unit}</color>";

            Color change = to > from ? Better : Worse;
            return $"{name} {a} → <color=#{ColorUtility.ToHtmlStringRGB(change)}><b>{b}</b></color>{unit}";
        }

        static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
    }
}
