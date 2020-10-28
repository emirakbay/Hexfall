using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text scoreText;

    public int score = 0;

    public static GameManager Instance;

    private void Update()
    {
        SetScore();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void SetScore()
    {
        scoreText.text = score.ToString();
    }

    public int GetScore()
    {
        return score;
    }

    public void UpdateScore(int scoreMultiplier)
    {
        score = score + 5 * scoreMultiplier;
    }
}
