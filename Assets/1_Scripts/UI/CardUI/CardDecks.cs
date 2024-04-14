using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDecks : MonoBehaviour
{
    [SerializeField]
    private List<CardInHand> cardDecks = new List<CardInHand>();

    [SerializeField] private Button cardDrawButton;
    [SerializeField] private TextMeshProUGUI showingDeckText;
    private int showingDeckIndex = -1;

    public static float HIDDEN_DECK_POS_Y = -500f;


    private void Start()
    {
        cardDrawButton.onClick.RemoveListener(OnCardDraw);
        cardDrawButton.onClick.AddListener(OnCardDraw);

        showingDeckText.text = "";

        Managers.Input.cardAction -= OnKeyboard;
        Managers.Input.cardAction += OnKeyboard;
    }

    private void OnKeyboard()
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

        if (Input.GetKeyDown(KeyCode.R))
        {
            OnDeckChangeClicked();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            OnCardDraw();
        }
    }

    private void OnCardDraw()
    {
        int tmpId = Managers.Inven.DrawCard();

        if (tmpId == -1)
        {
            Managers.Inven.MoveUsedtoUnused();
        }
        else if (tmpId == -2)
        {
            Debug.Log("mana ∫Œ¡∑"); return;
        }

        AddCardInDeck(tmpId);

    }

    private void ShowCardDeck(int deckIdx)
    {
        SoundManager.Instance.PlayButtonSound(Define.ButtonSoundType.ShowButton);
        if (showingDeckIndex != -1) cardDecks[showingDeckIndex].SetTargetPosY(HIDDEN_DECK_POS_Y);

        if (showingDeckIndex == deckIdx) {
            showingDeckIndex = -1;
            showingDeckText.text = "";
            return; 
        }

        showingDeckIndex = deckIdx;
        showingDeckText.text = (showingDeckIndex+1).ToString();
        cardDecks[showingDeckIndex].SetTargetPosY(0);
    }

    public void AddCardInDeck(int cardId)
    {
        if (cardId == -1) return;

        cardDecks[(int)Managers.Resource.GetCardInfo(cardId).cardType].AddCardInHand(cardId);
    }

    public void OnDeckChangeClicked()
    {
        if (showingDeckIndex == -1)
        {
            ShowCardDeck(0);
        }
        else if (showingDeckIndex == cardDecks.Count - 1)
        {
            ShowCardDeck(showingDeckIndex);
        }
        else
        {
            ShowCardDeck(showingDeckIndex + 1);
        }
    }


}
