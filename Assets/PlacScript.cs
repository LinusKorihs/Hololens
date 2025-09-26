using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

public class PlaceScript : MonoBehaviour
{
    private TapToPlace tapToPlace;
    private Vector3 originalScale;
    public Vector3 placingScale = new Vector3(0.1f, 0.1f, 0.1f);

    void Start()
    {
        tapToPlace = GetComponent<TapToPlace>();
        originalScale = transform.localScale;
        EnablePlacing(true);
    }

    public void EnablePlacing(bool enable)
    {
        tapToPlace.enabled = enable;
        transform.localScale = enable ? placingScale : originalScale;
    }

    public void DisablePlacing()
    {
        EnablePlacing(false);
        tapToPlace.enabled = false;
        transform.localScale = originalScale;
    }

    private void OnEnable()
    {
        var tapToPlace = GetComponent<TapToPlace>();
        tapToPlace.KeepOrientationVertical = true;
        tapToPlace.UseDefaultSurfaceNormalOffset = false;
        tapToPlace.OnPlacingStarted.AddListener(OnPlacingUpdate);
        tapToPlace.SurfaceNormalOffset = 0.4f;
    }

    private void OnPlacingUpdate()
    {
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }
}