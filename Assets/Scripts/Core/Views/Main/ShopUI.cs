﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manager.View;
using Manager.Sound;
using UnityEngine.UI;

public enum ChestKind : byte
{
    한,
    조셉트,
    폴,
    굿,
    예아,
    백,
    오수,
    김도,
    준,
    실버,
    골드,
    다이아,
    MAX,
    NONE = 99
}

[System.Serializable]
public class DiamondChest
{
    public GameObject obj = null;  // 상자 오브젝트
    public Image chestImage = null; // 상자 이미지
    public Text chestNameText = null, chestArenaText = null;
    public Text chestPriceText = null;
    public int chestPrice = 100; // 상자 가격
    public ChestKind chestKind = 0; // 상자 종류
}

[System.Serializable]
public class Item
{
    public GameObject obj = null;  // 아이템 오브젝트
    public GameObject hideObj = null; // 아이템 품절 시 가리기용 오브젝트
    public Image itemImage = null; // 아이템 이미지
    public Image cloverImage = null;
    public Text itemNameText = null, itemCountText = null; // 아이템 이름과 개수
    public Text itemPriceText = null;
    public int itemPrice = 100; // 아이템 가격
    public int itemKind = 0; // 아이템 종류
    public int itemCount = 1, itemMaxCount = 1; // 아이템 현재,최대 개수
}

// 상점 관련
public partial class MainUI : BaseScreen<MainUI>
{
    // Product
    [Header("Product")]
    [SerializeField] private Sprite[] characterImgs = null; // 캐릭터 스프라이트
    [SerializeField] private Item[] items = null; // 상점에 배치되어 있는 아이템들
    int curSelectItem = 0; // 현재 고른 아이템

    [SerializeField] private Sprite[] chestImgs = null; // 상자 스프라이트
    [SerializeField] private DiamondChest[] chests = null; // 상점에 배치되어 있는 상자들
    int curSelectChest = 0; // 현재 고른 상자
    // Product

    // Purchase
    [Header("Purchase")]
    [SerializeField] private Image purchaseCharacterImg = null;
    [SerializeField] private Text itemNameText = null, itemCountText = null;
    [SerializeField] private Text itemPriceText = null;
    [SerializeField] private GameObject hideObj = null;

    [SerializeField] private Image purchaseChestImg = null;
    [SerializeField] private Text chestKindText = null;
    [SerializeField] private Text chestGoldText = null, chestCardText = null;
    [SerializeField] private Text chestPriceText = null;

    [SerializeField] private GameObject speHideObj = null;
    // Purchase

    void ShopInit()
    {
        foreach (var item in items)
        {
            item.itemImage = item.obj.transform.GetChild(0).GetComponent<Image>();
            item.hideObj = item.obj.transform.GetChild(1).gameObject;
            item.itemNameText = item.obj.transform.GetChild(2).GetComponent<Text>();
            item.itemCountText = item.obj.transform.GetChild(3).GetComponent<Text>();
            item.itemPriceText = item.obj.transform.GetChild(4).GetComponent<Text>();
            item.cloverImage = item.obj.transform.GetChild(5).GetComponent<Image>();
        }

        foreach (var chest in chests)
        {
            chest.chestImage = chest.obj.transform.GetChild(0).GetComponent<Image>();
            chest.chestNameText = chest.obj.transform.GetChild(1).GetComponent<Text>();
            chest.chestPriceText = chest.obj.transform.GetChild(2).GetComponent<Text>();
        }
    }

    // 상점 열기
    // 현재 카드 구매 가능여부 보여주기
    public void ShowShop()
    {
        foreach (var item in items)
        {
            if (item.itemCount <= 0)
            {
                hideObj.SetActive(true);

                item.hideObj.SetActive(true);
                item.itemCountText.text = itemCountText.text = "판매 종료";
                item.itemPriceText.text = itemPriceText.text = "";
                item.cloverImage.gameObject.SetActive(false);
            }
            else
            {
                item.hideObj.SetActive(false);
                item.itemCountText.text = "x" + item.itemMaxCount;
                item.itemPriceText.text = item.itemPrice.ToString();
                item.cloverImage.gameObject.SetActive(true);
            }
            item.itemNameText.text = ((CharacterKind)item.itemKind).ToString();
        }
        PlayerStats.instance.Save();
    }

    // 카드 구매
    // 카드가 다 소모되면 회색으로 바꿔주고 카드 줄여주기
    public void PurchaseItem()
    {
        if (BackEndServerManager.instance.myInfo.gold >= items[curSelectItem].itemPrice)
        {
            BackEndServerManager.instance.myInfo.gold -= items[curSelectItem].itemPrice;
            if (CheckHaveCard(items[curSelectItem].itemKind))
            {
                SoundPlayer.instance.PlaySound("Purchase");
                Debug.Log("캐릭터 있음");

                var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == items[curSelectItem].itemKind);
                BackEndServerManager.instance.myInfo.levelExp[value] += items[curSelectItem].itemCount;
            }
            else
            {
                Debug.Log("캐릭터 없음 : " + items[curSelectItem].itemKind);

                BackEndServerManager.instance.myInfo.haveCharacters.Add(items[curSelectItem].itemKind);
                BackEndServerManager.instance.myInfo.charactersLevel.Add(1);
                BackEndServerManager.instance.myInfo.levelExp.Add(1);

                var value = BackEndServerManager.instance.myInfo.haveCharacters.FindIndex(find => find == items[curSelectItem].itemKind);
                BackEndServerManager.instance.myInfo.levelExp[value] += items[curSelectItem].itemCount;
            }
            items[curSelectItem].itemCount = 0;
            
            ShowShop();
            SetInventory();

            BackEndServerManager.instance.myInfo.cardKind[curSelectItem] = items[curSelectItem].itemKind;
            BackEndServerManager.instance.myInfo.cardCount[curSelectItem] = items[curSelectItem].itemCount;

            SetGoldUI();
            PlayerStats.instance.Save();
        }
        else
        {
            StartCoroutine(OnShowBroadCast("클로버 부족"));
        }
    }

    // 상점 아이템 설정
    public void SetShopItems()
    {
        hideObj.SetActive(false);

        BackEndServerManager.instance.myInfo.cardKind.Clear();
        BackEndServerManager.instance.myInfo.cardCount.Clear();
        BackEndServerManager.instance.myInfo.cardKind.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });
        BackEndServerManager.instance.myInfo.cardCount.InsertRange(index: 0, collection: new List<int>() { 99, 99, 99 });

        int index = 0;

        foreach (var item in items)
        {
            Debug.Log(index);

            // 이미지 설정하는 코드 추가 [...]
            item.itemKind = Random.Range(0, 30);

            item.itemMaxCount = Random.Range(1, 65);
            item.itemCount = item.itemMaxCount;

            item.itemPrice = item.itemMaxCount * 10;

            item.hideObj.SetActive(false);
            item.itemNameText.text = ((CharacterKind)item.itemKind).ToString();
            item.itemCountText.text = "x" + item.itemMaxCount;
            item.itemPriceText.text = item.itemPrice.ToString();

            item.itemImage.sprite = characterImgs[item.itemKind];

            BackEndServerManager.instance.myInfo.cardKind[index] = item.itemKind;
            BackEndServerManager.instance.myInfo.cardCount[index] = item.itemCount;

            index++;
        }

        foreach (var chest in chests)
        {
            chest.chestPrice = ((int)chest.chestKind + 1) * 10;

            chest.chestNameText.text = chest.chestKind.ToString() + " 상자";
            chest.chestPriceText.text = chest.chestPrice.ToString();
            // 아레나 설정 , 상자 이미지 설정 추가
        }
        ShowShop();
    }

    // 상점 아이템 재설정
    public void ResetShopItems()
    {
        hideObj.SetActive(false);

        for (int i = 0; i < items.Length; i++)
        {
            items[i].itemKind = BackEndServerManager.instance.myInfo.cardKind[i];

            items[i].itemMaxCount = BackEndServerManager.instance.myInfo.cardCount[i];
            items[i].itemCount = items[i].itemMaxCount;

            items[i].itemPrice = items[i].itemMaxCount * 10;

            items[i].hideObj.SetActive(false);
            items[i].itemNameText.text = ((CharacterKind)items[i].itemKind).ToString();
            items[i].itemCountText.text = "x" + items[i].itemMaxCount;
            items[i].itemPriceText.text = items[i].itemPrice.ToString();

            items[i].itemImage.sprite = characterImgs[items[i].itemKind];
        }

        foreach (var chest in chests)
        {
            chest.chestPrice = ((int)chest.chestKind + 1) * 10;

            chest.chestNameText.text = chest.chestKind.ToString() + " 상자";
            chest.chestPriceText.text = chest.chestPrice.ToString();
            // 아레나 설정 , 상자 이미지 설정 추가
        }

        ShowShop();
    }

    public void OpenPurchaseUI(int index)
    {
        SoundPlayer.instance.PlaySound("Click");

        curSelectItem = index;
        if (items[curSelectItem].itemCount > 0)
        {
            hideObj.SetActive(false);
            purchaseCharacterImg.sprite = characterImgs[items[curSelectItem].itemKind];
            itemNameText.text = ((CharacterKind)items[index].itemKind).ToString();
            itemCountText.text = "x" + items[index].itemCount;
            itemPriceText.text = items[index].itemPrice.ToString();

            cardPurchase.SetActive(true);
        }
    }

    public void ClosePurchaseUI()
    {
        cardPurchase.SetActive(false);
    }

    // 상자 구매
    public void PurchaseChest()
    {
        // 상자 열기
        if (BackEndServerManager.instance.myInfo.diamond >= chests[curSelectChest].chestPrice)
        {
            SoundPlayer.instance.PlaySound("Purchase");

            BackEndServerManager.instance.myInfo.diamond -= chests[curSelectChest].chestPrice;
            OpenDiamondChest(chests[curSelectChest].chestKind);
        }
        else
        {
            StartCoroutine(OnShowBroadCast("보석 부족"));
        }
    }

    public void OpenPurchaseChestUI(int index)
    {
        curSelectChest = index;

        chestKindText.text = chests[index].chestKind.ToString() + " 상자";
        chestPriceText.text = chests[index].chestPrice.ToString();

        chestCardText.text = "x" + (index + 1) * 5;
        chestGoldText.text = (index + 1) * 100 + "-" + (index + 1) * 999;
        
        diamondChestDisObject.SetActive(true);
    }

    public void ClosePurchaseChestUI()
    {
        diamondChestDisObject.SetActive(false);
    }

    public IEnumerator CheckingDay()
    {
        System.DateTime midnight = System.DateTime.Parse(BackEndServerManager.instance.myInfo.joinTime);
        Debug.Log("날짜 체크 시작 \n현재 시각 : " + System.DateTime.Now + "\n이전 접속 시각 : " + midnight);

        while (GameManager.instance.gameState == GameManager.GameState.MatchLobby && midnight.Day <= 0)
        {
            Debug.Log("날짜 체크 중 : " + midnight.Day + "\n날짜 체크 중 : " + System.DateTime.Now.Day);

            if (midnight.Day <= System.DateTime.Now.Day)
            {
                Debug.Log("아이템 셋팅 완료");
                // 자정이 되어서 아이템 초기화
                SetShopItems();
                BackEndServerManager.instance.myInfo.joinTime = System.DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss");
                break;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public bool CheckHaveCard(int index)
    {
        if (BackEndServerManager.instance.myInfo.haveCharacters.Contains(index))
        {
            Debug.Log("캐릭터 있음");
            return true;
        }
        else
        {
            Debug.Log("캐릭터 없음 : " + index);
            return false;
        }
    }
}
