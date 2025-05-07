using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    private static DifficultyManager _instance;

    public static DifficultyManager Instance => _instance;

    [SerializeField] private Difficulty _currentDifficulty = Difficulty.Easy;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this); 
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public Difficulty CurrentDifficulty => _currentDifficulty;

    public void SetDifficulty(Difficulty d)
    {
        _currentDifficulty = d;
    }

    public float GetObstacleProbability()
    {
        switch (_currentDifficulty)
        {
            case Difficulty.Easy: return 0.3f;
            case Difficulty.Medium: return 0.2f;
            case Difficulty.Hard: return 0.1f;
            default: return 0.2f;
        }
    }

    public float GetWidenProbability()
    {
        switch (_currentDifficulty)
        {
            case Difficulty.Easy: return 0f;
            case Difficulty.Medium: return 0.3f;
            case Difficulty.Hard: return 0.5f;
            default: return 0f;
        }
    }

    public int GetMinHostileCells()
    {
        switch (_currentDifficulty)
        {
            case Difficulty.Easy: return 0;
            case Difficulty.Medium: return 1;
            case Difficulty.Hard: return 2;
            default: return 0;
        }
    }

    public int GetMinLargePathCells()
    {
        switch (_currentDifficulty)
        {
            case Difficulty.Easy: return 0;
            case Difficulty.Medium: return 2;
            case Difficulty.Hard: return 2;
            default: return 0;
        }
    }

    public int GetNumWaypoints()
    {
        switch (_currentDifficulty)
        {
            case Difficulty.Easy: return 0;
            case Difficulty.Medium: return 1;
            case Difficulty.Hard: return 2;
            default: return 0;
        }
    }
}