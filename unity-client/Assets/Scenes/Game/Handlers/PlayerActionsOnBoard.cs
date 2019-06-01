﻿using System.Collections;
using System.Collections.Generic;
using UnibusEvent;
using UnityEngine;

public class PlayerActionsOnBoard : MonoBehaviour
{
    public static readonly string CARD_MOVE = "PlayerActionsOnBoard:CARD_MOVE";
    public static readonly string CARD_ATTACK = "PlayerActionsOnBoard:CARD_ATTACK";

    public NoSelectionsState noSelectionsState;
    public OwnUnitSelectedState ownUnitSelectedState;
    public SelectingPushTargetState selectingPushTargetState;

    public ClickOutOfBoardEmmiter clickOutOfBoardEmmiter;

    public BoardCreator boardCreator;

    private void Awake()
    {
        boardCreator = this.transform.Find("Board").GetComponent<BoardCreator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        clickOutOfBoardEmmiter = new ClickOutOfBoardEmmiter();

        noSelectionsState = new NoSelectionsState(this);
        ownUnitSelectedState = new OwnUnitSelectedState(this);
        selectingPushTargetState = new SelectingPushTargetState(this);

        noSelectionsState.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        clickOutOfBoardEmmiter.CheckClickOutOfAnyCard();
    }

    public void EmmitCardAttackAction(UnitDisplay attackerUnit, UnitDisplay attackedUnit, Point pushPoint = null)
    {
        bool isCardsAdjacent = this.boardCreator.CheckCardsAdjacency(attackerUnit.gameObject, attackedUnit.gameObject);
        bool isRangeAttack = !isCardsAdjacent;

        AttackCardAction attackCardAction = new AttackCardAction
        {
            attackerCardId = attackerUnit.CardData.id,
            attackedCardId = attackedUnit.CardData.id,
            isRangeAttack = isRangeAttack
        };

        if (pushPoint != null)
        {
            AbilitiesParams abilitiesParams = new AbilitiesParams { attackedPushAttackedAt = pushPoint };

            attackCardAction.abilitiesParams = abilitiesParams;
        }

        Unibus.Dispatch<AttackCardAction>(PlayerActionsOnBoard.CARD_ATTACK, attackCardAction);
    }

    public void EmmitCardMoveAction(UnitDisplay movingUnit, Point point)
    {
        Unibus.Dispatch<MoveCardAction>(PlayerActionsOnBoard.CARD_MOVE, new MoveCardAction
        {
            cardId = movingUnit.CardData.id,
            x = point.x.ToString(),
            y = point.y.ToString()
        });
    }
}


public class ClickOutOfBoardEmmiter
{
    public static string CLICK_OUT_OF_BOARD = "ClickOutOfBoardEmmiter:CLICK_OUT_OF_BOARD";
    public static string RIGHT_CLICK = "ClickOutOfBoardEmmiter:RIGHT_CLICK";

    private bool MouseOnTile = false;
    private bool skipedFirstCheckClickOutOfAnyCard = false;

    public ClickOutOfBoardEmmiter()
    {
        Unibus.Subscribe<Point>(TileDisplay.TILE_MOUSE_ENTER, OnTileMouseEnter);
        Unibus.Subscribe<Point>(TileDisplay.TILE_MOUSE_EXIT, OnTileMouseExit);
    }

    private void OnTileMouseEnter(Point point)
    {
        this.MouseOnTile = true;
    }

    private void OnTileMouseExit(Point point)
    {
        this.MouseOnTile = false;
    }

    public void CheckClickOutOfAnyCard()
    {
        // Хоть убей я не придумал как сделать по другому.
        if (!this.skipedFirstCheckClickOutOfAnyCard)
        {
            this.skipedFirstCheckClickOutOfAnyCard = true;
        }
        else
        {
            var leftMouseClicked = Input.GetButtonDown("Fire1");
            if (leftMouseClicked && MouseOnTile == false)
            {
                Unibus.Dispatch<string>(ClickOutOfBoardEmmiter.CLICK_OUT_OF_BOARD, "");
            }

            var rightMouseClicked = Input.GetButtonDown("Fire2");
            if (rightMouseClicked)
            {
                Unibus.Dispatch<string>(ClickOutOfBoardEmmiter.RIGHT_CLICK, "");
            }
        }
    }
    
    public void Reset()
    {
        this.skipedFirstCheckClickOutOfAnyCard = false;
    }
}



public class NoSelectionsState
{
    private PlayerActionsOnBoard playerActionsOnBoard;

    public NoSelectionsState(PlayerActionsOnBoard playerActionsOnBoard)
    {
        this.playerActionsOnBoard = playerActionsOnBoard;
    }

    public void Enable()
    {
        Unibus.Subscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
    }

    private void Disable()
    {
        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
    }

    private void OnUnitSelectedOnBoard(UnitDisplay clickedUnitDisplay)
    {
        if (clickedUnitDisplay.CardData.ownerId == GameState.mainPlayerId)
        {
            this.Disable();
            this.playerActionsOnBoard.ownUnitSelectedState.Enable(clickedUnitDisplay);
        }
    }
}



public class OwnUnitSelectedState
{
    private PlayerActionsOnBoard playerActionsOnBoard;

    private bool MouseOnTile = false;
    private UnitDisplay selectedUnit;

    public OwnUnitSelectedState(PlayerActionsOnBoard playerActionsOnBoard)
    {
        this.playerActionsOnBoard = playerActionsOnBoard;
    }

    public void Enable(UnitDisplay selectedUnit)
    {
        this.Select(selectedUnit);
        this.playerActionsOnBoard.clickOutOfBoardEmmiter.Reset();
        Unibus.Subscribe<string>(ClickOutOfBoardEmmiter.CLICK_OUT_OF_BOARD, OnClickOutOfBoard);
        Unibus.Subscribe<string>(ClickOutOfBoardEmmiter.RIGHT_CLICK, OnRightClick);
        Unibus.Subscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
        Unibus.Subscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);
    }

    private void Disable()
    {
        Unibus.Unsubscribe<string>(ClickOutOfBoardEmmiter.CLICK_OUT_OF_BOARD, OnClickOutOfBoard);
        Unibus.Unsubscribe<string>(ClickOutOfBoardEmmiter.RIGHT_CLICK, OnRightClick);
        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
        Unibus.Unsubscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);
    }

    private void EnableNoSelectionsState()
    {
        this.UnselectSelected();
        Disable();
        this.playerActionsOnBoard.noSelectionsState.Enable();
    }

    private void EnableOwnUnitSelectedState(UnitDisplay unitDisplay)
    {
        this.UnselectSelected();
        this.Disable();
        this.playerActionsOnBoard.ownUnitSelectedState.Enable(unitDisplay);
    }

    private void EnableSelectingPushTargetState(UnitDisplay unitDisplay)
    {
        this.Disable();
        this.playerActionsOnBoard.selectingPushTargetState.Enable(this.selectedUnit, unitDisplay);
    }

    private void OnClickOutOfBoard(string clickEvent)
    {
        this.EnableNoSelectionsState();
    }

    private void OnRightClick(string clickEvent)
    {
        this.EnableNoSelectionsState();
    }

    private void OnUnitSelectedOnBoard(UnitDisplay clickedUnitDisplay)
    {
        if (clickedUnitDisplay.CardData.ownerId == GameState.mainPlayerId)
        {
            this.EnableOwnUnitSelectedState(clickedUnitDisplay);
        }
        else
        {
            if (this.selectedUnit.CardData.abilities.push != null)
            {
                this.EnableSelectingPushTargetState(clickedUnitDisplay);
            }
            else
            {
                this.playerActionsOnBoard.EmmitCardAttackAction(this.selectedUnit, clickedUnitDisplay);

                this.EnableNoSelectionsState();
            }
        }
    }

    private void OnClickedOnVoidTile(Point point)
    {
        this.playerActionsOnBoard.EmmitCardMoveAction(this.selectedUnit, point);

        this.EnableNoSelectionsState();
    }

    private void Select(UnitDisplay selectedUnit)
    {
        this.selectedUnit = selectedUnit;

        // Сделать чтобы tile слушал события selectedUnit и сам переключался.
        GameObject tile = this.playerActionsOnBoard.boardCreator.GetTileByUnit(selectedUnit.gameObject);
        tile.GetComponent<TileDisplay>().SelectedHighlightOn();

        selectedUnit.CardDisplay.SelectedHighlightOn();
    }

    private void UnselectSelected()
    {
        GameObject tile = this.playerActionsOnBoard.boardCreator.GetTileByUnit(this.selectedUnit.gameObject);
        tile.GetComponent<TileDisplay>().SelectedHighlightOff();

        this.selectedUnit.CardDisplay.SelectedHighlightOff();
    }
}



public class SelectingPushTargetState
{
    private PlayerActionsOnBoard playerActionsOnBoard;

    private UnitDisplay attackerSelectedUnit;
    private UnitDisplay attackedSelectedUnit;

    public SelectingPushTargetState(PlayerActionsOnBoard playerActionsOnBoard)
    {
        this.playerActionsOnBoard = playerActionsOnBoard;
    }

    public void Enable(UnitDisplay attackerSelectedUnit, UnitDisplay attackedSelectedUnit)
    {
        this.attackerSelectedUnit = attackerSelectedUnit;
        this.attackedSelectedUnit = attackedSelectedUnit;

        this.playerActionsOnBoard.clickOutOfBoardEmmiter.Reset();
        Unibus.Subscribe<string>(ClickOutOfBoardEmmiter.CLICK_OUT_OF_BOARD, OnClickOutOfBoard);
        Unibus.Subscribe<string>(ClickOutOfBoardEmmiter.RIGHT_CLICK, OnRightClick);
        Unibus.Subscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);

        this.Select(attackedSelectedUnit);
    }

    private void Disable()
    {
        Unibus.Unsubscribe<string>(ClickOutOfBoardEmmiter.CLICK_OUT_OF_BOARD, OnClickOutOfBoard);
        Unibus.Unsubscribe<string>(ClickOutOfBoardEmmiter.RIGHT_CLICK, OnRightClick);
        Unibus.Unsubscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);
    }

    private void EnableNoSelectionsState()
    {
        this.Unselect(this.attackerSelectedUnit);
        this.Unselect(this.attackedSelectedUnit);

        this.Disable();
        this.playerActionsOnBoard.noSelectionsState.Enable();
    }

    private void OnClickedOnVoidTile(Point point)
    {
        this.playerActionsOnBoard.EmmitCardAttackAction(this.attackerSelectedUnit, this.attackedSelectedUnit, point);

        this.EnableNoSelectionsState();
    }

    private void OnClickOutOfBoard(string clickEvent)
    {
        this.EnableNoSelectionsState();
    }

    private void OnRightClick(string clickEvent)
    {
        this.EnableNoSelectionsState();
    }

    private void Select(UnitDisplay selectedUnit)
    {
        // Сделать чтобы tile слушал события selectedUnit и сам переключался.
        GameObject tile = this.playerActionsOnBoard.boardCreator.GetTileByUnit(selectedUnit.gameObject);
        tile.GetComponent<TileDisplay>().SelectedHighlightOn();

        selectedUnit.CardDisplay.SelectedHighlightOn();
    }

    private void Unselect(UnitDisplay selectedUnit)
    {
        GameObject tile = this.playerActionsOnBoard.boardCreator.GetTileByUnit(selectedUnit.gameObject);
        tile.GetComponent<TileDisplay>().SelectedHighlightOff();

        selectedUnit.CardDisplay.SelectedHighlightOff();
    }
}