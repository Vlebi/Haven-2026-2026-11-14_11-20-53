using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenui : MonoBehaviour
{
    private GameObject settingsPanel;
    private AudioSource[] musicSources;

    private void Start()
    {
        Transform panel = FindFirstObjectByType<Canvas>()?.transform.Find("Main/Settings");
        if (panel == null)
            panel = FindSettingsPanel();
        settingsPanel = panel != null ? panel.gameObject : null;
        musicSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (settingsPanel != null)
            BuildSettingsControls();
        ApplyMusicVolume(GameSettings.MusicVolume);
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }

    private Transform FindSettingsPanel()
    {
        foreach (Transform transform in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (transform.name == "Settings")
                return transform;
        }

        return null;
    }

    private void BuildSettingsControls()
    {
        CreateSettingsLabel("Music Volume", new Vector2(0f, 90f));
        Slider slider = CreateSlider("MusicVolumeSlider", new Vector2(0f, 30f));
        slider.value = GameSettings.MusicVolume;
        slider.onValueChanged.AddListener(ApplyMusicVolume);

        CreateSettingsLabel("Player Color", new Vector2(0f, -45f));
        for (int i = 0; i < GameSettings.PlayerColors.Length; i++)
        {
            int colorIndex = i;
            GameObject buttonObject = new GameObject("PlayerColor" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(settingsPanel.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(52f, 52f);
            rect.anchoredPosition = new Vector2((i - (GameSettings.PlayerColors.Length - 1) * 0.5f) * 64f, -110f);
            Image image = buttonObject.GetComponent<Image>();
            image.color = GameSettings.PlayerColors[i];
            buttonObject.GetComponent<Button>().onClick.AddListener(() => GameSettings.SetPlayerColor(colorIndex));
        }

    }

    private GameObject CreateSettingsLabel(string text, Vector2 position)
    {
        GameObject labelObject = new GameObject(text + "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(settingsPanel.transform, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 50f);
        rect.anchoredPosition = position;
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return labelObject;
    }

    private Slider CreateSlider(string name, Vector2 position)
    {
        GameObject sliderObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderObject.transform.SetParent(settingsPanel.transform, false);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 30f);
        rect.anchoredPosition = position;

        GameObject backgroundObject = CreateSliderGraphic("Background", sliderObject.transform, new Color(0.15f, 0.15f, 0.15f));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5f, 0f);
        fillAreaRect.offsetMax = new Vector2(-5f, 0f);
        GameObject fill = CreateSliderGraphic("Fill", fillArea.transform, new Color(0.25f, 0.75f, 1f));
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObject.transform, false);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);
        GameObject handle = CreateSliderGraphic("Handle", handleArea.transform, Color.white);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 36f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        return slider;
    }

    private GameObject CreateSliderGraphic(string name, Transform parent, Color color)
    {
        GameObject graphicObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        graphicObject.transform.SetParent(parent, false);
        graphicObject.GetComponent<Image>().color = color;
        return graphicObject;
    }

    private void ApplyMusicVolume(float volume)
    {
        GameSettings.SetMusicVolume(volume);
        if (musicSources == null)
            return;

        foreach (AudioSource source in musicSources)
            source.volume = volume;
    }

    public void SetPlayerColor(int colorIndex)
    {
        GameSettings.SetPlayerColor(colorIndex);
    }
}
