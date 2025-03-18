using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Linq;

// GameManager Script
[DefaultExecutionOrder(-1)]     // 스크립트 실행 순서 조정(3번째)
public class GameManager : MonoBehaviour, ISaveable
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Main Cat Data
    private Cat[] allCatData;                                               // 모든 고양이 데이터
    public Cat[] AllCatData => allCatData;

    // Main Mouse Data
    private Mouse[] allMouseData;                                           // 모든 쥐 데이터
    public Mouse[] AllMouseData => allMouseData;

    //
    public GameObject catPrefab;                                            // 고양이 UI 프리팹
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

            if (GoogleManager.Instance != null)
            {
                Debug.Log("구글 저장");
                GoogleManager.Instance.SaveGameState();
            }
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

            if (GoogleManager.Instance != null)
            {
                Debug.Log("구글 저장");
                GoogleManager.Instance.SaveGameState();
            }
        }
    }

    [SerializeField] private TextMeshProUGUI coinText;                      // 기본재화 텍스트
    private decimal coin = 100000000;                                       // 기본재화
    public decimal Coin
    {
        get => coin;
        set
        {
            coin = value;
            UpdateCoinText();

            if (GoogleManager.Instance != null)
            {
                Debug.Log("구글 저장");
                GoogleManager.Instance.SaveGameState();
            }
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

            if (GoogleManager.Instance != null)
            {
                Debug.Log("구글 저장");
                GoogleManager.Instance.SaveGameState();
            }
        }
    }

    [Header("---[Quit Panel]")]
    [SerializeField] private GameObject quitPanel;                          // 종료 확인 패널
    [SerializeField] private Button closeButton;                            // 종료 패널 닫기 버튼
    [SerializeField] private Button okButton;                               // 종료 확인 버튼
    private bool isBackButtonPressed = false;                               // 뒤로가기 버튼이 눌렸는지 여부

    private bool isDataLoaded = false;

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
        InitializeExitSystem();
    }

    private void Start()
    {
        if (isDataLoaded == false)
        {
            currentCatCount = 0;
            maxCats = 8;
            coin = 100000000;
            cash = 1000;

            UpdateCatCountText();
            UpdateCoinText();
            UpdateCashText();
        }
    }

    private void Update()
    {
        SortCatUIObjectsByYPosition();
        CheckQuitInput();
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

    // Y축 위치를 기준으로 고양이 UI 정렬하는 함수
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

    // 게임 종료 버튼 초기화 함수
    private void InitializeExitSystem()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
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

    // 뒤로가기버튼 입력 딜레이 설정 함수
    private IEnumerator ResetBackButtonPress()
    {
        yield return new WaitForSeconds(0.2f);
        isBackButtonPressed = false;
    }

    // 종료 패널 표시 함수
    private void ShowQuitPanel()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(true);
        }
    }

    // 종료 취소 함수
    public void CancelQuit()
    {
        if (quitPanel != null)
        {
            quitPanel.SetActive(false);
        }
    }

    // 게임 종료 함수
    public void QuitGame()
    {
        if (GoogleManager.Instance != null)
        {
            // 저장 완료 콜백을 받아 종료하도록 수정
            GoogleManager.Instance.SaveGameStateSync(OnSaveCompleted);
        }
        else
        {
            QuitApplication();
        }
    }

    // 저장 완료 후 호출될 콜백 함수 추가 함수
    private void OnSaveCompleted(bool success)
    {
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

    // ======================================================================================================================

    #region Content

    // 필드의 모든 고양이 정보 업데이트 함수
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

    // ======================================================================================================================

    #region Save System
    [Serializable]
    private class CatInstanceData
    {
        public int catId;              // 고양이 ID
        public float posX;             // X 위치
        public float posY;             // Y 위치
    }

    [Serializable]
    private class SaveData
    {
        public int currentCatCount;     // 현재 고양이 수
        public int maxCats;             // 최대 고양이 수
        public string coin;             // 기본 재화
        public string cash;             // 캐시 재화
        public List<CatInstanceData> fieldCats = new List<CatInstanceData>();  // 필드에 있는 고양이들 정보
    }

    public string GetSaveData()
    {
        // 현재 필드에 있는 모든 고양이 정보 수집
        int actualCatCount = 0;
        List<CatInstanceData> fieldCats = new List<CatInstanceData>();

        foreach (Transform child in gamePanel)
        {
            CatData catData = child.GetComponent<CatData>();
            if (catData != null)
            {
                actualCatCount++;
                CatInstanceData instanceData = new CatInstanceData
                {
                    catId = catData.catData.CatId,
                    posX = child.GetComponent<RectTransform>().anchoredPosition.x,
                    posY = child.GetComponent<RectTransform>().anchoredPosition.y
                };
                fieldCats.Add(instanceData);
            }
        }

        // currentCatCount 동기화
        if (actualCatCount != currentCatCount)
        {
            currentCatCount = actualCatCount;
        }

        SaveData data = new SaveData
        {
            currentCatCount = this.currentCatCount,
            maxCats = this.maxCats,
            coin = this.coin.ToString(),
            cash = this.cash.ToString(),
            fieldCats = fieldCats
        };

        string jsonData = JsonUtility.ToJson(data);

        return jsonData;
    }

    public void LoadFromData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        try
        {
            SaveData savedData = JsonUtility.FromJson<SaveData>(data);
            //Debug.Log($"로드된 데이터: 저장된 고양이 수={savedData.currentCatCount}, 필드 고양이={savedData.fieldCats?.Count ?? 0}");

            // 기존 필드의 고양이들 제거
            foreach (Transform child in gamePanel)
            {
                if (child.GetComponent<CatData>() != null)
                {
                    Destroy(child.gameObject);
                }
            }

            int actualCatCount = 0;

            // 저장된 고양이들 재생성
            if (savedData.fieldCats != null)
            {
                foreach (var catInstance in savedData.fieldCats)
                {
                    Cat catData = allCatData.FirstOrDefault(c => c.CatId == catInstance.catId);
                    if (catData != null)
                    {
                        GameObject catUIObject = Instantiate(catPrefab, gamePanel);
                        RectTransform rectTransform = catUIObject.GetComponent<RectTransform>();
                        rectTransform.anchoredPosition = new Vector2(catInstance.posX, catInstance.posY);

                        CatData catComponent = catUIObject.GetComponent<CatData>();
                        if (catComponent != null)
                        {
                            catComponent.SetCatData(catData);
                            catComponent.SetAutoMoveState(AutoMoveManager.Instance.IsAutoMoveEnabled());
                            actualCatCount++;
                        }
                    }
                }
            }

            // 저장된 값과 실제 고양이 수 비교하여 동기화
            if (savedData.currentCatCount != actualCatCount)
            {
                //Debug.LogWarning($"저장된 고양이 수({savedData.currentCatCount})와 실제 고양이 수({actualCatCount})가 불일치합니다. 실제 수로 동기화합니다.");
            }

            // 데이터 복원
            this.currentCatCount = actualCatCount;
            this.maxCats = savedData.maxCats;
            if (decimal.TryParse(savedData.coin, out decimal parsedCoin))
            {
                this.coin = parsedCoin;
            }
            if (decimal.TryParse(savedData.cash, out decimal parsedCash))
            {
                this.cash = parsedCash;
            }

            // UI 업데이트
            UpdateCatCountText();
            UpdateCoinText();
            UpdateCashText();

            isDataLoaded = true;
            //Debug.Log($"데이터 로드 완료 - 최종 고양이 수: {currentCatCount}");
        }
        catch (Exception e)
        {
            //Debug.LogError($"데이터 로드 중 오류: {e.Message}");
            this.currentCatCount = 0;
            UpdateCatCountText();
        }
    }

    #endregion

}
