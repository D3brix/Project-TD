using ProjectTD.Towers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ProjectTD.Placement
{
    /// <summary>
    /// The player's pointer on the battlefield. In placement mode a ghost tower with its range follows the cursor,
    /// tinted by whether the spot is valid; left-click builds, right-click or Escape cancels. Placement that began
    /// with a press on a roster card also builds where the button is released, if that is over the map (drag to place).
    /// Clicks on the HUD never reach the battlefield.
    /// Otherwise, clicking a built tower selects it (showing its range) and clicking empty ground deselects.
    /// All rules and money live in <see cref="TowerBuilder"/>; this class only turns input into requests.
    /// </summary>
    public class TowerInteraction : MonoBehaviour
    {
        [SerializeField] TowerBuilder builder;
        [SerializeField] Camera worldCamera;
        [SerializeField] Color validColor = new Color(0.35f, 1f, 0.45f);
        [SerializeField] Color invalidColor = new Color(1f, 0.3f, 0.25f);
        [SerializeField] Color selectedColor = new Color(0.55f, 0.85f, 1f);

        Tower placingPrefab;
        Tower ghost;
        SpriteRenderer[] ghostRenderers;
        Color[] ghostBaseColors;
        bool dragging;

        public bool IsPlacing => placingPrefab != null;
        public Tower PlacingPrefab => placingPrefab;
        public PlacementResult PlacementState { get; private set; }
        public Tower Selected { get; private set; }

        /// <param name="fromPress">Started by pressing (not yet releasing) a roster card: releasing over the map builds there.</param>
        public void BeginPlacement(Tower prefab, bool fromPress = false)
        {
            CancelPlacement();
            ClearSelection();
            if (builder.IsLocked || !builder.CanAfford(prefab))
                return;

            placingPrefab = prefab;
            dragging = fromPress;
            ghost = Instantiate(prefab, transform);
            ghost.name = "Placement Ghost";
            ghost.enabled = false; // a ghost never targets or fires
            ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>(true);
            ghostBaseColors = new Color[ghostRenderers.Length];
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                ghostBaseColors[i] = ghostRenderers[i].color;
                ghostRenderers[i].sortingOrder += 30; // draw above everything on the map
            }
            PreviewAt(PointerWorldPosition() ?? (Vector2)transform.position);
        }

        public void CancelPlacement()
        {
            placingPrefab = null;
            dragging = false;
            if (ghost != null)
                Destroy(ghost.gameObject);
            ghost = null;
        }

        /// <summary>Tries to build the tower being placed at <paramref name="position"/>. Placement mode ends on success.</summary>
        public Tower ConfirmPlacement(Vector2 position)
        {
            if (!IsPlacing)
                return null;

            Tower tower = builder.TryPlace(placingPrefab, position);
            if (tower != null)
                CancelPlacement();
            return tower;
        }

        public void Select(Tower tower)
        {
            ClearSelection();
            if (tower == null)
                return;

            Selected = tower;
            tower.ShowRange(true, selectedColor);
        }

        public void ClearSelection()
        {
            if (Selected != null)
                Selected.ShowRange(false, selectedColor);
            Selected = null;
        }

        void Update()
        {
            if (builder.IsLocked)
            {
                CancelPlacement();
                ClearSelection();
                return;
            }

            // A sold (destroyed) tower compares equal to null.
            if (Selected == null && !ReferenceEquals(Selected, null))
                Selected = null;

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;
            bool cancel = (mouse != null && mouse.rightButton.wasPressedThisFrame) || (keyboard != null && keyboard.escapeKey.wasPressedThisFrame);

            Vector2? pointer = PointerWorldPosition();
            bool pointerOverUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool click = mouse != null && mouse.leftButton.wasPressedThisFrame && !pointerOverUi && pointer.HasValue;

            if (IsPlacing)
            {
                if (cancel)
                {
                    CancelPlacement();
                    return;
                }
                if (pointer.HasValue)
                    PreviewAt(pointer.Value);
                if (dragging && mouse != null && mouse.leftButton.wasReleasedThisFrame)
                {
                    dragging = false;
                    if (!pointerOverUi && pointer.HasValue)
                        ConfirmPlacement(pointer.Value); // dropped on the map; an invalid spot just stays in placement mode
                }
                else if (click)
                    ConfirmPlacement(pointer.Value);
                return;
            }

            if (cancel)
                ClearSelection();
            else if (click)
                Select(builder.TowerAt(pointer.Value));
        }

        /// <summary>Moves the placement ghost to <paramref name="position"/> and re-checks whether it can be built there.</summary>
        internal void PreviewAt(Vector2 position)
        {
            if (!IsPlacing)
                return;

            ghost.transform.position = new Vector3(position.x, position.y, 0f);
            PlacementState = builder.CheckPlacement(placingPrefab, position);

            bool valid = PlacementState == PlacementResult.Valid;
            for (int i = 0; i < ghostRenderers.Length; i++)
            {
                Color tint = valid ? ghostBaseColors[i] : Color.Lerp(ghostBaseColors[i], invalidColor, 0.6f);
                tint.a = ghostBaseColors[i].a * 0.75f;
                ghostRenderers[i].color = tint;
            }
            ghost.ShowRange(true, valid ? validColor : invalidColor); // after the tint, so the range keeps its own colours
        }

        Vector2? PointerWorldPosition()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || worldCamera == null)
                return null;
            return worldCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        }
    }
}
