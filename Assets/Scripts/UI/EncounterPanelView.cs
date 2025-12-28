using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EncounterPanelView : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button fightButton;
    public Button fleeButton;
    public PanelController panelController;

    public UnityEvent onFight;
    public UnityEvent onFlee;

    void Reset()
    {
        panelController = GetComponent<PanelController>();
    }

    public void SetTexts(string title, string description)
    {
        if (titleText) titleText.text = title;
        if (descriptionText) descriptionText.text = description;
    }

    public void WireButtons()
    {
        if (fightButton)
        {
            fightButton.onClick.RemoveAllListeners();
            fightButton.onClick.AddListener(() => onFight?.Invoke());
        }
        if (fleeButton)
        {
            fleeButton.onClick.RemoveAllListeners();
            fleeButton.onClick.AddListener(() => onFlee?.Invoke());
        }
    }

    public void Show(string title, string description)
    {
        SetTexts(title, description);
        WireButtons();
        panelController?.Show();
    }

    public void Hide()
    {
        panelController?.Hide();
    }
}