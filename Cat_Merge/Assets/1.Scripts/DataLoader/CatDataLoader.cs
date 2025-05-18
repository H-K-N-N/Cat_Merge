using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 고양이 데이터를 로드하고 관리하는 스크립트
[DefaultExecutionOrder(-10)]
public class CatDataLoader : MonoBehaviour
{


    #region Variables

    
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();     // 고양이 데이터를 관리할 Dictionary

    private readonly List<string> values = new List<string>();
    private readonly List<string> validValues = new List<string>();

    #endregion


    #region Unity Methods

    private void Awake()
    {
        LoadCatDataFromCSV();
    }

    #endregion


    #region Data Loading

    // CSV 파일을 읽어 Cat 객체로 변환 후 Dictionary에 저장하는 함수
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
        if (csvFile == null)
        {
            //Debug.LogError("CSV 파일이 연결되지 않았습니다");
            return;
        }

        StringReader stringReader = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 1) continue;
            if (lineNumber >= 32) continue;

            // CSV 파싱 전에 List 초기화 (재사용)
            values.Clear();
            validValues.Clear();

            // CSV 파싱 - 따옴표 내부의 쉼표는 무시하고 실제 구분자만 처리
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

            // 마지막 값 추가
            string lastValue = line.Substring(startIndex).Trim();
            lastValue = lastValue.Trim('"');
            values.Add(lastValue);

            // 빈 칸을 발견하면 거기까지만 처리
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
                ParseAndAddCatData(validValues, lineNumber);
            }
            catch (System.Exception ex)
            {
                //Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
            }
        }
    }

    // 파싱된 데이터로 Cat 객체 생성 및 추가하는 함수
    private void ParseAndAddCatData(List<string> data, int lineNumber)
    {
        int catId = int.Parse(data[0]);
        string catName = data[1];
        int catGrade = int.Parse(data[2]);
        int catDamage = int.Parse(data[3]);
        int catGetCoin = int.Parse(data[4]);
        int catHp = int.Parse(data[5]);
        Sprite catImage = LoadSprite(data[6]);
        string catExplain = data[7];
        int catAttackSpeed = int.Parse(data[8]);
        int catArmor = int.Parse(data[9]);
        int catMoveSpeed = int.Parse(data[10]);
        int canOpener = int.Parse(data[11]);
        int catFirstOpenCash = int.Parse(data[12]);

        // Cat 객체 생성 및 Dictionary에 추가
        Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain, catAttackSpeed, catArmor, catMoveSpeed, canOpener, catFirstOpenCash);
        if (!catDictionary.ContainsKey(catId))
        {
            catDictionary.Add(catId, newCat);
        }
        else
        {
            //Debug.LogWarning($"중복된 CatId가 발견되었습니다: {catId}");
        }
    }

    // Resources 폴더에서 스프라이트 로드하는 함수
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
        if (sprite == null)
        {
            //Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
        }
        return sprite;
    }

    #endregion


}
