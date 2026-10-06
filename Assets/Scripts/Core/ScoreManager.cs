using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private int startingScore = 0;

    public int CurrentScore { get; private set; }

    public System.Action<int> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentScore = startingScore;
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;

        OnScoreChanged?.Invoke(CurrentScore);

        Debug.Log("Score: " + CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = startingScore;

        OnScoreChanged?.Invoke(CurrentScore);
    }
}