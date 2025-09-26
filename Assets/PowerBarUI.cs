using UnityEngine;
using UnityEngine.UI;

public class PowerBarUI : MonoBehaviour
{
    [Header("HUD Positioning")]
    public Transform playerCamera;
    public Vector3 hudOffset = new Vector3(0, 0.2f, 1.5f);
    public bool followPlayer = true;
    public float followSpeed = 5f;
    public bool alwaysOnTop = true;

    [Header("UI Components")]
    public Slider powerSlider;
    public Image fillImage;
    public Text powerText;
    public Text instructionText;
    
    [Header("Visual Settings")]
    public Color lowPowerColor = Color.red;
    public Color midPowerColor = Color.yellow;
    public Color highPowerColor = Color.green;
    public bool smoothTransition = true;
    public float transitionSpeed = 5f;
    
    [Header("Animation")]
    public bool pulseEffect = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.2f;
    
    private float targetValue = 0f;
    private Color targetColor = Color.white;
    private Vector3 originalScale;
    private bool isVisible = false;
    
    void Start()
    {
        InitializeComponents();
        originalScale = transform.localScale;
        SetupCanvasForHUD();
        if (playerCamera == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null) playerCamera = mainCam.transform;
        }
        SetVisible(false);
    }
    
    void SetupCanvasForHUD()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
            
        if (canvas != null && alwaysOnTop)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 1000;
            canvas.planeDistance = 0.1f;
            canvas.sortingLayerName = "UI";
            canvas.transform.localScale = Vector3.one * 0.001f;
        }
        else if (canvas != null) canvas.renderMode = RenderMode.WorldSpace;
    }
    
    void InitializeComponents()
    {
        if (powerSlider == null) powerSlider = GetComponentInChildren<Slider>();
            
        if (fillImage == null && powerSlider != null) fillImage = powerSlider.fillRect.GetComponent<Image>();
            
        if (powerText == null) powerText = GetComponentInChildren<Text>();
            
        if (instructionText == null)
        {
            Text[] texts = GetComponentsInChildren<Text>();
            if (texts.Length > 1) instructionText = texts[1];
        }
        if (powerSlider != null)
        {
            powerSlider.minValue = 0;
            powerSlider.maxValue = 100;
            powerSlider.value = 0;
            powerSlider.direction = Slider.Direction.LeftToRight;
        }

        if (instructionText != null) instructionText.text = "Click to charge power!";
    }
    
    void Update()
    {
        // HUD Position and Rotation update
        if (followPlayer && playerCamera != null && isVisible) UpdateHUDPosition();
        
        if (!isVisible) return;
        
        if (smoothTransition && powerSlider != null) powerSlider.value = Mathf.Lerp(powerSlider.value, targetValue, transitionSpeed * Time.deltaTime);

        if (fillImage != null) fillImage.color = Color.Lerp(fillImage.color, targetColor, transitionSpeed * Time.deltaTime);
        
        if (pulseEffect && targetValue > 0)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            transform.localScale = originalScale * pulse;
        }
        else transform.localScale = Vector3.Lerp(transform.localScale, originalScale, transitionSpeed * Time.deltaTime);
    }
    
    void UpdateHUDPosition()
    {
        Vector3 targetPosition = playerCamera.position + playerCamera.TransformDirection(hudOffset);
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        Vector3 directionToPlayer = playerCamera.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        gameObject.SetActive(visible);
        
        if (visible && instructionText != null)
        {
            instructionText.text = "Click to charge power!";
        }
    }
    
    public void UpdatePower(float currentPower, float maxPower)
    {
        if (!isVisible) return;
        
        float powerPercent = currentPower / maxPower;
        targetValue = powerPercent * 100f; // Convert to slider scale
        
        if (powerPercent < 0.33f) targetColor = lowPowerColor;
        else if (powerPercent < 0.66f) targetColor = midPowerColor;
        else targetColor = highPowerColor;
        
        if (!smoothTransition && powerSlider != null)
        {
            powerSlider.value = powerPercent * 100f;
            if (fillImage != null) fillImage.color = targetColor;
        }
        
        if (powerText != null)
        {
            powerText.text = $"{Mathf.RoundToInt(currentPower)}/{Mathf.RoundToInt(maxPower)}";
        }
        
        if (instructionText != null)
        {
            if (powerPercent < 0.2f) instructionText.text = "Low Power";
            else if (powerPercent < 0.5f) instructionText.text = "Medium Power";
            else if (powerPercent < 0.8f) instructionText.text = "High Power";
            else instructionText.text = "MAX POWER!";
        }
    }
    
    public void SetMaxValue(float maxValue)
    {
        if (powerSlider != null) powerSlider.maxValue = maxValue;
    }
    
    public void SetSliderDirection(Slider.Direction direction)
    {
        if (powerSlider != null) powerSlider.direction = direction;
    }
    
    public void SetPlayerCamera(Transform camera)
    {
        playerCamera = camera;
    }
    
    public void SetHUDOffset(Vector3 offset)
    {
        hudOffset = offset;
    }
    
    public void SetFollowPlayer(bool follow)
    {
        followPlayer = follow;
    }
    
    public void ShowThrowInstruction()
    {
        if (instructionText != null) instructionText.text = "Click to throw!";
    }
    
    public void PlayThrowEffect()
    {
        if (instructionText != null) instructionText.text = "THROWN!";
    }
}