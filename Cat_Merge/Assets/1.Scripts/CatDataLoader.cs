using System.Collections.Generic;
using System.IO;
using UnityEngine;

// CatDataLoader Script
[DefaultExecutionOrder(-2)]     // 스크립트 실행 순서 조정
public class CatDataLoader : MonoBehaviour
{
    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();

    // ======================================================================================================================

    private void Awake()
    {
        LoadCatDataFromCSV();

        // catDataDictionary.TryGetValue(1, out Cat cat) : 1번 인덱스의 Cat 정보를 가져오는 코드
    }

    // ======================================================================================================================

    // CSV 파일을 읽어 Cat 객체로 변환 후 Dictionary에 저장
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 연결되지 않았습니다");
            return;
        }

        StringReader stringReader = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = stringReader.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 2) continue;
            if (lineNumber >= 6) continue;

            string[] values = line.Split(',');

            // 빈 칸을 발견하면 거기까지만 처리
            List<string> validValues = new List<string>();
            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    //Debug.Log($"라인 {lineNumber}: 빈 칸 발견 - 해당 칸 이후 데이터를 무시합니다.");
                    break;
                }
                validValues.Add(value);
            }

            try
            {
                // 데이터 파싱
                int catId = int.Parse(validValues[0]);
                string catName = validValues[1];
                int catGrade = int.Parse(validValues[2]);
                int catDamage = int.Parse(validValues[3]);
                int catGetCoin = int.Parse(validValues[4]);
                int catHp = int.Parse(validValues[5]);
                Sprite catImage = LoadSprite(validValues[6]);
                string catExplain = validValues[7];

                // Cat 객체 생성 및 Dictionary에 추가
                Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain);
                if (!catDictionary.ContainsKey(catId))
                {
                    catDictionary.Add(catId, newCat);
                }
                else
                {
                    Debug.LogWarning($"중복된 CatId가 발견되었습니다: {catId}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"라인 {lineNumber}: 데이터 처리 중 오류 발생 - {ex.Message}");
            }
        }

        //Debug.Log("고양이 데이터 로드 완료: " + catDictionary.Count + "개");
    }

    // Resources 폴더에서 스프라이트 로드
    private Sprite LoadSprite(string path)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/Cats/" + path);
        if (sprite == null)
        {
            Debug.LogError($"이미지를 찾을 수 없습니다: {path}");
        }
        return sprite;
    }
}
