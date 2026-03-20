using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    public static BossHPBar Instance;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private CanvasGroup canvasGroup; 

    private void Awake()
    {
        Instance = this;
        ShowHPBar(false);
    }

    public void ShowHPBar(bool show)
    {
        canvasGroup.alpha = show ? 1 : 0;
        canvasGroup.blocksRaycasts = show;
    }
    void Die()
    {
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.StopBossBGM(2f);
        }

        Destroy(gameObject);
    }

    public void UpdateHP(int current, int max)
    {
        float newValue = (float)current / max;
        hpSlider.value = newValue;
    }
}
