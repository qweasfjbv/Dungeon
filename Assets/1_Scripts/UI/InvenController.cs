using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenController : MonoBehaviour
{

    [SerializeField] private Button invenButton;
    [SerializeField] private Button invenCloseButton;
    [SerializeField] private List<Button> invenSortButton = new List<Button>();
    [SerializeField] private GameObject inventory;
    [SerializeField] private Transform invenContent;

    private Vector3 invenShowPos = new Vector3(0, 0);
    private Vector3 invenHidePos = new Vector3(0, 1500);

    private bool isShowing = false;

    private Define.CardType curSortedType = Define.CardType.None;

    private void Start()
    {

        isShowing = false;
        inventory.GetComponent<RectTransform>().anchoredPosition = invenHidePos;
        invenButton.onClick.RemoveAllListeners();
        invenButton.onClick.AddListener(Toggle);
        invenCloseButton.onClick.RemoveAllListeners();
        invenCloseButton.onClick.AddListener(Toggle);

        invenSortButton[0].onClick.RemoveAllListeners();
        invenSortButton[1].onClick.RemoveAllListeners();
        invenSortButton[0].onClick.AddListener(() => OnSortButtonClicked(Define.CardType.Summon));
        invenSortButton[1].onClick.AddListener(() => OnSortButtonClicked(Define.CardType.Magic));

        Managers.Input.invenAction -= OnKeyboard;
        Managers.Input.invenAction += OnKeyboard;
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.I))
            Toggle();
    }

    public void Toggle()
    {
        SoundManager.Instance.PlaySfxSound(Define.SFXSoundType.Paper);
        if (isShowing)
        {
            isShowing = false;
            Managers.Input.InvenBlock = false;
            GetComponent<BlockPanelController>().OffBlock();
            Hide();

        }
        else
        {
            isShowing = true;
            Managers.Input.InvenBlock = true;
            GetComponent<BlockPanelController>().OnBlock();
            Show();
        }
    }

    public void Hide()
    {
        inventory.GetComponent<RectTransform>().DOAnchorPos(invenHidePos, 0.7f).SetEase(Ease.OutCubic);
    }

    public void Show()
    {
        inventory.GetComponent<RectTransform>().DOAnchorPos(invenShowPos, 0.7f).SetEase(Ease.OutBounce);
    }


    private void OnSortButtonClicked(Define.CardType type)
    {

        SoundManager.Instance.PlaySfxSound(Define.SFXSoundType.Paper);
        if (curSortedType == type)
        {
            invenSortButton[(int)curSortedType].GetComponent<OnHover_2>().OnUnselected();
            curSortedType = Define.CardType.None;

            foreach (Transform tr in invenContent)
            {
                if (tr.GetComponent<CardBase>() != null) tr.gameObject.SetActive(true);
            }
            return;
        }

        curSortedType = type;
        invenSortButton[(int)curSortedType].GetComponent<OnHover_2>().OnSelected();
        invenSortButton[1 - (int)curSortedType].GetComponent<OnHover_2>().OnUnselected();

        foreach (Transform tr in invenContent)
        {
            if (tr.GetComponent<CardBase>() == null) continue;

            if (tr.GetComponent<CardBase>().Cardtype == type) tr.gameObject.SetActive(true);
            else tr.gameObject.SetActive(false);
        }
    }



}
