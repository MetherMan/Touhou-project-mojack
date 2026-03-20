using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("점수 표시")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("메뉴 옵션")]
    [SerializeField] private TextMeshProUGUI[] menuOptions;
    [SerializeField] private TextMeshProUGUI selectionArrow;

    [Header("색상 설정")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("씬 이름")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "Title";

    [Header("애니메이션 설정")]
    [SerializeField] private float scoreCountDuration = 2f;
    [SerializeField] private bool useCountUpAnimation = true;

    private int currentSelection = 0;
    private RectTransform arrowRect;
    private Vector2[] menuPositions;

    private void Start()
    {
        if (selectionArrow == null || menuOptions == null || menuOptions.Length == 0)
        {
            Debug.LogError("[GameOverUI] menuOptions 또는 selectionArrow가 설정되지 않았습니다!");
            return;
        }

        arrowRect = selectionArrow.GetComponent<RectTransform>();
        menuPositions = new Vector2[menuOptions.Length];
        for (int i = 0; i < menuOptions.Length; i++)
        {
            menuPositions[i] = menuOptions[i].GetComponent<RectTransform>().localPosition;
        }
        DisplayFinalScore();
        UpdateSelectionUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentSelection = (currentSelection - 1 + menuOptions.Length) % menuOptions.Length;
            UpdateSelectionUI();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentSelection = (currentSelection + 1) % menuOptions.Length;
            UpdateSelectionUI();
        }
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnSelect();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRetry();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnMainMenu();
        }
    }

    private void DisplayFinalScore()
    {
        int finalScore = UIManager.FinalScore;

        if (finalScoreText != null)
        {
            if (useCountUpAnimation)
            {
                StartCoroutine(CountUpScore(finalScore));
            }
            else
            {
                finalScoreText.text = $"{finalScore:N0}";
            }
        }

        Debug.Log($"[GameOverUI] 최종 점수: {finalScore:N0}");
    }

    private System.Collections.IEnumerator CountUpScore(int targetScore)
    {
        float elapsed = 0f;
        int currentScore = 0;

        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scoreCountDuration;
            float smoothT = 1f - (1f - t) * (1f - t);
            currentScore = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, smoothT));
            finalScoreText.text = $"{currentScore:N0}";
            yield return null;
        }

        finalScoreText.text = $"{targetScore:N0}";
    }

    private void UpdateSelectionUI()
    {
        Debug.Log($"[GameOverUI] 선택: {currentSelection} ({menuOptions[currentSelection].text})");
        for (int i = 0; i < menuOptions.Length; i++)
        {
            RectTransform rect = menuOptions[i].GetComponent<RectTransform>();
            rect.localPosition = menuPositions[i];
            menuOptions[i].color = (i == currentSelection) ? selectedColor : defaultColor;
            menuOptions[i].ForceMeshUpdate();
        }
        RectTransform selectedRect = menuOptions[currentSelection].GetComponent<RectTransform>();
        Vector2 arrowPos = arrowRect.localPosition;
        arrowPos.y = selectedRect.localPosition.y;
        arrowRect.localPosition = arrowPos;
        selectionArrow.color = selectedColor;
    }

    private void OnSelect()
    {
        Debug.Log($"[GameOverUI] 선택 확정: {menuOptions[currentSelection].text}");

        switch (currentSelection)
        {
            case 0:
                OnRetry();
                break;
            case 1:
                OnMainMenu();
                break;
        }
    }

    private void OnRetry()
    {
        Debug.Log("[GameOverUI] 재시작");
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnMainMenu()
    {
        Debug.Log("[GameOverUI] 메인 메뉴로");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
