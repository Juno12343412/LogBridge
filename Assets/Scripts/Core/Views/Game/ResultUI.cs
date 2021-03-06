﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Manager.View;
using Manager.Sound;
using BackEnd.Tcp;

// Result
public partial class GameUI : BaseScreen<GameUI>
{
    [SerializeField] private Text ResultText = null;
    [SerializeField] private Text ScoreText = null;

    [SerializeField] private Image timerImage = null;
    [SerializeField] private GameObject resultObject = null;
    [SerializeField] private GameObject rewardObject = null;

    public override void ShowScreen()
    {
        Debug.Log("ShowScreen");
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        Debug.Log("HideScreen");
        base.HideScreen();
    }

    public void ShowResultBoard(MatchGameResult matchGameResult)
    {
        Debug.Log("Result Board : " + matchGameResult + "/ Match Type : " + (int)BackEndMatchManager.instance.nowMatchType);
        BackEndMatchManager.instance.GetMyMatchRecord(0, null);

        foreach (var user in matchGameResult.m_winners)
        {
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                SoundPlayer.instance.PlaySound("WinSound");

                BackEndServerManager.instance.myInfo.point += 30;
                ResultText.text = "승리";
                ScoreText.text = BackEndServerManager.instance.myInfo.point + "+(30)";
            }
        }

        foreach (var user in matchGameResult.m_losers)
        {
            if (BackEndMatchManager.instance.IsMySessionId(user))
            {
                BackEndServerManager.instance.myInfo.point += 18;
                ResultText.text = "패배";
                ScoreText.text = BackEndServerManager.instance.myInfo.point + "+(18)";
            }
        }

        PlayerStats.instance.SavePoint();
        Invoke("StartShowResult", 2f);
    }

    void StartShowResult()
    {
        ShowScreen();
        resultObject.SetActive(true);
        if (BackEndServerManager.instance.myInfo.haveChests < 2 && ResultText.text == "승리")
        {
            Debug.Log("보상 지급");

            rewardObject.SetActive(true);
            BackEndServerManager.instance.myInfo.giveChest = true;
        }

        StartCoroutine(TimeCheck());
    }


    IEnumerator TimeCheck(float time = 3f)
    {
        float progress = time;
        yield return new WaitForSeconds(1f);

        while (progress >= 0f)
        {
            progress -= Time.unscaledDeltaTime;
            timerImage.fillAmount = progress / time;
            yield return null;
        }
        OnLeaveGameRoom();
    }

    public void OnLeaveGameRoom()
    {
        Debug.Log("Game Result");

        if (GameManager.instance.gameState != GameManager.GameState.MatchLobby)
        {
            GameManager.instance.ChangeState(GameManager.GameState.MatchLobby);
            if (!BackEndServerManager.instance.myInfo.ads)
                AdsManager.instance.AdsShow();
        }
    }
}