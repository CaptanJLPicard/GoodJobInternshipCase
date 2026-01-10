using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GoodJobInternshipCase.Core
{
    /// <summary>
    /// Simple UI manager for score display and restart button.
    /// Minimal UI implementation as per requirements.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private Button _restartButton;

        [Header("Settings")]
        [SerializeField] private string _scoreFormat = "Score: {0}";

        private void Start()
        {
            // Setup restart button
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            // Subscribe to game events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                UpdateScore(GameManager.Instance.Score);
            }
        }

        private void OnDestroy()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(OnRestartClicked);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
            }
        }

        /// <summary>
        /// Update score display
        /// </summary>
        public void UpdateScore(int score)
        {
            if (_scoreText != null)
            {
                _scoreText.text = string.Format(_scoreFormat, score);
            }
        }

        /// <summary>
        /// Handle restart button click
        /// </summary>
        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
    }
}
