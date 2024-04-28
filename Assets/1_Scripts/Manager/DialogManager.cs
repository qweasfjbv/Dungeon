using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;

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
    private List<GameObject> selects = new List<GameObject>();

    private const float SELECTEHEIGHT = 50f;
    private const float SELECTEDOFFSET = 30f;
    private const int EVENTKEYOFFSET = 1000;

    private bool dialogInProgress = false;
    private bool selectinProgress = false;
    private bool isTyping = false;

    private List<string> dialogKeys = new List<string>();
    private string prevDialogue = "";

    private int curLineIndex = 0;

    private int curSelectedIndex = 0;

    private float dialogHidePosY = -1000;
    private float dialogRevealPosY = 0;


    private void Start()
    {
        selectorImage.SetActive(false);
        dialogBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, dialogHidePosY, 0);

        Managers.Game.DialogDelegate = SetEvent;

        Managers.Input.dialogAction -= OnKeyboard;
        Managers.Input.dialogAction += OnKeyboard;
    }



    private void OnKeyboard()
    {

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
                if (curLineIndex < dialogKeys.Count)
                {
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

        if (Input.GetKeyDown(KeyCode.W) && selectinProgress)
        {
            SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.SelectChange);
            OnUpArrow();
        }
        else if (Input.GetKeyDown(KeyCode.S) && selectinProgress)
        {
            SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.SelectChange);
            OnDownArrow();
        }
        else if (Input.GetKeyDown(KeyCode.F) && selectinProgress)
        {
            OnChooseSelect();
        }
    }


    public void SetEvent(int id)
    {
        if (dialogInProgress) return;

        SoundManager.Instance.PlayNPCSound((Define.NpcSoundType)id);

            Managers.Input.DialogBlock = true;
        GetComponent<BlockPanelController>().OnBlock();

        SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ClickButton);
        DeleteSelection();

        // dialog Text table에서 받아오기
        eventInfo = Managers.Resource.GetEventInfo(id);


        // 대사 키 저장
        dialogKeys.Clear();
        for (int i = 0; i < eventInfo.eventDialogCnt; i++)
        {
            dialogKeys.Add((EVENTKEYOFFSET + eventInfo.eventID * 100 + 10 + i).ToString());
        }


        // Init
        dialogInProgress = true;
        dialogText.text = "";
        dialogActorName.text = LocalizationSettings.StringDatabase.GetLocalizedString("ActorTable", eventInfo.eventActor.ToString());


        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogRevealPosY, 0.6f).SetEase(Ease.OutBounce);

        prevDialogue = LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex]);
        ShowNextLine();
    }

    private void UnsetEvent()
    {
        Managers.Input.DialogBlock = false;

        GetComponent<BlockPanelController>().OffBlock();
        SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ShowButton);
        dialogBox.GetComponent<RectTransform>().DOAnchorPosY(dialogHidePosY, 0.6f).SetEase(Ease.InOutElastic);
        dialogInProgress = false;

        dialogKeys.Clear();
        curLineIndex = 0;

        prevDialogue = ""; 
        selectinProgress = false;

    }

    private void ShowNextLine()
    {
        if (prevDialogue.Length < 10)
            SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.ShortWrite);
        else if (prevDialogue.Length < 50)
            SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.MediumWrite);
        else
            SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.LongWrite);

        if (curLineIndex < dialogKeys.Count)
        {
            StartCoroutine(TypeDialogue(LocalizationSettings.StringDatabase.GetLocalizedString("DialogInfo", dialogKeys[curLineIndex])));
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
            yield return new WaitForSeconds(0.02f);
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
                 (EVENTKEYOFFSET + 100 * eventInfo.eventID + 20 + i).ToString());
        }
        
        curSelectedIndex = 0;
        SelectIndex(0);
        // 자동으로 첫번째 선택

    }

    private void DeleteSelection()
    {
        for (int i = 0; i < selects.Count; i++) Destroy(selects[i]);
        selects.Clear();

        selectorImage.SetActive(false);
        curSelectedIndex = 0;

        return;
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

    private void OnChooseSelect()
    {
        SoundManager.Instance.PlayWriteSound(Define.DialogSoundType.SelectChoose);
        UnsetEvent();

        EventManager.Instance.EventSelectTrigger(eventInfo.eventID, curSelectedIndex);

        return;
    }

}
