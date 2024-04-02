using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDecks : MonoBehaviour
{
    [SerializeField]
    private List<CardInHand> cardDecks = new List<CardInHand>();
    private int showingDeckIndex = -1;

    public static float HIDDEN_DECK_POS_Y = -500f;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowCardDeck(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowCardDeck(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShowCardDeck(2);
        }

        if (Input.GetKeyDown(KeyCode.A)) AddCardInDeck(11);
        if (Input.GetKeyDown(KeyCode.S)) AddCardInDeck(12);
        if (Input.GetKeyDown(KeyCode.D)) AddCardInDeck(13);
        if (Input.GetKeyDown(KeyCode.F)) AddCardInDeck(14);
        if (Input.GetKeyDown(KeyCode.G)) AddCardInDeck(15);
    }

    private void ShowCardDeck(int deckIdx)
    {
        if (showingDeckIndex != -1) cardDecks[showingDeckIndex].SetTargetPosY(HIDDEN_DECK_POS_Y);

        if (showingDeckIndex == deckIdx) {
            showingDeckIndex = -1;
            return; 
        }

        showingDeckIndex = deckIdx;
        cardDecks[showingDeckIndex].SetTargetPosY(0);
    }

    public void AddCardInDeck(int cardId)
    {
        if (cardId == -1) return;

        cardDecks[(int)Managers.Resource.GetCardInfo(cardId).cardType].AddCardInHand(cardId);
    }


}
