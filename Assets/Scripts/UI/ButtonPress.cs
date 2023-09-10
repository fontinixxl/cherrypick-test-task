using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gerard.CherryPickGames.UI
{
    public class ButtonPress : MonoBehaviour, IPointerDownHandler
    {
        public event Action OnButtonPressed;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnButtonPressed?.Invoke();
        }
    }
}
