using UnityEngine;
using UnityEngine.UI;

namespace ProjectAutomate
{
    public sealed class FPSCounter : MonoBehaviour
    {
        private int frameCount;
        private float dt;
        private const float UpdateRate = 5f; // 4 updates per sec.
        private Text thisText;

        private void Awake()
        {
            thisText = GetComponent<Text>();
        }

        private void Update()
        {
            frameCount++;
            dt += Time.deltaTime;
            if (dt < 1.0f / UpdateRate) return;
            int fps = Mathf.RoundToInt(frameCount / dt);
            frameCount = 0;
            dt -= 1.0f / UpdateRate;
            thisText.text = string.Concat(fps.ToString(), " FPS");
        }
    }
}
