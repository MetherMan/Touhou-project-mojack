using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("연출 설정")]
    public float introDuration = 3f;
    public string[] dialogue;
    public float dialogueDelay = 2f;
    public bool BattleStarted { get; private set; } = false;

    void Start()
    {
        DisableBoss();
        StartCoroutine(BossIntroSequence());
    }

    void DisableBoss()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this) s.enabled = false;
        }
    }

    void EnableBoss()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
        {
            if (s != this) s.enabled = true;
        }
    }

    IEnumerator BossIntroSequence()
    {
        yield return new WaitForSeconds(introDuration);
        foreach (string line in dialogue)
        {
            yield return new WaitForSeconds(dialogueDelay);
        }
        EnableBoss();
        BattleStarted = true;
    }
}
