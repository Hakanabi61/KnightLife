using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Button für Encounter-Entscheidungen
/// </summary>
public class EncounterButton : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;

    private System.Action onClickAction;

    void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void Setup(string text, System.Action onClick)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }

        onClickAction = onClick;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        Debug.Log($"🖱️ Button clicked: {buttonText.text}");
        onClickAction?.Invoke();
    }
}