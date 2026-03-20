using UnityEngine;

public class EnemyDeathDrop : MonoBehaviour
{
    [Header("드롭 아이템 프리팹")]
    [SerializeField] private GameObject powerUpPrefab; 
    [SerializeField] private GameObject bigPowerUpPrefab; 
    [SerializeField] private GameObject bombPrefab;       

    [Header("드롭 확률 (%)")]
    [Range(0, 100)][SerializeField] private float bigPowerUpChance = 10f;
    [Range(0, 100)][SerializeField] private float bombChance = 2f;

    public void DropItems()
    {
        float offsetRange = 0.5f; 

        Vector3 randomOffset()
        {
            return new Vector3(
                Random.Range(-offsetRange, offsetRange),
                Random.Range(-offsetRange, offsetRange),
                0f
            );
        }
        Instantiate(powerUpPrefab, transform.position + randomOffset(), Quaternion.identity);

        if (Random.value < bigPowerUpChance / 100f)
        {
            Instantiate(bigPowerUpPrefab, transform.position + randomOffset(), Quaternion.identity);
        }

        if (Random.value < bombChance / 100f)
        {
            Instantiate(bombPrefab, transform.position + randomOffset(), Quaternion.identity);
        }
    }

}
