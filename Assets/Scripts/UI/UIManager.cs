using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField, Tooltip("The fail panel UI GameObject with 'You Fail' text and retry button.")] private GameObject failPanel;
    [SerializeField, Tooltip("The winning panel UI GameObject with 'You Win' text and buttons.")] private GameObject winningPanel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject backToMenuButton;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (failPanel != null)
        {
            failPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Fail panel is not assigned in UIManager.");
        }
        if (winningPanel != null)
        {
            winningPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Winning panel is not assigned in UIManager.");
        }
    }

    public void ShowFailUI()
    {
        if (failPanel != null)
        {
            failPanel.SetActive(true);
        }
    }

    public void ShowWinningUI(int score)
    {
        if (winningPanel != null)
        {
            winningPanel.SetActive(true);
            if (scoreText != null)
            {
                scoreText.text = "Score: " + score.ToString();
            }

            DifficultyManager.Difficulty currentDifficulty = DifficultyManager.Instance.CurrentDifficulty;
            if (currentDifficulty == DifficultyManager.Difficulty.Hard)
            {
                if (nextLevelButton != null) nextLevelButton.SetActive(false);
                if (backToMenuButton != null) backToMenuButton.SetActive(true);
            }
            else
            {
                if (nextLevelButton != null) nextLevelButton.SetActive(true);
                if (backToMenuButton != null) backToMenuButton.SetActive(false);
            }
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        DifficultyManager.Difficulty currentDifficulty = DifficultyManager.Instance.CurrentDifficulty;
        if (currentDifficulty == DifficultyManager.Difficulty.Easy)
        {
            DifficultyManager.Instance.SetDifficulty(DifficultyManager.Difficulty.Medium);
        }
        else if (currentDifficulty == DifficultyManager.Difficulty.Medium)
        {
            DifficultyManager.Instance.SetDifficulty(DifficultyManager.Difficulty.Hard);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0); 
    }
}