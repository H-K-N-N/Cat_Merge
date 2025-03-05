using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

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
    private decimal coin = 100;                                      // 기본재화
    public decimal Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();
        }
    }

    [SerializeField] private TextMeshProUGUI cashText;                      // 캐쉬재화 텍스트
    private decimal cash = 1000;                                            // 캐쉬재화
    public decimal Cash
    {
        get => cash;
        set
        {
            cash = value;
            UpdateCashText();
        }
    }

    [Header("---[Exit Panel]")]
    [SerializeField] private GameObject exitPanel;                          // 종료 확인 패널
    [SerializeField] private Button closeButton;                            // 종료 패널 닫기 버튼
    [SerializeField] private Button okButton;                               // 종료 확인 버튼
    private bool isBackButtonPressed = false;                               // 뒤로가기 버튼이 눌렸는지 여부

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

        InitializeExitSystem();
    }

    private void Update()
    {
        SortCatUIObjectsByYPosition();
        CheckExitInput();
    }

    // ======================================================================================================================

    // 고양이 정보 Load 함수
    private void LoadAllCats()
    {
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();

        if (catDataLoader == null || catDataLoader.catDictionary == null)
        {
            Debug.LogError("CatDataLoader가 없거나 고양이 데이터가 로드되지 않았습니다.");
            return;
        }

        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);
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
            // 숫자를 3자리마다 콤마를 추가하여 표시
            coinText.text = coin.ToString("N0");
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

    // 게임 종료 버튼 초기화
    private void InitializeExitSystem()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CancelQuit);
        }
        if (okButton != null)
        {
            okButton.onClick.AddListener(QuitGame);
        }
    }

    // 종료 입력 체크
    private void CheckExitInput()
    {
        // 유니티 에디터 및 안드로이드에서 뒤로가기 버튼
        if ((Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape)) ||
            (Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Escape)))
        {
            HandleExitInput();
        }
    }

    // 종료 입력 처리
    public void HandleExitInput()
    {
        if (exitPanel != null && !isBackButtonPressed)
        {
            isBackButtonPressed = true;
            if (exitPanel.activeSelf)
            {
                CancelQuit();
            }
            else
            {
                ShowExitPanel();
            }
            StartCoroutine(ResetBackButtonPress());
        }
    }

    // 뒤로가기버튼 입력 딜레이 설정
    private IEnumerator ResetBackButtonPress()
    {
        yield return new WaitForSeconds(0.2f);
        isBackButtonPressed = false;
    }

    // 종료 패널 표시
    private void ShowExitPanel()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(true);
        }
    }

    // 게임 종료
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 종료 취소
    public void CancelQuit()
    {
        if (exitPanel != null)
        {
            exitPanel.SetActive(false);
        }
    }

    // ======================================================================================================================

    // 필드의 모든 고양이 정보 업데이트
    public void UpdateAllCatsInField()
    {
        // 이미 생성된 고양이의 데이터 수치를 새로 업데이트된 수치로 적용
        // 기존 Cat 수치로 생성된 고양이들을 성장으로 추가된 수치가 적용된 Cat으로 업데이트하기 위함
        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                foreach (Cat cat in allCatData)
                {
                    if (cat.CatId == catData.catData.CatId)
                    {
                        catData.SetCatData(cat);
                        //Debug.Log($"{catData.catData.CatDamage}, {catData.catData.CatHp}");
                        break;
                    }
                }
            }
        }
    }

    // 모든 고양이의 훈련 데이터를 저장하는 메서드
    public void SaveTrainingData(Cat[] cats)
    {
        // 여기에 실제 저장 로직 구현
        // 예: PlayerPrefs나 파일 시스템을 사용하여 각 고양이의 성장 스탯 저장
    }


}