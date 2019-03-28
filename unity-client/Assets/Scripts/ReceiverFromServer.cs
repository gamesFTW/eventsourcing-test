﻿using UnityEngine;
using System;
using Newtonsoft.Json;

[Serializable]
public class Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

namespace ServerActions
{
    [Serializable]
    public class EndTurnAction
    {
        public int currentTurn;
        public string endedPlayerId;
        public string startedPlayerId;
        public MovingPoints[] cardsMovingPointsUpdated;
        public string[] cardsUntapped;
        public string[] cardsDrawn;
    }

    public class MovingPoints
    {
        public string id;
        public int currentMovingPoints;
    }

    [Serializable]
    public class PlayCardAsManaAction
    {
        public string cardId;
        public string playerId;
        public bool tapped;
    }

    [Serializable]
    public class PlayCardAction
    {
        public string cardId;
        public string playerId;
        public string[] manaCardsTapped;
        public int newHp;
        public bool tapped;
        public Point position;
    }

    [Serializable]
    public class MoveCardAction
    {
        public string cardId;
        public string playerId;
        public Point position;
        public int currentMovingPoints;
    }

    [Serializable]
    public class CardAttackedAction
    {
        public CardAfterBattle attackerCard;
        public CardAfterBattle attackedCard;
    }

    [Serializable]
    public class CardAfterBattle
    {
        public string id;
        public bool isTapped;
        public int newHp;
        public bool killed;
    }
}

public class ReceiverFromServer : MonoBehaviour
{
    private CardManager cardManger;

    void Awake()
    {
        cardManger = this.GetComponent<CardManager>();
    }

    public void ProcessAction(string type, int index, string message)
    {
        if (type == "EndTurnAction")
        {
            SocketData<ServerActions.EndTurnAction> data = JsonConvert.DeserializeObject<SocketData<ServerActions.EndTurnAction>>(message);
            this.OnEndTurnAction(data.actions[index]);
        }

        if (type == "PlayCardAsManaAction")
        {
            SocketData<ServerActions.PlayCardAsManaAction> data = JsonConvert.DeserializeObject<SocketData<ServerActions.PlayCardAsManaAction>>(message);
            this.OnPlayCardAsManaAction(data.actions[index]);
        }

        if (type == "MoveCardAction")
        {
            SocketData<ServerActions.MoveCardAction> data = JsonConvert.DeserializeObject<SocketData<ServerActions.MoveCardAction>>(message);
            this.OnMoveCardAction(data.actions[index]);
        }

        if (type == "PlayCardAction")
        {
            SocketData<ServerActions.PlayCardAction> data = JsonConvert.DeserializeObject<SocketData<ServerActions.PlayCardAction>>(message);
            this.OnPlayCardAction(data.actions[index]);
        }

        if (type == "CardAttackedAction")
        {
            SocketData<ServerActions.CardAttackedAction> data = JsonConvert.DeserializeObject<SocketData<ServerActions.CardAttackedAction>>(message);
            this.OnCardAttackedAction(data.actions[index]);
        }
    }

    public void OnEndTurnAction(ServerActions.EndTurnAction action)
    {
        cardManger.DrawCards(action.endedPlayerId, action.cardsDrawn);
        cardManger.UntapCards(action.endedPlayerId, action.cardsUntapped);

        GameState.playerIdWhoMakesMove = action.startedPlayerId;
    }

    public void OnPlayCardAction(ServerActions.PlayCardAction action)
    {
        cardManger.PlayCard(action.playerId, action.cardId, action.position, action.tapped, action.newHp);
        cardManger.UntapCards(action.playerId, action.manaCardsTapped);
    }

    public void OnPlayCardAsManaAction(ServerActions.PlayCardAsManaAction action)
    {
        cardManger.PlayCardAsMana(action.playerId, action.cardId, action.tapped);
    }

    public void OnMoveCardAction(ServerActions.MoveCardAction action)
    {
        cardManger.MoveCard(action.playerId, action.cardId, action.position, action.currentMovingPoints);
    }

    public void OnCardAttackedAction(ServerActions.CardAttackedAction action)
    {
        cardManger.CardWasInBattle(action.attackerCard);
        cardManger.CardWasInBattle(action.attackedCard);
    }
}
