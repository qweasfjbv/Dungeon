using System.Collections.Generic;
using UnityEngine;

public class CardInHand : MonoBehaviour
{

    private List<CardBase> cardsInHand = new List<CardBase>();

    private Vector3 targetPos = new Vector3(0, CardDecks.HIDDEN_DECK_POS_Y, 0);

    [SerializeField] private GameObject monsterPrefab;

    public void SetTargetPosY(float y)
    {
        targetPos.y = y;
    }

    #region 상수값
    const int CARD_INS_X = 0;
    const int CARD_INS_Y = 0;

    const int MAX_CARD_COUNT = 8;
    const float ANGLE_PER_CARD = 4f;

    const float CARD_POS_Y_OFFSET = 20f;
    #endregion


    public bool AddCardInHand(int cardId)
    {

        if (cardsInHand.Count >= 8)
        {
            Debug.Log("카드 최대 개수 도달");
            return false;
        }

        var tmpCard = Instantiate(Managers.Resource.GetCardPrefab(cardId), transform.TransformPoint(new Vector3(0, Settings.HEIGHT/2, 0)), Quaternion.identity, transform);
        tmpCard.GetComponent<CardBase>().SetCard(cardId);
        UpdateCardLayout();
        return true;
    }

    public bool RemoveCardInHand(int id)
    {
        UpdateCardList();
        if (cardsInHand.Count == 0)
        {
            Debug.Log("이미 비어있음");
            return false;
        }


        cardsInHand[id].transform.SetParent(null);
        Destroy(cardsInHand[id].gameObject);



        return true;
    }

    private void Update()
    {
        var targetV = UtilFunctions.CardLerp(GetComponent<RectTransform>().anchoredPosition, targetPos, 6f);
        this.GetComponent<RectTransform>().anchoredPosition = new Vector3(targetV.x, targetV.y);

        if (Input.GetKeyDown(KeyCode.G))
        {

            var tmpCard = Instantiate(monsterPrefab, transform.TransformPoint(new Vector3(0, Settings.HEIGHT / 2, 0)), Quaternion.identity, transform);
            UpdateCardLayout();
        }

    }

    public void UpdateCardList()
    {
        cardsInHand.Clear();
        int q = 0;
        foreach (Transform child in transform)
        {
            q++;
            if (child.GetComponent<CardBase>().IsDragged) continue;
            cardsInHand.Add(child.GetComponent<CardBase>());
        }

    }

    public void UpdateCardLayout()
    {

        UpdateCardList();
        

        
        float posXUnit = 0.43f;
        float sum = 0;

        // posX 로직구현
        if (cardsInHand.Count % 2 == 0) sum += posXUnit;


        for (int i = cardsInHand.Count/2; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].SetTargetPosX(CardBase.CARD_WIDTH * sum);
            cardsInHand[cardsInHand.Count - i - 1].SetTargetPosX(-1 * CardBase.CARD_WIDTH * sum);
            sum += posXUnit * 2;
            posXUnit -= 0.05f;
        }
        


        //angle과 posy 로직 구현

        for (int i = 0; i < cardsInHand.Count/2; i++)
        {
            cardsInHand[i].SetTargetAngle(ANGLE_PER_CARD * (cardsInHand.Count/2 - i));
            cardsInHand[cardsInHand.Count -i -1].SetTargetAngle(-1 * ANGLE_PER_CARD * (cardsInHand.Count / 2 - i));

            float offsetSum = 0;
            if (i == 0) { 
                cardsInHand[i].SetTargetPosY(0);
                cardsInHand[cardsInHand.Count - i - 1].SetTargetPosY(0);
            }
            else
            {
                for (int t = 0; t < i; t++) offsetSum += CARD_POS_Y_OFFSET * Mathf.Pow(0.86f, t);

                cardsInHand[i].SetTargetPosY(offsetSum);
                cardsInHand[cardsInHand.Count - i - 1].SetTargetPosY(offsetSum);

            }

        }
        if (cardsInHand.Count % 2 != 0)
        {
            cardsInHand[cardsInHand.Count / 2].SetTargetAngle(0);
            float offsetSum = 0;

            for (int t = 0; t < cardsInHand.Count / 2; t++) offsetSum += CARD_POS_Y_OFFSET * Mathf.Pow(0.86f, t);

            cardsInHand[cardsInHand.Count /2].SetTargetPosY(offsetSum);
        }


    }

    private int HasAnyHovoeredCard()
    {
        int hoverIdx = -1;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].IsHover)
            {
                hoverIdx = i; break;
            }
        }

        return hoverIdx;
    }
    private void UpdateWhenHovered()
    {
        var hoverIdx = HasAnyHovoeredCard();
        if (hoverIdx == -1) return;


        // hover시 다른 카드들이 밀려나도록 만듦
        for (int i=0; i< cardsInHand.Count; i++)
        {
            if (i == hoverIdx) continue;

            cardsInHand[i].SetTargetPosX(cardsInHand[i].GetTargetPosX() + Mathf.Pow(0.86f, Mathf.Abs(i-hoverIdx)) * CardBase.CARD_WIDTH * Mathf.Sign(i-hoverIdx));
        }
    }


    public void OnHover()
    {
        UpdateWhenHovered();
    }
    public void OnUnHover()
    {
        UpdateCardLayout();
    }

}
