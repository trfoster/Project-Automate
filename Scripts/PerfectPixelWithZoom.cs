using UnityEngine;

namespace ProjectAutomate
{
    public sealed class PerfectPixelWithZoom : MonoBehaviour
    {
        [SerializeField]
        private float pixelsPerUnit = 32;
        //[SerializeField] // Uncomment if you want to watch scaling in the editor
        [SerializeField]
        private float zoomScaleMax = 10f;
        [SerializeField]
        private float zoomScaleStart = 1f;
        [SerializeField]
        private float zoomScaleMin = 1f;
        [SerializeField]
        private bool smoovZoom = true;
        [SerializeField]
        private float smoovZoomDuration = 0.5f; // In seconds

        private int screenHeight;

        private float cameraSize;
        private Camera cameraComponent;

        private float zoomStartTime;
        private float zoomCurrentValue = 1f;
        private float zoomNextValue = 1f;
        private float zoomInterpolation = 1f;

        public float panSpeed = 5f;
        public float panBorderThickness = 5f;
        public Vector2 panLimit;

        private float CurrentZoomScale { get; set; } = 1;

        private void Start()
        {
            screenHeight = Screen.height;
            cameraComponent = gameObject.GetComponent<Camera>();
            SetZoomImmediate(zoomScaleStart);
        }

        private void Update()
        {
            if (screenHeight != Screen.height)
            {
                screenHeight = Screen.height;
                UpdateCameraScale();
            }

            if (MidZoom)
            {
                if (smoovZoom)
                {
                    zoomInterpolation = (Time.time - zoomStartTime) / smoovZoomDuration;
                }
                else
                {
                    zoomInterpolation = 1f; // express to the end
                }
                CurrentZoomScale = Mathf.Lerp(zoomCurrentValue, zoomNextValue, zoomInterpolation);
                UpdateCameraScale();
            }
            Vector3 pos = transform.position;
            if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - panBorderThickness && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
            {
                pos.y += panSpeed * Time.deltaTime * cameraSize;
            }
            if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= panBorderThickness && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
            {
                pos.y -= panSpeed * Time.deltaTime * cameraSize;
            }
            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.LeftControl) || Input.mousePosition.x >= Screen.width - panBorderThickness && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
            {
                pos.x += panSpeed * Time.deltaTime * cameraSize;
            }
            if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= panBorderThickness && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
            {
                pos.x -= panSpeed * Time.deltaTime * cameraSize;
            }
            pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
            pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);
            transform.position = pos;
        }

        private void UpdateCameraScale()
        {
            // The magic formula from the Unity Docs
            cameraSize = screenHeight / (CurrentZoomScale * pixelsPerUnit) * 0.5f;
            cameraComponent.orthographicSize = cameraSize;
        }

        private bool MidZoom { get { return zoomInterpolation < 1; } }

        private void SetUpSmoovZoom()
        {
            zoomStartTime = Time.time;
            zoomCurrentValue = CurrentZoomScale;
            zoomInterpolation = 0f;
        }

        public void SetPixelsPerUnit(int pixelsPerUnitValue)
        {
            pixelsPerUnit = pixelsPerUnitValue;
            UpdateCameraScale();
        }

        // Has to be >= zoomScaleMin
        public void SetZoomScaleMax(int zoomScaleMaxValue)
        {
            zoomScaleMax = Mathf.Max(zoomScaleMaxValue, zoomScaleMin);
        }

        public void SetSmoovZoomDuration(float smoovZoomDurationValue)
        {
            smoovZoomDuration = Mathf.Max(smoovZoomDurationValue, 0.0333f); // 1/30th of a second sounds small enough
        }

        // Clamped to the range [1, zoomScaleMax], Integer values will be pixel-perfect
        public void SetZoom(float scale)
        {
            SetUpSmoovZoom();
            zoomNextValue = Mathf.Max(Mathf.Min(scale, zoomScaleMax), zoomScaleMin);
        }

        // Clamped to the range [1, zoomScaleMax], Integer values will be pixel-perfect
        private void SetZoomImmediate(float scale)
        {
            CurrentZoomScale = Mathf.Max(Mathf.Min(scale, zoomScaleMax), zoomScaleMin);
            UpdateCameraScale();
        }

        public void ZoomIn()
        {
            if (MidZoom) return;
            SetUpSmoovZoom();
            zoomNextValue = Mathf.Min(CurrentZoomScale + 1, zoomScaleMax);
        }

        public void ZoomOut()
        {
            SetUpSmoovZoom();
            zoomNextValue = Mathf.Max(CurrentZoomScale - 1, zoomScaleMin);
        }
    }
}
