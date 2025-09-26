using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;

public class PowerThrowBall : MonoBehaviour, IMixedRealityPointerHandler
{
    [Header("Power Settings")]
    public float maxForce = 15f;
    public float chargeSpeed = 10f;
    
    [Header("Auto-Charging Settings")]
    public bool autoStartCharging = true;
    public float autoChargingDelay = 2f;
    
    [Header("Transform Settings")]
    public Transform cameraTransform;
    public Vector3 holdOffset = new Vector3(0, -0.2f, 1f);
    public Vector3 throwDirection = new Vector3(0, 0.5f, 1f);

    [Header("UI Elements")]
    public PowerBarUI powerBarUI;
    public Slider powerBar;
    public GameObject powerBarContainer;
    public Text powerText;
    public Color lowPowerColor = Color.red;
    public Color midPowerColor = Color.yellow;
    public Color highPowerColor = Color.green;

    [Header("MRTK Components")]
    public bool autoSetupMRTK = true;

    private Rigidbody rb;
    private bool isHeld = false;
    private bool isCharging = false;
    private bool chargingUp = true;
    private float currentForce = 0f;
    private Image powerBarFill;
    private float holdTimer = 0f;
    private bool hasStartedAutoCharging = false;
    private NearInteractionGrabbable grabbable;
    private ObjectManipulator manipulator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupMRTKComponents();
        InitializePowerBar();
    }

    void SetupMRTKComponents()
    {
        if (!autoSetupMRTK) return;

        grabbable = GetComponent<NearInteractionGrabbable>();
        if (grabbable == null) grabbable = gameObject.AddComponent<NearInteractionGrabbable>();

        manipulator = GetComponent<ObjectManipulator>();
        if (manipulator == null) manipulator = gameObject.AddComponent<ObjectManipulator>();

        if (manipulator != null)
        {
            manipulator.AllowFarManipulation = false;
            manipulator.enabled = false;
        }

        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.radius = 0.15f; // Etwas größer für einfacheres Klicken
        }
        
        if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void InitializePowerBar()
    {
        if (powerBarUI == null)
        {
            powerBarUI = FindFirstObjectByType<PowerBarUI>();
            
            if (powerBarUI == null)
            {
                Canvas[] allCanvas = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (Canvas canvas in allCanvas)
                {
                    PowerBarUI pui = canvas.GetComponentInChildren<PowerBarUI>(true);
                    if (pui != null)
                    {
                        powerBarUI = pui;
                        if (!canvas.gameObject.activeInHierarchy) canvas.gameObject.SetActive(true);
                        break;
                    }
                }
            }
            
            if (powerBarUI == null)
            {
                GameObject powerBarObj = GameObject.Find("PowerBarUI");
                if (powerBarObj == null) powerBarObj = GameObject.Find("PowerBar");
                if (powerBarObj == null) powerBarObj = GameObject.Find("HUD");
                if (powerBarObj == null) powerBarObj = GameObject.Find("UI");
                
                if (powerBarObj != null)
                {
                    powerBarUI = powerBarObj.GetComponent<PowerBarUI>();
                    if (powerBarUI == null) powerBarUI = powerBarObj.GetComponentInChildren<PowerBarUI>(true);
                }
            }
            
            if (powerBarUI == null)
            {
                PowerBarUI[] allPowerBars = Resources.FindObjectsOfTypeAll<PowerBarUI>();
                for (int i = 0; i < allPowerBars.Length; i++)
                {
                    PowerBarUI pb = allPowerBars[i];
                    if (powerBarUI == null && pb != null)
                    {
                        powerBarUI = pb;
                        if (!pb.gameObject.activeInHierarchy) pb.gameObject.SetActive(true);
                    }
                }
            }
        }
        
        if (powerBarUI == null && powerBar == null) powerBar = FindFirstObjectByType<Slider>();
        
        if (powerBarContainer == null)
        {
            GameObject powerBarObj = GameObject.Find("PowerBar");
            if (powerBarObj != null) powerBarContainer = powerBarObj;
        }
        
        if (powerText == null)
        {
            GameObject textObj = GameObject.Find("PowerText");
            if (textObj != null) powerText = textObj.GetComponent<Text>();
        }

        if (powerBar != null && powerBarUI == null)
        {
            powerBarFill = powerBar.fillRect.GetComponent<Image>();
            powerBar.minValue = 0;
            powerBar.maxValue = maxForce;
            powerBar.value = 0;
        }

        if (powerBarUI != null)
        {
            powerBarUI.SetMaxValue(maxForce);
            powerBarUI.SetPlayerCamera(cameraTransform);
            powerBarUI.SetVisible(false);
        }
        else if (powerBarContainer != null)
        {
            powerBarContainer.SetActive(false);
        }
    }

    void Update()
    {
        if (isHeld && !isCharging)
        {
            rb.isKinematic = true;
            transform.position = cameraTransform.position + cameraTransform.TransformDirection(holdOffset);
            
            // Auto-Charging Timer
            if (autoStartCharging && !hasStartedAutoCharging)
            {
                holdTimer += Time.deltaTime;
                if (powerBarUI != null && holdTimer > autoChargingDelay * 0.5f)
                {
                    powerBarUI.SetVisible(true);
                    float timerProgress = Mathf.Clamp01((holdTimer - autoChargingDelay * 0.5f) / (autoChargingDelay * 0.5f));
                    powerBarUI.UpdatePower(timerProgress * 2f, maxForce);
                }
                
                if (holdTimer >= autoChargingDelay)
                {
                    StartCharging();
                    hasStartedAutoCharging = true;
                }
            }
        }

        if (isCharging)
        {
            if (chargingUp)
            {
                currentForce += chargeSpeed * Time.deltaTime;
                if (currentForce >= maxForce) chargingUp = false;
            }
            else
            {
                currentForce -= chargeSpeed * Time.deltaTime;
                if (currentForce <= 0) chargingUp = true;
            }

            UpdatePowerBarUI();
        }
        
        if (isHeld && !isCharging && !hasStartedAutoCharging)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                StartCharging();
                hasStartedAutoCharging = true;
            }
        }
    }

    void StartCharging()
    {
        isCharging = true;
        currentForce = 0f;
        chargingUp = true;
        ShowPowerBar(true);
    }

    void UpdatePowerBarUI()
    {
        if (powerBarUI != null)
        {
            powerBarUI.UpdatePower(currentForce, maxForce);
            return;
        }
        
        if (powerBar != null)
        {
            powerBar.value = currentForce;
            
            if (powerBarFill != null)
            {
                float powerPercent = currentForce / maxForce;
                
                if (powerPercent < 0.33f) powerBarFill.color = lowPowerColor;
                else if (powerPercent < 0.66f) powerBarFill.color = midPowerColor;
                else powerBarFill.color = highPowerColor;
            }
        }
        
        if (powerText != null)
        {
            powerText.text = $"Power: {Mathf.RoundToInt(currentForce)}/{Mathf.RoundToInt(maxForce)}";
        }
    }

    void ShowPowerBar(bool show)
    {
        if (powerBarUI != null)
        {
            powerBarUI.SetVisible(show);
        }
        else if (powerBarContainer != null)
        {
            powerBarContainer.SetActive(show);
        }
    }

    public void OnSelect()
    {
        if (!isHeld && !isCharging)
        {
            // 1. Take and Hold
            isHeld = true;
            rb.isKinematic = true;
            holdTimer = 0f;
            hasStartedAutoCharging = false;
        }
        else if (isHeld && !isCharging)
        {
            // 2. Charging start
            StartCharging();
            hasStartedAutoCharging = true;
        }
        else if (isHeld && isCharging)
        {
            // 3. Throw
            isCharging = false;
            isHeld = false;
            rb.isKinematic = false;
            
            if (powerBarUI != null) powerBarUI.PlayThrowEffect();
            ShowPowerBar(false);

            Vector3 dir = (cameraTransform.TransformDirection(throwDirection)).normalized;
            rb.AddForce(dir * currentForce, ForceMode.Impulse);

            currentForce = 0f;
            holdTimer = 0f;
            hasStartedAutoCharging = false;
        }
    }
    
    public void OnPointerDown(MixedRealityPointerEventData eventData) 
    {
        if (isHeld && !isCharging && !hasStartedAutoCharging)
        {
            StartCharging();
            hasStartedAutoCharging = true;
        }
    }
    
    public void OnPointerUp(MixedRealityPointerEventData eventData) { }
    public void OnPointerDragged(MixedRealityPointerEventData eventData) { }
    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        if (eventData.Pointer is IMixedRealityNearPointer)
        {
            OnSelect();
        }
        else
        {
            if (isHeld && !isCharging && !hasStartedAutoCharging)
            {
                StartCharging();
                hasStartedAutoCharging = true;
            }
        }
    }
}