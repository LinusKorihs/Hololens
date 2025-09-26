using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    public int points = 2;
    public GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"Score zone triggered by: {other.gameObject.name}, velocity: {rb.linearVelocity}");
            
            if (rb.linearVelocity.y < 0.5f)
            {
                Debug.Log($"SCORE! Adding {points} points!");
                if (gameManager != null)
                {
                    gameManager.AddScore(points);
                }
                else
                {
                    Debug.LogError("GameManager reference is null!");
                }
            }
            else
            {
                Debug.Log($"Ball velocity too high upward: {rb.linearVelocity.y}, no score");
            }
        }
        else
        {
            Debug.Log($"Object {other.gameObject.name} has no Rigidbody, ignored");
        }
    }
}
