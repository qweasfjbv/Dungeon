using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private TextMeshProUGUI dialogActorName;
    [SerializeField] private Image dialogActorSprite;
    [SerializeField] private TextMeshProUGUI dialogText;


    [Header("Select")]
    [SerializeField] private GameObject selectsParent;
    // Height 는 y: 50단위
    // select 됐을때에는 x : -20
    // selector는 y따라가고 -2.5 (offset)
    [SerializeField] private GameObject selectTextPrefab;
    [SerializeField] private GameObject selectorImage;

    private DialogEventInfo eventInfo;
    private List<GameObject> selects;

    private const float SELECTEHEIGHT = 50f;
    private const float SELECTEDOFFSET = 30f;


    private bool dialogInProgress = false;
    private bool selectinProgress = false;
    private bool isTyping = false;

    private List<string> dialogKeys = new List<string>();
    private string prevDialogue = "";

    private int curDialogueIndex = 0;
    private int curLineIndex = 0;

    private int curSelectedIndex = 0;

    private float dialogHidePosY = -1000;
    private float dialogRevealPosY = 0;


    private void Start()
    {

        selectorImage.SetActive(false);
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
                if (curLineIndex < dialogKeys.Count) {
                    prevDialogue = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex]);
                    ShowNextLine();
                }
                else
                {
                    if (!selectinProgress)
                    {
                        MakeSelection();
                        // 여기 이벤트 선택지 생성하면됨
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && selectinProgress)
        {
            OnUpArrow();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && selectinProgress) {
            OnDownArrow();
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
        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogHidePosY, 0.6f).SetEase(Ease.InOutElastic);
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

    private void MakeSelection()
    {

        selectinProgress = true;
        selectorImage.SetActive(true);
        selectorImage.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10f, 0);

        selects = new List<GameObject>();

        for (int i = 0; i < eventInfo.eventSelectCnt; i++)
        {
            selects.Add(Instantiate(selectTextPrefab, selectsParent.transform));
            selects[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0, SELECTEHEIGHT * (eventInfo.eventSelectCnt - i - 1), 0);
            selects[i].GetComponent<TextMeshProUGUI>().text = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", 
                 eventInfo.eventName + "_S" + i.ToString());
        }
        
        curSelectedIndex = 0;
        SelectIndex(0);
        // 자동으로 첫번째 선택

    }

    private void OnUpArrow()
    {
        SelectIndex((curSelectedIndex - 1) < 0 ? eventInfo.eventSelectCnt -1 : (curSelectedIndex - 1));
    }
    private void OnDownArrow()
    {
        SelectIndex((curSelectedIndex + 1) % eventInfo.eventSelectCnt);
    }

    private void SelectIndex(int ind)
    {
        selects[curSelectedIndex].GetComponent<RectTransform>().DOAnchorPosX(0, 0.5f);

        selectorImage.GetComponent<RectTransform>().anchoredPosition = selects[ind].GetComponent<RectTransform>().anchoredPosition - new Vector2(0, 5f);
        selects[ind].GetComponent<RectTransform>().DOAnchorPosX(-SELECTEDOFFSET, 0.5f);

        curSelectedIndex = ind;
    }
}
