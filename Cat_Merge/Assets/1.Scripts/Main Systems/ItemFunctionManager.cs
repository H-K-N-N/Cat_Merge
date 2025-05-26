using UnityEngine;
using System.Collections.Generic;

// 아이템 기능 및 업그레이드 데이터 관리 스크립트
[DefaultExecutionOrder(-8)]
public class ItemFunctionManager : MonoBehaviour
{


    #region Variables

    public static ItemFunctionManager Instance { get; private set; }

    // 아이템 업그레이드 데이터 리스트들
    public List<(int step, float value, decimal fee)> maxCatsList;
    public List<(int step, float value, decimal fee)> reduceCollectingTimeList;
    public List<(int step, float value, decimal fee)> maxFoodsList;
    public List<(int step, float value, decimal fee)> reduceProducingFoodTimeList;
    public List<(int step, float value, decimal fee)> foodUpgradeList;
    public List<(int step, float value, decimal fee)> foodUpgrade2List;
    public List<(int step, float value, decimal fee)> autoCollectingList;
    public List<(int step, float value, decimal fee)> autoMergeList;

    // 리스트 초기 용량 설정
    private const int INITIAL_LIST_CAPACITY = 50;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeLists();
        InitListContents();
    }

    #endregion


    #region Initialization

    // 리스트들의 초기 용량을 설정하는 함수
    private void InitializeLists()
    {
        maxCatsList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        reduceCollectingTimeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        maxFoodsList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        reduceProducingFoodTimeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        foodUpgradeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        foodUpgrade2List = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        autoCollectingList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
        autoMergeList = new List<(int, float, decimal)>(INITIAL_LIST_CAPACITY);
    }

    // 각 아이템 업그레이드 데이터를 리스트에 로드하는 함수
    private void InitListContents()
    {
        LoadItemData(1, maxCatsList);                       // 고양이 최대치 증가
        LoadItemData(2, reduceCollectingTimeList);          // 재화 획득 시간 감소
        LoadItemData(3, maxFoodsList);                      // 먹이 최대치 증가
        LoadItemData(4, reduceProducingFoodTimeList);       // 먹이 생성 시간 감소
        LoadItemData(5, foodUpgradeList);                   // 먹이 업그레이드
        LoadItemData(6, foodUpgrade2List);                  // 먹이 업그레이드2
        LoadItemData(7, autoCollectingList);                // 자동 먹이주기 시간
        LoadItemData(8, autoMergeList);                     // 자동 합성 시간
    }

    // 아이템 데이터를 로드하여 리스트에 추가하는 함수
    private void LoadItemData(int dataNumber, List<(int step, float value, decimal fee)> targetList)
    {
        var itemData = ItemUpgradeDataLoader.Instance.GetDataByNumber(dataNumber);
        if (itemData != null)
        {
            foreach (var item in itemData)
            {
                targetList.Add((item.step, item.value, item.fee));
            }
        }
    }

    #endregion


}
