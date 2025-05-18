using System.Collections.Generic;
using UnityEngine;

// 아이템 데이터를 로드하고 관리하는 스크립트
[DefaultExecutionOrder(-10)]
public class ItemItemUpgradeDataLoader : MonoBehaviour
{


    #region Variables

    public static ItemItemUpgradeDataLoader Instance { get; private set; }

    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, List<(string title, int type, int step, float value, decimal fee)>> dataByNumber
        = new Dictionary<int, List<(string title, int type, int step, float value, decimal fee)>>();

    private readonly List<(string title, int type, int step, float value, decimal fee)> tempDataList =
        new List<(string title, int type, int step, float value, decimal fee)>(10);

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

        ParseCSV("ItemUpgradeDB");
    }

    #endregion


    #region Data Loading

    // 아이템 데이터를 파싱하고 Dictionary에 추가하는 함수
    public void ParseCSV(string fileName)
    {
        // Resources 폴더에서 파일 읽기
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        if (csvFile == null)
        {
            //Debug.LogError($"CSV file '{fileName}' not found in Resources folder!");
            return;
        }

        // 파일 내용 읽기
        string[] lines = csvFile.text.Split('\n');

        // 첫 번째 줄(헤더) 무시하고 데이터 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] values = line.Split(',');

            string title = values[0];
            int type = int.Parse(values[1]);
            int step = int.Parse(values[2]);
            float value = float.Parse(values[3]);
            decimal fee = decimal.Parse(values[4]);

            // 번호별로 데이터 추가
            if (!dataByNumber.ContainsKey(type))
            {
                tempDataList.Clear();
                dataByNumber[type] = new List<(string title, int type, int step, float value, decimal fee)>();
            }

            tempDataList.Add((title, type, step, value, fee));
            dataByNumber[type] = new List<(string, int, int, float, decimal)>(tempDataList);
        }
    }

    // Resources 폴더에서 스프라이트 로드하는 함수
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
    //    }
    //    return sprite;
    //}

    #endregion


    #region Data Access

    // 특정 번호에 해당하는 데이터 반환하는 함수
    public List<(string title, int type, int step, float value, decimal fee)> GetDataByNumber(int typeNum)
    {
        if (dataByNumber.ContainsKey(typeNum))
        {
            return dataByNumber[typeNum];
        }
        //Debug.LogWarning($"No data found for number {typeNum}");
        return null;
    }

    #endregion

    
}
