using System.Collections.Generic;
using System.IO;
using UnityEngine;

// catDataDictionary.TryGetValue(1, out Cat cat) : 1번 인덱스의 Cat 정보를 가져오는 코드
// CatDataLoader Script
[DefaultExecutionOrder(-1)]     // 스크립트 실행 순서 조정
public class CatDataLoader : MonoBehaviour
{
    // 고양이 데이터를 관리할 Dictionary
    public Dictionary<int, Cat> catDictionary = new Dictionary<int, Cat>();

    private void Awake()
    {
        LoadCatDataFromCSV();
    }
    
    // CSV 파일을 읽어 Cat 객체로 변환 후 Dictionary에 저장
    public void LoadCatDataFromCSV()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("CatDB");
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일이 연결되지 않았습니다!");
            return;
        }

        StringReader sr = new StringReader(csvFile.text);
        int lineNumber = 0;

        while (true)
        {
            string line = sr.ReadLine();
            if (line == null) break;

            lineNumber++;
            if (lineNumber <= 2) continue;
            if (lineNumber >= 6) continue;

            string[] values = line.Split(',');

            // 데이터 파싱
            int catId = int.Parse(values[0]);
            string catName = values[1];
            int catGrade = int.Parse(values[2]);
            int catDamage = int.Parse(values[3]);
            int catGetCoin = int.Parse(values[4]);
            int catHp = int.Parse(values[5]);
            Sprite catImage = LoadSprite(values[6]);
            string catExplain = values[7];

            // Cat 객체 생성 및 Dictionary에 추가
            Cat newCat = new Cat(catId, catName, catGrade, catDamage, catGetCoin, catHp, catImage, catExplain);
            if (!catDictionary.ContainsKey(catId))
            {
                catDictionary.Add(catId, newCat);
                Debug.Log(newCat.CatId + ", " + newCat.CatName);
            }
            else
            {
                Debug.LogWarning($"중복된 CatId가 발견되었습니다: {catId}");
            }
        }

        Debug.Log("고양이 데이터 로드 완료: " + catDictionary.Count + "개");
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
