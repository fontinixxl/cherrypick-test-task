using UnityEngine;

namespace Gerard.CherrypickGames
{
    [RequireComponent(typeof(Camera))]
    public class CameraPan : MonoBehaviour
    {
        [SerializeField] private float panSpeed = 20f; // Speed of the camera movement
        private Vector3 _lastMousePosition; // Stores the position of the mouse in the last frame

        public void HandleCameraMovement(Vector2 currentDelta, bool isTouchDevice)
        {
            // TODO: Move it to settings (editor)
            if(isTouchDevice)
            {
                currentDelta *= 0.05f; // example scaling value; adjust as needed
            }
            var delta = new Vector3(currentDelta.x, currentDelta.y, 0);
            transform.Translate(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0);
        }
    }
}