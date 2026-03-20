
using UnityEngine;

public class PowerUpItem : MonoBehaviour
{
    [Header("아이템 종류 설정")]
    [SerializeField] private int powerAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
                ui.AddPower(powerAmount);

            Destroy(gameObject);
        }
    }
}
