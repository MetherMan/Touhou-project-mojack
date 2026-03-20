using UnityEngine;

public class ColliderPixelChecker : MonoBehaviour
{
    void Start()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (box != null && sr != null)
        {
            float widthUnits = box.size.x * transform.localScale.x;
            float widthPixels = widthUnits * sr.sprite.pixelsPerUnit;

            Debug.Log("Collider 가로 픽셀: " + widthPixels);
        }
        else
        {
            Debug.LogWarning("BoxCollider2D 또는 SpriteRenderer 없음!");
        }
    }
}
