using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairySpawner : MonoBehaviour
{
    [Header("스폰 영역 (단일 스폰용)")]
    [SerializeField] private Vector2 spawnMin;
    [SerializeField] private Vector2 spawnMax;

    [Header("스폰 설정")]
    [SerializeField] private GameObject fairyPrefab;
    [SerializeField] private float spawnInterval = 5.0f;
    [SerializeField] private int maxFairies = 10;
    [SerializeField] private bool spawnImmediately = true;
    [Header("단일 스폰 다중 소환 설정")]
    [SerializeField] private int minSingleSpawnCount = 1;
    [SerializeField] private int maxSingleSpawnCount = 1;
    public enum FormationType
    {
        Horizontal,
        Vertical,
        VShape,
        InvertedV,
        Diagonal
    }

    [Header("편대 설정")]
    [SerializeField] private bool useFormation = false;
    [SerializeField] private FormationType formationType = FormationType.Horizontal;
    [SerializeField] private int formationCount = 3;
    [SerializeField] private float formationSpacing = 1.5f;
    [SerializeField] private float formationSpawnDelay = 0.2f;
    [Header("편대 스폰 포인트 (편대용)")]
    [Tooltip("편대 스폰 시 이 포인트들을 중심으로 스폰됩니다")]
    [SerializeField] private Transform[] formationSpawnPoints;
    [SerializeField] private bool randomFormationPoint = true;

    private List<GameObject> activeFairies = new List<GameObject>();
    private int currentPointIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnFairies());
    }

    IEnumerator SpawnFairies()
    {
        if (!spawnImmediately)
        {
            yield return new WaitForSeconds(spawnInterval);
        }

        while (true)
        {
            activeFairies.RemoveAll(f => f == null);

            if (activeFairies.Count < maxFairies)
            {
                if (useFormation)
                {
                    yield return StartCoroutine(SpawnFormation());
                }
                else
                {
                    SpawnSingleFairy();
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
    void SpawnSingleFairy()
    {
        int spawnCount = Random.Range(minSingleSpawnCount, maxSingleSpawnCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            if (activeFairies.Count >= maxFairies) break;

            float x = Random.Range(spawnMin.x, spawnMax.x);
            float y = Random.Range(spawnMin.y, spawnMax.y);
            Vector3 spawnPos = new Vector3(x, y, 0);

            GameObject fairy = Instantiate(fairyPrefab, spawnPos, Quaternion.identity);
            activeFairies.Add(fairy);
        }
    }
    IEnumerator SpawnFormation()
    {
        Vector3 centerPos;
        if (formationSpawnPoints != null && formationSpawnPoints.Length > 0)
        {
            if (randomFormationPoint)
            {
                int randomIndex = Random.Range(0, formationSpawnPoints.Length);
                centerPos = formationSpawnPoints[randomIndex].position;
            }
            else
            {
                centerPos = formationSpawnPoints[currentPointIndex].position;
                currentPointIndex = (currentPointIndex + 1) % formationSpawnPoints.Length;
            }
        }
        else
        {
            float centerX = Random.Range(spawnMin.x, spawnMax.x);
            float centerY = Random.Range(spawnMin.y, spawnMax.y);
            centerPos = new Vector3(centerX, centerY, 0);
        }

        Vector3[] positions = GetFormationPositions(centerPos);

        foreach (Vector3 pos in positions)
        {
            if (activeFairies.Count >= maxFairies) break;

            GameObject fairy = Instantiate(fairyPrefab, pos, Quaternion.identity);
            activeFairies.Add(fairy);

            yield return new WaitForSeconds(formationSpawnDelay);
        }
    }
    Vector3[] GetFormationPositions(Vector3 center)
    {
        Vector3[] positions = new Vector3[formationCount];

        switch (formationType)
        {
            case FormationType.Horizontal:
                for (int i = 0; i < formationCount; i++)
                {
                    float offset = (i - (formationCount - 1) / 2f) * formationSpacing;
                    positions[i] = center + new Vector3(offset, 0, 0);
                }
                break;

            case FormationType.Vertical:
                for (int i = 0; i < formationCount; i++)
                {
                    float offset = (i - (formationCount - 1) / 2f) * formationSpacing;
                    positions[i] = center + new Vector3(0, offset, 0);
                }
                break;

            case FormationType.VShape:
                for (int i = 0; i < formationCount; i++)
                {
                    float xOffset = (i - (formationCount - 1) / 2f) * formationSpacing;
                    float yOffset = -Mathf.Abs(xOffset) * 0.5f;
                    positions[i] = center + new Vector3(xOffset, yOffset, 0);
                }
                break;

            case FormationType.InvertedV:
                for (int i = 0; i < formationCount; i++)
                {
                    float xOffset = (i - (formationCount - 1) / 2f) * formationSpacing;
                    float yOffset = Mathf.Abs(xOffset) * 0.5f;
                    positions[i] = center + new Vector3(xOffset, yOffset, 0);
                }
                break;

            case FormationType.Diagonal:
                for (int i = 0; i < formationCount; i++)
                {
                    float offset = (i - (formationCount - 1) / 2f) * formationSpacing;
                    positions[i] = center + new Vector3(offset, -offset * 0.5f, 0);
                }
                break;
        }

        return positions;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = (spawnMin + spawnMax) / 2f;
        Vector3 size = spawnMax - spawnMin;
        Gizmos.DrawWireCube(center, size);
        if (formationSpawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in formationSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                    Gizmos.DrawLine(point.position, point.position + Vector3.down * 2f);
                    Gizmos.color = new Color(0, 1, 1, 0.3f);
                    float width = (formationCount - 1) * formationSpacing;
                    Gizmos.DrawWireCube(point.position, new Vector3(width, 1, 0));
                    Gizmos.color = Color.cyan;
                }
            }
        }
    }
}
