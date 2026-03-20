using UnityEngine;

public class CanvasSafeArea : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Rect camRect = cam.rect;
            Vector2 minAnchor = new Vector2(camRect.x, camRect.y);
            Vector2 maxAnchor = new Vector2(camRect.x + camRect.width, camRect.y + camRect.height);

            if (rectTransform.anchorMin != minAnchor || rectTransform.anchorMax != maxAnchor)
            {
                rectTransform.anchorMin = minAnchor;
                rectTransform.anchorMax = maxAnchor;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }
    }

    void ApplySafeArea()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Rect camRect = cam.rect;
            rectTransform.anchorMin = new Vector2(camRect.x, camRect.y);
            rectTransform.anchorMax = new Vector2(camRect.x + camRect.width, camRect.y + camRect.height);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
