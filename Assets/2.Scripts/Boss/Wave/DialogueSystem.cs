using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Touhou/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("캐릭터 이름")]
        public string characterName;

        [Tooltip("캐릭터 초상화")]
        public Sprite characterPortrait;

        [Tooltip("캐릭터 위치 (0=왼쪽, 1=오른쪽)")]
        [Range(0, 1)]
        public int portraitPosition = 0;

        [Tooltip("대사")]
        [TextArea(3, 10)]
        public string dialogue;

        [Tooltip("대사 표시 속도 (글자/초)")]
        public float textSpeed = 30f;

        [Tooltip("이 대사 후 자동 넘김 시간 (0이면 수동)")]
        public float autoAdvanceDelay = 0f;

        [Tooltip("대사와 함께 재생할 사운드")]
        public AudioClip voiceClip;
    }

    [Header("대화 설정")]
    public DialogueLine[] dialogueLines;

    [Header("폰트 설정")]
    public TMP_FontAsset dialogueFont;
    public TMP_FontAsset nameFont;
}
