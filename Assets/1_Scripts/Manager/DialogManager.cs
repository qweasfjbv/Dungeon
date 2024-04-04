using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TextMeshProUGUI dialogActorName;
    [SerializeField] private Image dialogActorSprite;
    [SerializeField] private TextMeshProUGUI dialogText;

    private DialogEventInfo eventInfo;

    private bool dialogInProgress = false;
    private bool isTyping = false;

    private List<string> dialogKeys = new List<string>();
    private string prevDialogue = "";

    private int curDialogueIndex = 0;
    private int curLineIndex = 0;



    private float dialogHidePosY = -1000;
    private float dialogRevealPosY = 0;


    private void Start()
    {


        dialogBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, dialogHidePosY, 0);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!dialogInProgress) SetEvent(1);
        }

        if (Input.GetKeyDown(KeyCode.Space) && dialogInProgress)
        {
            if (isTyping)
            {

                StopAllCoroutines();
                dialogText.text = prevDialogue;
                isTyping = false;
            }
            else
            {
                if (curLineIndex < dialogKeys.Count) {
                    prevDialogue = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex]);
                    ShowNextLine();
                }
                else
                {
                    // 여기 이벤트 선택지 생성하면됨
                }
            }
        }
    }



    private void SetEvent(int id)
    {


        // dialog Text table에서 받아오기
        eventInfo = Managers.Resource.GetEventInfo(id);


        // 대사 키 저장
        dialogKeys.Clear();
        for (int i = 0; i < eventInfo.eventDialogCnt; i++)
        {
            dialogKeys.Add(eventInfo.eventName + "_D" + i.ToString());
        }


        // Init
        dialogInProgress = true;
        dialogText.text = "";
        dialogActorName.text = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", eventInfo.eventName);


        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogRevealPosY, 0.6f).SetEase(Ease.OutBounce);

        prevDialogue = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex]);
        ShowNextLine();
    }

    private void UnsetEvent()
    {
        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogHidePosY, 0.6f);
        dialogInProgress = false;
    }

    private void ShowNextLine()
    {


        if (curDialogueIndex < dialogKeys.Count &&
            curLineIndex < dialogKeys[curDialogueIndex].Length)
        {
            StartCoroutine(TypeDialogue(LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex])));
        }
        else
        {
            curDialogueIndex++;
            curLineIndex = 0;

            if (curDialogueIndex < dialogKeys.Count)
            {
                StartCoroutine(TypeDialogue(LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex])));
            }
            else
            {
                dialogText.text = "";
            }
        }

        curLineIndex++;
    }

    private IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
    }

}
