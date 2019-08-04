﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnibusEvent;
using LateExe;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public Text errorText;
    public GameObject changeTurn;
    public Text changeTurnText;

    private Executer executer;
    private InvokeId taskId;

    void Start()
    {
        Button endOfTurnButton = this.transform.Find("UI/EndOfTurn").GetComponent<Button>();
        endOfTurnButton.onClick.AddListener(OnClick);

        Unibus.Subscribe<string>(HttpRequest.HTTP_ERROR, OnHttpError);

        this.executer = new Executer(this);

        this.changeTurn.GetComponent<CanvasGroup>().DOFade(0, 0);

        Unibus.Subscribe<string>(ReceiverFromServer.TURN_ENDED, OnTurnEnded);
        Unibus.Subscribe<string>(CardCreator.GAME_BUILDED, OnGameBuilded);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    void OnClick()
    {
        ServerApi.EndOfTurn();
    }

    private void OnHttpError(string errorMessage)
    {
        if (!(this.taskId is null))
        {
            this.executer.StopExecute(this.taskId);
        }

        errorText.text = errorMessage;

        errorText.GetComponent<CanvasRenderer>().SetAlpha(0);

        var colorToFadeTo = new Color(1f, 1f, 1f, 1);
        errorText.CrossFadeColor(colorToFadeTo, 0.2f, true, true);

        this.taskId = this.executer.DelayExecute(10, x => {
            colorToFadeTo = new Color(1f, 1f, 1f, 0);
            errorText.CrossFadeColor(colorToFadeTo, 0.2f, true, true);
        });
    }

    private void OnGameBuilded(string _)
    {
        ShowTurn();
    }

    private void OnTurnEnded (string _)
    {
        ShowTurn();
    }

    private void ShowTurn()
    {
        if (GameState.isMainPlayerTurn)
        {
            changeTurnText.text = "Your turn";
        }
        else
        {
            changeTurnText.text = "Opponent turn";
        }

        var colorToFadeTo = new Color(1f, 1f, 1f, 1);
        this.changeTurn.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        this.executer.DelayExecute(1, x => {
            colorToFadeTo = new Color(1f, 1f, 1f, 0);
            this.changeTurn.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
        });
    }
}
