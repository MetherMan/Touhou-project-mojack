using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI[] menuOptions;  
    public TextMeshProUGUI selectionArrow; 
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;

    private int currentSelection = 0;
    private RectTransform arrowRect;
    private Vector2[] menuPositions; 

    private void Start()
    {
        if (selectionArrow == null || menuOptions == null || menuOptions.Length == 0)
        {
            Debug.LogError("[MenuManager] menuOptions와 selectionArrow를 인스펙터에 설정해야 합니다.");
            return;
        }

        arrowRect = selectionArrow.GetComponent<RectTransform>();
        menuPositions = new Vector2[menuOptions.Length];
        for (int i = 0; i < menuOptions.Length; i++)
        {
            menuPositions[i] = menuOptions[i].GetComponent<RectTransform>().localPosition;
        }

        UpdateSelectionUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection = (currentSelection - 1 + menuOptions.Length) % menuOptions.Length;
            UpdateSelectionUI();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = (currentSelection + 1) % menuOptions.Length;
            UpdateSelectionUI();
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            OnSelect();
        }
    }

    private void OnSelect()
    {
        switch (currentSelection)
        {
            case 0:
                SceneManager.LoadScene("GameScene");
                break;
            case 1:
                Application.Quit();
                break;
        }
    }

    private void UpdateSelectionUI()
    {
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
}
