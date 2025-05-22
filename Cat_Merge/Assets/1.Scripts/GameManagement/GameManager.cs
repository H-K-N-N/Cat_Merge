using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

// 게임매니저 스크립트
[DefaultExecutionOrder(-8)]
public class GameManager : MonoBehaviour, ISaveable
{


    #region Variables

    public static GameManager Instance { get; private set; }

    [Header("---[Cat Data]")]
    private Cat[] allCatData;                                               // 모든 고양이 데이터
    public Cat[] AllCatData => allCatData;

    [Header("---[Mouse Data]")]
    private Mouse[] allMouseData;                                           // 모든 쥐 데이터
    public Mouse[] AllMouseData => allMouseData;

    [Header("---[Game Objects]")]
    public GameObject catPrefab;                                            // 고양이 UI 프리팹
    [SerializeField] private Transform gamePanel;                           // 고양이 정보를 가져올 부모 Panel
    private List<RectTransform> catUIObjects = new List<RectTransform>();   // 고양이 UI 객체 리스트

    private float lastSortTime = 0f;                                        // 마지막 정렬 시간
    private const float SORT_INTERVAL = 0.5f;                               // 정렬 간격
    private Dictionary<int, Vector2> lastPositions = new Dictionary<int, Vector2>();  // 마지막 위치 저장

    [Header("---[Quit Panel]")]
    [SerializeField] private GameObject quitPanel;                          // 종료 확인 패널
    [SerializeField] private Button closeButton;                            // 종료 패널 닫기 버튼
    [SerializeField] private Button okButton;                               // 종료 확인 버튼

    [Header("---[Game Data]")]
    [SerializeField] private TextMeshProUGUI catCountText;                  // 고양이 수 텍스트
    private int currentCatCount;                                            // 화면 내 고양이 수
    public int CurrentCatCount
    {
        get => currentCatCount;
        set
        {
            if (currentCatCount != value)
            {
                currentCatCount = value;
                UpdateCatCountText();
            }
        }
    }

    private int maxCats;                                                    // 최대 고양이 수
    public int MaxCats
    {
        get => maxCats;
        set
        {
            if (maxCats != value)
            {
                maxCats = value;
                UpdateCatCountText();
            }
        }
    }

    private int passiveCatCount = 0;                                        // 패시브 효과로 인한 추가 고양이 수
    public int TotalMaxCats => maxCats + passiveCatCount;                   // 총 최대 고양이 수 (기본 + 패시브)

    [SerializeField] private TextMeshProUGUI coinText;                      // 기본재화 텍스트
    private decimal coin;                                                   // 기본재화
    public decimal Coin
    {
        get => coin;
        set
        {
            if (coin != value)
            {
                coin = value;
                UpdateCoinText();
            }
        }
    }

    [SerializeField] private TextMeshProUGUI cashText;                      // 캐쉬재화 텍스트
    private decimal cash;                                                   // 캐쉬재화
    public decimal Cash
    {
        get => cash;
        set
        {
            if (cash != value)
            {
                cash = value;
                UpdateCashText();
            }
        }
    }

    private bool isBackButtonPressed = false;                               // 뒤로가기 버튼이 눌렸는지 여부
    [HideInInspector] public bool isQuiting = false;                        // 종료 여부

    [Header("---[ETC]")]
    private bool isDataLoaded = false;                                      // 데이터 로드 확인

    #endregion


    #region Unity Methods

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

        // 화면 절전 모드 비활성화
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        InitializeGameData();
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        // GoogleManager에서 데이터를 로드하지 못한 경우에만 초기화
        if (!isDataLoaded)
        {
            InitializeDefaultValues();
        }
        UpdateAllUI();
    }

    private void Update()
    {
        if (!isQuiting)
        {
            if (Time.time - lastSortTime >= SORT_INTERVAL)
            {
                CheckAndSortCatUIObjects();
                lastSortTime = Time.time;
            }

            CheckQuitInput();
        }
    }

    #endregion


    #region Initialize

    // 게임 데이터 기본값 초기화 함수
    private void InitializeDefaultValues()
    {
        currentCatCount = 0;
        maxCats = 8;
        coin = 250;
        cash = 100;
    }

    // 게임 데이터 초기 로드 함수
    private void InitializeGameData()
    {
        LoadAllCats();
        LoadAllMouses();
        InitializeQuitSystem();
    }

    // 고양이 정보 로드 함수
    private void LoadAllCats()
    {
        CatDataLoader catDataLoader = FindObjectOfType<CatDataLoader>();
        if (catDataLoader?.catDictionary == null) return;

        allCatData = new Cat[catDataLoader.catDictionary.Count];
        catDataLoader.catDictionary.Values.CopyTo(allCatData, 0);
    }

    // 쥐 정보 로드 함수
    private void LoadAllMouses()
    {
        MouseDataLoader mouseDataLoader = FindObjectOfType<MouseDataLoader>();
        if (mouseDataLoader?.mouseDictionary == null) return;

        allMouseData = new Mouse[mouseDataLoader.mouseDictionary.Count];
        mouseDataLoader.mouseDictionary.Values.CopyTo(allMouseData, 0);
    }

    #endregion


    #region Cat Management

    // 고양이 생성 가능 여부 확인 함수
    public bool CanSpawnCat()
    {
        return CurrentCatCount < TotalMaxCats;
    }

    // 고양이 수 증가 함수
    public void AddCatCount()
    {
        CurrentCatCount++;
    }

    // 고양이 수 감소 함수
    public void DeleteCatCount()
    {
        if (CurrentCatCount > 0)
        {
            CurrentCatCount--;
        }
    }

    // 패시브로 인한 추가 고양이 수를 조절하는 메서드
    public void AddPassiveCatCapacity(int amount)
    {
        passiveCatCount += amount;
        UpdateCatCountText();
    }

    #endregion


    #region UI Updates

    // 모든 UI 업데이트 함수
    private void UpdateAllUI()
    {
        UpdateCatCountText();
        UpdateCoinText();
        UpdateCashText();
    }

    // 고양이 수 텍스트 UI 업데이트 함수
    private void UpdateCatCountText()
    {
        if (catCountText != null)
        {
            if (passiveCatCount > 0)
            {
                catCountText.text = $"{currentCatCount} / {maxCats}+{passiveCatCount}";
            }
            else
            {
                catCountText.text = $"{currentCatCount} / {maxCats}";
            }
        }
    }

    // 기본재화 텍스트 UI 업데이트 함수
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = FormatNumber(coin);
        }
    }

    // 캐쉬재화 텍스트 UI 업데이트 함수
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            //cashText.text = cash.ToString("N0");
            cashText.text = FormatNumber(cash);
        }
    }

    // 모든 단위 통일화
    public string FormatNumber(long number)
    {
        if (number < 1000)
        {
            return number.ToString(); // 999까지는 그대로 출력
        }

        List<string> suffixes = new List<string>();

        // 1글자 단위 생성 (A-Z)
        for (char c = 'A'; c <= 'Z'; c++)
        {
            suffixes.Add(c.ToString());
        }

        // 2글자 단위 생성 (AA-ZZ)
        for (char c1 = 'A'; c1 <= 'Z'; c1++)
        {
            for (char c2 = 'A'; c2 <= 'Z'; c2++)
            {
                suffixes.Add(c1.ToString() + c2.ToString());
            }
        }

        int suffixIndex = 0;
        double value = number;

        // 1,000씩 나누며 접미사를 선택 (접미사 A부터 시작)
        while (value >= 1000 && suffixIndex < suffixes.Count)
        {
            value /= 1000;
            suffixIndex++;
        }

        // 소수점 2자리까지 표시하며 반올림 없이 출력
        return $"{Math.Floor(value * 100) / 100:F2}{suffixes[suffixIndex - 1]}";
    }

    // decimal 타입 오버로딩 추가
    public string FormatNumber(decimal number)
    {
        return FormatNumber((long)number);
    }

    #endregion


    #region UI Management

    // 고양이 UI 객체 목록 업데이트 함수
    private void UpdateCatUIObjects()
    {
        catUIObjects.Clear();
        foreach (Transform child in gamePanel)
        {
            if (child.TryGetComponent<RectTransform>(out var rectTransform))
            {
                catUIObjects.Add(rectTransform);
            }
        }
    }

    // 정렬이 필요한지 확인하고 수행하는 함수
    private void CheckAndSortCatUIObjects()
    {
        UpdateCatUIObjects();
        if (catUIObjects.Count == 0) return;

        bool needsSort = false;
        Dictionary<int, Vector2> currentPositions = new Dictionary<int, Vector2>();

        // 현재 위치 기록 및 변화 확인
        foreach (var rectTransform in catUIObjects)
        {
            int id = rectTransform.GetInstanceID();
            Vector2 currentPos = rectTransform.anchoredPosition;
            currentPositions[id] = currentPos;

            if (lastPositions.TryGetValue(id, out Vector2 lastPos))
            {
                if (Vector2.Distance(currentPos, lastPos) > 0.1f)
                {
                    needsSort = true;
                }
            }
            else
            {
                needsSort = true;
            }
        }

        // 위치가 변경된 경우에만 정렬
        if (needsSort)
        {
            SortCatUIObjectsByPosition();
        }

        lastPositions = currentPositions;
    }

    // Y축 기준으로 고양이 UI 정렬 함수
    private void SortCatUIObjectsByPosition()
    {
        if (catUIObjects.Count == 0) return;

        // 동일한 Y좌표에 있는 고양이들을 그룹화
        var groups = catUIObjects.GroupBy(rt => Mathf.Round(rt.anchoredPosition.y * 10f) / 10f)
                                .OrderByDescending(g => g.Key)
                                .ToList();

        int currentIndex = 0;
        foreach (var group in groups)
        {
            // 각 Y좌표 그룹 내에서 X좌표로 정렬
            var sortedGroup = group.OrderBy(rt => rt.anchoredPosition.x).ToList();

            foreach (var rectTransform in sortedGroup)
            {
                if (rectTransform.GetSiblingIndex() != currentIndex)
                {
                    rectTransform.SetSiblingIndex(currentIndex);
                }
                currentIndex++;
            }
        }
    }

    #endregion


    #region Quit System

    // 게임 종료 버튼 초기화 함수
    private void InitializeQuitSystem()
    {
        quitPanel.SetActive(false);

        closeButton?.onClick.RemoveAllListeners();
        closeButton?.onClick.AddListener(CancelQuit);

        okButton?.onClick.RemoveAllListeners();
        okButton?.onClick.AddListener(QuitGame);
    }

    // 종료 입력 체크 함수
    private void CheckQuitInput()
    {
        // 메인 튜토리얼이 진행 중이면 뒤로가기 버튼 무시
        if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive)
        {
            return;
        }

        //// 도감 튜토리얼이 진행 중이면 뒤로가기 버튼 무시
        if (TutorialManager.Instance != null && TutorialManager.Instance.isDictionaryTutorialActive)
        {
            return;
        }

        // 유니티 에디터 및 안드로이드에서 뒤로가기 버튼
        if ((Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape)) ||
            (Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Escape)))
        {
            HandleQuitInput();
        }
    }

    // 종료 입력 처리 함수
    private void HandleQuitInput()
    {
        if (!isBackButtonPressed)
        {
            isBackButtonPressed = true;

            if (quitPanel.activeSelf)
            {
                CancelQuit();
            }
            else if (ActivePanelManager.Instance.HasActivePanel())
            {
                ActivePanelManager.Instance.CloseAllPanels();
            }
            else
            {
                ShowQuitPanel();
            }

            StartCoroutine(ResetBackButtonPress());
        }
    }

    // OptionManager의 종료하기 버튼 함수
    public void QuitButtonInput()
    {
        ShowQuitPanel();
    }

    // 뒤로가기버튼 입력 딜레이 설정 코루틴
    private IEnumerator ResetBackButtonPress()
    {
        yield return new WaitForSeconds(0.2f);
        isBackButtonPressed = false;
    }

    // 종료 패널 표시 함수
    private void ShowQuitPanel()
    {
        quitPanel?.SetActive(true);
    }

    // 종료 패널 취소 함수
    public void CancelQuit()
    {
        quitPanel?.SetActive(false);
    }

    // 게임 종료 함수
    public void QuitGame()
    {
        if (isQuiting) return;

        isQuiting = true;
        Time.timeScale = 0f;

        //QuitApplication();
        StartCoroutine(SaveAndQuitCoroutine());
    }

    // 저장 후 종료하는 코루틴
    private IEnumerator SaveAndQuitCoroutine()
    {
        // 게임 일시정지
        Time.timeScale = 0f;

        // GoogleManager를 통해 암호화하여 저장
        GoogleManager.Instance?.ForceSaveAllData();

        // 1초 대기
        yield return new WaitForSecondsRealtime(1f);

        QuitApplication();
    }

    // 실제 종료 처리 함수
    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion


    #region Content System

    // 필드의 모든 고양이 정보 업데이트 함수
    public void UpdateAllCatsInField()
    {
        // 이미 생성된 고양이의 데이터 수치를 새로 업데이트된 수치로 적용
        // 기존 Cat 수치로 생성된 고양이들을 성장으로 추가된 수치가 적용된 Cat으로 업데이트하기 위함

        // 기존 시스템에서 오브젝트 풀링 시스템으로 변경해서 손봐줘야할듯함
        //foreach (Transform child in gamePanel)
        //{
        //    if (child.TryGetComponent<CatData>(out var catData))
        //    {
        //        foreach (Cat cat in allCatData)
        //        {
        //            if (cat.CatId == catData.catData.CatId)
        //            {
        //                catData.SetCatData(cat);
        //                break;
        //            }
        //        }
        //    }
        //}
    }

    // 모든 고양이의 훈련 데이터를 저장하는 함수
    public void SaveTrainingData(Cat[] cats)
    {
        // 여기에 실제 저장 로직 구현 (이건 나중에 기존 저장로직을 따라서 변경할 예정)
    }

    #endregion


    #region Save System

    [Serializable]
    private class SaveData
    {
        public string coin;             // 기본 재화
        public string cash;             // 캐시 재화
    }

    public string GetSaveData()
    {
        SaveData data = new SaveData
        {
            coin = this.coin.ToString(),
            cash = this.cash.ToString()
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        LoadBasicData(savedData);

        UpdateAllUI();

        isDataLoaded = true;
    }

    // 기본 데이터 로드 함수
    private void LoadBasicData(SaveData savedData)
    {
        if (decimal.TryParse(savedData.coin, out decimal parsedCoin))
        {
            coin = parsedCoin;
        }
        if (decimal.TryParse(savedData.cash, out decimal parsedCash))
        {
            cash = parsedCash;
        }
    }

    #endregion


}
