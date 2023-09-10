using UnityEngine;

namespace Gerard.CherrypickGames
{
    [RequireComponent(typeof(Camera))]
    public class CameraPan : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 20f; // Speed of the camera movement
        private Vector3 _lastMousePosition; // Stores the position of the mouse in the last frame

        public void HandleCameraMovement()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastMousePosition = Input.mousePosition;
            }

            // Check for left mouse button hold
            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                transform.Translate(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0);
                _lastMousePosition = Input.mousePosition;
            }
        }
    }
}