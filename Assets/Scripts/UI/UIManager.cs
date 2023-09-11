using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gerard.CherryPickGames.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private ButtonPressRelease spawnButton;
        [SerializeField] private ButtonPressRelease clearingButton;
        [SerializeField] private Slider zoomSlider;

        private ButtonPressRelease _sliderPointerUpDown;

        private void Awake()
        {
            _sliderPointerUpDown = zoomSlider.GetComponent<ButtonPressRelease>();
        }

        private void OnEnable()
        {
            _sliderPointerUpDown.OnButtonStateChanged += UpdateUIInteractionState;
            spawnButton.OnButtonStateChanged += UpdateUIInteractionState;
            clearingButton.OnButtonStateChanged += UpdateUIInteractionState;
        }

        private void OnDisable()
        {
            _sliderPointerUpDown.OnButtonStateChanged -= UpdateUIInteractionState;
            spawnButton.OnButtonStateChanged -= UpdateUIInteractionState;
            clearingButton.OnButtonStateChanged -= UpdateUIInteractionState;
        }

        private void UpdateUIInteractionState(bool state)
        {
            IsUIBeingInteracted = state;
        }

        public bool IsUIBeingInteracted { get; private set; }
        public ButtonPressRelease SpawnButton => spawnButton;
        public ButtonPressRelease ClearingButton => clearingButton;
        public Slider ZoomSlider => zoomSlider;
    }
}