using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gerard.CherryPickGames.UI
{
    public class ButtonPressRelease : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event Action<bool> OnButtonStateChanged;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            OnButtonStateChanged?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnButtonStateChanged?.Invoke(false);
        }
    }
}
