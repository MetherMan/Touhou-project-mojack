using UnityEngine;

public class BombItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            UIManager ui = FindObjectOfType<UIManager>();
            if (ui != null)
            {
                ui.AddBomb(1);
            }
            Destroy(gameObject);
        }
    }
}
