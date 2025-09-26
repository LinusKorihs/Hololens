using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public Text scoreText;
    public Text timerText;
    public float gameTime = 30f;
    private float timer;
    private bool gameActive = false;
    public GameObject ballPrefab;
    public Transform ballSpawnPoint;
    public int ballsPerRound = 3;
    public List<GameObject> spawnedBalls = new();
    private BallReset ballReset;

    [Header("Highscore System")]
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public Text highscoreRankText;
    public UnityEngine.UI.Button submitScoreButton;
    public Transform highscoreContainer;
    private float finalTime;

    void Start()
    {
        FindObjects();
        timer = gameTime;
        UpdateUI();

        // Setup UI buttons
        if (submitScoreButton != null)
        {
            submitScoreButton.onClick.AddListener(SubmitHighscore);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (gameActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                EndGame();
            }
            UpdateUI();
        }

        if (!gameActive && spawnedBalls.Count > 0)
        {
            // Deactivate and remove all balls when game is not active
            foreach (GameObject ball in spawnedBalls)
            {
                if (ball != null && ball.GetComponent<NearInteractionGrabbable>() != null && ball.GetComponent<NearInteractionGrabbable>().enabled == true)
                {
                    NearInteractionGrabbable grabbable = ball.GetComponent<NearInteractionGrabbable>();
                    if (grabbable != null) grabbable.enabled = false;

                    ObjectManipulator manipulator = ball.GetComponent<ObjectManipulator>();
                    if (manipulator != null)
                    {
                        manipulator.enabled = false;
                        Destroy(ball);
                        spawnedBalls.Remove(ball);
                    }
                }

                if (ball.GetComponent<Rigidbody>().linearVelocity == Vector3.zero)
                {
                    Destroy(ball);
                    spawnedBalls.Remove(ball);
                }
            }
        }
    }

    public void StartGame()
    {
        if (gameActive) return;
        else Debug.Log("Game Started");

        score = 0;
        timer = gameTime;
        gameActive = true;
        UpdateUI();
        SpawnBalls();
    }

    void SpawnBalls()
    {
        if (ballReset == null) ballReset = FindFirstObjectByType<BallReset>();
        else if (ballReset != null) ballReset.enabled = false;

        if (ballPrefab == null || ballSpawnPoint == null)
        {
            Debug.LogError("BallPrefab or BallSpawnPoint is missing!");
            return;
        }

        float x = -2;
        for (int i = 0; i < ballsPerRound; i++)
        {
            Vector3 offset = new Vector3(x * 0.05f, 0, i * 0.3f);
            GameObject ball = Instantiate(ballPrefab, ballSpawnPoint.position + offset, Quaternion.identity);
            if (ball != null)
            {
                spawnedBalls.Add(ball);
                Debug.Log($"Ball {i + 1} spawned successfully");
            }
            x += 1.5f;
        }

        // Safe enable ballReset only if it exists
        if (ballReset != null) ballReset.enabled = true;
    }

    public void AddScore(int amount)
    {
        if (!gameActive) return;
        score += amount;
        Debug.Log("Score: " + score);
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        timerText.text = "Time: " + Mathf.Ceil(timer).ToString();
    }

    void EndGame()
    {
        Debug.Log("Game Over! Final Score: " + score);
        gameActive = false;
        finalTime = gameTime - timer; // Actual play time

        foreach (GameObject ball in spawnedBalls)
        {
            if (ball != null)
            {
                Destroy(ball);
            }
        }
        spawnedBalls.Clear();
        ShowGameOverScreen();
    }

    void ShowGameOverScreen()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {score}";
        }

        // Check if it's a new highscore
        if (HighscoreManager.Instance != null)
        {
            bool isNewHighscore = HighscoreManager.Instance.IsNewHighscore(score);
            int rank = HighscoreManager.Instance.GetHighscoreRank(score);

            if (highscoreRankText != null)
            {
                if (isNewHighscore && rank > 0)
                {
                    highscoreRankText.text = $"NEW HIGHSCORE! Rank #{rank}";
                    highscoreRankText.color = Color.yellow;
                }
                else if (rank > 0)
                {
                    highscoreRankText.text = $"Great! You reached rank #{rank}";
                    highscoreRankText.color = Color.green;
                }
                else
                {
                    highscoreRankText.text = "Good try! Keep practicing!";
                    highscoreRankText.color = Color.white;
                }
            }

            if (submitScoreButton != null)
            {
                submitScoreButton.interactable = isNewHighscore;
            }

            // Display current highscores ONLY if it's a new highscore
            if (isNewHighscore)
            {
                DisplayHighscores();
            }
        }
    }

    public void SubmitHighscore()
    {
        if (HighscoreManager.Instance == null) return;

        HighscoreManager.Instance.AddHighscore(score, finalTime);

        // Disable submit button after submission
        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = false;
            submitScoreButton.GetComponentInChildren<Text>().text = "Submitted!";
        }

        Debug.Log($"Highscore submitted: {score} points");

        // Refresh highscore display after submission
        DisplayHighscores();
    }

    void DisplayHighscores()
    {
        if (HighscoreManager.Instance == null || highscoreContainer == null) return;

        ClearHighscoreDisplay();

        // Get top 5 highscores for display
        var highscores = HighscoreManager.Instance.GetHighscores();
        int displayCount = Mathf.Min(5, highscores.Count);

        for (int i = 0; i < displayCount; i++)
        {
            var entry = highscores[i];
            CreateHighscoreDisplayEntry(entry, i + 1);
        }
    }

    void ClearHighscoreDisplay()
    {
        if (highscoreContainer == null) return;

        for (int i = highscoreContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = highscoreContainer.GetChild(i);
            if (child.name.StartsWith("HighscoreEntry_"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    void CreateHighscoreDisplayEntry(HighscoreEntry entry, int rank)
    {
        GameObject entryObj = new GameObject($"HighscoreEntry_{rank}");
        entryObj.transform.SetParent(highscoreContainer, false);

        // Add RectTransform
        RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(350, 25);

        // Add Text component
        UnityEngine.UI.Text text = entryObj.AddComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleLeft;

        // Set rank and score text
        string rankEmoji = GetRankEmoji(rank);
        text.text = $"{rankEmoji} {entry.score} pts - {entry.date}";

        // Color based on rank
        if (rank == 1) text.color = Color.yellow;
        else if (rank == 2) text.color = Color.white;
        else if (rank == 3) text.color = new Color(1f, 0.5f, 0f);
        else text.color = Color.gray;
    }

    string GetRankEmoji(int rank)
    {
        switch (rank)
        {
            case 1: return "NUMBER 1";
            case 2: return "Nummero 2";
            case 3: return "DIE NUMMER 3";
            default: return $"{rank}.";
        }
    }

    public void RestartGame()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (submitScoreButton != null)
        {
            submitScoreButton.interactable = true;
            submitScoreButton.GetComponentInChildren<Text>().text = "Submit Score";
        }

        ClearHighscoreDisplay();

        if (gameActive) EndGame();

        StartGame();
    }

    public void FindObjects()
    {
        if (scoreText == null) scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        if (timerText == null) timerText = GameObject.Find("TimerText").GetComponent<Text>();
        if (ballSpawnPoint == null) ballSpawnPoint = GameObject.Find("BallSpawnPoint").transform;
        if (ballPrefab == null) ballPrefab = Resources.Load<GameObject>("BallPrefab");
    }

    public void QuitGame()
    {
        #if UNITY_STANDALONE
            Application.Quit();
        #endif
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
