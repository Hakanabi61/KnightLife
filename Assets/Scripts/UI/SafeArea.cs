using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    Rect _lastSafeArea = Rect.zero;
    ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;
    RectTransform _panel;

    void Awake()
    {
        _panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void OnEnable()
    {
        ApplySafeArea();
    }

    void OnRectTransformDimensionsChange()
    {
        ApplySafeArea();
    }

    void Update()
    {
#if UNITY_EDITOR
        ApplySafeArea();
#else
        if (Screen.safeArea != _lastSafeArea || Screen.orientation != _lastOrientation)
            ApplySafeArea();
#endif
    }

    void ApplySafeArea()
    {
        var safe = Screen.safeArea;
        if (safe == _lastSafeArea && Screen.orientation == _lastOrientation) return;

        _lastSafeArea = safe;
        _lastOrientation = Screen.orientation;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.pixelRect.width <= 0 || canvas.pixelRect.height <= 0) return;

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        _panel.anchorMin = anchorMin;
        _panel.anchorMax = anchorMax;
        _panel.offsetMin = Vector2.zero;
        _panel.offsetMax = Vector2.zero;
    }
}