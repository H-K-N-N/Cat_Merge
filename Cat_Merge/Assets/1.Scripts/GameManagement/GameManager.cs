using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// GameManager Script
[DefaultExecutionOrder(-1)]     // 스크립트 실행 순서 조정(3번째)
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Main Cat Data
    private Cat[] allCatData;                                               // 모든 고양이 데이터
    public Cat[] AllCatData => allCatData;

    // Main Mouse Data
    private Mouse[] allMouseData;                                           // 모든 쥐 데이터
    public Mouse[] AllMouseData => allMouseData;

    [SerializeField] private Transform gamePanel;                           // 고양이 정보를 가져올 부모 Panel
    private List<RectTransform> catUIObjects = new List<RectTransform>();   // 고양이 UI 객체 리스트

    // Main UI Text
    [Header("---[Main UI Text]")]
    [SerializeField] private TextMeshProUGUI catCountText;                  // 고양이 수 텍스트
    private int currentCatCount = 0;                                        // 화면 내 고양이 수
    public int CurrentCatCount
    {
        get => currentCatCount;
        set
        {
            currentCatCount = value;
            UpdateCatCountText();
        }
    }

    private int maxCats = 8;                                                // 최대 고양이 수
    public int MaxCats
    {
        get => maxCats;
        set
        {
            maxCats = value;
            UpdateCatCountText();
        }
    }

    [SerializeField] private TextMeshProUGUI coinText;                      // 기본재화 텍스트
    private int coin = 1000000;                                             // 기본재화
    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    [SerializeField] private TextMeshProUGUI cashText;                      // 캐쉬재화 텍스트
    private int cash = 1000;                                                // 캐쉬재화
    public int Cash
    {
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 기본 세팅
        LoadAllCats();
        LoadAllMouses();
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();
    }

    private void Update()
    {
        SortCatUIObjectsByYPosition();
    }

    // ======================================================================================================================

    // 고양이 정보 Load 함수
    private void LoadAllCats()
    {
        // CatDataLoader에서 catDictionary 가져오기
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader가 없거나 고양이 데이터가 로드되지 않았습니다.");
            return;
        }

        // Dictionary의 모든 값을 배열로 변환
        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);

        //Debug.Log($"고양이 데이터 {allCatData.Length}개가 로드되었습니다.");
    }

    // 쥐 정보 Load 함수
    private void LoadAllMouses()
    {
        // MouseDataLoader mouseDictionary 가져오기
        MouseDataLoader mouseDataLoader = FindObjectOfType<MouseDataLoader>();
        if (mouseDataLoader == null || mouseDataLoader.mouseDictionary == null)
        {
            Debug.LogError("MouseDataLoader가 없거나 쥐 데이터가 로드되지 않았습니다.");
            return;
        }

        // Dictionary의 모든 값을 배열로 변환
        allMouseData = new Mouse[mouseDataLoader.mouseDictionary.Count];
        mouseDataLoader.mouseDictionary.Values.CopyTo(allMouseData, 0);
    }

    // ======================================================================================================================

    // 고양이 수 판별 함수
    public bool CanSpawnCat()
    {
        return CurrentCatCount < MaxCats;
    }

    // 현재 고양이 수 증가시키는 함수
    public void AddCatCount()
    {
        if (CurrentCatCount < MaxCats)
        {
            CurrentCatCount++;
        }
    }

    // 현재 고양이 수 감소시키는 함수
    public void DeleteCatCount()
    {
        if (CurrentCatCount > 0)
        {
            CurrentCatCount--;
        }
    }

    // 고양이 수 텍스트 UI 업데이트하는 함수
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // 기본재화 텍스트 UI 업데이트하는 함수
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{coin}";
        }
    }

    // 캐쉬재화 텍스트 UI 업데이트하는 함수
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            // 숫자를 3자리마다 콤마를 추가하여 표시
            cashText.text = cash.ToString("N0");
        }
    }

    // ======================================================================================================================

    // GamePanel에서 모든 RectTransform 자식 객체 가져오는 함수
    private void UpdateCatUIObjects()
    {
        catUIObjects.Clear();

        foreach (Transform child in gamePanel)
        {
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                catUIObjects.Add(rectTransform);
            }
        }
    }

    // Y축 위치를 기준으로 고양이 UI 정렬
    private void SortCatUIObjectsByYPosition()
    {
        UpdateCatUIObjects();

        // Y축을 기준으로 정렬 (높은 Y값이 뒤로 가게 설정)
        catUIObjects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

        // 정렬된 순서대로 UI 계층 구조 업데이트
        for (int i = 0; i < catUIObjects.Count; i++)
        {
            catUIObjects[i].SetSiblingIndex(i);
        }
    }

    // ======================================================================================================================


}