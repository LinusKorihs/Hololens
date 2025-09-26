using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class HighscoreUIGenerator : MonoBehaviour
{
    [Header("Auto UI Generation")]
    [SerializeField] private bool generateUI = false;
    
    [Header("Generated UI References")]
    public Canvas mainCanvas;
    public GameObject gameOverPanel;
    public GameObject highscorePanel;
    public Text finalScoreText;
    public Text highscoreRankText;
    public Button submitScoreButton;
    public Button restartButton;
    public Transform highscoreContainer;
    
    [Header("UI Styling")]
    public Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public Color buttonColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color textColor = Color.white;
    public Font uiFont;

    void Start()
    {
        if (generateUI)
        {
            CreateCompleteUI();
            generateUI = false;
        }
    }

    [ContextMenu("Generate Highscore UI")]
    public void CreateCompleteUI()
    {
        Debug.Log("Starting UI Generation:");
        
        CreateMainCanvas();
        
        CreateGameOverPanel();
        
        CreateHighscorePanel();
        
        ConnectToGameManager();
        
        ConnectToHighscoreManager();
        
        Debug.Log("UI Generation Completed.");
    }

    void CreateMainCanvas()
    {
        if (mainCanvas == null)
        {
            GameObject canvasObj = new GameObject("MainCanvas");
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add GraphicRaycaster for UI interaction
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("📱 Main Canvas created");
        }
    }

    void CreateGameOverPanel()
    {
        if (gameOverPanel != null) DestroyImmediate(gameOverPanel);
        
        gameOverPanel = CreatePanel("GameOverPanel", mainCanvas.transform);
        gameOverPanel.GetComponent<Image>().color = panelColor;
        
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 500);
        panelRect.anchoredPosition = Vector2.zero;
        
        GameObject title = CreateText("Game Over!", gameOverPanel.transform);
        SetupText(title, 48, FontStyle.Bold, TextAnchor.MiddleCenter);
        PositionElement(title, new Vector2(0, 180), new Vector2(500, 60));
        
        GameObject finalScoreObj = CreateText("Final Score: 0", gameOverPanel.transform);
        finalScoreText = finalScoreObj.GetComponent<Text>();
        SetupText(finalScoreObj, 28, FontStyle.Normal, TextAnchor.MiddleCenter);
        PositionElement(finalScoreObj, new Vector2(0, 120), new Vector2(400, 40));
        
        GameObject rankObj = CreateText("", gameOverPanel.transform);
        highscoreRankText = rankObj.GetComponent<Text>();
        SetupText(rankObj, 22, FontStyle.Italic, TextAnchor.MiddleCenter);
        PositionElement(rankObj, new Vector2(0, 80), new Vector2(500, 30));
        
        GameObject submitObj = CreateButton("Submit Score", gameOverPanel.transform);
        submitScoreButton = submitObj.GetComponent<Button>();
        PositionElement(submitObj, new Vector2(0, 40), new Vector2(200, 50));
        
        GameObject highscoreContainerObj = CreateHighscoreContainer(gameOverPanel.transform);
        highscoreContainer = highscoreContainerObj.transform;
        PositionElement(highscoreContainerObj, new Vector2(0, -20), new Vector2(350, 120));
        
        GameObject restartObj = CreateButton("Play Again", gameOverPanel.transform);
        restartButton = restartObj.GetComponent<Button>();
        PositionElement(restartObj, new Vector2(0, -120), new Vector2(200, 50));
        
        gameOverPanel.SetActive(false);
    }

    void CreateHighscorePanel()
    {
        if (highscorePanel != null) DestroyImmediate(highscorePanel);
        
        // Main Panel
        highscorePanel = CreatePanel("HighscorePanel", mainCanvas.transform);
        highscorePanel.GetComponent<Image>().color = panelColor;
        
        // Panel size and position
        RectTransform panelRect = highscorePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(800, 600);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Title
        GameObject title = CreateText("HIGH SCORE!", highscorePanel.transform);
        SetupText(title, 36, FontStyle.Bold, TextAnchor.MiddleCenter);
        PositionElement(title, new Vector2(0, 250), new Vector2(700, 50));
        
        // Scroll View for highscores
        GameObject scrollObj = CreateScrollView("HighscoreScrollView", highscorePanel.transform);
        PositionElement(scrollObj, new Vector2(0, 50), new Vector2(700, 400));
        
        // Get the content transform for highscore entries
        ScrollRect scrollRect = scrollObj.GetComponent<ScrollRect>();
        highscoreContainer = scrollRect.content;
        
        // Setup content layout
        VerticalLayoutGroup layout = highscoreContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(20, 20, 20, 20);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        
        ContentSizeFitter fitter = highscoreContainer.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Close Button
        GameObject closeObj = CreateButton("Close", highscorePanel.transform);
        Button closeButton = closeObj.GetComponent<Button>();
        closeButton.onClick.AddListener(() => highscorePanel.SetActive(false));
        PositionElement(closeObj, new Vector2(0, -230), new Vector2(150, 50));
        
        highscorePanel.SetActive(false);
    }

    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = panelColor;
        
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        return panel;
    }

    GameObject CreateText(string text, Transform parent)
    {
        // Safe name generation to avoid substring errors
        string cleanText = text.Replace(" ", "").Replace("!", "").Replace("🎉", "").Replace("🏆", "");
        string safeName = "Text_" + (cleanText.Length > 0 ? cleanText.Substring(0, Mathf.Min(10, cleanText.Length)) : "Empty");
        
        GameObject textObj = new GameObject(safeName);
        textObj.transform.SetParent(parent, false);
        
        // Add RectTransform first (essential for UI)
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        
        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = uiFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.color = textColor;
        textComp.fontSize = 14;
        textComp.alignment = TextAnchor.MiddleCenter;
        
        return textObj;
    }

    GameObject CreateButton(string text, Transform parent)
    {
        GameObject buttonObj = new GameObject("Button_" + text.Replace(" ", ""));
        buttonObj.transform.SetParent(parent, false);
        
        // Add RectTransform first (essential for UI)
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = buttonColor;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // Button text
        GameObject textObj = CreateText(text, buttonObj.transform);
        Text textComp = textObj.GetComponent<Text>();
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.fontSize = 18;
        textComp.color = Color.white; // Ensure text is visible
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        return buttonObj;
    }

    GameObject CreateHighscoreContainer(Transform parent)
    {
        GameObject containerObj = new GameObject("HighscoreContainer");
        containerObj.transform.SetParent(parent, false);
        
        // Add RectTransform
        RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
        
        // Add VerticalLayoutGroup for automatic arrangement
        var layoutGroup = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.spacing = 5f;
        layoutGroup.childControlHeight = false;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = false;
        
        // Add ContentSizeFitter for dynamic sizing
        var sizeFitter = containerObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        
        // Add title text
        GameObject titleObj = CreateText("TOP SCORES", containerObj.transform);
        Text titleText = titleObj.GetComponent<Text>();
        titleText.fontSize = 16;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.yellow;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        return containerObj;
    }

    GameObject CreateInputField(string placeholder, Transform parent)
    {
        GameObject inputObj = new GameObject("InputField");
        inputObj.transform.SetParent(parent, false);
        
        Image image = inputObj.AddComponent<Image>();
        image.color = Color.white;
        
        InputField input = inputObj.AddComponent<InputField>();
        
        // Placeholder
        GameObject placeholderObj = CreateText(placeholder, inputObj.transform);
        Text placeholderText = placeholderObj.GetComponent<Text>();
        placeholderText.color = Color.gray;
        placeholderText.fontStyle = FontStyle.Italic;
        
        // Text
        GameObject textObj = CreateText("", inputObj.transform);
        Text inputText = textObj.GetComponent<Text>();
        inputText.color = Color.black;
        
        input.textComponent = inputText;
        input.placeholder = placeholderText;
        
        // Setup RectTransforms
        SetupInputFieldTransforms(placeholderObj, textObj);
        
        return inputObj;
    }

    GameObject CreateScrollView(string name, Transform parent)
    {
        GameObject scrollObj = new GameObject(name);
        scrollObj.transform.SetParent(parent, false);
        
        Image image = scrollObj.AddComponent<Image>();
        image.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
        
        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObj.transform, false);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        // Add RectTransform manually since UI objects need it
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        contentRect.anchoredPosition = Vector2.zero;
        
        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.vertical = true;
        scroll.horizontal = false;
        
        return scrollObj;
    }

    void SetupText(GameObject textObj, int fontSize, FontStyle style, TextAnchor alignment)
    {
        Text text = textObj.GetComponent<Text>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = textColor;
    }

    void PositionElement(GameObject obj, Vector2 position, Vector2 size)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    void SetupInputFieldTransforms(GameObject placeholder, GameObject text)
    {
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        placeholderRect.anchoredPosition = Vector2.zero;
        placeholderRect.offsetMin = new Vector2(10, 5);
        placeholderRect.offsetMax = new Vector2(-10, -5);
        
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
    }

    void ConnectToGameManager()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.gameOverPanel = gameOverPanel;
            gameManager.finalScoreText = finalScoreText;
            gameManager.highscoreRankText = highscoreRankText;
            gameManager.submitScoreButton = submitScoreButton;
            gameManager.highscoreContainer = highscoreContainer;
            
            // Connect restart button
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(gameManager.RestartGame);
            }
        }
    }

    void ConnectToHighscoreManager()
    {
        HighscoreManager highscoreManager = FindFirstObjectByType<HighscoreManager>();
        if (highscoreManager != null)
        {
            highscoreManager.highscorePanel = highscorePanel;
            highscoreManager.highscoreContainer = highscoreContainer;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(HighscoreUIGenerator))]
    public class HighscoreUIGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GUILayout.Space(20);
            
            HighscoreUIGenerator generator = (HighscoreUIGenerator)target;
            
            if (GUILayout.Button("Generate Complete UI", GUILayout.Height(40)))
            {
                generator.CreateCompleteUI();
            }
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Game Over Panel"))
            {
                if (generator.gameOverPanel != null)
                {
                    DestroyImmediate(generator.gameOverPanel);
                    generator.gameOverPanel = null;
                }
            }
            
            if (GUILayout.Button("Clear Highscore Panel"))
            {
                if (generator.highscorePanel != null)
                {
                    DestroyImmediate(generator.highscorePanel);
                    generator.highscorePanel = null;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
#endif
}