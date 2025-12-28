using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class DungeonUIBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        var go = new GameObject("DungeonUIRuntime");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<DungeonUIRuntime>();
    }
}

public class DungeonUIRuntime : MonoBehaviour
{
    Canvas _canvas;
    RectTransform _safeArea;
    EncounterPanelView _encounterView;
    ChoicePanelView _choiceView;

    void Start()
    {
        EnsureEventSystem();
        EnsureCanvas();
        EnsurePanels();

        // Smoke test: show a single Encounter on Start for verification
        // The DungeonUIController should be used by DungeonManager to control UI in-game
        Debug.Log("DungeonUIRuntime: Smoke test - showing encounter panel");
        _encounterView?.Show("GEGNER!", "Ein wilder Gegner erscheint!");
    }

    void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<InputSystemUIInputModule>();
        }
    }

    void EnsureCanvas()
    {
        var existing = GameObject.Find("UIRoot");
        if (existing != null && existing.TryGetComponent(out Canvas c))
        {
            _canvas = c;
        }
        else
        {
            var go = new GameObject("UIRoot");
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        var saGo = GameObject.Find("SafeArea") ?? new GameObject("SafeArea");
        if (saGo.transform.parent != _canvas.transform)
            saGo.transform.SetParent(_canvas.transform, false);
        _safeArea = saGo.GetComponent<RectTransform>();
        if (_safeArea == null)
            _safeArea = saGo.AddComponent<RectTransform>();
        if (saGo.GetComponent<SafeArea>() == null)
            saGo.AddComponent<SafeArea>();
        _safeArea.anchorMin = Vector2.zero;
        _safeArea.anchorMax = Vector2.one;
        _safeArea.offsetMin = Vector2.zero;
        _safeArea.offsetMax = Vector2.zero;
    }

    void EnsurePanels()
    {
        _encounterView = EnsureEncounterPanel();
        _choiceView = EnsureChoicePanel();
        // Provide to controller if present
        var controller = Object.FindObjectOfType<DungeonUIController>();
        if (controller)
        {
            controller.encounterView = _encounterView;
            controller.choiceView = _choiceView;
        }
    }

    EncounterPanelView EnsureEncounterPanel()
    {
        var root = GameObject.Find("EncounterPanel");
        if (root == null)
        {
            root = CreatePanelRoot("EncounterPanel", new Vector2(900, 300), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150));
        }
        AttachToSafeArea(root);

        var cg = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        var pc = root.GetComponent<PanelController>() ?? root.AddComponent<PanelController>();
        pc.startHidden = true;

        var view = root.GetComponent<EncounterPanelView>() ?? root.AddComponent<EncounterPanelView>();
        view.panelController = pc;

        var content = EnsureChild(root.transform, "Content");
        var vlg = content.GetComponent<VerticalLayoutGroup>() ?? content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f; vlg.childAlignment = TextAnchor.MiddleCenter; vlg.childControlWidth = true; vlg.childControlHeight = true;
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.05f, 0.1f);
        crt.anchorMax = new Vector2(0.95f, 0.9f);
        crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

        if (view.titleText == null)
        {
            var tt = CreateTMP("TitleText", content, "GEGNER!", 48, FontStyles.Bold);
            view.titleText = tt;
        }
        if (view.descriptionText == null)
        {
            var dt = CreateTMP("DescriptionText", content, "Ein wilder Gegner erscheint!", 32, FontStyles.Normal);
            view.descriptionText = dt;
        }

        var btnCont = EnsureChild(content, "ButtonContainer");
        var hlg = btnCont.GetComponent<HorizontalLayoutGroup>() ?? btnCont.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 24f; hlg.childAlignment = TextAnchor.MiddleCenter; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = false;

        if (view.fightButton == null)
        {
            var fight = CreateButton("Button_K�mpfen", btnCont, "K�mpfen");
            view.fightButton = fight;
        }
        if (view.fleeButton == null)
        {
            var flee = CreateButton("Button_Fliehen", btnCont, "Fliehen");
            view.fleeButton = flee;
        }

        return view;
    }

    ChoicePanelView EnsureChoicePanel()
    {
        var root = GameObject.Find("ChoicePanel");
        if (root == null)
        {
            root = CreatePanelRoot("ChoicePanel", new Vector2(900, 400), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50));
        }
        AttachToSafeArea(root);

        var cg = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        var pc = root.GetComponent<PanelController>() ?? root.AddComponent<PanelController>();
        pc.startHidden = true;

        var view = root.GetComponent<ChoicePanelView>() ?? root.AddComponent<ChoicePanelView>();
        view.panelController = pc;

        if (view.headerText == null)
        {
            var header = CreateTMP("ChoiceTitleText", root.transform, "W�hle eine Option", 42, FontStyles.Bold);
            var hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0.05f, 0.75f);
            hrt.anchorMax = new Vector2(0.95f, 0.95f);
            hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;
            view.headerText = header;
        }

        if (view.buttonContainer == null)
        {
            var cont = EnsureChild(root.transform, "ChoiceButtonContainer");
            var vlg = cont.GetComponent<VerticalLayoutGroup>() ?? cont.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16f; vlg.childAlignment = TextAnchor.MiddleCenter; vlg.childControlWidth = true; vlg.childControlHeight = true;
            var crt = cont.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.05f, 0.1f);
            crt.anchorMax = new Vector2(0.95f, 0.7f);
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
            view.buttonContainer = cont;
        }

        if (view.buttonPrefab == null)
        {
            var pf = CreateButton("ChoiceButton_Prefab", view.buttonContainer, "Option");
            pf.gameObject.SetActive(false);
            view.buttonPrefab = pf;
        }

        var opts = new List<ChoiceOption>
        {
            new ChoiceOption{ label = "Shop", onSelect = new UnityEngine.Events.UnityEvent() },
            new ChoiceOption{ label = "Truhe �ffnen", onSelect = new UnityEngine.Events.UnityEvent() },
            new ChoiceOption{ label = "Weiter", onSelect = new UnityEngine.Events.UnityEvent() },
        };
        view.Show("W�hle eine Option", opts);
        view.Hide();

        return view;
    }

    GameObject CreatePanelRoot(string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        var root = new GameObject(name, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(_safeArea, false);
        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size; rt.anchoredPosition = anchoredPos;

        var img = root.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        return root;
    }

    void AttachToSafeArea(GameObject go)
    {
        if (go.transform.parent != _safeArea)
            go.transform.SetParent(_safeArea, false);
    }

    Transform EnsureChild(Transform parent, string name)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        return t;
    }

    TMP_Text CreateTMP(string name, Transform parent, string text, int size, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        var rt = tmp.rectTransform;
        rt.sizeDelta = new Vector2(0, 0);
        return tmp;
    }

    Button CreateButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.9f, 1f);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(260, 80);

        var txt = CreateTMP("Text", go.transform, label, 32, FontStyles.Bold);
        txt.alignment = TextAlignmentOptions.Center;
        var txtRt = txt.rectTransform;
        txtRt.anchorMin = new Vector2(0, 0);
        txtRt.anchorMax = new Vector2(1, 1);
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;

        var btn = go.GetComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = img.color;
        colors.highlightedColor = new Color(0.25f, 0.5f, 1f, 1f);
        colors.pressedColor = new Color(0.15f, 0.35f, 0.85f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        btn.colors = colors;

        return btn;
    }
}