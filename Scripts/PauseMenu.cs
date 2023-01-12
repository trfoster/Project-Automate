using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectAutomate
{
    public sealed class PauseMenu : MonoBehaviour
    {
        public static bool IsGamePaused;

        [SerializeField] private GameObject pauseMenuUI;
        
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (IsGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            IsGamePaused = false;
        }

        private void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            IsGamePaused = true;
        }

        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            IsGamePaused = false;
            TickSystem.Destroy();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }
}
