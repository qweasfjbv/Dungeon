using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvenController : MonoBehaviour
{
    [SerializeField] private GameObject inventory;
    [SerializeField] private Button invenButton;

    private Vector3 invenShowPos = new Vector3(0, 0);
    private Vector3 invenHidePos = new Vector3(0, 1500);

    private bool isShowing = false;

    private void OnEnable()
    {

        isShowing = false;
        inventory.GetComponent<RectTransform>().anchoredPosition = invenHidePos;
        invenButton.onClick.RemoveAllListeners();
        invenButton.onClick.AddListener(Toggle);
    }

    private void Toggle()
    {
        if (isShowing)
        {
            isShowing = false;
            // TODO : ��Ʈ������ �ű��  BlockPanelController ����
        }
        else
        {
            isShowing = true;
            // 
        }
    }

    
}
