﻿using UnibusEvent;
using UnityEngine;

public class OwnUnitSelectedState : SelectingState
{
    private bool MouseOnTile = false;
    private UnitDisplay selectedUnit;

    private TileDisplay hoveredTile;

    public OwnUnitSelectedState(PlayerActionsOnBoardStates states, BoardCreator boardCreator) : base(states, boardCreator) { }

    public void Enable(UnitDisplay selectedUnit)
    {
        this.Enable();

        this.selectedUnit = selectedUnit;

        selectedUnit.CardDisplay.Select();
        selectedUnit.ShowAbilities();

        this.boardCreator.ShowPathReach(selectedUnit);
        this.ShowRangeAttackReach(selectedUnit);

        Unibus.Dispatch(CardManager.CARD_SELECTED, selectedUnit.CardDisplay);

        Unibus.Subscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
        Unibus.Subscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);
        Unibus.Subscribe<AbilityActivated>(UnitDisplay.ABILITY_ACTIVATED, OnAbilityActivated);
        Unibus.Subscribe<UnitDisplay>(BoardCreator.UNIT_MOUSE_ENTER_ON_BOARD, OnUnitMouseEnterOnBoard);
        Unibus.Subscribe<UnitDisplay>(BoardCreator.UNIT_MOUSE_EXIT_ON_BOARD, OnUnitMouseExitOnBoard);
        Unibus.Subscribe<TileDisplay>(BoardCreator.TILE_WITHOUT_UNIT_MOUSE_ENTER_ON_BOARD, OnTileMouseEnterOnBoard);
        Unibus.Subscribe<TileDisplay>(BoardCreator.TILE_WITHOUT_UNIT_MOUSE_EXIT_ON_BOARD, OnTileMouseExitOnBoard);
        Unibus.Subscribe<CardDisplay>(CardDisplay.CARD_CLICKED, OnCardClicked);
        Unibus.Subscribe<CardDisplay>(CardManager.CARD_MOVED, OnCardMoved);
    }

    protected override void Disable()
    {
        base.Disable();
        this.boardCreator.RemoveAllBlinks();
        selectedUnit.HideAbilities();

        if (this.hoveredTile)
        {
            this.hoveredTile.HighlightOff();
        }

        CursorController.SetDefault();

        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_CLICKED_ON_BOARD, OnUnitSelectedOnBoard);
        Unibus.Unsubscribe<Point>(BoardCreator.CLICKED_ON_VOID_TILE, OnClickedOnVoidTile);
        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_MOUSE_ENTER_ON_BOARD, OnUnitMouseEnterOnBoard);
        Unibus.Unsubscribe<UnitDisplay>(BoardCreator.UNIT_MOUSE_EXIT_ON_BOARD, OnUnitMouseExitOnBoard);
        Unibus.Unsubscribe<TileDisplay>(BoardCreator.TILE_WITHOUT_UNIT_MOUSE_ENTER_ON_BOARD, OnTileMouseEnterOnBoard);
        Unibus.Unsubscribe<TileDisplay>(BoardCreator.TILE_WITHOUT_UNIT_MOUSE_EXIT_ON_BOARD, OnTileMouseExitOnBoard);
        Unibus.Unsubscribe<AbilityActivated>(UnitDisplay.ABILITY_ACTIVATED, OnAbilityActivated);
        Unibus.Unsubscribe<CardDisplay>(CardDisplay.CARD_CLICKED, OnCardClicked);
        Unibus.Unsubscribe<CardDisplay>(CardManager.CARD_MOVED, OnCardMoved);
    }

    protected override void EnableNoSelectionsState()
    {
        this.selectedUnit.CardDisplay.Unselect();
        Disable();
        this.states.noSelectionsState.Enable();
    }

    private void AttackEnemy(UnitDisplay enemyDisplay)
    {
        this.actionEmmiter.EmmitCardAttackAction(this.selectedUnit, enemyDisplay);

        this.EnableNoSelectionsState();
    }

    private void UseManaAbility()
    {
        this.actionEmmiter.EmmitCardUseManaAbilityAction(this.selectedUnit);

        this.EnableNoSelectionsState();
    }

    private void ToAim()
    {
        this.actionEmmiter.EmmitCardToAimAction(this.selectedUnit);

        this.EnableNoSelectionsState();
    }

    private void MoveUnit(Point point)
    {
        var tile = this.boardCreator.GetTileByPoint(point);
        tile.HighlightOff();

        this.actionEmmiter.EmmitCardMoveAction(this.selectedUnit, point);
    }

    private void ChangeSelectedToAnotherAlly(UnitDisplay unitDisplay)
    {
        this.selectedUnit.CardDisplay.Unselect();
        this.Disable();
        this.states.ownUnitSelectedState.Enable(unitDisplay);
    }

    private void EnableSelectingPushTargetState(UnitDisplay unitDisplay)
    {
        this.Disable();
        this.states.selectingPushTargetState.Enable(this.selectedUnit, unitDisplay);
    }

    private void EnableSelectingRicochetTargetState(UnitDisplay unitDisplay)
    {
        this.Disable();
        this.states.selectingRicochetTargetState.Enable(this.selectedUnit, unitDisplay);
    }

    private void EnableSelectingHealingTargetState(HealingAbility ability)
    {
        this.Disable();
        this.states.selectingHealingTargetState.Enable(this.selectedUnit, ability);
    }

    private void InteractWithUnit(UnitDisplay clickedUnitDisplay)
    {
        if (clickedUnitDisplay.CardDisplay.IsAlly)
        {
            if (!clickedUnitDisplay.CardData.tapped)
            {
                this.ChangeSelectedToAnotherAlly(clickedUnitDisplay);
            }
        }
        else
        {
            if (this.selectedUnit.CardData.abilities.push != null)
            {
                this.EnableSelectingPushTargetState(clickedUnitDisplay);
            }
            else if (this.selectedUnit.CardData.abilities.ricochet && this.boardCreator.UnitHaveRicochetTargetNearby(clickedUnitDisplay))
            {
                this.EnableSelectingRicochetTargetState(clickedUnitDisplay);
            }
            else
            {
                this.AttackEnemy(clickedUnitDisplay);
            }
        }
    }

    private void OnUnitSelectedOnBoard(UnitDisplay clickedUnitDisplay)
    {
        this.InteractWithUnit(clickedUnitDisplay);
    }

    private void OnCardClicked(CardDisplay cardDisplay)
    {
        this.InteractWithUnit(cardDisplay.UnitDisplay);
    }

    private void OnClickedOnVoidTile(Point point)
    {
        this.MoveUnit(point);
    }

    private void OnUnitMouseEnterOnBoard(UnitDisplay unit)
    {
        if (!unit.CardDisplay.IsAlly)
        {
            this.boardCreator.RemoveAllBlinks();
            Point attackerPosition = this.boardCreator.GetUnitPosition(this.selectedUnit);
            this.boardCreator.ShowPathReach(this.selectedUnit);

            var unitsAdjacent = this.boardCreator.CheckCardsAdjacency(unit, this.selectedUnit);
            var selectedUnitCanGoAndAttack = this.boardCreator.CanUnitGoToUnit(this.selectedUnit, unit);
            var selectedUnitRange = this.selectedUnit.CardData.abilities.range != null;

            if (unitsAdjacent)
            {
                unit.CardDisplay.OverHighlightOn();
                CursorController.SetAttack();
            }
            else if (selectedUnitRange)
            {
                var fromPosition = this.boardCreator.GetUnitPosition(this.selectedUnit);
                var positions = this.boardCreator.GetPositionsForRangeAttack(this.selectedUnit, fromPosition);

                if (!this.selectedUnit.CardData.abilities.range.blockedInBeginningOfTurn && this.boardCreator.CheckUnitInPoints(unit, positions))
                {
                    CursorController.SetRangeAttack();
                } else
                {
                    CursorController.SetRangeAttackForbidden();
                }
            }
            else if (selectedUnitCanGoAndAttack)
            {
                unit.CardDisplay.OverHighlightOn();
                CursorController.SetAttack();
            }
            else
            {
                CursorController.SetAttackForbidden();
            }
        }
        else
        {
            if (unit == this.selectedUnit)
            {
                CursorController.SetDefault();
                this.boardCreator.RemoveAllBlinks();
                this.boardCreator.ShowPathReach(this.selectedUnit);
                this.ShowRangeAttackReach(this.selectedUnit);
            }
            else
            {
                if (!unit.CardData.tapped)
                {
                    unit.CardDisplay.OverHighlightOn();
                    CursorController.SetPointer();
                    this.boardCreator.RemoveAllBlinks();
                    this.boardCreator.ShowPathReach(unit);
                    this.ShowRangeAttackReach(unit);
                }
            }
        }
    }

    private void OnUnitMouseExitOnBoard(UnitDisplay unit)
    {
        unit.CardDisplay.OverHighlightOff();
        CursorController.SetDefault();
        this.boardCreator.RemoveAllBlinks();
        this.boardCreator.ShowPathReach(this.selectedUnit);
    }

    private void OnTileMouseEnterOnBoard(TileDisplay tile)
    {
        this.hoveredTile = tile;
        this.boardCreator.RemoveAllBlinks();

        Point fromPosition = this.boardCreator.GetTilePosition(tile);
        this.ShowRangeAttackReach(this.selectedUnit, fromPosition);

        var points = this.boardCreator.ShowPathReach(this.selectedUnit);
        var tilePosition = this.boardCreator.GetTilePosition(tile);

        if (this.boardCreator.CheckTileInPoints(tile, points))
        {
            tile.HighlightOn();
            CursorController.SetPointer();
        } else
        {
            CursorController.SetPointerForbidden();
        }
    }

    private void OnTileMouseExitOnBoard(TileDisplay tile)
    {
        this.hoveredTile = null;
        tile.HighlightOff();
        CursorController.SetDefault();
        this.boardCreator.RemoveAllBlinks();
        this.boardCreator.ShowPathReach(this.selectedUnit);
    }

    private void OnAbilityActivated(AbilityActivated abilityActivated)
    {
        if (abilityActivated.ability is HealingAbility)
        {
            this.EnableSelectingHealingTargetState(abilityActivated.ability as HealingAbility);
        }

        if (abilityActivated.ability is ManaAbility)
        {
            this.UseManaAbility();
        }

        if (abilityActivated.ability is AimingAbility)
        {
            this.ToAim();
        }
    }

    private void ShowRangeAttackReach(UnitDisplay unit, Point fromPosition = null)
    {
        if (unit.CardData.abilities.range != null)
        {
            if (fromPosition == null)
            {
                fromPosition = this.boardCreator.GetUnitPosition(unit);
            }
            this.boardCreator.ShowRangeAttackReach(unit, fromPosition);
        }
    }

    private void OnCardMoved(CardDisplay cardDisplay)
    {
        if (cardDisplay.UnitDisplay == this.selectedUnit)
        {
            this.boardCreator.RemoveAllBlinks();

            this.boardCreator.ShowPathReach(this.selectedUnit);
            this.ShowRangeAttackReach(this.selectedUnit);

            if (this.boardCreator.hoveredUnit)
            {
                this.OnUnitMouseEnterOnBoard(this.boardCreator.hoveredUnit);
            }

            if (this.boardCreator.hoveredTile)
            {
                var unit = this.boardCreator.GetUnitByTile(this.boardCreator.hoveredTile);

                if (unit == this.selectedUnit)
                {
                    this.OnUnitMouseEnterOnBoard(this.selectedUnit);
                }
                else
                {
                    this.OnTileMouseEnterOnBoard(this.boardCreator.hoveredTile);
                }
            }
        }
    }
}
