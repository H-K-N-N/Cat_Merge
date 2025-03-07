using UnityEngine;
using System.Collections.Generic;
public class ItemFunctionManager : MonoBehaviour
{
    // Singleton Instance
    public static ItemFunctionManager Instance { get; private set; }

    public List<(int step, float value, decimal fee)> maxCatsList = new List<(int step, float value, decimal fee)>();
    public List<(int step, float value, decimal fee)> reduceCollectingTimeList = new List<(int step, float value, decimal fee)>();
    public List<(int step, float value, decimal fee)> maxFoodsList = new List<(int step, float value, decimal fee)>();
    public List<(int step, float value, decimal fee)> reduceProducingFoodTimeList = new List<(int step, float value, decimal fee)>();
    public List<(int step, float value, decimal fee)> autoCollectingList = new List<(int step, float value, decimal fee)>();

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

        InitListContents();
    }

    private void InitListContents()
    {
        // 고양이 최대치 증가
        var itemData1 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(1);
        if (itemData1 != null)
        {
            foreach (var item in itemData1)
            {
                maxCatsList.Add((item.step, item.value, item.fee));
            }
        }

        // 재화 획득 시간 감소
        var itemData2 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(2);
        if (itemData2 != null)
        {
            foreach (var item in itemData2)
            {
                reduceCollectingTimeList.Add((item.step, item.value, item.fee));
            }
        }

        // 먹이 최대치 증가
        var itemData3 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(3);
        if (itemData3 != null)
        {
            foreach (var item in itemData3)
            {
                maxFoodsList.Add((item.step, item.value, item.fee));
            }
        }

        // 먹이 생성 시간 감소
        var itemData4 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(4);
        if (itemData4 != null)
        {
            foreach (var item in itemData4)
            {
                reduceProducingFoodTimeList.Add((item.step, item.value, item.fee));
            }
        }

        // 자동 먹이주기 시간
        var itemData7 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(7);
        if(itemData7 != null)
        {
            foreach(var item in itemData7)
            {
                autoCollectingList.Add((item.step, item.value, item.fee));
            }
        }

        
    }
}
