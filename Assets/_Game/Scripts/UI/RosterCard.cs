using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectTD.UI
{
    /// <summary>
    /// A tower card in the build roster. Reacts on press rather than on click, so the player can either
    /// click the card and then click the map, or press it and drag straight onto the map.
    /// </summary>
    public class RosterCard : MonoBehaviour, IPointerDownHandler
    {
        public event Action Pressed;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                Pressed?.Invoke();
        }
    }
}
