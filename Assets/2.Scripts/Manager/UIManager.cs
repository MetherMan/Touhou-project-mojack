using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI 텍스트")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private TextMeshProUGUI bombText;
    [SerializeField] private TextMeshProUGUI lifeText;

    [Header("기본 설정값")]
    [SerializeField] private int startBomb = 3;
    [SerializeField] private float autoScorePerSecond = 1500f;
    [SerializeField] private int enemyKillScore = 5000;

    private float displayedScore;
    private float realScore;
    private int power;
    private int bomb;
    private PlayerHealth playerHealth;
    public static int FinalScore { get; private set; }

    void Start()
    {
        bomb = startBomb;
        power = 0;
        realScore = 0;
        displayedScore = 0;
        playerHealth = FindObjectOfType<PlayerHealth>();

        UpdateUIInstant();
    }

    void Update()
    {
        realScore += autoScorePerSecond * Time.deltaTime;
        displayedScore = Mathf.Lerp(displayedScore, realScore, Time.deltaTime * 10f);
        scoreText.text = $"Score: {Mathf.RoundToInt(displayedScore):N0}";

        UpdateLife();
    }

    private void UpdateUIInstant()
    {
        scoreText.text = $"Score: {realScore:N0}";
        powerText.text = $"Power: {power}";
        bombText.text = $"Boom: {bomb}";
        UpdateLife();
    }

    private void UpdateLife()
    {
        if (playerHealth != null && lifeText != null)
        {
            lifeText.text = $"Life: {playerHealth.GetLives()}";
        }
    }

    public void AddScore(int amount)
    {
        realScore += amount;
    }

    public void AddEnemyKillScore()
    {
        AddScore(enemyKillScore);
    }

    public void AddPower(int amount)
    {
        power += amount;
        if (power > 400) power = 400;
        powerText.text = $"Power: {power}";
    }

    public bool UseBomb()
    {
        if (bomb > 0)
        {
            bomb--;
            bombText.text = $"Boom: {bomb}";
            return true;
        }
        return false;
    }

    public void AddBomb(int amount)
    {
        bomb = Mathf.Clamp(bomb + amount, 0, 9);
        bombText.text = $"Boom: {bomb}";
    }

    public int GetPower()
    {
        return power;
    }
    public void SaveFinalScore()
    {
        FinalScore = Mathf.RoundToInt(realScore);
    }
    private void OnDestroy()
    {
        if (this != null)
        {
            FinalScore = Mathf.RoundToInt(realScore);
        }
    }
}
