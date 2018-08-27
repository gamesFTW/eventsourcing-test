import { getType, getReturnOfExpression } from 'typesafe-actions';
import lodash from 'lodash';

import * as cardsActions from './actions';
import { CardData, CardId } from '../../../typings/Cards';
const returnsOfActions = Object.values(cardsActions).map(getReturnOfExpression);
type Action = typeof returnsOfActions[number];

type CardsState = {
  player: PlayerCards;
  opponent: PlayerCards;
  allCards: {[id: string]: CardWithPositionData};
};

type PlayerCards = {
  deck: CardId[];
  hand: CardId[];
  mannaPool: CardId[];
  table: CardId[];
  graveyard: CardId[];
};

interface CardWithPositionData extends CardData {
  position: {
    screenX?: number;
    screenY?: number;
  };
}

const initState: CardsState = {
  player: {
    deck: [],
    hand: [],
    mannaPool: [],
    table: [],
    graveyard: []
  },
  opponent: {
    deck: [],
    hand: [],
    mannaPool: [],
    table: [],
    graveyard: []
  },
  allCards: {}
};

function returnIds (cards: CardWithPositionData[]): CardId[] {
  return cards.map((card: CardWithPositionData) => card.id);
}

const cardsReducer = (state: CardsState = initState, action: Action) => {
  switch (action.type) {
    case getType(cardsActions.initCards):
      return initCardsReducer(state, action);

    case getType(cardsActions.updateCards):
      return updateCardsReducer(state, action);

    case getType(cardsActions.cardPlaceChangePosition):
      return cardPlaceChangePositionReducer(state, action);

    case getType(cardsActions.drawCard):
      return drawCardReducer(state, action);

    default:
      return state;
  }
};

// FIXME: Посоны из будущего, нужно рефакторить. Но сил нет. Сорян. Конфеты нет.
function initCardsReducer (state: CardsState, action: Action): CardsState {
  let player = action.payload.player;
  let opponent = action.payload.opponent;

  let allCardList = player.deck.concat(
    player.hand,
    player.mannaPool,
    player.table,
    player.graveyard,
    opponent.deck,
    opponent.hand,
    opponent.mannaPool,
    opponent.table,
    opponent.graveyard
  );

  let allCards = {} as {[id: string]: CardWithPositionData};

  allCardList.forEach((card: CardWithPositionData) => {
    allCards[card.id] = card;
  });

  return {
    player: {
      deck: returnIds(player.deck),
      hand: returnIds(player.hand),
      mannaPool: returnIds(player.mannaPool),
      table: returnIds(player.table),
      graveyard: returnIds(player.graveyard)
    },
    opponent: {
      deck: returnIds(opponent.deck),
      hand: returnIds(opponent.hand),
      mannaPool: returnIds(opponent.mannaPool),
      table: returnIds(opponent.table),
      graveyard: returnIds(opponent.graveyard)
    },
    allCards: allCards
  };
}

function updateCardsReducer (state: CardsState, action: Action): CardsState {
  let player = action.payload.player;
  let opponent = action.payload.opponent;

  let allCardList = player.deck.concat(
    player.hand,
    player.mannaPool,
    player.table,
    player.graveyard,
    opponent.deck,
    opponent.hand,
    opponent.mannaPool,
    opponent.table,
    opponent.graveyard
  );

  let allCards = {} as {[id: string]: CardWithPositionData};

  allCardList.forEach((card: CardWithPositionData) => {
    card.position = state.allCards[card.id].position;
    allCards[card.id] = card;
  });

  let newState = {
    player: {
      deck: returnIds(player.deck),
      hand: returnIds(player.hand),
      mannaPool: returnIds(player.mannaPool),
      table: returnIds(player.table),
      graveyard: returnIds(player.graveyard)
    },
    opponent: {
      deck: returnIds(opponent.deck),
      hand: returnIds(opponent.hand),
      mannaPool: returnIds(opponent.mannaPool),
      table: returnIds(opponent.table),
      graveyard: returnIds(opponent.graveyard)

    },
    allCards: allCards
  };

  return newState;
}

function cardPlaceChangePositionReducer (state: CardsState, action: Action): CardsState {
  let card = state.allCards[action.payload.id];

  if (card) {
    let newCard = lodash.clone(card);
    let allCards = lodash.clone(state.allCards);

    newCard.position = {
      screenX: action.payload.x,
      screenY: action.payload.y
    };

    allCards[card.id] = newCard;

    state.allCards = allCards;

    return {...state};
  } else {
    return state;
  }
}

function drawCardReducer (state: CardsState, action: Action): CardsState {
  let card = state.allCards[action.payload.id];

  if (card) {
    let cardIndex = state.player.deck.indexOf(card.id);

    let newDeck = lodash.clone(state.player.deck);
    newDeck.splice(cardIndex, 1);

    let newHand = lodash.clone(state.player.hand);
    newHand.push(card.id);

    let newPlayer = lodash.clone(state.player);
    newPlayer.hand = newHand;
    newPlayer.deck = newDeck;

    return {...state, player: newPlayer};
  } else {
    return state;
  }
}

export {Action, CardsState, PlayerCards, cardsReducer};