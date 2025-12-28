using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class ChoiceOption
{
    public string label;
    public UnityEvent onSelect;
}

public class ChoicePanelView : MonoBehaviour
{
    public TMP_Text headerText;
    public Transform buttonContainer;
    public Button buttonPrefab;
    public PanelController panelController;

    readonly List<Button> _spawned = new();

    void Reset()
    {
        panelController = GetComponent<PanelController>();
    }

    public void Show(string header, IEnumerable<ChoiceOption> options)
    {
        if (headerText) headerText.text = header;
        Rebuild(options);
        panelController?.Show();
    }

    public void Hide()
    {
        panelController?.Hide();
    }

    void Rebuild(IEnumerable<ChoiceOption> options)
    {
        foreach (var b in _spawned) if (b) Destroy(b.gameObject);
        _spawned.Clear();

        if (buttonContainer == null || buttonPrefab == null) return;

        foreach (var opt in options)
        {
            var btn = Object.Instantiate(buttonPrefab, buttonContainer);
            var txt = btn.GetComponentInChildren<TMP_Text>();
            if (txt) txt.text = opt.label;
            btn.onClick.RemoveAllListeners();
            if (opt.onSelect != null) btn.onClick.AddListener(() => opt.onSelect.Invoke());
            _spawned.Add(btn);
        }
    }
}