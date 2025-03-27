using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

// 게임매니저 스크립트
[DefaultExecutionOrder(-5)]
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
    private List<GameObject> tempCatList = new List<GameObject>();          // 임시 저장용

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
                GoogleSave();
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
                GoogleSave();
            }
        }
    }

    private bool isBackButtonPressed = false;                               // 뒤로가기 버튼이 눌렸는지 여부
    [HideInInspector] public bool isQuiting = false;                        // 종료 여부


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

        InitializeGameData();
    }

    private void Start()
    {
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
            SortCatUIObjectsByYPosition();
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
        coin = 100000000000;
        cash = 1000;
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
        return CurrentCatCount < MaxCats;
    }

    // 고양이 수 증가 함수
    public void AddCatCount()
    {
        if (CurrentCatCount < MaxCats)
        {
            CurrentCatCount++;
        }
    }

    // 고양이 수 감소 함수
    public void DeleteCatCount()
    {
        if (CurrentCatCount > 0)
        {
            CurrentCatCount--;
        }
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
            catCountText.text = $"{currentCatCount} / {maxCats}";
        }
    }

    // 기본재화 텍스트 UI 업데이트 함수
    public void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = FormatCoinNumber(coin);
        }
    }

    // 캐쉬재화 텍스트 UI 업데이트 함수
    public void UpdateCashText()
    {
        if (cashText != null)
        {
            cashText.text = cash.ToString("N0");
        }
    }

    // 재화단위 설정 함수
    public string FormatCoinNumber(long number)
    {
        if (number >= 1_0000_0000_0000) // 1조 이상
        {
            long trillion = number / 1_0000_0000_0000; // 조 단위
            long billion = (number % 1_0000_0000_0000) / 1_0000_0000; // 억 단위

            if (billion == 0)
                return $"{trillion}조";
            return $"{trillion}조 {billion}억";
        }
        if (number >= 1_0000_0000) // 1억 이상
        {
            long billion = number / 1_0000_0000; // 억 단위
            long tenThousand = (number % 1_0000_0000) / 10000; // 만 단위

            if (tenThousand == 0)
                return $"{billion}억";
            return $"{billion}억 {tenThousand}만";
        }
        if (number >= 10000) // 1만 이상
        {
            long tenThousand = number / 10000; // 만 단위
            long remainder = number % 10000; // 나머지

            if (remainder == 0)
                return $"{tenThousand}만";
            return $"{tenThousand}만 {remainder}";
        }

        return number.ToString(); // 1만 미만은 그대로 출력
    }

    // decimal 타입 오버로딩 추가
    public string FormatCoinNumber(decimal number)
    {
        return FormatCoinNumber((long)number);
    }

    public string FormatPriceNumber(long number)
    {
        if (number >= 1_0000_0000_0000) // 1조 이상
        {
            long trillion = number / 1_0000_0000_0000; // 조 단위

            return $"{trillion}조";
        }
        if (number >= 1_0000_0000) // 1억 이상
        {
            long billion = number / 1_0000_0000; // 억 단위

            return $"{billion}억";
        }
        if (number >= 10000) // 1만 이상
        {
            long tenThousand = number / 10000; // 만 단위

            return $"{tenThousand}만";
        }

        return number.ToString(); // 1만 미만은 그대로 출력
    }

    // decimal 타입 오버로딩 추가
    public string FormatPriceNumber(decimal number)
    {
        return FormatPriceNumber((long)number);
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

    // Y축 기준으로 고양이 UI 정렬 함수
    private void SortCatUIObjectsByYPosition()
    {
        UpdateCatUIObjects();
        if (catUIObjects.Count == 0) return;

        // Y축을 기준으로 정렬 (높은 Y값이 뒤로 가게 설정)
        catUIObjects.Sort((a, b) => b.anchoredPosition.y.CompareTo(a.anchoredPosition.y));

        // 정렬된 순서대로 UI 계층 구조 업데이트
        for (int i = 0; i < catUIObjects.Count; i++)
        {
            catUIObjects[i].SetSiblingIndex(i);
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
        // 유니티 에디터 및 안드로이드에서 뒤로가기 버튼
        if ((Application.platform == RuntimePlatform.Android && Input.GetKey(KeyCode.Escape)) ||
            (Application.platform == RuntimePlatform.WindowsEditor && Input.GetKeyDown(KeyCode.Escape)))
        {
            HandleQuitInput();
        }
    }

    // 종료 입력 처리 함수
    public void HandleQuitInput()
    {
        if (quitPanel != null && !isBackButtonPressed)
        {
            isBackButtonPressed = true;
            if (quitPanel.activeSelf)
            {
                CancelQuit();
            }
            else
            {
                ShowQuitPanel();
            }
            StartCoroutine(ResetBackButtonPress());
        }
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

        if (GoogleManager.Instance != null)
        {
            StartCoroutine(SaveAndQuitCoroutine());
        }
        else
        {
            QuitApplication();
        }
    }

    // 저장 후 종료하는 코루틴
    private IEnumerator SaveAndQuitCoroutine()
    {
        bool saveCompleted = false;
        GoogleManager.Instance.SaveGameStateSync((success) => saveCompleted = true);

        // 저장 완료 또는 타임아웃 대기 (최대 3초)
        float waitTime = 0f;
        while (!saveCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // 두 번째 저장 시도 (첫 번째가 실패했을 경우를 대비) (이거없으면 제대로 안됌)
        if (!saveCompleted)
        {
            saveCompleted = false;

            GoogleManager.Instance.SaveGameStateSync((success) => saveCompleted = true);

            // 두 번째 저장 완료 대기
            waitTime = 0f;
            while (!saveCompleted && waitTime < 3.0f)
            {
                waitTime += 0.1f;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

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


    #region Content

    // 필드의 모든 고양이 정보 업데이트 함수
    public void UpdateAllCatsInField()
    {
        // 이미 생성된 고양이의 데이터 수치를 새로 업데이트된 수치로 적용
        // 기존 Cat 수치로 생성된 고양이들을 성장으로 추가된 수치가 적용된 Cat으로 업데이트하기 위함
        foreach (Transform child in gamePanel)
        {
            if (child.TryGetComponent<CatData>(out var catData))
            {
                foreach (Cat cat in allCatData)
                {
                    if (cat.CatId == catData.catData.CatId)
                    {
                        catData.SetCatData(cat);
                        break;
                    }
                }
            }
        }
    }

    // 모든 고양이의 훈련 데이터를 저장하는 함수
    public void SaveTrainingData(Cat[] cats)
    {
        // 여기에 실제 저장 로직 구현
    }

    #endregion


    #region Save System

    [Serializable]
    private class CatInstanceData
    {
        public int catId;               // 고양이 ID
        public float posX;              // X 위치
        public float posY;              // Y 위치
    }

    [Serializable]
    private class SaveData
    {
        public string coin;             // 기본 재화
        public string cash;             // 캐시 재화
        public string currentCatCount;  // 현재 고양이 수
        public List<CatInstanceData> fieldCats = new List<CatInstanceData>();  // 필드에 있는 고양이들 정보
    }

    public string GetSaveData()
    {
        List<CatInstanceData> fieldCats = new List<CatInstanceData>();
        foreach (Transform child in gamePanel)
        {
            if (child.TryGetComponent<CatData>(out var catData))
            {
                fieldCats.Add(new CatInstanceData
                {
                    catId = catData.catData.CatId,
                    posX = child.GetComponent<RectTransform>().anchoredPosition.x,
                    posY = child.GetComponent<RectTransform>().anchoredPosition.y
                });
            }
        }

        SaveData data = new SaveData
        {
            coin = this.coin.ToString(),
            cash = this.cash.ToString(),
            currentCatCount = this.currentCatCount.ToString(),
            fieldCats = fieldCats
        };

        return JsonUtility.ToJson(data);
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        SaveData savedData = JsonUtility.FromJson<SaveData>(data);

        RemoveExistingCats();
        int newCatCount = RecreateFieldCats(savedData.fieldCats);

        LoadBasicData(savedData);
        currentCatCount = newCatCount;

        UpdateAllUI();

        isDataLoaded = true;
    }

    // 기존 고양이 제거 함수
    private void RemoveExistingCats()
    {
        tempCatList.Clear();
        foreach (Transform child in gamePanel)
        {
            if (child.GetComponent<CatData>() != null)
            {
                tempCatList.Add(child.gameObject);
            }
        }

        foreach (var cat in tempCatList)
        {
            Destroy(cat);
        }
    }

    // 필드 고양이 재생성 함수
    private int RecreateFieldCats(List<CatInstanceData> fieldCats)
    {
        if (fieldCats == null) return 0;

        int newCatCount = 0;
        foreach (var catInstance in fieldCats)
        {
            Cat catData = allCatData.FirstOrDefault(c => c.CatId == catInstance.catId);
            if (catData == null) continue;

            GameObject catUIObject = Instantiate(catPrefab, gamePanel);
            if (catUIObject == null) continue;

            if (SetupCatInstance(catUIObject, catData, catInstance))
            {
                newCatCount++;
            }
        }
        return newCatCount;
    }

    // 고양이 인스턴스 설정 함수
    private bool SetupCatInstance(GameObject catUIObject, Cat catData, CatInstanceData instance)
    {
        if (!catUIObject.TryGetComponent<RectTransform>(out var rectTransform) ||
            !catUIObject.TryGetComponent<CatData>(out var catComponent))
            return false;

        rectTransform.anchoredPosition = new Vector2(instance.posX, instance.posY);
        catComponent.SetCatData(catData);

        if (AutoMoveManager.Instance != null)
        {
            catComponent.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
        }

        return true;
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

    private void GoogleSave()
    {
        if (GoogleManager.Instance != null)
        {
            Debug.Log("구글 저장");
            GoogleManager.Instance.SaveGameState();
        }
    }

    // 로그 기록 함수
    private void SaveLog(string message)
    {
        string path = $"{Application.persistentDataPath}/cat_count_log.txt";
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}\n";
        System.IO.File.AppendAllText(path, logMessage);
    }

    #endregion


}