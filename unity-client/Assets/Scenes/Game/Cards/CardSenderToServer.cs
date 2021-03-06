﻿using UnityEngine;
using UnibusEvent;

public class PlayCardAction
{
    public string cardId;
    public string x;
    public string y;
}

public class MoveCardAction
{
    public string cardId;
    public string x;
    public string y;
}

public class AttackCardAction
{
    public string attackerCardId;
    public string attackedCardId;
    public bool isRangeAttack;
    public AbilitiesParams abilitiesParams;
}

public class HealCardAction
{
    public string healerCardId;
    public string healedCardId;
}

public class SimpleAbilityCardAction
{
    public string cardId;
}

public class AbilitiesParams
{
    public Point pushAt;
    public string ricochetTargetCardId;
}

public class CardSenderToServer : MonoBehaviour
{
    void Start()
    {
        Unibus.Subscribe<CardDisplay>(CardDisplay.CARD_PLAY_AS_MANA, OnCardPlayAsMana);
        Unibus.Subscribe<PlayCardAction> (ActionEmmiter.CARD_PLAY, OnCardPlay);
        Unibus.Subscribe<MoveCardAction> (ActionEmmiter.CARD_MOVE, OnCardMove);
        Unibus.Subscribe<AttackCardAction> (ActionEmmiter.CARD_ATTACK, OnCardAttack);
        Unibus.Subscribe<AttackCardAction> (ActionEmmiter.CARD_MOVE_AND_ATTACK, OnCardMoveAndAttack);
        Unibus.Subscribe<HealCardAction> (ActionEmmiter.CARD_HEAL, OnCardHeal);
        Unibus.Subscribe<SimpleAbilityCardAction> (ActionEmmiter.CARD_USE_MANA_ABILITY, OnUseManaAbility);
        Unibus.Subscribe<SimpleAbilityCardAction> (ActionEmmiter.CARD_TO_AIM, OnToAim);
    }

    void Update() 
    {

    }

    async void OnCardPlayAsMana(CardDisplay card)
    {
        await ServerApi.PlayCardAsMana(card.cardData.id);
    }

    async void OnCardPlay(PlayCardAction action)
    {
        await ServerApi.PlayCard(action);
    }

    async void OnCardMove(MoveCardAction action)
    {
        await ServerApi.MoveCard(action);
    }

    async void OnCardAttack(AttackCardAction action)
    {
        await ServerApi.AttackCard(action);
    }

    async void OnCardMoveAndAttack(AttackCardAction action)
    {
        await ServerApi.MoveAndAttackCard(action);
    }

    async void OnCardHeal(HealCardAction action)
    {
        await ServerApi.HealCard(action);
    }

    async void OnUseManaAbility(SimpleAbilityCardAction action)
    {
        await ServerApi.UseManaAbility(action);
    }

    async void OnToAim(SimpleAbilityCardAction action)
    {
        await ServerApi.ToAim(action);
    }
}
