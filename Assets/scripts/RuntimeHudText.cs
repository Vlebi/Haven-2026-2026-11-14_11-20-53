using TMPro;
using UnityEngine;

public static class RuntimeHudText
{
    public static TMP_Text GetOrCreate(string objectName, string initialText, Vector2 anchoredPosition)
    {
        Canvas canvas = null;
        foreach (Canvas candidate in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (candidate.CompareTag("Canvas"))
            {
                canvas = candidate;
                break;
            }
        }

        if (canvas == null)
        {
            Debug.LogWarning("Cannot create HUD text because no Canvas-tagged Canvas was found.");
            return null;
        }

        canvas.transform.localScale = Vector3.one;
        Transform existing = canvas.transform.Find(objectName);
        if (existing != null)
            return existing.GetComponent<TMP_Text>();

        GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(300f, 50f);

        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = initialText;
        text.fontSize = 28f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.raycastTarget = false;
        return text;
    }
}
