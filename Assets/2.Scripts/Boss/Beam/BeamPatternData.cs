using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeamPatternData
{
    [Header("기본 정보")]
    public string patternName = "빔 패턴";

    [Header("빔 설정")]
    public GameObject beamPrefab;
    public float beamLength = 10f;
    public float spawnOffset = 0.5f;
    public float beamLifetime = 1f;

    [Header("애니메이션 설정")]
    [Tooltip("회전 중에는 애니메이션 멈춤, 패턴 완료 후 재생")]
    public bool disableAnimationDuringRotation = false;

    [HideInInspector] public List<GameObject> currentBeamList;

    [Header("빔 탄환 설정")]
    public GameObject beamBulletPrefab;
    public int beamBulletCount = 20;
    public float beamBulletSpeed = 3f;

    [Tooltip("빔이 지나간 뒤 탄환 생성 지연 시간")]
    public float bulletSpawnDelay = 0.5f;

    [Tooltip("빔 길이 기준 탄환 위치 오프셋 배열")]
    public float[] bulletOffsets = { 1f, 3f, 5f, 7f, 9f };

    [Tooltip("동시에 생성되는 탄환 사이의 간격")]
    public float bulletSpacing = 0.4f;

    [Header("회전 설정")]
    public bool clockwise = false;
    public float rotationSpeed = 60f;
    [Tooltip("회전 속도에 맞춰 발사 간격을 자동으로 조정")]
    public bool syncWithRotationSpeed = false;

    [Header("회전 빔 설정")]
    public int beamCount = 12;
    public float startAngle = -120f;
    public float endAngle = 120f;

    [Header("스윕 레이저 설정")]
    [Tooltip("각도 간격 기준으로 발사할지 여부")]
    public bool useLaserAngleStep = false;
    public int laserBeamCount = 10;
    public float laserAngleStep = 20f;
    public float moveStartX = -5f;
    public float moveEndX = 5f;

    [Header("타이밍")]
    public float shootInterval = 0.1f;
    public float delayAfterPattern = 1f;

    [Header("사운드")]
    public AudioClip beamFireSound;
}
