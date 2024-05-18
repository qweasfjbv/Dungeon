using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardMerchant : MonoBehaviour
{

    private const int MAX_COUNT = 5;
    private const int WIDTH_UNIT = 300;
    List<GameObject> cardList = new List<GameObject>(); 


    [SerializeField] private Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(() => RemoveAll());
    }

    public void SetMerchant(Define.CardType type)
    {
        gameObject.SetActive(true);
        for(int i = 0;i<MAX_COUNT; i++)
        {
            // TODO : Range 변경 ( 상점에 맞게 )
            int cardId = Random.Range(1, 8);

            while (Managers.Resource.GetCardInfo(cardId).cardType != type)
            {
                cardId = Random.Range(1, 8);
            }

            GameObject tmpCard = Instantiate(Managers.Resource.GetCardPrefab(cardId), transform);
            tmpCard.GetComponent<RectTransform>().anchoredPosition = new Vector2((i - 2) * WIDTH_UNIT, 0);
            tmpCard.GetComponent<CardBase>().SetCard(cardId, false);

            tmpCard.AddComponent<Button>();
            tmpCard.GetComponent<Button>().onClick.AddListener(() => OnClickGoods(tmpCard));

            cardList.Add(tmpCard);
        }
    }

    private void RemoveAll()
    {
        foreach(GameObject card in cardList)
        {
            if(card != null) Destroy(card); 
        }
        cardList.Clear();
        Managers.Game.OnPositiveEventEnd();
        gameObject.SetActive(false);
    }

    private void OnClickGoods(GameObject go)
    {
        // TODO : 돈 검사 해야됨

        SoundManager.Instance.PlayEffectSound(Define.EffectSoundType.Coin);
        Managers.Inven.AddCard(go.GetComponent<CardBase>().CardID);
        Destroy(go);
    }

}
