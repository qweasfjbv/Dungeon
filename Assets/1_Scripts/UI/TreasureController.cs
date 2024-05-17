using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TreasureController : MonoBehaviour
{
    [SerializeField] private RectTransform treasureList;
    [SerializeField] private Button treasureShowButton;
    [SerializeField] private GameObject itemPrefab;

    [SerializeField] private Vector2 padding;
    [SerializeField] private Vector2 spacing;

    [Space]
    [Header("Expand Duration")]
    [SerializeField] private float collapseDuration;
    [SerializeField] private float expandDuration;
    [SerializeField] private Ease expandEase;
    [SerializeField] private Ease collapseEase;

    [Space]
    [Header("Fade Duration")]
    [SerializeField] private float fadeInDuration;
    [SerializeField] private float fadeOutDuration;



    private List<TreasureListItem> treasureItems = new List<TreasureListItem>();
    private Vector2 mainButtonPos;
    private bool isExtended;

    private void Awake()
    {
        mainButtonPos = Vector2.zero;
    }

    private void Start()
    {
        Managers.Input.treasureAction -= OnKeyboard;
        Managers.Input.treasureAction += OnKeyboard;

        treasureShowButton.onClick.RemoveListener(Toggle);
        treasureShowButton.onClick.AddListener(Toggle);


        isExtended = false;
    }

    private void OnKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Toggle();
        }

    }


    private void Toggle()
    {

        int itemCount = treasureItems.Count;

        if (isExtended) // 열려있음. 닫는 부분
        {
            isExtended = false;

            for (int i = 0; i < itemCount; i++)
            {
                treasureItems[i].rect.DOAnchorPos(mainButtonPos, collapseDuration).SetEase(collapseEase);
                treasureItems[i].image.DOFade(0f, fadeOutDuration);
            }
        }
        else
        {

            for (int i = 0; i < itemCount; i++)
            {
                treasureItems[i].rect.DOAnchorPos(mainButtonPos + spacing * (i + 1) + padding, expandDuration).SetEase(expandEase);
                treasureItems[i].image.DOFade(1f, fadeInDuration);
            }

            isExtended = true;
        }
    }

    public void AddItemList(int id)
    {
        GameObject itemGo = Instantiate(itemPrefab, treasureList.position, Quaternion.identity, treasureList);
        
        // resourceManager에서 스프라이트/툴팁정보 받고 설정

        treasureItems.Add(itemGo.GetComponent<TreasureListItem>());
        Toggle(); Toggle();
    }

}
