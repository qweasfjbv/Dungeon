using DG.Tweening;
using UnityEngine;

public class TreasureController : MonoBehaviour
{
    [SerializeField] private RectTransform treasureList;

    private bool isTListOpen;

    private void Awake()
    {
        isTListOpen = false;
    }

    private void Start()
    {
        Managers.Input.treasureAction -= OnKeyboard;
        Managers.Input.treasureAction += OnKeyboard;

        treasureList.localScale = new Vector3(0f, 1f, 1f);
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {

            if (Managers.Input.TreasureBlock) // 열려있음. 닫는 부분
            {
                Managers.Input.TreasureBlock = false;
                treasureList.DOScaleX(0f, 0.5f);
            }
            else
            {
                treasureList.DOScaleX(1f, 0.5f);
                Managers.Input.TreasureBlock = true;
            }
        }
    }


}
