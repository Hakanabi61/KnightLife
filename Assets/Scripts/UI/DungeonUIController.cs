using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Integrates UI views with DungeonManager using SendMessage to avoid hard compile-time dependencies.
/// Attach this to your DungeonManager GameObject or any central controller.
/// Methods are optional; if not present on the target, no error is thrown.
/// </summary>
public class DungeonUIController : MonoBehaviour
{
    [Header("Views (auto-linked by bootstrap if present)")]
    public EncounterPanelView encounterView;
    public ChoicePanelView choiceView;

    [Header("DungeonManager target")]
    public GameObject dungeonManagerTarget; // usually the DungeonManager GameObject

    [Header("Method names on DungeonManager (SendMessage)")]
    public string methodStartFight = "StartFight";
    public string methodTryFlee = "TryFlee";
    public string methodOpenShop = "OpenShop";
    public string methodOpenChest = "OpenChest";
    public string methodGoToBoss = "GoToBoss";
    public string methodContinueExploration = "ContinueExploration";

    void Awake()
    {
        if (!dungeonManagerTarget)
        {
            // Try to find DungeonManager if not assigned
            var dm = FindObjectOfType<DungeonManager>();
            if (dm) dungeonManagerTarget = dm.gameObject;
        }
    }

    void Start()
    {
        // Ensure the bootstrap has linked the views
        if (encounterView == null || choiceView == null)
        {
            var runtime = FindObjectOfType<DungeonUIRuntime>();
            if (runtime != null)
            {
                // Views should be auto-linked by bootstrap
                Debug.Log("DungeonUIController: Waiting for bootstrap to link views");
            }
        }
    }

    // Public API the DungeonManager can call:
    
    /// <summary>
    /// Shows the encounter panel with title and description.
    /// Wire Fight button to call StartFight on DungeonManager.
    /// Wire Flee button to call TryFlee on DungeonManager.
    /// </summary>
    public void ShowEncounter(string title, string description)
    {
        if (!encounterView) return;
        encounterView.onFight.RemoveAllListeners();
        encounterView.onFlee.RemoveAllListeners();
        encounterView.onFight.AddListener(() => SendDM(methodStartFight));
        encounterView.onFlee.AddListener(() => SendDM(methodTryFlee));
        encounterView.Show(title, description);
    }

    /// <summary>
    /// Hides the encounter panel.
    /// </summary>
    public void HideEncounter() => encounterView?.Hide();

    /// <summary>
    /// Shows the choice panel with default options.
    /// Maps choices to DungeonManager methods via SendMessage.
    /// </summary>
    public void ShowDefaultChoices()
    {
        if (!choiceView) return;
        var options = new List<ChoiceOption>
        {
            MakeChoice("Shop", () => SendDM(methodOpenShop)),
            MakeChoice("Truhe öffnen", () => SendDM(methodOpenChest)),
            MakeChoice("Boss", () => SendDM(methodGoToBoss)),
            MakeChoice("Weiter", () => SendDM(methodContinueExploration)),
        };
        choiceView.Show("Wähle eine Option", options);
    }

    /// <summary>
    /// Shows the choice panel with custom options.
    /// </summary>
    public void ShowChoices(string header, IEnumerable<ChoiceOption> options)
    {
        if (!choiceView) return;
        choiceView.Show(header, options);
    }

    /// <summary>
    /// Hides the choice panel.
    /// </summary>
    public void HideChoices() => choiceView?.Hide();

    ChoiceOption MakeChoice(string label, System.Action action)
    {
        var opt = new ChoiceOption { label = label, onSelect = new UnityEngine.Events.UnityEvent() };
        opt.onSelect.AddListener(() => action());
        return opt;
    }

    void SendDM(string method)
    {
        if (!dungeonManagerTarget)
        {
            Debug.LogWarning($"DungeonUIController: No DungeonManager target set; tried to call {method}");
            return;
        }
        Debug.Log($"DungeonUIController: Calling {method} on {dungeonManagerTarget.name}");
        dungeonManagerTarget.SendMessage(method, SendMessageOptions.DontRequireReceiver);
    }
}
