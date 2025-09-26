using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class HoloLensOptimizedPlaceScript : MonoBehaviour // Optimized for HoloLens because forgot it before
{
    [Header("HoloLens Optimized Settings")]
    public bool useStabilization = true;
    public float stabilizationThreshold = 0.01f;
    public float positionLerpSpeed = 8f;
    public float rotationLerpSpeed = 5f;
    
    [Header("Placement Settings")]
    public Vector3 placingScale = new Vector3(0.1f, 0.1f, 0.1f);
    public bool autoConfirmPlacement = false;
    public float autoConfirmDelay = 3f;
    
    [Header("Visual Feedback")]
    public bool showPlacementGrid = true;
    public Material placementMaterial;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    
    private TapToPlace tapToPlace;
    private Vector3 originalScale;
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;
    private float positionStabilityTimer = 0f;
    private bool isPlacing = false;
    private GameObject stabilizationIndicator;
    private Renderer indicatorRenderer;

    void Start()
    {
        tapToPlace = GetComponent<TapToPlace>();
        originalScale = transform.localScale;
        
        SetupHoloLensOptimizedTapToPlace();
        CreateStabilizationIndicator();
        EnablePlacing(false);
    }
    
    void SetupHoloLensOptimizedTapToPlace()
    {
        if (tapToPlace == null) return;
        
        tapToPlace.KeepOrientationVertical = true;
        tapToPlace.UseDefaultSurfaceNormalOffset = false;
        tapToPlace.SurfaceNormalOffset = 0.4f;
        tapToPlace.DefaultPlacementDistance = 2.0f;
        tapToPlace.MaxRaycastDistance = 20f;
        tapToPlace.OnPlacingStarted.AddListener(OnPlacingStarted);
        tapToPlace.OnPlacingStopped.AddListener(OnPlacingStopped);
    }
    
    void CreateStabilizationIndicator()
    {
        stabilizationIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stabilizationIndicator.name = "StabilizationIndicator";
        stabilizationIndicator.transform.SetParent(transform);
        stabilizationIndicator.transform.localScale = Vector3.one * 0.05f;
        stabilizationIndicator.transform.localPosition = Vector3.up * 0.2f;
        
        indicatorRenderer = stabilizationIndicator.GetComponent<Renderer>();
        if (placementMaterial != null)
        {
            indicatorRenderer.material = placementMaterial;
        }
        
        Destroy(stabilizationIndicator.GetComponent<Collider>());
        
        stabilizationIndicator.SetActive(false);
    }
    
    void Update()
    {
        if (isPlacing && useStabilization)
        {
            PerformStabilization();
            UpdateVisualFeedback();
        }
    }

    public void EnablePlacing(bool enable)
    {
        if (tapToPlace == null) return;
        
        tapToPlace.enabled = enable;
        isPlacing = enable;
        transform.localScale = enable ? placingScale : originalScale;
        
        if (stabilizationIndicator != null)
        {
            stabilizationIndicator.SetActive(enable);
        }

        Debug.Log($"Placement {(enable ? "enabled" : "disabled")}");
    }

    public void DisablePlacing()
    {
        EnablePlacing(false);
        if (tapToPlace != null)
        {
            tapToPlace.enabled = false;
        }
        transform.localScale = originalScale;
        isPlacing = false;
    }
    
    void PerformStabilization()
    {
        if (tapToPlace == null || !tapToPlace.IsBeingPlaced) return;
        
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        
        float positionDelta = Vector3.Distance(currentPosition, lastValidPosition);
        
        if (positionDelta < stabilizationThreshold)
        {
            positionStabilityTimer += Time.deltaTime;
            
            if (positionStabilityTimer > 0.1f)
            {
                transform.position = Vector3.Lerp(transform.position, lastValidPosition, positionLerpSpeed * Time.deltaTime);
            }
        }
        else
        {
            positionStabilityTimer = 0f;
            lastValidPosition = currentPosition;
        }
        
        // Rotation stabilisation (  vertical only)
        Vector3 eulerAngles = currentRotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(0, eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
        
        lastValidRotation = transform.rotation;
    }
    
    void UpdateVisualFeedback()
    {
        if (indicatorRenderer == null) return;
        
        Color targetColor = positionStabilityTimer > 0.5f ? validPlacementColor : invalidPlacementColor;
        indicatorRenderer.material.color = Color.Lerp(indicatorRenderer.material.color, targetColor, 3f * Time.deltaTime);
        float pulse = Mathf.Sin(Time.time * 4f) * 0.2f + 1f;
        stabilizationIndicator.transform.localScale = Vector3.one * 0.05f * pulse;
    }

    // Event Handler
    void OnPlacingStarted()
    {
        isPlacing = true;
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;
        positionStabilityTimer = 0f;

        Debug.Log("Placement started");
    }
    
    void OnPlacingUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            cameraForward.y = 0;
            if (cameraForward != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(cameraForward);
            }
        }
    }
    
    void OnPlacingStopped()
    {
        isPlacing = false;
        transform.localScale = originalScale;
        
        if (stabilizationIndicator != null)
        {
            stabilizationIndicator.SetActive(false);
        }
    }
    
    [ContextMenu("Force Place at Camera")]
    public void ForcePlaceAtCamera()
    {
        if (Camera.main == null) return;
        
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        
        Vector3 targetPosition = cameraPos + cameraForward * 2f;
        targetPosition.y = cameraPos.y;
        
        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(cameraForward);
        
        DisablePlacing();
    }
    
    public void StartPlacement()
    {
        EnablePlacing(true);
    }
    
    public void StopPlacement()
    {
        DisablePlacing();
    }
    
    public void ResetToOriginalPosition()
    {
        if (Camera.main != null)
        {
            ForcePlaceAtCamera();
        }
    }
}