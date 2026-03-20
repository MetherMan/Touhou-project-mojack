using UnityEngine;

public class CameraViewportAdjuster : MonoBehaviour
{
    [SerializeField] private float targetWidth = 9f;
    [SerializeField] private float targetHeight = 16f;
    [SerializeField] private Color letterboxColor = Color.black;

    private Camera mainCamera;
    private Camera backgroundCamera;
    private int lastWidth;
    private int lastHeight;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        CreateBackgroundCamera();
        AdjustViewport();
    }

    void Start()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    void CreateBackgroundCamera()
    {
        GameObject bgCamObj = new GameObject("Background Camera");
        bgCamObj.transform.parent = transform;

        backgroundCamera = bgCamObj.AddComponent<Camera>();
        backgroundCamera.depth = mainCamera.depth - 1;
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = letterboxColor;
        backgroundCamera.cullingMask = 0;
        backgroundCamera.farClipPlane = mainCamera.farClipPlane;
        backgroundCamera.nearClipPlane = mainCamera.nearClipPlane;
    }

    void AdjustViewport()
    {
        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;
        float scaleHeight = screenAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            Rect rect = mainCamera.rect;
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
            mainCamera.rect = rect;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            Rect rect = mainCamera.rect;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
            mainCamera.rect = rect;
        }

        Debug.Log($"[CameraViewportAdjuster] Screen: {Screen.width}x{Screen.height}");
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            AdjustViewport();
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
}