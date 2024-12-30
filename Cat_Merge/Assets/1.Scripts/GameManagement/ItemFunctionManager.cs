using UnityEngine;
using System.Collections.Generic;
public class ItemFunctionManager : MonoBehaviour
{
    // Singleton Instance
    public static ItemFunctionManager Instance { get; private set; }

    //private int maxCats;                // 고양이 최대 수
    //public int MaxCats { get => maxCats; set => maxCats = value; }

    //private int collectingTime;         // 재화 획득 시간
    //public int CollectingTime { get => collectingTime; set => collectingTime = value; }

    //private int maxFoods;               // 먹이 최대치
    //public int MaxFoods { get => maxFoods; set => maxFoods = value; }

    //private int producingFoodsTime;     // 먹이 생성 시간
    //public int ProducingFoodsTime { get => producingFoodsTime; set => producingFoodsTime = value; }

    //private int foodUpgrade;            // 먹이 업그레이드
    //public int FoodUpgrade { get => foodUpgrade; set => foodUpgrade = value; }

    //private int foodUpgradeVer2;        // 먹이 업그레이드2
    //public int FoodUpgradeVer2 { get => foodUpgradeVer2; set => foodUpgradeVer2 = value; }

    //private int autoFeedingTime;        // 자동 먹이주기 시간
    //public int AutoFeedingTime { get => autoFeedingTime; set => autoFeedingTime = value; }

    public List<(int step, float value, float fee)> maxCatsList = new List<(int step, float value, float fee)>();
    public List<(int step, float value, float fee)> reduceCollectingTimeList = new List<(int step, float value, float fee)>();
    public List<(int step, float value, float fee)> maxFoodsList = new List<(int step, float value, float fee)>();
    public List<(int step, float value, float fee)> reduceProducingFoodTimeList = new List<(int step, float value, float fee)>();
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
        {
            if (itemData2 != null)
            {
                foreach (var item in itemData2)
                {
                    reduceCollectingTimeList.Add((item.step, item.value, item.fee));
                }
            }
        }

        // 먹이 최대치 증가
        var itemData3 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(3);
        {
            if (itemData3 != null)
            {
                foreach (var item in itemData3)
                {
                    maxFoodsList.Add((item.step, item.value, item.fee));
                }
            }
        }

        // 먹이 생성 시간 감소
        var itemData4 = ItemItemUpgradeDataLoader.Instance.GetDataByNumber(4);
        {
            if (itemData4 != null)
            {
                foreach (var item in itemData4)
                {
                    reduceProducingFoodTimeList.Add((item.step, item.value, item.fee));
                }
            }
        }
    }
}
