using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInHand : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    List<CardBase> cardsInHand = new List<CardBase>();

    const int CARD_INS_X = 0;
    const int CARD_INS_Y = 0;

    const int MAX_CARD_COUNT = 8;
    const float ANGLE_PER_CARD = 4f;

    const float CARD_POS_Y_OFFSET = 20f;



    [ContextMenu("AddCardInHand")]
    public bool AddCardInHand()
    {

        if (cardsInHand.Count >= 8)
        {
            Debug.Log("ī�� �ִ� ���� ����");
            return false;
        }

        var tmpCard = Instantiate(cardPrefab, transform.TransformPoint(new Vector3(0, Settings.HEIGHT/2, 0)), Quaternion.identity, transform);
        UpdateCardList();
        return true;
    }

    [ContextMenu("RemoveCardInHand")]
    public bool RemoveCardInHand()
    {
        if(cardsInHand.Count == 0)
        {
            Debug.Log("�̹� �������");
            return false;
        }

        cardsInHand[0].transform.SetParent(null);
        Destroy(cardsInHand[0].gameObject);


        UpdateCardList();

        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) AddCardInHand();
        else if(Input.GetKeyDown(KeyCode.A)) RemoveCardInHand();


    }

    private void UpdateCardList()
    {
        cardsInHand.Clear();
        int q = 0;
        foreach (Transform child in transform)
        {
            q++;
            cardsInHand.Add(child.GetComponent<CardBase>());
        }


        switch (cardsInHand.Count)
        {
            case 0:
                return;
            case 1:
                cardsInHand[0].SetTargetPosX(0);
                break;
            case 2:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 0.47f);
                cardsInHand[1].SetTargetPosX(CardBase.CARD_WIDTH * 0.53f);
                break;
            case 3:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 0.9f);
                cardsInHand[1].SetTargetPosX(0);
                cardsInHand[2].SetTargetPosX(CardBase.CARD_WIDTH * 0.9f);

                break;
            case 4:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 1.36f);
                cardsInHand[1].SetTargetPosX(-CardBase.CARD_WIDTH * 0.46f);
                cardsInHand[2].SetTargetPosX(CardBase.CARD_WIDTH * 0.47f);
                cardsInHand[3].SetTargetPosX(CardBase.CARD_WIDTH * 1.36f);
                break;
            case 5:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 1.7f);
                cardsInHand[1].SetTargetPosX(-CardBase.CARD_WIDTH * 0.9f);
                cardsInHand[2].SetTargetPosX(0);
                cardsInHand[3].SetTargetPosX(CardBase.CARD_WIDTH * 0.9f);
                cardsInHand[4].SetTargetPosX(CardBase.CARD_WIDTH * 1.7f);
                break;
            case 6:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 2.1f);
                cardsInHand[1].SetTargetPosX(-CardBase.CARD_WIDTH * 1.3f);
                cardsInHand[2].SetTargetPosX(-CardBase.CARD_WIDTH * 0.43f);
                cardsInHand[3].SetTargetPosX(CardBase.CARD_WIDTH * 0.43f);
                cardsInHand[4].SetTargetPosX(CardBase.CARD_WIDTH * 1.3f);
                cardsInHand[5].SetTargetPosX(CardBase.CARD_WIDTH * 2.1f);
                break;
            case 7:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 2.4f);
                cardsInHand[1].SetTargetPosX(-CardBase.CARD_WIDTH * 1.7f);
                cardsInHand[2].SetTargetPosX(-CardBase.CARD_WIDTH * 0.9f);
                cardsInHand[3].SetTargetPosX(0);
                cardsInHand[4].SetTargetPosX(CardBase.CARD_WIDTH * 0.9f);
                cardsInHand[5].SetTargetPosX(CardBase.CARD_WIDTH * 1.7f);
                cardsInHand[6].SetTargetPosX(CardBase.CARD_WIDTH * 2.4f);
                break;
            case 8:
                cardsInHand[0].SetTargetPosX(-CardBase.CARD_WIDTH * 2.5f);
                cardsInHand[1].SetTargetPosX(-CardBase.CARD_WIDTH * 1.82f);
                cardsInHand[2].SetTargetPosX(-CardBase.CARD_WIDTH * 1.1f);
                cardsInHand[3].SetTargetPosX(-CardBase.CARD_WIDTH * 0.38f);
                cardsInHand[4].SetTargetPosX(CardBase.CARD_WIDTH * 0.38f);
                cardsInHand[5].SetTargetPosX(CardBase.CARD_WIDTH * 1.1f);
                cardsInHand[6].SetTargetPosX(CardBase.CARD_WIDTH * 1.77f);
                cardsInHand[7].SetTargetPosX(CardBase.CARD_WIDTH * 2.5f);


                break;
            default:
                Debug.Log("ī�� �� ���� ����");
                return;
        }


        /*
         * angle�� posy ���� ����
         * �� ���� �ݺ������� ó�� ����
         */

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
                for (int t = 0; t < i; t++) offsetSum += CARD_POS_Y_OFFSET * Mathf.Pow(0.65f, t);

                cardsInHand[i].SetTargetPosY(offsetSum);
                cardsInHand[cardsInHand.Count - i - 1].SetTargetPosY(offsetSum);

            }

        }
        if (cardsInHand.Count % 2 != 0)
        {
            cardsInHand[cardsInHand.Count / 2].SetTargetAngle(0);
            float offsetSum = 0;

            for (int t = 0; t < cardsInHand.Count / 2; t++) offsetSum += CARD_POS_Y_OFFSET * Mathf.Pow(0.65f, t);

            cardsInHand[cardsInHand.Count /2].SetTargetPosY(offsetSum);
        }

        /*
         * ���콺 �ö����� �� �߰� ���� ó�� �κ�
         * scale�� CardBase���� �ø��ų� ����
         * ���⼭�� pos�� ���ϰ� angle�� ���
         */


    }

    private void UpdateWhenHovered()
    {
        int hoverIdx = -1;
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].IsHover)
            {
                hoverIdx = i; break;
            }
        }

        if (hoverIdx == -1) return;

        cardsInHand[hoverIdx].SetTargetAngle(0f);

        cardsInHand[hoverIdx].SetTargetPosY(cardsInHand[hoverIdx].GetTargetPosY() + CardBase.CARD_SCALE_HOVERED * CardBase.CARD_HEIGHT / 2f);

    }


    public void OnHover()
    {
        UpdateWhenHovered();
    }
    public void OnUnHover()
    {
        UpdateWhenHovered();
        UpdateCardList();
    }

}
