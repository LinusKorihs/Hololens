using UnityEngine;
using UnityEngine.UI;

public class HighscoreEntryUI : MonoBehaviour
{
    [Header("UI Components")]
    public Text rankText;
    public Text scoreText;
    public Text dateText;
    public Image backgroundImage;
    
    [Header("Rank Colors")]
    public Color firstPlaceColor = Color.yellow;
    public Color secondPlaceColor = Color.white;
    public Color thirdPlaceColor = new Color(1f, 0.5f, 0f); // Orange
    public Color normalColor = Color.gray;

    public void SetHighscoreData(HighscoreEntry entry, int rank)
    {
        if (rankText != null)
        {
            rankText.text = GetRankString(rank);
            rankText.color = GetRankColor(rank);
        }

        if (scoreText != null)
        {
            scoreText.text = $"{entry.score} pts";
            scoreText.color = GetRankColor(rank);
        }
        
        if (dateText != null)
        {
            dateText.text = entry.date;
            dateText.color = GetRankColor(rank) * 0.8f; // Slightly dimmed
        }
        
        if (backgroundImage != null)
        {
            Color bgColor = GetRankColor(rank);
            bgColor.a = 0.1f; // Very transparent background
            backgroundImage.color = bgColor;
        }
    }
    
    string GetRankString(int rank)
    {
        switch (rank)
        {
            case 1: return "1st";
            case 2: return "2nd";
            case 3: return "3rd";
            default: return $"{rank}th";
        }
    }
    
    Color GetRankColor(int rank)
    {
        switch (rank)
        {
            case 1: return firstPlaceColor;
            case 2: return secondPlaceColor;
            case 3: return thirdPlaceColor;
            default: return normalColor;
        }
    }
}