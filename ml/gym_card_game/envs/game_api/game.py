#!/usr/bin/python3

import requests
import logging

from .consts import API_URLS, CREATE_GAME_JSON

logger = logging.getLogger(__name__)



def create_game():
  r = requests.post(API_URLS["CREATE_GAME"], json = CREATE_GAME_JSON)
  game_id = r.json()["gameId"]
  logger.debug("Created", game_id)
  return game_id

def end_turn(raw_state):
  data = {
    "playerId": raw_state["game"]["currentPlayersTurn"],
    "gameId": raw_state["game"]["id"]
  }
  r = requests.post(API_URLS["END_TURN"], json = data)
  # game_id = r.json()["gameId"]
  logger.debug("End of turn")

def get_game_state(game_id):
  r = requests.get(API_URLS["GET_GAME"], {"gameId": game_id})
  if not r.status_code == 200:
    raise Exception('Error on get game {}'.format(game_id))
  return r.json()


def clear_cache_in_memory():
  r = requests.post(API_URLS["CLREAR_CACHE"])  
  if r.status_code == 200:
    logger.debug("Cache clreaed")

  
# getGame("v6sywzzngkg8bdeye0afxvmg6is4df")
