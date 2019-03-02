﻿using UnityEngine;
using System.Net;
using System.IO;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

[Serializable]
public class CardData
{
    public string id;
    public Boolean alive;
    public Boolean tapped;
    public string name;
    public int maxHp;
    public int damage;
    public int manaCost;
    public int movingPoints;
    public int currentMovingPoints;
    public int currentHp;
    public int x;
    public int y;
}

[Serializable]
public class PlayerData
{
    public string id;
    public CardData[] deck;
    public CardData[] hand;
    public CardData[] manaPool;
    public CardData[] table;
    public CardData[] graveyard;
}

[Serializable]
public class GameData
{
    public string id;
    public string player1Id;
    public string player2Id;
    public string currentPlayersTurn;
    public string boardId;
    public PlayerData player1;
    public PlayerData player2;
}

public class ServerApi
{
    static public String gameId = "ewpnvs34xvqqcjxtby50agpbvxkrh0";
    static public String currentPlayerId = "xmq46b1a0wcmbz0s7pwkvlvjstpikg";
    static public String serverURL = "http://127.0.0.1:3000";

    public async static Task<GameData> GetGame()
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(String.Format(serverURL + "/getGame?gameId=" + gameId));
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync();

        GameData gameData = JsonUtility.FromJson<GameData>(responseContent);
        return gameData;
    }

    public async static Task EndOfTurn()
    {
        var values = new Dictionary<string, string>
        {
           { "gameId", gameId },
           { "playerId", currentPlayerId }
        };

        await HttpRequest.Post("/endTurn", values);
    }

    public async static Task PlayCardAsMana(string cardId)
    {
        var values = new Dictionary<string, string>
        {
           { "gameId", gameId },
           { "playerId", currentPlayerId },
           { "cardId", cardId }
        };

        await HttpRequest.Post("/playCardAsMana", values);
    }

    public async static Task PlayCard(Dictionary<string, string> values)
    {
        values.Add("gameId", gameId);
        values.Add("playerId", currentPlayerId);

        await HttpRequest.Post("/playCard", values);
    }
}