using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 훈련 데이터를 담는 클래스
public class TrainingData
{
    private int growthDamage;
    public int GrowthDamage { get => growthDamage; set => growthDamage = value; }

    private int growthHp;
    public int GrowthHp { get => growthHp; set => growthHp = value; }

    private double trainingCoin;
    public double TrainingCoin { get => trainingCoin; set => trainingCoin = value; }

    private double levelUpCoin;
    public double LevelUpCoin { get => levelUpCoin; set => levelUpCoin = value; }

    // 추가 능력치 관련 변수들
    private string extraAbilityName;    // 추가 획득 능력치 이름
    public string ExtraAbilityName { get => extraAbilityName; set => extraAbilityName = value; }

    private double extraAbilityValue;   // 추가 획득 능력치 수치
    public double ExtraAbilityValue { get => extraAbilityValue; set => extraAbilityValue = value; }

    private string extraAbilityUnit;    // 단위
    public string ExtraAbilityUnit { get => extraAbilityUnit; set => extraAbilityUnit = value; }

    private string extraAbilitySymbol;  // 보조 기호
    public string ExtraAbilitySymbol { get => extraAbilitySymbol; set => extraAbilitySymbol = value; }

    public TrainingData(int growthDamage, int growthHp, double trainingCoin, double levelUpCoin,
        string extraAbilityName, double extraAbilityValue, string extraAbilityUnit, string extraAbilitySymbol)
    {
        GrowthDamage = growthDamage;
        GrowthHp = growthHp;
        TrainingCoin = trainingCoin;
        LevelUpCoin = levelUpCoin;
        ExtraAbilityName = extraAbilityName;
        ExtraAbilityValue = extraAbilityValue;
        ExtraAbilityUnit = extraAbilityUnit;
        ExtraAbilitySymbol = extraAbilitySymbol;
    }
}

// 체력단련 데이터를 로드하고 관리하는 스크립트
[DefaultExecutionOrder(-10)]
public class TrainingDataLoader : MonoBehaviour
{


    #region Variables

    // 고양이별 훈련 데이터를 관리할 Dictionary (Key: CatId)
    public Dictionary<int, TrainingData> trainingDictionary = new Dictionary<int, TrainingData>();

    private readonly List<string> values = new List<string>(20);
    private readonly List<string> validValues = new List<string>(20);

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadTrainingDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV 파일을 읽어 TrainingData 객체로 변환 후 Dictionary에 저장하는 함수
    private void LoadTrainingDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("TrainingDB");
        if (csvFile == null)
        {
            //Debug.LogError("Training CSV 파일이 연결되지 않았습니다");
            return;
        }

        using (StringReader stringReader = new StringReader(csvFile.text))
        {
            int lineNumber = 0;

            while (true)
            {
                string line = stringReader.ReadLine();
                if (line == null) break;

                lineNumber++;
                if (lineNumber <= 1) continue;

                ParseCSVLine(line, values);

                // 빈 칸을 발견하면 거기까지만 처리
                validValues.Clear();
                foreach (string value in values)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        break;
                    }
                    validValues.Add(value);
                }

                try
                {
                    ParseAndAddTrainingData(validValues, lineNumber);
                }
                catch (System.Exception ex)
                {
                    //Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
                }
            }
        }
    }

    // CSV 라인을 파싱하여 values 리스트에 저장하는 함수
    private void ParseCSVLine(string line, List<string> values)
    {
        values.Clear();
        bool insideQuotes = false;
        int startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (line[i] == ',' && !insideQuotes)
            {
                string value = line.Substring(startIndex, i - startIndex).Trim();
                value = value.Trim('"');
                values.Add(value);
                startIndex = i + 1;
            }
        }

        string lastValue = line.Substring(startIndex).Trim();
        lastValue = lastValue.Trim('"');
        values.Add(lastValue);
    }

    // 파싱된 데이터를 TrainingData 객체로 변환하여 Dictionary에 추가하는 함수
    private void ParseAndAddTrainingData(List<string> values, int lineNumber)
    {
        int catId = int.Parse(values[1]);
        int growthDamage = int.Parse(values[2]);
        int growthHp = int.Parse(values[3]);
        string extraAbilityName = values[4];
        double extraAbilityValue = double.Parse(values[5]);
        string extraAbilityUnit = values[6];
        string extraAbilitySymbol = values[7];
        double trainingCoin = double.Parse(values[8]);
        double levelUpCoin = double.Parse(values[9]);

        TrainingData trainingData = new TrainingData(growthDamage, growthHp, trainingCoin, levelUpCoin,
            extraAbilityName, extraAbilityValue, extraAbilityUnit, extraAbilitySymbol);

        if (!trainingDictionary.ContainsKey(catId))
        {
            trainingDictionary.Add(catId, trainingData);
        }
        else
        {
            //Debug.LogWarning($"중복된 CatId가 발견되었습니다: {catId}");
        }
    }

    #endregion


}
