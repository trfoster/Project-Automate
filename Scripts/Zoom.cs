using UnityEngine;

namespace ProjectAutomate
{
    public sealed class Zoom : MonoBehaviour
    {
        private PerfectPixelWithZoom ppwz;

        private void Start()
        {
            ppwz = GetComponent<PerfectPixelWithZoom>();
        }

        private void Update()
        {
            if(PauseMenu.IsGamePaused) return;
            if (Input.mouseScrollDelta.y == 0) return;
            if (Input.mouseScrollDelta.y > 0)
            {
                ppwz.ZoomIn();
            }
            else
            {
                ppwz.ZoomOut();
            }
        }
    }
}
