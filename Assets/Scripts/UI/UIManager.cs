using UnityEngine;
using UnityEngine.UI;

namespace Gerard.CherryPickGames.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private ButtonPressRelease spawnButton;
        [SerializeField] private ButtonPress clearingButton;
        [SerializeField] private Slider zoomSlider;

        private ButtonPressRelease _sliderPointerUpDown;

        private void OnEnable()
        {
            _sliderPointerUpDown = zoomSlider.GetComponent<ButtonPressRelease>();
            _sliderPointerUpDown.OnButtonStateChanged += UpdateSliderState;
        }

        private void OnDisable()
        {
            _sliderPointerUpDown.OnButtonStateChanged -= UpdateSliderState;
        }

        private void UpdateSliderState(bool state)
        {
            IsSliderBeingInteracted = state;
        }

        public bool IsSliderBeingInteracted { get; private set; }
        public ButtonPressRelease SpawnButton => spawnButton;
        public ButtonPress ClearingButton => clearingButton;
        public Slider ZoomSlider => zoomSlider;
    }
}