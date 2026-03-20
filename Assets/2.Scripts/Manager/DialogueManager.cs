using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DialogueManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private TextMeshProUGUI nameTextLeft;
    [SerializeField] private TextMeshProUGUI dialogueTextLeft;

    [SerializeField] private TextMeshProUGUI nameTextRight;
    [SerializeField] private TextMeshProUGUI dialogueTextRight;

    [SerializeField] private Image portraitImageLeft;
    [SerializeField] private Image portraitImageRight;
    [SerializeField] private GameObject continueIndicator;

    [Header("입력 설정")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private KeyCode skipKey = KeyCode.X;

    [Header("텍스트 사운드")]
    [SerializeField] private AudioClip textSound;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private bool canAdvance = false;
    private Coroutine typingCoroutine;

    public bool IsDialogueComplete => !isDialogueActive;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;
        if (Input.GetKeyDown(advanceKey) && canAdvance)
        {
            if (isTyping)
            {
                CompleteText();
            }
            else
            {
                AdvanceDialogue();
            }
        }
        if (Input.GetKeyDown(skipKey))
        {
            EndDialogue();
        }
    }
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Length == 0)
        {
            Debug.LogWarning("[Dialogue] 대화 데이터가 비어있습니다!");
            return;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        DisablePlayerForIntro();
        DisplayLine(currentLineIndex);
    }
    private void DisplayLine(int index)
    {
        if (index >= currentDialogue.dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.dialogueLines[index];
        if (line.portraitPosition == 0)
        {
            if (nameTextLeft != null)
            {
                nameTextLeft.text = line.characterName;
                if (currentDialogue.nameFont != null)
                    nameTextLeft.font = currentDialogue.nameFont;
            }

            if (portraitImageLeft != null && line.characterPortrait != null)
            {
                portraitImageLeft.sprite = line.characterPortrait;
                portraitImageLeft.enabled = true;
            }
            else if (portraitImageLeft != null)
            {
                portraitImageLeft.enabled = false;
            }

            if (portraitImageRight != null)
                portraitImageRight.enabled = false;
            if (nameTextRight != null) nameTextRight.text = "";
            if (dialogueTextRight != null) dialogueTextRight.text = "";
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(line, true));
        }
        else
        {
            if (nameTextRight != null)
            {
                nameTextRight.text = line.characterName;
                if (currentDialogue.nameFont != null)
                    nameTextRight.font = currentDialogue.nameFont;
            }

            if (portraitImageRight != null && line.characterPortrait != null)
            {
                portraitImageRight.sprite = line.characterPortrait;
                portraitImageRight.enabled = true;
            }
            else if (portraitImageRight != null)
            {
                portraitImageRight.enabled = false;
            }

            if (portraitImageLeft != null)
                portraitImageLeft.enabled = false;
            if (nameTextLeft != null) nameTextLeft.text = "";
            if (dialogueTextLeft != null) dialogueTextLeft.text = "";
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(line, false));
        }
    }
    private IEnumerator TypeText(DialogueData.DialogueLine line, bool isLeft)
    {
        isTyping = true;
        canAdvance = false;

        if (continueIndicator != null)
            continueIndicator.SetActive(false);

        TextMeshProUGUI targetText = isLeft ? dialogueTextLeft : dialogueTextRight;

        if (targetText != null)
        {
            if (currentDialogue.dialogueFont != null)
                targetText.font = currentDialogue.dialogueFont;

            targetText.text = "";
        }

        float delay = 1f / line.textSpeed;

        foreach (char c in line.dialogue)
        {
            if (targetText != null)
                targetText.text += c;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        canAdvance = true;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
        if (line.autoAdvanceDelay > 0f)
        {
            yield return new WaitForSeconds(line.autoAdvanceDelay);
            AdvanceDialogue();
        }
    }
    private void CompleteText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        var line = currentDialogue.dialogueLines[currentLineIndex];

        TextMeshProUGUI targetText = (line.portraitPosition == 0) ? dialogueTextLeft : dialogueTextRight;
        if (targetText != null)
            targetText.text = line.dialogue;

        isTyping = false;
        canAdvance = true;

        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }
    private void AdvanceDialogue()
    {
        currentLineIndex++;
        DisplayLine(currentLineIndex);
    }
    private void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isDialogueActive = false;
        currentDialogue = null;
        currentLineIndex = 0;
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        EnablePlayerInput();

        Debug.Log("[Dialogue] 대화 종료");
    }
    public bool IsComplete()
    {
        return !isDialogueActive;
    }
    private void DisablePlayerForIntro()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetInput();
            playerController.enabled = false;
        }

        var playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }
    }
    private void EnablePlayerInput()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        var playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.enabled = true;
        }
    }
}
