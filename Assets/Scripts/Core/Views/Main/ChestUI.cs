﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Manager.View;
using Manager.Sound;
using BackEnd;

public enum ChestState : byte
{
    Idle,
    Dismissing,
    Open,
    NONE = 99
}

[System.Serializable]
public class Chest
{
    public GameObject idleChest = null;
    public GameObject disChest = null;
    public GameObject openChest = null;

    public Image idleImage = null, disImage = null, openImage = null;
    public Text idleTimeText = null, disTimeText = null;
    public Text idleArenaText = null;
    public Text disDiamondText = null;

    public ChestKind chestKind = ChestKind.NONE;
    public ChestState chestState = ChestState.Idle;

    public int disTime = 0;
    public int diamondPrice = 0;

    public System.DateTime startTime = System.DateTime.Now;
}

// 메인상자
public partial class MainUI : BaseScreen<MainUI>
{
    // Main
    [Header("Chest")]
    public Chest[] myChests = null;
    public int curHaveChests = 0;
    public int curSelectMyChest = 0;
    public int curDisChest = -1;
    bool pack = false;
    // Main

    // Dismissing
    [Header("Dismissing")]
    [SerializeField] private Image disChestImg = null;
    [SerializeField] private Text disKindText = null;
    [SerializeField] private Text disGoldText = null, disCardText = null;
    [SerializeField] private Text disTimeText = null;
    [SerializeField] private Button disButton = null, openButton = null;
    // Dismissing

    // ChestOpen
    [SerializeField] private Image chestImg = null;                // 상자 종류 이미지
    [SerializeField] private Image cardGradeImg = null;                // 카드 등급 이미지
    [SerializeField] private Image cardImg = null, etcImg = null; // 카드 종류 이미지
    [SerializeField] private Text cardText = null;                // 카드 이름
    [SerializeField] private Text cardCountText = null;                // 카드 개수
    [SerializeField] private Text chestItemCountText = null;                // 상자 남은 아이템 개수 
    // ChestOpen

    // ETC
    int count = 0;

    public void ChestInit()
    {
        foreach (var chest in myChests)
        {
            chest.idleImage = chest.idleChest.transform.GetChild(0).GetComponent<Image>();
            chest.disImage = chest.disChest.transform.GetChild(0).GetComponent<Image>();
            chest.openImage = chest.openChest.transform.GetChild(0).GetComponent<Image>();

            chest.idleTimeText = chest.idleChest.transform.GetChild(1).GetComponent<Text>();
            chest.idleArenaText = chest.idleChest.transform.GetChild(2).GetComponent<Text>();

            chest.disTimeText = chest.disChest.transform.GetChild(0).GetComponent<Text>();
            chest.disDiamondText = chest.disChest.transform.GetChild(2).GetComponent<Text>();
        }
    }

    // 자신의 상자 설정
    public void AddChest(ChestKind kind, ChestState state = ChestState.Idle, string startTime = "", int index = 0)
    {
        if (curHaveChests == 3 || kind == ChestKind.NONE)
            return;

        myChests[index].chestKind = kind;
        myChests[index].chestState = state;

        if (startTime == "")
            myChests[index].startTime = System.DateTime.Now;
        else
            myChests[index].startTime = System.DateTime.Parse(startTime);

        myChests[index].disTime = 1;
        myChests[index].diamondPrice = ((int)kind + 1) * 10;

        myChests[index].idleTimeText.text = myChests[index].disTime + "분";
        myChests[index].idleArenaText.text = myChests[index].chestKind.ToString();

        myChests[index].disTimeText.text = myChests[index].disTime + "분";
        myChests[index].disDiamondText.text = myChests[index].diamondPrice.ToString();

        // 아레나 설정 등등 해주기 ...

        switch (myChests[index].chestState)
        {
            case ChestState.Idle:
                myChests[index].idleChest.SetActive(true);
                myChests[index].disChest.SetActive(false);
                myChests[index].openChest.SetActive(false);
                break;
            case ChestState.Dismissing:
                myChests[index].idleChest.SetActive(false);
                myChests[index].disChest.SetActive(true);
                myChests[index].openChest.SetActive(false);
                StartCoroutine(CheckingChest(index));
                break;
            case ChestState.Open:
                myChests[index].idleChest.SetActive(false);
                myChests[index].disChest.SetActive(false);
                myChests[index].openChest.SetActive(true);
                break;
            default:
                break;
        }

        BackEndServerManager.instance.myInfo.haveChestKind[index] = (int)kind;
        BackEndServerManager.instance.myInfo.haveChestState[index] = (int)state;

        curHaveChests++;
        BackEndServerManager.instance.myInfo.haveChests = curHaveChests;
        BackEndServerManager.instance.myInfo.disStartTime = startTime;

        PlayerStats.instance.SaveChest();
    }

    // 해당 인덱스의 상자 체킹 (열 시간이 지났는가 안지났는가)
    public void CheckMyChest(int index)
    {
        System.DateTime endTime = System.DateTime.Now;
        System.TimeSpan timeCal = endTime - myChests[index].startTime;

        Debug.Log("시작 시간 : " + myChests[index].startTime + " / 현재 시간 : " + endTime + " / 남은 시간 : " + timeCal.Minutes + "분");

        myChests[index].disTimeText.text = ((myChests[index].disTime - 1) - timeCal.Minutes) + "분" + (60 - timeCal.Seconds) + "초";

        if (myChests[index].disTime <= timeCal.Minutes && myChests[index].chestState == ChestState.Dismissing)
        {
            myChests[index].chestState = ChestState.Open;
            myChests[index].idleChest.SetActive(false);
            myChests[index].disChest.SetActive(false);
            myChests[index].openChest.SetActive(true);

            BackEndServerManager.instance.myInfo.haveChestState[index] = (int)ChestState.Open;
            CheckDis();
        }

        BackEndServerManager.instance.myInfo.disChest = curDisChest;
    }

    public void OnDismissing()
    {
        if (myChests[curSelectMyChest].chestState == ChestState.Idle && curDisChest == -1)
        {
            chestDisObject.SetActive(false);

            curDisChest = curSelectMyChest;

            myChests[curSelectMyChest].chestState = ChestState.Dismissing;
            myChests[curSelectMyChest].startTime = System.DateTime.Now;

            myChests[curSelectMyChest].idleChest.SetActive(false);
            myChests[curSelectMyChest].disChest.SetActive(true);
            myChests[curSelectMyChest].openChest.SetActive(false);

            StartCoroutine(CheckingChest(curSelectMyChest));

            BackEndServerManager.instance.myInfo.haveChestState[curSelectMyChest] = (int)ChestState.Dismissing;
            BackEndServerManager.instance.myInfo.disStartTime = myChests[curSelectMyChest].startTime.ToString("yyyy/MM/dd hh:mm:ss");
            BackEndServerManager.instance.myInfo.disChest = curDisChest;
        }
    }

    public void OpenDismissing(int index)
    {
        SoundPlayer.instance.PlaySound("Click");

        curSelectMyChest = index;

        disKindText.text = myChests[index].chestKind.ToString();

        if (myChests[index].chestState == ChestState.Idle)
        {
            disButton.gameObject.SetActive(true);
            openButton.gameObject.SetActive(false);
            disTimeText.text = myChests[index].disTime.ToString() + "분";

            // ...
            disGoldText.text = (index + 1) * 100 + "-" + (index + 1) * 999;
            disCardText.text = "x" + (index + 1) * 5;
        }
        else
        {
            disButton.gameObject.SetActive(false);
            openButton.gameObject.SetActive(true);
            disTimeText.text = "해제중";
        }

        chestDisObject.SetActive(true);
    }

    public void CloseDismissing()
    {
        chestDisObject.SetActive(false);
    }

    // 다이아몬드 체스트
    public void OpenDiamondChestUI(int index)
    {
        SoundPlayer.instance.PlaySound("Click");

        count = index + 2;
        chestItemCountText.text = count.ToString();

        chestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        if (index == (int)ChestKind.실버)
            chestImg.sprite = chestImgs[1];
        else if (index == (int)ChestKind.골드)
            chestImg.sprite = chestImgs[2];
        else if (index == (int)ChestKind.다이아)
            chestImg.sprite = chestImgs[3];
        else
            chestImg.sprite = chestImgs[0];

        chestImg.gameObject.SetActive(true);

        Invoke("ContinueOpenChest", 1.5f);
    }

    public void BuyDiamondInApp(int dia)
    {
        if (!BackEndServerManager.instance.myInfo.pack)
        {
            BackEndServerManager.instance.myInfo.pack = true;
            BackEndServerManager.instance.myInfo.diamond += dia;

            speHideObj.SetActive(true);
            SetGoldUI();
        }
    }

    public void OpenChestPack()
    {
        curSelectChest = 2;
        //SoundPlayer.instance.PlaySound("Click");
        //Debug.Log("다이아몬드로 상자 열음");

        //count = (int)myChests[curSelectChest].chestKind + 2;
        //chestItemCountText.text = count.ToString();

        //chestDisObject.SetActive(false);
        //chestOpenObject.SetActive(true);

        //chestImg.gameObject.SetActive(true);

        //myChests[curSelectChest].idleChest.SetActive(false);
        //myChests[curSelectChest].disChest.SetActive(false);
        //myChests[curSelectChest].openChest.SetActive(false);

        //Invoke("ContinueOpenChest", 1.5f);
        OpenDiamondChestUI((int)ChestKind.다이아);
    }

    public void OpenChestDiamondUI()
    {
        if (BackEndServerManager.instance.myInfo.diamond >= myChests[curSelectChest].diamondPrice)
        {
            SoundPlayer.instance.PlaySound("Click");
            Debug.Log("다이아몬드로 상자 열음");

            BackEndServerManager.instance.myInfo.diamond -= myChests[curSelectChest].diamondPrice;


            count = (int)myChests[curSelectMyChest].chestKind + 2;
            chestItemCountText.text = count.ToString();

            chestDisObject.SetActive(false);
            chestOpenObject.SetActive(true);

            chestImg.gameObject.SetActive(true);

            myChests[curSelectMyChest].idleChest.SetActive(false);
            myChests[curSelectMyChest].disChest.SetActive(false);
            myChests[curSelectMyChest].openChest.SetActive(false);

            Invoke("ContinueOpenChest", 1.5f);
        }
        else
        {
            StartCoroutine(OnShowBroadCast("보석 부족"));
        }
    }

    // 메인에서 잠금해제가 완료된 상자 터치할 때
    public void OpenChestUI(int index)
    {
        SoundPlayer.instance.PlaySound("Click");

        count = (int)myChests[curSelectMyChest].chestKind + 2;
        chestItemCountText.text = count.ToString();

        chestDisObject.SetActive(false);
        chestOpenObject.SetActive(true);

        if (index == (int)ChestKind.실버)
            chestImg.sprite = chestImgs[1];
        else if (index == (int)ChestKind.골드)
            chestImg.sprite = chestImgs[2];
        else if (index == (int)ChestKind.다이아)
            chestImg.sprite = chestImgs[3];
        else
            chestImg.sprite = chestImgs[0];

        chestImg.gameObject.SetActive(true);

        myChests[curSelectMyChest].idleChest.SetActive(false);
        myChests[curSelectMyChest].disChest.SetActive(false);
        myChests[curSelectMyChest].openChest.SetActive(false);

        Invoke("ContinueOpenChest", 1.5f);
    }

    // 그 다음에 상자를 열고 있을 때 남은 아이템들을 열어볼 때
    void ContinueOpenChest()
    {
        OpenChest(curSelectMyChest);
    }

    public void OpenChest(int index)
    {
        SoundPlayer.instance.PlaySound("Click");
        chestItemCountText.text = (count - 1).ToString();

        if (count == 0)
        {
            chestItemCountText.text = "0";

            chestImg.gameObject.SetActive(false);
            cardGradeImg.gameObject.SetActive(false);

            curHaveChests--;

            myChests[index].chestState = ChestState.NONE;

            BackEndServerManager.instance.myInfo.haveChestKind[index] = 99;
            BackEndServerManager.instance.myInfo.haveChestState[index] = 99;
            BackEndServerManager.instance.myInfo.disChest = curDisChest;
            BackEndServerManager.instance.myInfo.haveChests = curHaveChests;
            BackEndServerManager.instance.myInfo.disStartTime = "";

            SetGoldUI();
            CheckDis();

            CloseChest();

            PlayerStats.instance.Save();
            return;
        }
        else if (count == index + 2)
        {
            // 맨 처음 골드 부분
            int gold = Random.Range((index + 1) * 100, (index + 1) * 999);
            BackEndServerManager.instance.myInfo.gold += gold;

            cardText.text = "클로버";
            ShowResultCard(characterImgs.Length - 2, gold);
        }
        else if (count == 1)
        {
            // 확률적으로 다이아몬드
            if (Random.Range(0f, 101f) <= 30f)
            {
                // 다이아몬드
                int diamond = Random.Range(index + 1, (index + 1) * 5);
                BackEndServerManager.instance.myInfo.diamond += diamond;

                cardText.text = "다이아";
                ShowResultCard(characterImgs.Length - 1, diamond);
            }
            else
            {
                // 카드
                GiveCard(index);
            }
        }
        else
        {
            // 중간 부분 카드 줌
            GiveCard(index);
        }
        count--;
        //OpenChest(index, count);
    }

    public void OpenDiamondChest(ChestKind kind)
    {
        diamondChestDisObject.SetActive(false);
        OpenDiamondChestUI((int)kind);
        SetGoldUI();
    }

    public void CloseChest()
    {
        chestOpenObject.SetActive(false);
    }

    public IEnumerator CheckingChest(int index)
    {
        Debug.Log("체크 시작 : " + (GameManager.instance.gameState == GameManager.GameState.MatchLobby || myChests[index].chestState == ChestState.Open));

        while (GameManager.instance.gameState == GameManager.GameState.MatchLobby && myChests[index].chestState == ChestState.Dismissing)
        {
            CheckMyChest(index);
            yield return new WaitForSeconds(1f);
        }
    }

    public void CheckDis()
    {
        Debug.Log("체크 시작");
        foreach (var chest in myChests)
        {
            if (chest.chestState == ChestState.Dismissing)
            {
                return;
            }
        }
        Debug.Log("해제중인 상자 없음");
        curDisChest = -1;
        BackEndServerManager.instance.myInfo.disChest = curDisChest;
    }

    public void SettingMyChest()
    {
        if (BackEndServerManager.instance.myInfo.haveChests <= 0)
            return;

        Debug.Log("상자 셋팅 시작 : " + BackEndServerManager.instance.myInfo.haveChests);

        curDisChest = BackEndServerManager.instance.myInfo.disChest;

        for (int i = 0; i < 3; i++)
        {
            Debug.Log("상자 셋팅 중... " + i);
            if (BackEndServerManager.instance.myInfo.haveChestKind[i] != 99)
                AddChest((ChestKind)BackEndServerManager.instance.myInfo.haveChestKind[i], (ChestState)BackEndServerManager.instance.myInfo.haveChestState[i], BackEndServerManager.instance.myInfo.disStartTime, i);
        }
    }

    void GiveCard(int index)
    {
        if (index == (int)ChestKind.골드)
        {
            SendQueue.Enqueue(Backend.Probability.GetProbability, "976", callback =>
            {
                {
                    // 그 다음 카드 부분
                    if (callback.IsSuccess())
                    {
                        int card = -1;
                        int count = Random.Range((index + 1) * 2, (index + 1) * 5);

                        var log = callback.GetReturnValuetoJSON()["element"]["item"]["S"].ToString();
                        Debug.Log(log);

                        switch (log)
                        {
                            case "기사":
                                card = 0;
                                break;
                            case "벤전스":
                                card = 1;
                                break;
                            case "도끼":
                                card = 2;
                                break;
                            case "듀얼":
                                card = 3;
                                break;
                            case "스탭":
                                card = 4;
                                break;
                            case "시프":
                                card = 5;
                                break;
                            case "피오라":
                                card = 6;
                                break;
                            case "사이드":
                                card = 7;
                                break;
                            case "스미스":
                                card = 8;
                                break;
                            case "라운드":
                                card = 9;
                                break;
                            case "듀크":
                                card = 10;
                                break;
                            case "빈센트":
                                card = 11;
                                break;
                            case "플레타":
                                card = 12;
                                break;
                            case "더스틴":
                                card = 13;
                                break;
                            case "루이스":
                                card = 14;
                                break;
                            case "윌리":
                                card = 15;
                                break;
                            case "아일린":
                                card = 16;
                                break;
                            case "체이스":
                                card = 17;
                                break;
                            case "랄프":
                                card = 18;
                                break;
                            case "알버트":
                                card = 19;
                                break;
                            case "재클린":
                                card = 20;
                                break;
                            case "앤드류":
                                card = 21;
                                break;
                            case "콜린":
                                card = 22;
                                break;
                            case "찰스":
                                card = 23;
                                break;
                            case "케빈":
                                card = 24;
                                break;
                            case "다비":
                                card = 25;
                                break;
                            case "모냇":
                                card = 26;
                                break;
                            case "조지":
                                card = 27;
                                break;
                            case "아모스":
                                card = 28;
                                break;
                            case "던칸":
                                card = 29;
                                break;
                            case "로랜스":
                                card = 30;
                                break;
                            case "해럴드":
                                card = 31;
                                break;
                            case "스니퍼":
                                card = 32;
                                break;
                            case "레오나드":
                                card = 33;
                                break;
                            case "스팅":
                                card = 34;
                                break;
                            case "비아나":
                                card = 35;
                                break;
                            case "미셀":
                                card = 36;
                                break;
                            case "제이스":
                                card = 37;
                                break;
                            case "안토니":
                                card = 38;
                                break;
                            case "렉스":
                                card = 39;
                                break;
                            case "사무엘":
                                card = 40;
                                break;
                            case "에드윈":
                                card = 41;
                                break;
                            case "로이드":
                                card = 42;
                                break;
                            case "아돌프":
                                card = 43;
                                break;
                            case "아폴로":
                                card = 44;
                                break;
                            case "닐":
                                card = 45;
                                break;
                            case "마샤":
                                card = 46;
                                break;
                            case "리퍼":
                                card = 47;
                                break;
                            case "도글라스":
                                card = 48;
                                break;
                            case "스텔라":
                                card = 49;
                                break;
                            default:
                                break;
                        }

                        if (CheckHaveCard(card))
                        {
                            Debug.Log("캐릭터 있음 : " + card);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }
                        else
                        {
                            Debug.Log("캐릭터 없음");
                            BackEndServerManager.instance.myInfo.haveCharacters.Add(card);
                            BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
                            BackEndServerManager.instance.myInfo.levelExp.Add(1);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }

                        cardText.text = log;
                        ShowResultCard(card, count);

                        SetInventory();
                    }
                    else
                        Debug.Log("실패 !");
                }
            });
        }
        else if (index == (int)ChestKind.다이아)
        {
            SendQueue.Enqueue(Backend.Probability.GetProbability, "975", callback =>
            {
                {
                    // 그 다음 카드 부분
                    if (callback.IsSuccess())
                    {
                        int card = -1;
                        int count = Random.Range((index + 1) * 2, (index + 1) * 5);

                        var log = callback.GetReturnValuetoJSON()["element"]["item"]["S"].ToString();
                        Debug.Log(log);

                        switch (log)
                        {
                            case "기사":
                                card = 0;
                                break;
                            case "벤전스":
                                card = 1;
                                break;
                            case "도끼":
                                card = 2;
                                break;
                            case "듀얼":
                                card = 3;
                                break;
                            case "스탭":
                                card = 4;
                                break;
                            case "시프":
                                card = 5;
                                break;
                            case "피오라":
                                card = 6;
                                break;
                            case "사이드":
                                card = 7;
                                break;
                            case "스미스":
                                card = 8;
                                break;
                            case "라운드":
                                card = 9;
                                break;
                            case "듀크":
                                card = 10;
                                break;
                            case "빈센트":
                                card = 11;
                                break;
                            case "플레타":
                                card = 12;
                                break;
                            case "더스틴":
                                card = 13;
                                break;
                            case "루이스":
                                card = 14;
                                break;
                            case "윌리":
                                card = 15;
                                break;
                            case "아일린":
                                card = 16;
                                break;
                            case "체이스":
                                card = 17;
                                break;
                            case "랄프":
                                card = 18;
                                break;
                            case "알버트":
                                card = 19;
                                break;
                            case "재클린":
                                card = 20;
                                break;
                            case "앤드류":
                                card = 21;
                                break;
                            case "콜린":
                                card = 22;
                                break;
                            case "찰스":
                                card = 23;
                                break;
                            case "케빈":
                                card = 24;
                                break;
                            case "다비":
                                card = 25;
                                break;
                            case "모냇":
                                card = 26;
                                break;
                            case "조지":
                                card = 27;
                                break;
                            case "아모스":
                                card = 28;
                                break;
                            case "던칸":
                                card = 29;
                                break;
                            case "로랜스":
                                card = 30;
                                break;
                            case "해럴드":
                                card = 31;
                                break;
                            case "스니퍼":
                                card = 32;
                                break;
                            case "레오나드":
                                card = 33;
                                break;
                            case "스팅":
                                card = 34;
                                break;
                            case "비아나":
                                card = 35;
                                break;
                            case "미셀":
                                card = 36;
                                break;
                            case "제이스":
                                card = 37;
                                break;
                            case "안토니":
                                card = 38;
                                break;
                            case "렉스":
                                card = 39;
                                break;
                            case "사무엘":
                                card = 40;
                                break;
                            case "에드윈":
                                card = 41;
                                break;
                            case "로이드":
                                card = 42;
                                break;
                            case "아돌프":
                                card = 43;
                                break;
                            case "아폴로":
                                card = 44;
                                break;
                            case "닐":
                                card = 45;
                                break;
                            case "마샤":
                                card = 46;
                                break;
                            case "리퍼":
                                card = 47;
                                break;
                            case "도글라스":
                                card = 48;
                                break;
                            case "스텔라":
                                card = 49;
                                break;
                            default:
                                break;
                        }

                        if (CheckHaveCard(card))
                        {
                            Debug.Log("캐릭터 있음 : " + card);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }
                        else
                        {
                            Debug.Log("캐릭터 없음");
                            BackEndServerManager.instance.myInfo.haveCharacters.Add(card);
                            BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
                            BackEndServerManager.instance.myInfo.levelExp.Add(1);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }

                        cardText.text = log;
                        ShowResultCard(card, count);

                        SetInventory();
                    }
                    else
                        Debug.Log("실패 !");
                }
            });
        }
        else
        {
            SendQueue.Enqueue(Backend.Probability.GetProbability, "977", callback =>
            {
                {
                    // 그 다음 카드 부분
                    if (callback.IsSuccess())
                    {
                        int card = -1;
                        int count = Random.Range((index + 1) * 2, (index + 1) * 5);

                        var log = callback.GetReturnValuetoJSON()["element"]["item"]["S"].ToString();
                        Debug.Log(log);

                        switch (log)
                        {
                            case "기사":
                                card = 0;
                                break;
                            case "벤전스":
                                card = 1;
                                break;
                            case "도끼":
                                card = 2;
                                break;
                            case "듀얼":
                                card = 3;
                                break;
                            case "스탭":
                                card = 4;
                                break;
                            case "시프":
                                card = 5;
                                break;
                            case "피오라":
                                card = 6;
                                break;
                            case "사이드":
                                card = 7;
                                break;
                            case "스미스":
                                card = 8;
                                break;
                            case "라운드":
                                card = 9;
                                break;
                            case "듀크":
                                card = 10;
                                break;
                            case "빈센트":
                                card = 11;
                                break;
                            case "플레타":
                                card = 12;
                                break;
                            case "더스틴":
                                card = 13;
                                break;
                            case "루이스":
                                card = 14;
                                break;
                            case "윌리":
                                card = 15;
                                break;
                            case "아일린":
                                card = 16;
                                break;
                            case "체이스":
                                card = 17;
                                break;
                            case "랄프":
                                card = 18;
                                break;
                            case "알버트":
                                card = 19;
                                break;
                            case "재클린":
                                card = 20;
                                break;
                            case "앤드류":
                                card = 21;
                                break;
                            case "콜린":
                                card = 22;
                                break;
                            case "찰스":
                                card = 23;
                                break;
                            case "케빈":
                                card = 24;
                                break;
                            case "다비":
                                card = 25;
                                break;
                            case "모냇":
                                card = 26;
                                break;
                            case "조지":
                                card = 27;
                                break;
                            case "아모스":
                                card = 28;
                                break;
                            case "던칸":
                                card = 29;
                                break;
                            case "로랜스":
                                card = 30;
                                break;
                            case "해럴드":
                                card = 31;
                                break;
                            case "스니퍼":
                                card = 32;
                                break;
                            case "레오나드":
                                card = 33;
                                break;
                            case "스팅":
                                card = 34;
                                break;
                            case "비아나":
                                card = 35;
                                break;
                            case "미셀":
                                card = 36;
                                break;
                            case "제이스":
                                card = 37;
                                break;
                            case "안토니":
                                card = 38;
                                break;
                            case "렉스":
                                card = 39;
                                break;
                            case "사무엘":
                                card = 40;
                                break;
                            case "에드윈":
                                card = 41;
                                break;
                            case "로이드":
                                card = 42;
                                break;
                            case "아돌프":
                                card = 43;
                                break;
                            case "아폴로":
                                card = 44;
                                break;
                            case "닐":
                                card = 45;
                                break;
                            case "마샤":
                                card = 46;
                                break;
                            case "리퍼":
                                card = 47;
                                break;
                            case "도글라스":
                                card = 48;
                                break;
                            case "스텔라":
                                card = 49;
                                break;
                            default:
                                break;
                        }

                        if (CheckHaveCard(card))
                        {
                            Debug.Log("캐릭터 있음 : " + card);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }
                        else
                        {
                            Debug.Log("캐릭터 없음");
                            BackEndServerManager.instance.myInfo.haveCharacters.Add(card);
                            BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
                            BackEndServerManager.instance.myInfo.levelExp.Add(1);

                            var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == card);
                            BackEndServerManager.instance.myInfo.levelExp[value] += count;
                        }

                        cardText.text = log;
                        ShowResultCard(card, count);

                        SetInventory();
                    }
                    else
                        Debug.Log("실패 !");
                }
            });
        }
    }

    // 상자 열때 무슨 아이템 나왔는지 보여주는 함수
    void ShowResultCard(int index, int count)
    {
        Debug.Log("아이템 개수 : " + count);

        // 애니메이션 재생을 위함
        cardGradeImg.gameObject.SetActive(false);
        cardGradeImg.gameObject.SetActive(true);
        // 애니메이션 재생을 위함

        cardImg.gameObject.SetActive(false);
        etcImg.gameObject.SetActive(false);
        if (index == characterImgs.Length - 2) // 골드 일경우
        {
            cardCountText.text = count.ToString();
            etcImg.sprite = characterImgs[index];
            etcImg.gameObject.SetActive(true);
        }
        else if (index == characterImgs.Length - 1)
        {
            cardCountText.text = count.ToString();
            etcImg.sprite = characterImgs[index];
            etcImg.gameObject.SetActive(true);
        }
        else
        {
            cardCountText.text = "x" + count;
            cardImg.sprite = characterImgs[index];
            cardImg.gameObject.SetActive(true);
        }

        // 상자랑 희귀도 차이도 나중에 나눠야됨
    }
}