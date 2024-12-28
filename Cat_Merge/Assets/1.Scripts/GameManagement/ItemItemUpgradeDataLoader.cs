using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ItemItemUpgradeDataLoader Script
[DefaultExecutionOrder(-2)]     // 스크립트 실행 순서 조정 (2번째)
public class ItemItemUpgradeDataLoader : MonoBehaviour
{
    public static ItemItemUpgradeDataLoader Instance { get; private set; }

    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, List<(string title, int type, int step, float value, float fee)>> dataByNumber = new Dictionary<int, List<(string title, int type, int step, float value, float fee)>>();

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

        ParseCSV("Item_Item_UpgradeDB");
    }

    public void ParseCSV(string fileName)
    {
        // Resources 폴더에서 파일 읽기
        TextAsset csvFile = Resources.Load<TextAsset>(fileName);

        if (csvFile == null)
        {
            Debug.LogError($"CSV file '{fileName}' not found in Resources folder!");
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
            float fee = float.Parse(values[4]);

            // 번호별로 데이터 추가
            if (!dataByNumber.ContainsKey(type))
            {
                dataByNumber[type] = new List<(string title, int type, int step, float value, float fee)>();
            }
            dataByNumber[type].Add((title, type, step, value, fee));
        }

        // 데이터 확인 (디버깅용)
        //foreach (var entry in dataByNumber)
        //{
        //    Debug.Log($"Number: {entry.Key}");
        //    foreach (var item in entry.Value)
        //    {
        //        Debug.Log($"  Title: {item.title}, type: {item.type}, step: {item.step}, value: {item.value}, fee: {item.fee}");
        //    }
        //}
    }

    // 특정 번호에 해당하는 데이터 반환
    public List<(string title, int type, int step, float value, float fee)> GetDataByNumber(int typeNum)
    {
        if (dataByNumber.ContainsKey(typeNum))
        {
            return dataByNumber[typeNum];
        }
        else
        {
            Debug.LogWarning($"No data found for number {typeNum}");
            return null;
        }
    }

    // Resources 폴더에서 스프라이트 로드 (언젠간 쓰일듯)
    //private Sprite LoadSprite(string path)
    //{
    //    Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
    //    if (sprite == null)
    //    {
    //        Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
    //    }
    //    return sprite;
    //}
}
