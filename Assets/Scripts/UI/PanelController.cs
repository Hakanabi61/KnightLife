using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    public bool startHidden = true;
    public float fadeDuration = 0.15f;

    CanvasGroup _group;
    Coroutine _fade;

    void Awake()
    {
        _group = GetComponent<CanvasGroup>();
        if (startHidden) SetVisible(false, instant: true);
        else SetVisible(true, instant: true);
    }

    public void Show() => SetVisible(true);
    public void Hide() => SetVisible(false);

    public void SetVisible(bool visible, bool instant = false)
    {
        if (_fade != null) StopCoroutine(_fade);
        if (instant || fadeDuration <= 0f)
        {
            _group.alpha = visible ? 1f : 0f;
            _group.interactable = visible;
            _group.blocksRaycasts = visible;
            return;
        }
        _fade = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    System.Collections.IEnumerator FadeTo(float target)
    {
        float start = _group.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            _group.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        _group.alpha = target;
        bool visible = target > 0.99f;
        _group.interactable = visible;
        _group.blocksRaycasts = visible;
        _fade = null;
    }
}