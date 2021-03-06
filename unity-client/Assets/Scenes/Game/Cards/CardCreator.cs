﻿using System.Collections.Generic;
using UnityEngine;

public class PlayerTransformsStacks
{
    public Transform deck;
    public Transform hand;
    public Transform manaPool;
    public Transform table;
    public Transform graveyard;
}

public class CardCreator : MonoBehaviour
{
    public Transform CardPrefab;

    public Transform PlayerDeck;
    public Transform PlayerHand;
    public Transform PlayerManaPool;
    public Transform PlayerTable;
    public Transform PlayerGraveyard;

    public Transform OpponentDeck;
    public Transform OpponentHand;
    public Transform OpponentManaPool;
    public Transform OpponentTable;
    public Transform OpponentGraveyard;

    private BoardCreator boardCreator;
    private CardsContainer cardsContainer;

    public Dictionary<string, Transform> cardIdToCards = new Dictionary<string, Transform>();

    public Dictionary<string, PlayerTransformsStacks> playersTransformsStacks = new Dictionary<string, PlayerTransformsStacks>();

    public void Awake()
    {
        boardCreator = this.transform.Find("Board").GetComponent<BoardCreator>();
        cardsContainer = this.transform.Find("CardsContainer").GetComponent<CardsContainer>();
    }

    public void Start ()
    {
    }

    public List<CardDisplay> CreateCards(GameData gameData)
    {
        CardData[][] stacksData = this.CreateStacksData(gameData);

        Transform[] stacksTransforms = new Transform[] {
            PlayerDeck, PlayerHand, PlayerManaPool, PlayerTable, PlayerGraveyard,
            OpponentDeck, OpponentHand, OpponentManaPool, OpponentTable, OpponentGraveyard
        };

        List<CardDisplay> cardDisplays = new List<CardDisplay>();

        for (int i = 0; i < stacksTransforms.Length; i++)
        {
            foreach (CardData card in stacksData[i])
            {
                string playerId;
                // Простите меня за такое
                if (i <= 3)
                {
                    playerId = GameState.mainPlayerId;
                } else
                {
                    playerId = GameState.enemyOfMainPlayerId;
                }

                CardDisplay cardDisplay = CreateCardIn(card, playerId, stacksTransforms[i]);
                cardDisplays.Add(cardDisplay);
            }
        }

        this.CreateAreas(gameData.areas);

        return cardDisplays;
    }

    private void CreateAreas(AreaData[] areas)
    {
        foreach (AreaData area in areas)
        {
            boardCreator.CreateArea(area);
        }
    }

    private CardData[][] CreateStacksData(GameData gameData)
    {
        string playerId = GameState.mainPlayerId;

        PlayerData player;
        PlayerData opponent;

        if (gameData.player1.id == playerId)
        {
            player = gameData.player1;
            opponent = gameData.player2;
        }
        else
        {
            player = gameData.player2;
            opponent = gameData.player1;
        }

        CardData[][] stacksData = new CardData[][] {
            player.deck, player.hand, player.manaPool, player.table, player.graveyard,
            opponent.deck, opponent.hand, opponent.manaPool, opponent.table, opponent.graveyard,
        };

        playersTransformsStacks.Add(player.id, new PlayerTransformsStacks { deck = PlayerDeck, hand = PlayerHand, manaPool = PlayerManaPool, table = PlayerTable, graveyard = PlayerGraveyard });
        playersTransformsStacks.Add(opponent.id, new PlayerTransformsStacks { deck = OpponentDeck, hand = OpponentHand, manaPool = OpponentManaPool, table = OpponentTable, graveyard = OpponentGraveyard });

        return stacksData;
    }

    private CardDisplay CreateCardIn(CardData cardData, string playerId, Transform stack)
    {
        Transform newCard = (Transform)Instantiate(CardPrefab, new Vector2(0, 0), new Quaternion());
        cardIdToCards.Add(cardData.id, newCard);

        CardDisplay cardDisplay = newCard.GetComponent<CardDisplay>();

        cardsContainer.AddCard(cardDisplay);

        cardData.ownerId = playerId;
        cardDisplay.cardData = cardData;

        stack.GetComponent<StackDisplay>().AddCard(cardDisplay);

        if (cardData.alive)
        {
            boardCreator.CreateUnit(cardDisplay, new Point(cardData.x, cardData.y));
        }

        if (stack.GetComponent<StackDisplay>().IsFaceUp)
        {
            cardDisplay.FaceUp();
        } else
        {
            cardDisplay.FaceDown();
        }

        if (cardData.tapped)
        {
            cardDisplay.Tap();
        }

        if (cardData.hero)
        {
            if (playerId == GameState.mainPlayerId) {
                boardCreator.allyHeroes.Add(cardDisplay);
            }
        }

        return cardDisplay;
    }
}
