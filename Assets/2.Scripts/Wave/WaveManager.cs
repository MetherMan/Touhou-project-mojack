using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveData
{

    [Header("웨이브 시작 설정")]
    [Tooltip("이 웨이브를 시작할 시간 (초)")]
    public float startAfter = 0f;

    [Header("기본 설정")]
    public string waveName = "Wave 1";
    public float delayBeforeStart = 1f;

    [Header("스폰 설정")]
    public GameObject enemyPrefab;
    public bool useFormation = false;

    [Header("단일 스폰")]
    public int singleSpawnCount = 5;
    public float singleSpawnInterval = 0.5f;
    public Vector2 spawnAreaMin = new Vector2(-8, 6);
    public Vector2 spawnAreaMax = new Vector2(8, 8);

    [Header("편대 스폰")]
    public FormationType formationType;
    public int formationCount = 3;
    public float formationSpacing = 1.5f;
    public int formationRepeat = 1;
    public float formationRepeatDelay = 2f;
    public Transform[] formationSpawnPoints;

    [Header("웨이브 지속 시간")]
    public float waveDuration = 0f;

    [Header("보스 설정")]
    public bool isBossWave = false;
    public GameObject bossPrefab;

    [Header("기즈모 설정")]
    public Color gizmoColor = Color.yellow;
    public bool showGizmo = true;
}

public enum FormationType
{
    Horizontal,
    Vertical,
    VShape,
    InvertedV,
    Diagonal
}

public class WaveManager : MonoBehaviour
{
    [Header("웨이브 목록")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

    [Header("시작 준비 시간")]
    [SerializeField] private float initialPrepareTime = 3f;

    [Header("디버그")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private int currentWaveIndex = 0;

    [Header("기즈모 설정")]
    [SerializeField] private bool showAllWaveGizmos = true;
    [SerializeField] private bool showOnlySelectedWave = false;
    [SerializeField] private int selectedWaveIndex = 0;

    private bool isWaveActive = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (autoStart)
        {
            StartCoroutine(StartWithPrepareTime());
        }
    }

    IEnumerator StartWithPrepareTime()
    {
        yield return new WaitForSeconds(initialPrepareTime);
        StartCoroutine(StartWaveSequence());
    }
    IEnumerator StartWaveSequence()
    {
        float timer = 0f;
        for (int i = 0; i < waves.Count; i++)
        {
            WaveData wave = waves[i];

            float waitTime = wave.startAfter - timer;
            if (waitTime > 0) yield return new WaitForSeconds(waitTime);

            timer = wave.startAfter;

            StartCoroutine(StartWaveWithDelay(wave));
        }
    }

    IEnumerator StartWaveWithDelay(WaveData wave)
    {
        yield return new WaitForSeconds(wave.delayBeforeStart);

        if (!wave.isBossWave)
        {
            StartCoroutine(ExecuteWave(wave));
            if (wave.waveDuration > 0)
            {
                yield return StartCoroutine(WaitWaveDuration(wave));
            }
            else
            {
                yield return new WaitUntil(() => AllEnemiesDead());
            }
        }
        else
        {
            yield return StartCoroutine(HandleBossWave(wave));
        }
    }

    IEnumerator ExecuteWave(WaveData wave)
    {
        isWaveActive = true;

        if (wave.useFormation)
        {
            for (int i = 0; i < wave.formationRepeat; i++)
            {
                yield return StartCoroutine(SpawnFormation(wave));
                if (i < wave.formationRepeat - 1)
                    yield return new WaitForSeconds(wave.formationRepeatDelay);
            }
        }
        else
        {
            for (int i = 0; i < wave.singleSpawnCount; i++)
            {
                SpawnSingleEnemy(wave);
                yield return new WaitForSeconds(wave.singleSpawnInterval);
            }
        }

        isWaveActive = false;
    }

    void SpawnSingleEnemy(WaveData wave)
    {
        if (wave.enemyPrefab == null)
        {
            return;
        }

        float x = Random.Range(wave.spawnAreaMin.x, wave.spawnAreaMax.x);
        float y = Random.Range(wave.spawnAreaMin.y, wave.spawnAreaMax.y);
        Vector3 spawnPos = new Vector3(x, y, 0);

        GameObject enemy = Instantiate(wave.enemyPrefab, spawnPos, Quaternion.identity);
        if (enemy != null)
            activeEnemies.Add(enemy);
    }

    IEnumerator SpawnFormation(WaveData wave)
    {
        if (wave.enemyPrefab == null) yield break;

        Vector3 centerPos;
        if (wave.formationSpawnPoints != null && wave.formationSpawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, wave.formationSpawnPoints.Length);
            centerPos = wave.formationSpawnPoints[randomIndex].position;
        }
        else
        {
            float x = Random.Range(wave.spawnAreaMin.x, wave.spawnAreaMax.x);
            float y = Random.Range(wave.spawnAreaMin.y, wave.spawnAreaMax.y);
            centerPos = new Vector3(x, y, 0);
        }

        Vector3[] positions = GetFormationPositions(centerPos, wave.formationType,
                                                     wave.formationCount, wave.formationSpacing);

        foreach (Vector3 pos in positions)
        {
            GameObject enemy = Instantiate(wave.enemyPrefab, pos, Quaternion.identity);
            if (enemy != null) activeEnemies.Add(enemy);
            yield return new WaitForSeconds(0.2f);
        }
    }

    Vector3[] GetFormationPositions(Vector3 center, FormationType type,
                                     int count, float spacing)
    {
        Vector3[] positions = new Vector3[count];
        switch (type)
        {
            case FormationType.Horizontal:
                for (int i = 0; i < count; i++)
                    positions[i] = center + new Vector3((i - (count - 1) / 2f) * spacing, 0, 0);
                break;
            case FormationType.Vertical:
                for (int i = 0; i < count; i++)
                    positions[i] = center + new Vector3(0, (i - (count - 1) / 2f) * spacing, 0);
                break;
            case FormationType.VShape:
                for (int i = 0; i < count; i++)
                {
                    float x = (i - (count - 1) / 2f) * spacing;
                    positions[i] = center + new Vector3(x, -Mathf.Abs(x) * 0.5f, 0);
                }
                break;
            case FormationType.InvertedV:
                for (int i = 0; i < count; i++)
                {
                    float x = (i - (count - 1) / 2f) * spacing;
                    positions[i] = center + new Vector3(x, Mathf.Abs(x) * 0.5f, 0);
                }
                break;
            case FormationType.Diagonal:
                for (int i = 0; i < count; i++)
                {
                    float offset = (i - (count - 1) / 2f) * spacing;
                    positions[i] = center + new Vector3(offset, -offset * 0.5f, 0);
                }
                break;
        }
        return positions;
    }

    IEnumerator WaitWaveDuration(WaveData wave)
    {
        float timer = 0f;
        while (timer < wave.waveDuration && !AllEnemiesDead())
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator HandleBossWave(WaveData wave)
    {
        yield return new WaitUntil(() => AllEnemiesDead());
        yield return new WaitForSeconds(3f);

        if (wave.bossPrefab == null)
        {
            yield break;
        }
        GameObject boss = Instantiate(wave.bossPrefab);
        activeEnemies.Add(boss);
        BossIntroController introCtrl = boss.GetComponent<BossIntroController>();
        if (introCtrl != null)
        {
            yield return new WaitUntil(() => introCtrl.BattleStarted);
        }
        else
        {
            BossController bossCtrl = boss.GetComponent<BossController>();
            if (bossCtrl != null)
            {
                yield return new WaitUntil(() => bossCtrl.BattleStarted);
            }
        }

        yield return new WaitUntil(() => AllEnemiesDead());
    }

    bool AllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null);
        return activeEnemies.Count == 0 && !isWaveActive;
    }

    void OnAllWavesComplete()
    {
    }

    [ContextMenu("웨이브 시작")]
    public void ManualStartWaves()
    {
        StartCoroutine(StartWaveSequence());
    }
    private void OnDrawGizmos()
    {
        if (waves == null || waves.Count == 0) return;

        if (showOnlySelectedWave)
        {
            if (selectedWaveIndex >= 0 && selectedWaveIndex < waves.Count)
            {
                DrawWaveGizmo(waves[selectedWaveIndex], selectedWaveIndex);
            }
        }
        else if (showAllWaveGizmos)
        {
            for (int i = 0; i < waves.Count; i++)
            {
                DrawWaveGizmo(waves[i], i);
            }
        }
    }

    private void DrawWaveGizmo(WaveData wave, int index)
    {
        if (!wave.showGizmo) return;

        Color gizmoColor = wave.gizmoColor;
        gizmoColor.a = 0.3f;

        if (wave.isBossWave)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(0, 6, 0), 1f);
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawSphere(new Vector3(0, 6, 0), 1f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(new Vector3(0, 7, 0), $"BOSS: {wave.waveName}");
#endif
        }
        else
        {
            Vector3 center = new Vector3(
                (wave.spawnAreaMin.x + wave.spawnAreaMax.x) / 2f,
                (wave.spawnAreaMin.y + wave.spawnAreaMax.y) / 2f,
                0
            );

            Vector3 size = new Vector3(
                wave.spawnAreaMax.x - wave.spawnAreaMin.x,
                wave.spawnAreaMax.y - wave.spawnAreaMin.y,
                0.1f
            );
            Gizmos.color = gizmoColor;
            Gizmos.DrawCube(center, size);
            Gizmos.color = wave.gizmoColor;
            Gizmos.DrawWireCube(center, size);

            Vector3[] corners = new Vector3[4]
            {
                new Vector3(wave.spawnAreaMin.x, wave.spawnAreaMin.y, 0),
                new Vector3(wave.spawnAreaMax.x, wave.spawnAreaMin.y, 0),
                new Vector3(wave.spawnAreaMax.x, wave.spawnAreaMax.y, 0),
                new Vector3(wave.spawnAreaMin.x, wave.spawnAreaMax.y, 0)
            };

            Gizmos.color = wave.gizmoColor;
            foreach (var corner in corners)
            {
                Gizmos.DrawWireSphere(corner, 0.2f);
            }
#if UNITY_EDITOR
            UnityEditor.Handles.color = wave.gizmoColor;
            UnityEditor.Handles.Label(center + Vector3.up * (size.y / 2f + 0.5f),
                $"Wave {index + 1}: {wave.waveName}\n스폰: {wave.singleSpawnCount}개");
#endif

            if (wave.useFormation && wave.formationSpawnPoints != null)
            {
                foreach (var point in wave.formationSpawnPoints)
                {
                    if (point != null)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawWireSphere(point.position, 0.3f);

                        Vector3[] formationPos = GetFormationPositions(
                            point.position,
                            wave.formationType,
                            wave.formationCount,
                            wave.formationSpacing
                        );

                        Gizmos.color = new Color(0, 1, 1, 0.5f);
                        foreach (var pos in formationPos)
                        {
                            Gizmos.DrawWireSphere(pos, 0.15f);
                        }

                        Gizmos.color = new Color(0, 1, 1, 0.3f);
                        for (int i = 0; i < formationPos.Length - 1; i++)
                        {
                            Gizmos.DrawLine(formationPos[i], formationPos[i + 1]);
                        }
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waves == null || waves.Count == 0) return;

        for (int i = 0; i < waves.Count; i++)
        {
            WaveData wave = waves[i];
            if (!wave.showGizmo) continue;

            Vector3 center = new Vector3(
                (wave.spawnAreaMin.x + wave.spawnAreaMax.x) / 2f,
                (wave.spawnAreaMin.y + wave.spawnAreaMax.y) / 2f,
                0
            );

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(center, new Vector3(
                wave.spawnAreaMax.x - wave.spawnAreaMin.x + 0.2f,
                wave.spawnAreaMax.y - wave.spawnAreaMin.y + 0.2f,
                0.1f
            ));
        }
    }
}
