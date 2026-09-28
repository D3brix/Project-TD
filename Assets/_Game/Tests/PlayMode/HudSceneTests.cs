using System.Collections;
using System.Globalization;
using NUnit.Framework;
using ProjectTD.Core;
using ProjectTD.Placement;
using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjectTD.Tests
{
    /// <summary>
    /// The HUD on the real level, driven by a simulated mouse through the real EventSystem: the roster builds by
    /// click-then-click or by dragging, HUD clicks never reach the battlefield, and the tower panel shows the real numbers.
    /// </summary>
    public class HudSceneTests
    {
        const int Rapid = 0, Heavy = 1, Balanced = 2;
        static readonly Vector2 BuildSpot = new Vector2(2.2f, 2.8f);   // inside the bridge horseshoe
        static readonly Vector2 SecondSpot = new Vector2(-2.7f, 2.1f);

        readonly InputTestFixture input = new InputTestFixture();
        Mouse mouse;
        Camera cam;
        GameController game;
        TowerBuilder builder;
        TowerInteraction interaction;

        [UnitySetUp]
        public IEnumerator LoadLevelWithATestMouse()
        {
            input.Setup(); // before the level loads, so the UI input module binds to the simulated mouse
            mouse = InputSystem.AddDevice<Mouse>();
            yield return SceneManager.LoadSceneAsync("Prototype", LoadSceneMode.Single);
            yield return null;
            cam = Camera.main;
            game = Object.FindAnyObjectByType<GameController>();
            builder = Object.FindAnyObjectByType<TowerBuilder>();
            interaction = Object.FindAnyObjectByType<TowerInteraction>();
            input.Set(mouse.position, WorldToScreen(new Vector2(-12f, -5f)));
            yield return null;
        }

        [TearDown]
        public void RemoveTheTestMouse()
        {
            input.TearDown();
        }

        Vector2 WorldToScreen(Vector2 world) => cam.WorldToScreenPoint(world);

        static Vector2 ScreenCentre(string hudPath)
        {
            GameObject target = GameObject.Find("HUD/" + hudPath);
            Assert.IsNotNull(target, $"HUD/{hudPath} not found or hidden");
            var rect = (RectTransform)target.transform;
            Canvas canvas = target.GetComponentInParent<Canvas>();
            return RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.TransformPoint(rect.rect.center));
        }

        IEnumerator MoveTo(Vector2 screen)
        {
            input.Set(mouse.position, screen);
            yield return null;
        }

        IEnumerator Click(Vector2 screen)
        {
            yield return MoveTo(screen);
            input.Press(mouse.leftButton);
            yield return null;
            input.Release(mouse.leftButton);
            yield return null;
            yield return null;
        }

        IEnumerator ClickHud(string hudPath) => Click(ScreenCentre(hudPath));

        [UnityTest]
        public IEnumerator ClickingTheRosterCard_StartsPlacement_ThenClickingTheMapBuildsThere()
        {
            yield return ClickHud("BottomLeft/RosterCard");

            Assert.IsTrue(interaction.IsPlacing, "The card click selects the tower for placement.");
            Assert.IsEmpty(builder.Towers, "Clicking the card builds nothing by itself.");
            Assert.AreEqual(50, game.Session.Currency);

            yield return MoveTo(WorldToScreen(BuildSpot));
            Assert.AreEqual(PlacementResult.Valid, interaction.PlacementState, "The ghost follows the pointer onto the map.");
            yield return Click(WorldToScreen(BuildSpot));

            Assert.AreEqual(1, builder.Towers.Count);
            Assert.Less(Vector2.Distance(builder.Towers[0].transform.position, BuildSpot), 0.05f);
            Assert.AreEqual(25, game.Session.Currency);
            Assert.IsFalse(interaction.IsPlacing);
        }

        [UnityTest]
        public IEnumerator DraggingFromTheRosterCard_OntoTheMap_BuildsWhereItIsDropped()
        {
            yield return MoveTo(ScreenCentre("BottomLeft/RosterCard"));
            input.Press(mouse.leftButton);
            yield return null;
            Assert.IsTrue(interaction.IsPlacing);

            Vector2 from = ScreenCentre("BottomLeft/RosterCard"), to = WorldToScreen(BuildSpot);
            for (int i = 1; i <= 6; i++)
                yield return MoveTo(Vector2.Lerp(from, to, i / 6f));
            input.Release(mouse.leftButton);
            yield return null;
            yield return null;

            Assert.AreEqual(1, builder.Towers.Count);
            Assert.Less(Vector2.Distance(builder.Towers[0].transform.position, BuildSpot), 0.05f);
            Assert.AreEqual(25, game.Session.Currency);
        }

        [UnityTest]
        public IEnumerator WhilePlacing_ClicksAndDropsOnTheHud_NeverBuild()
        {
            // A drag that ends on the HUD builds nothing, and placement carries on.
            yield return MoveTo(ScreenCentre("BottomLeft/RosterCard"));
            input.Press(mouse.leftButton);
            yield return null;
            yield return MoveTo(ScreenCentre("TopRight/WaveButton"));
            input.Release(mouse.leftButton);
            yield return null;
            Assert.IsTrue(interaction.IsPlacing);
            Assert.IsEmpty(builder.Towers);

            // Wave controls work while placing and build nothing.
            yield return ClickHud("TopRight/AutoWaveButton");
            Assert.IsTrue(game.AutoWave.AutoEnabled);
            yield return ClickHud("TopRight/AutoWaveButton");
            Assert.IsFalse(game.AutoWave.AutoEnabled);
            yield return ClickHud("TopLeft");
            Assert.IsEmpty(builder.Towers, "No tower was built under any HUD element.");
            Assert.AreEqual(50, game.Session.Currency);
            Assert.IsTrue(interaction.IsPlacing, "Still placing.");

            // Pressing the card again cancels.
            yield return ClickHud("BottomLeft/RosterCard");
            Assert.IsFalse(interaction.IsPlacing);
            Assert.IsEmpty(builder.Towers);
        }

        [UnityTest]
        public IEnumerator WithATowerSelected_UpgradeAutoAndSellClicks_OnlyDoWhatTheySay()
        {
            game.Session.AddCurrency(100); // test setup
            Tower tower = builder.TryPlace(builder.AvailableTowers[0], BuildSpot);
            yield return Click(WorldToScreen(BuildSpot));
            Assert.AreSame(tower, interaction.Selected, "Clicking a tower selects it.");
            Assert.IsTrue(IsShown("TowerPanel"), "The tower panel appears for the selection.");

            int gold = game.Session.Currency;
            yield return ClickHud("TowerPanel/Upgrade1");
            Assert.AreEqual(Heavy, tower.Progression.BranchIndex, "The Heavy card bought Heavy.");
            Assert.AreEqual(gold - tower.Progression.Current.cost, game.Session.Currency, "Charged once.");
            Assert.AreSame(tower, interaction.Selected, "Still selected.");
            Assert.IsFalse(IsShown("TowerPanel/Upgrade0"), "Rapid is no longer offered.");
            Assert.IsFalse(IsShown("TowerPanel/Upgrade2"), "Balanced is no longer offered.");

            yield return ClickHud("TopRight/AutoWaveButton");
            Assert.IsTrue(game.AutoWave.AutoEnabled);
            Assert.AreSame(tower, interaction.Selected, "Toggling Auto Wave leaves the selection alone.");
            game.SetAutoWave(false);

            int refund = builder.SellValue(tower);
            gold = game.Session.Currency;
            yield return ClickHud("TowerPanel/SellSlot/SellButton");
            Assert.AreEqual(gold + refund, game.Session.Currency);
            Assert.IsEmpty(builder.Towers, "Sold, and nothing was built in its place.");
            Assert.IsNull(interaction.Selected);
            Assert.IsFalse(IsShown("TowerPanel"), "The panel goes away with the selection.");
            Assert.AreEqual(0, game.Waves.CurrentWaveNumber, "No wave was started by any of this.");
        }

        [UnityTest]
        public IEnumerator ClickingEmptyGround_Deselects_AndTheCloseButtonDeselects()
        {
            Tower tower = builder.TryPlace(builder.AvailableTowers[0], BuildSpot);
            interaction.Select(tower);
            yield return null;
            yield return null; // the panel has been laid out
            yield return ClickHud("TowerPanel/CloseButton");
            Assert.IsNull(interaction.Selected);

            interaction.Select(tower);
            yield return Click(WorldToScreen(SecondSpot));
            Assert.IsNull(interaction.Selected);
            Assert.AreEqual(1, builder.Towers.Count, "Clicking ground outside placement mode builds nothing.");
        }

        [UnityTest]
        public IEnumerator TheTowerPanel_ShowsTheRealStatsAndCosts_ThroughEveryStep()
        {
            game.Session.AddCurrency(200); // test setup
            Tower tower = builder.TryPlace(builder.AvailableTowers[0], BuildSpot);
            interaction.Select(tower);
            yield return null;

            Assert.AreEqual(builder.AvailableTowers[0].BuildCost.ToString(), HudText("BottomLeft/RosterCard/Cost"));
            AssertPanelMatches(tower);
            for (int branch = 0; branch < 3; branch++)
                Assert.AreEqual(tower.Progression.NextIn(branch).cost.ToString(), HudText($"TowerPanel/Upgrade{branch}/Cost"));
            Assert.AreEqual("BASIC", HudText("TowerPanel/Info/BranchBadge/Text"));

            builder.TryUpgrade(tower, Balanced);
            yield return null;
            AssertPanelMatches(tower);
            Assert.AreEqual("BALANCED I", HudText("TowerPanel/Info/BranchBadge/Text"));
            Assert.AreEqual(tower.Progression.NextIn(Balanced).cost.ToString(), HudText("TowerPanel/Upgrade2/Cost"));
            Assert.AreEqual("BALANCED II", HudText("TowerPanel/Upgrade2/Title"));

            builder.TryUpgrade(tower, Balanced);
            yield return null;
            AssertPanelMatches(tower);
            Assert.AreEqual("Fully upgraded", HudText("TowerPanel/Upgrade2/Traits"));
            Assert.IsFalse(GameObject.Find("HUD/TowerPanel/Upgrade2").GetComponent<Button>().interactable);
            StringAssert.Contains($"+{builder.SellValue(tower)}", HudText("TowerPanel/SellSlot/SellButton/Label"));
        }

        static void AssertPanelMatches(Tower tower)
        {
            string stats = HudText("TowerPanel/Info/TowerStats");
            StringAssert.Contains($"<b>{tower.Damage.ToString("0.#", CultureInfo.InvariantCulture)}</b>", stats);
            StringAssert.Contains($"<b>{(1f / tower.AttackInterval).ToString("0.0", CultureInfo.InvariantCulture)}</b>/s", stats);
            StringAssert.Contains($"<b>{tower.Range.ToString("0.0", CultureInfo.InvariantCulture)}</b>", stats);
        }

        static bool IsShown(string path) => GameObject.Find("HUD/" + path)?.activeInHierarchy == true;

        static string HudText(string path) => GameObject.Find("HUD/" + path)?.GetComponent<Text>()?.text;
    }
}
