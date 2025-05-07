using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject panelStart;
    [SerializeField] private GameObject panelDifficulty;

    private void Awake()
    {
        panelStart.SetActive(true);
        panelDifficulty.SetActive(false);
    }

    public void OnStartClicked()
    {
        panelStart.SetActive(false);
        panelDifficulty.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnDifficultySelected(int difficultyIndex)
    {
        if (DifficultyManager.Instance == null)
        {
            var go = new GameObject("DifficultyManager");
            go.AddComponent<DifficultyManager>(); 
        }
        DifficultyManager.Instance.SetDifficulty((DifficultyManager.Difficulty)difficultyIndex);
        SceneManager.LoadScene(1);
    }
}