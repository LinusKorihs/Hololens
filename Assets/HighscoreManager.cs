using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HighscoreEntry
{
    public int score;
    public string date;
    public float gameTime;

    public HighscoreEntry(int playerScore, float time)
    {
        score = playerScore;
        gameTime = time;
        date = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
    }
}

public class HighscoreManager : MonoBehaviour
{
    [Header("Highscore Settings")]
    public int maxHighscores = 10;
    public string defaultPlayerName = "Anonymous";
    
    [Header("UI References")]
    public GameObject highscorePanel;
    public Transform highscoreContainer;
    public GameObject highscoreEntryPrefab;
    
    private List<HighscoreEntry> highscores = new List<HighscoreEntry>();
    private const string HIGHSCORE_KEY = "VR_Basketball_Highscores";
    
    public static HighscoreManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighscores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (highscorePanel != null)
        {
            highscorePanel.SetActive(false);
        }
    }

    public bool IsNewHighscore(int score)
    {
        if (highscores.Count < maxHighscores)
        {
            return true;
        }
        
        return score > highscores[highscores.Count - 1].score;
    }

    public int GetHighscoreRank(int score)
    {
        for (int i = 0; i < highscores.Count; i++)
        {
            if (score > highscores[i].score)
            {
                return i + 1; // Ranks are 1-based
            }
        }
        
        if (highscores.Count < maxHighscores)
        {
            return highscores.Count + 1;
        }
        
        return -1; // Not in top scores
    }

    public void AddHighscore(int score, float gameTime)
    {
        HighscoreEntry newEntry = new HighscoreEntry(score, gameTime);
        highscores.Add(newEntry);
        
        // Sort by score (descending)
        highscores.Sort((a, b) => b.score.CompareTo(a.score));
        
        // Keep only top scores
        if (highscores.Count > maxHighscores)
        {
            highscores.RemoveRange(maxHighscores, highscores.Count - maxHighscores);
        }
        
        SaveHighscores();
        Debug.Log($"New highscore added: {score} points on {newEntry.date}");
    }

    public List<HighscoreEntry> GetHighscores()
    {
        return new List<HighscoreEntry>(highscores);
    }

    public void ShowHighscores()
    {
        if (highscorePanel != null)
        {
            highscorePanel.SetActive(true);
            UpdateHighscoreDisplay();
        }
    }

    public void HideHighscores()
    {
        if (highscorePanel != null)
        {
            highscorePanel.SetActive(false);
        }
    }

    void UpdateHighscoreDisplay()
    {
        if (highscoreContainer == null) return;

        // Clear existing entries
        foreach (Transform child in highscoreContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < highscores.Count; i++)
        {
            CreateHighscoreEntry(highscores[i], i + 1);
        }
    }

    void CreateHighscoreEntry(HighscoreEntry entry, int rank)
    {
        GameObject entryObj;
        
        if (highscoreEntryPrefab != null)
        {
            entryObj = Instantiate(highscoreEntryPrefab, highscoreContainer);
            
            // Try to use custom HighscoreEntryUI component (got reworked)
            var entryUI = entryObj.GetComponent<HighscoreEntryUI>();
            if (entryUI != null)
            {
                entryUI.SetHighscoreData(entry, rank);
                return;
            }
        }
        else
        {
            // Fallback: Create simple text entry
            entryObj = new GameObject($"HighscoreEntry_{rank}");
            entryObj.transform.SetParent(highscoreContainer);
            
            var text = entryObj.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            
            var rectTransform = entryObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(400, 30);
        }

        var textComponent = entryObj.GetComponent<UnityEngine.UI.Text>();
        if (textComponent != null)
        {
            string rankText = GetRankString(rank);
            textComponent.text = $"{rankText} {entry.score} pts - {entry.date}";
            
            // Highlight top 3
            if (rank == 1) textComponent.color = Color.yellow;
            else if (rank == 2) textComponent.color = Color.white;
            else if (rank == 3) textComponent.color = new Color(1f, 0.5f, 0f); // Orange
            else textComponent.color = Color.gray;
        }
    }

    string GetRankString(int rank)
    {
        switch (rank)
        {
            case 1: return "🥇";
            case 2: return "🥈";
            case 3: return "🥉";
            default: return $"{rank}.";
        }
    }

    void SaveHighscores()
    {
        try
        {
            string json = JsonUtility.ToJson(new HighscoreList { scores = highscores });
            PlayerPrefs.SetString(HIGHSCORE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log("Highscores saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save highscores: {e.Message}");
        }
    }

    void LoadHighscores()
    {
        try
        {
            if (PlayerPrefs.HasKey(HIGHSCORE_KEY))
            {
                string json = PlayerPrefs.GetString(HIGHSCORE_KEY);
                HighscoreList loadedScores = JsonUtility.FromJson<HighscoreList>(json);
                highscores = loadedScores.scores ?? new List<HighscoreEntry>();
                Debug.Log($"Loaded {highscores.Count} highscores");
            }
            else
            {
                Debug.Log("No saved highscores found");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load highscores: {e.Message}");
            highscores = new List<HighscoreEntry>();
        }
    }

    public void ClearHighscores()
    {
        highscores.Clear();
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
        PlayerPrefs.Save();
        UpdateHighscoreDisplay();
        Debug.Log("All highscores cleared");
    }

    // Helper class for JSON serialization
    [System.Serializable]
    private class HighscoreList
    {
        public List<HighscoreEntry> scores;
    }
}