using UnityEngine;
using UnityEngine.UI;

public class ButtonDebugger : MonoBehaviour // AI generated script to help find UI buttons and canvases in the scene
{
    [Header("Debug Settings")]
    public bool logAllButtons = true;
    public bool logAllCanvases = true;
    public bool logAllGameObjects = false;
    
    void Start()
    {
        if (logAllButtons)
        {
            LogAllButtons();
        }
        
        if (logAllCanvases)
        {
            LogAllCanvases();
        }
        
        if (logAllGameObjects)
        {
            LogGameObjectsWithUI();
        }
    }
    
    void LogAllButtons()
    {
        Debug.Log("=== BUTTON ANALYSIS START ===");
        
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"Insgesamt gefundene Buttons: {allButtons.Length}");
        
        for (int i = 0; i < allButtons.Length; i++)
        {
            Button button = allButtons[i];
            Text buttonText = button.GetComponentInChildren<Text>();
            string text = buttonText != null ? buttonText.text : "KEIN TEXT";
            
            Debug.Log($"Button {i + 1}:");
            Debug.Log($"  Name: {button.name}");
            Debug.Log($"  Text: {text}");
            Debug.Log($"  Active: {button.gameObject.activeInHierarchy}");
            Debug.Log($"  Parent: {(button.transform.parent != null ? button.transform.parent.name : "NULL")}");
            Debug.Log($"  Position: {button.transform.position}");
            Debug.Log($"  Canvas: {GetCanvasName(button)}");
            
            if (button.name.ToLower().Contains("start") || (buttonText != null && buttonText.text.ToLower().Contains("start")))
            {
                Debug.Log($"  >>> POTENTIELLER START GAME BUTTON <<<");
            }
            
            Debug.Log("  ---");
        }
        
        Debug.Log("=== BUTTON ANALYSIS END ===");
    }
    
    void LogAllCanvases()
    {
        Debug.Log("=== CANVAS ANALYSIS START ===");
        
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Insgesamt gefundene Canvases: {allCanvases.Length}");
        
        for (int i = 0; i < allCanvases.Length; i++)
        {
            Canvas canvas = allCanvases[i];
            Button[] canvasButtons = canvas.GetComponentsInChildren<Button>(true);
            
            Debug.Log($"Canvas {i + 1}:");
            Debug.Log($"  Name: {canvas.name}");
            Debug.Log($"  Active: {canvas.gameObject.activeInHierarchy}");
            Debug.Log($"  RenderMode: {canvas.renderMode}");
            Debug.Log($"  Buttons in Canvas: {canvasButtons.Length}");
            
            foreach (Button btn in canvasButtons)
            {
                Text txt = btn.GetComponentInChildren<Text>();
                Debug.Log($"    Button: {btn.name} | Text: {(txt != null ? txt.text : "kein Text")}");
            }
            
            Debug.Log("  ---");
        }
        
        Debug.Log("=== CANVAS ANALYSIS END ===");
    }
    
    void LogGameObjectsWithUI()
    {
        Debug.Log("=== UI GAMEOBJECTS ANALYSIS START ===");
        
        // Suche nach GameObjects mit UI-verwandten Namen
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allObjects)
        {
            string name = obj.name.ToLower();
            
            if (name.Contains("button") || name.Contains("ui") || name.Contains("canvas") || 
                name.Contains("start") || name.Contains("game") || name.Contains("menu"))
            {
                Debug.Log($"UI GameObject gefunden: {obj.name} | Active: {obj.activeInHierarchy} | Parent: {(obj.transform.parent != null ? obj.transform.parent.name : "NULL")}");
            }
        }
        
        Debug.Log("=== UI GAMEOBJECTS ANALYSIS END ===");
    }
    
    string GetCanvasName(Button button)
    {
        Canvas canvas = button.GetComponentInParent<Canvas>();
        return canvas != null ? canvas.name : "KEIN CANVAS";
    }
    
    // Manuelle Suche für Tests
    [ContextMenu("Manual Button Search")]
    public void ManualButtonSearch()
    {
        Debug.Log("Manuelle Button-Suche gestartet...");
        LogAllButtons();
    }
    
    [ContextMenu("Test GameManager Reference")]
    public void TestGameManagerReference()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            Debug.Log($"GameManager gefunden: {gm.name}");
            
            if (gm.submitScoreButton != null)
            {
                Debug.Log($"GameManager.submitScoreButton: {gm.submitScoreButton.name}");
            }
            else
            {
                Debug.Log("GameManager.submitScoreButton ist NULL");
            }
        }
        else
        {
            Debug.Log("GameManager NICHT gefunden");
        }
    }
}