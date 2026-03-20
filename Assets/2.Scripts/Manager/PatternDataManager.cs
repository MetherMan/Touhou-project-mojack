using UnityEngine;

[System.Serializable]
public class PatternDataManager
{

    [Header("사운드 설정")]
    public AudioClip fireSound;

    [Header("패턴 식별")]
    public string patternName = "패턴";

    [Header("발사 설정")]
    public GameObject bulletPrefab;
    public int setsToShoot = 14;
    public int bulletsPerSet = 6;
    public float rotationStep = 10f;
    public float spawnOffset = 2f;
    public float shootInterval = 0.05f;
    public float freezeTime = 0.2f;

    [Header("속도 제어")]
    public float initialSpeed = 1f;
    public float burstSpeed = 5f;
    public float finalSpeed = 2f;
    public float speedIncreasePerSet = 0.3f;

    [Header("시간 제어")]
    public float accelerationTime = 0.3f;
    public float burstDuration = 0.5f;
    public float decelerationTime = 0.4f;

    [Header("패턴 타이밍")]
    public float delayAfterPattern = 2f;

}
