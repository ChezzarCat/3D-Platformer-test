using UnityEngine;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class CustomTMProOutline : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 1f;

    private TextMeshProUGUI originalText;
    private TextMeshProUGUI[] outlineCopies;

    private const int OutlineCount = 8; // Number of outline copies (top, bottom, left, right, diagonals)
    private GameObject mainTextObject; // Separate GameObject for the main text

    private void OnValidate()
    {
        ApplyOutline();
    }

    private void OnEnable()
    {
        ApplyOutline();
    }

    private void OnDisable()
    {
        RemoveOutline();
    }

    private void ApplyOutline()
    {
        if (originalText == null)
            SetupMainText();

        if (outlineCopies == null || outlineCopies.Length != OutlineCount)
        {
            CreateOutlineCopies();
        }

        // Update each outline's properties
        float halfWidth = outlineWidth / 2;
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-halfWidth, -halfWidth, 0), // Bottom-left
            new Vector3(-halfWidth, halfWidth, 0),  // Top-left
            new Vector3(halfWidth, halfWidth, 0),   // Top-right
            new Vector3(halfWidth, -halfWidth, 0), // Bottom-right
            new Vector3(-outlineWidth, 0, 0),      // Left
            new Vector3(outlineWidth, 0, 0),       // Right
            new Vector3(0, outlineWidth, 0),       // Top
            new Vector3(0, -outlineWidth, 0)       // Bottom
        };

        for (int i = 0; i < outlineCopies.Length; i++)
        {
            var copy = outlineCopies[i];
            if (copy == null) continue;

            copy.text = originalText.text;
            copy.color = outlineColor;
            copy.font = originalText.font;
            copy.fontSize = originalText.fontSize;
            copy.alignment = originalText.alignment;
            copy.raycastTarget = false; // Prevent blocking UI interactions
            copy.gameObject.transform.localPosition = offsets[i];
        }

        // Ensure main text remains visible
        if (mainTextObject)
            mainTextObject.transform.SetAsLastSibling(); // Render above outlines
    }

    private void SetupMainText()
    {
        originalText = GetComponent<TextMeshProUGUI>();

        // Create a new GameObject for the main text
        mainTextObject = new GameObject("MainText");
        mainTextObject.transform.SetParent(transform, false);
        mainTextObject.transform.localPosition = Vector3.zero;
        mainTextObject.transform.localScale = Vector3.one;

        // Move the original TextMeshProUGUI to the new GameObject
        var newText = mainTextObject.AddComponent<TextMeshProUGUI>();
        newText.text = originalText.text;
        newText.color = originalText.color;
        newText.font = originalText.font;
        newText.fontSize = originalText.fontSize;
        newText.alignment = originalText.alignment;
        newText.raycastTarget = originalText.raycastTarget;

        DestroyImmediate(originalText); // Remove old component
        originalText = newText;
    }

    private void CreateOutlineCopies()
    {
        RemoveOutline();

        outlineCopies = new TextMeshProUGUI[OutlineCount];
        for (int i = 0; i < OutlineCount; i++)
        {
            GameObject outlineObj = new GameObject($"Outline_{i}");
            outlineObj.transform.SetParent(transform, false);
            outlineObj.transform.localScale = Vector3.one;

            var copy = outlineObj.AddComponent<TextMeshProUGUI>();
            copy.raycastTarget = false; // Prevent blocking UI interactions
            outlineCopies[i] = copy;
        }
    }

    private void RemoveOutline()
    {
        if (outlineCopies != null)
        {
            foreach (var copy in outlineCopies)
            {
                if (copy != null)
                    DestroyImmediate(copy.gameObject);
            }
        }

        outlineCopies = null;

        // Clean up the main text object if needed
        if (mainTextObject != null)
        {
            DestroyImmediate(mainTextObject);
            mainTextObject = null;
        }
    }
}
