using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 체력 설정")]
    [SerializeField] private int maxHP = 3; 
    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }
    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EnemyDeathDrop drop = GetComponent<EnemyDeathDrop>();
        if (drop != null)
            drop.DropItems();
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.AddEnemyKillScore();

        Destroy(gameObject);
    }

}
