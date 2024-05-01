using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenController : MonoBehaviour
{

    [SerializeField] private Button invenButton;
    [SerializeField] private Button invenCloseButton;
    [SerializeField] private GameObject inventory;

    private Vector3 invenShowPos = new Vector3(0, 0);
    private Vector3 invenHidePos = new Vector3(0, 1500);

    private bool isShowing = false;

    private void Start()
    {

        isShowing = false;
        inventory.GetComponent<RectTransform>().anchoredPosition = invenHidePos;
        invenButton.onClick.RemoveAllListeners();
        invenButton.onClick.AddListener(Toggle);
        invenCloseButton.onClick.RemoveAllListeners();
        invenCloseButton.onClick.AddListener(Toggle);

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



}
