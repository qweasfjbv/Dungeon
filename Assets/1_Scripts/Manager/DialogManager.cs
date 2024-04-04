using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TextMeshProUGUI dialogActorName;
    [SerializeField] private Image dialogActorSprite;

    [SerializeField] private TextMeshProUGUI dialogText;

    private bool dialogInProgress = false;

    private List<string> dialogTexts = new List<string>();
    private string prevDialogue = "";

    private int curDialogueIndex = 0;
    private int curLineIndex = 0;
    private bool isTyping = false;

    private float dialogHidePosY = -1000;
    private float dialogRevealPosY = 0;


    private void Start()
    {
        dialogTexts.Add("Welcome to \n\"K-pler project\"!!  \n (space)");
        dialogTexts.Add("HIHIHIIHHIHIIHIHIHHIHIHIIHHIHHIH");
        dialogTexts.Add("QWEQWEWQEQWEWQEQWEQWEWQEQWE");

        dialogBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, dialogHidePosY, 0);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!dialogInProgress) SetEvent(0);
            else UnsetEvent();
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
                if (curLineIndex < dialogTexts.Count) {
                    prevDialogue = dialogTexts[curLineIndex];
                    ShowNextLine();
                }
                else
                {
                    // 여기 이벤트 선택지 생성하면됨
                }
            }
        }
    }


    private void SetActor(Define.ActorID actorId)
    {
        return;
    }

    private void SetEvent(int id)
    {
        var test = Managers.Resource.GetEventInfo(id);
        // List<string> 으로 대화 받아옴
        dialogInProgress = true;
        dialogText.text = "";

        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogRevealPosY, 0.6f).SetEase(Ease.OutBounce);

        prevDialogue = dialogTexts[curLineIndex];
        ShowNextLine();
    }

    private void UnsetEvent()
    {
        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogHidePosY, 0.6f);
        dialogInProgress = false;
    }

    private void ShowNextLine()
    {


        if (curDialogueIndex < dialogTexts.Count &&
            curLineIndex < dialogTexts[curDialogueIndex].Length)
        {
            StartCoroutine(TypeDialogue(dialogTexts[curLineIndex]));
        }
        else
        {
            curDialogueIndex++;
            curLineIndex = 0;

            if (curDialogueIndex < dialogTexts.Count)
            {
                StartCoroutine(TypeDialogue(dialogTexts[curLineIndex]));
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
