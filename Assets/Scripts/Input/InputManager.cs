using System;
using Gerard.CherrypickGames;
using Gerard.CherrypickGames.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gerard.CherryPickGames.Input
{
    [DefaultExecutionOrder(-1)]
    public class InputManager : Singleton<InputManager>
    {
        public event Action<Vector2, bool> CameraMoveEvent;
        private InputControls _inputControls;

        public override void Awake()
        {
            _inputControls = new InputControls();
            _inputControls.Camera.Pan.performed += ctx =>
            {
                CameraMoveEvent?.Invoke(ctx.ReadValue<Vector2>(), ctx.control.device is Touchscreen);
            };

            base.Awake();
        }

        private void OnEnable()
        {
            EnableCameraControls();
        }

        private void OnDisable()
        {
            _inputControls.Disable();
        }

        public void EnableCameraControls()
        {
            _inputControls.Camera.Enable();
            _inputControls.Dragging.Disable();
        }

        public void EnableDraggingControls()
        {
            _inputControls.Camera.Disable();
            _inputControls.Dragging.Enable();
        }

        public Vector2 GetCurrentMouseOrTouchPosition()
        {
            return _inputControls.Dragging.Position.ReadValue<Vector2>();
        }

    }
}