using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 저장/로드가 필요한 컴포넌트에 적용할 인터페이스
public interface ISaveable
{
    string GetSaveData();
    void LoadFromData(string data);
}

[System.Serializable]
public class ComponentData
{
    public string path;
    public string data;
}

[System.Serializable]
public class CompleteGameState
{
    public List<ComponentData> components = new List<ComponentData>();

    public void AddComponentData(string path, string data)
    {
        components.Add(new ComponentData { path = path, data = data });
    }

    public bool TryGetValue(string path, out string data)
    {
        var component = components.FirstOrDefault(c => c.path == path);
        if (component != null)
        {
            data = component.data;
            return true;
        }
        data = null;
        return false;
    }
}

public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private TextMeshProUGUI logText;                        // 로그 텍스트 (나중에 없앨거임)
    private GameObject loadingScreen;                       // 로딩 스크린 (나중에 없애거나 수정할듯)
    private Button deleteDataButton;                        // 게임 데이터 삭제 버튼

    private const string fileName = "GameCompleteState";        // 파일 이름
    private const string gameScene = "GameScene-Han";           // GameScene 이름
    private const string loadingScreenName = "LoadingScreen";   // 로딩 스크린 이름
    private const float autoSaveInterval = 30f;                 // 주기적 자동 저장 시간
    private float autoSaveTimer = 0f;                           // 자동 저장 시간 계산 타이머

    private bool isLoggedIn = false;                        // 구글 로그인 여부
    private bool isDataLoaded = false;                      // 데이터 로드 여부
    private bool isSaving = false;                          // 현재 데이터 저장중 여부
    private bool isDeletingData = false;                    // 현재 데이터 삭제중 여부

    private CompleteGameState loadedGameState;
    private Dictionary<Type, string> cachedData = new Dictionary<Type, string>();

    public delegate void SaveCompletedCallback(bool success);

    private Canvas loadingScreenCanvas;

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
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        Application.targetFrameRate = 60;

        InitializeGooglePlay();
        InitializeLoadingScreen();

        StartCoroutine(GPGS_Login());
    }

    private void Update()
    {
        if (CanSkipAutoSave()) return;

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveGameState();
            autoSaveTimer = 0f;
        }
    }

    #endregion


    #region Initialize

    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();
    }

    private void InitializeLoadingScreen()
    {
        loadingScreen = GameObject.Find(loadingScreenName);
        if (loadingScreen != null)
        {
            loadingScreenCanvas = loadingScreen.GetComponent<Canvas>();
            loadingScreen.SetActive(false);
            DontDestroyOnLoad(loadingScreen);
        }
    }

    private bool CanSkipAutoSave()
    {
        return isDeletingData || (GameManager.Instance != null && GameManager.Instance.isQuiting);
    }

    #endregion


    #region Scene Management

    // 씬 로드 완료시 데이터를 적용하는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // LoadingScreen의 카메라 업데이트
        UpdateLoadingScreenCamera();

        // 씬이 로드될 때마다 삭제 버튼 찾기
        FindAndSetupDeleteButton();

        // 게임 씬이 로드되면 데이터 로드 시작
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(LoadDataAndInitializeGame());
        }
    }

    private IEnumerator LoadDataAndInitializeGame()
    {
        bool dataApplied = false;

        // 로그인된 경우에만 데이터 로드
        if (isLoggedIn)
        {
            bool loadComplete = false;
            LoadGameState(() => {
                loadComplete = true;
                // 데이터 로드 완료 직후 한 번만 적용
                if (isDataLoaded && !dataApplied)
                {
                    ApplyLoadedGameState();
                    dataApplied = true;
                }
            });

            // 로드 완료 대기
            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // 로딩 화면 숨기기 (약간의 지연 후)
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion


    #region 구글 로그인 및 UI

    // logText 찾아서 설정하는 함수
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator GPGS_Login()
    {
        // 로그인
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
        });

        // 로그인 완료 대기
        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }
    }

    // 구글 로그인 결과를 처리하는 함수
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            isLoggedIn = true;
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            if (logText != null)
            {
                logText.text = $"로그인 성공 : {displayName}";
            }
        }
        else
        {
            isLoggedIn = false;
            if (logText != null)
            {
                logText.text = $"로그인 실패";
            }
        }
    }

    // 게임 시작 버튼에 연결할 public 메서드
    public void OnGameStartButtonClick()
    {
        SceneManager.LoadScene(gameScene);
    }

    // 게임 시작 버튼에서 호출할 메서드
    public IEnumerator StartGameWithLoad(Action onLoadComplete = null)
    {
        ShowLoadingScreen(true);

        if (isLoggedIn)
        {
            bool loadComplete = false;
            LoadGameState(() => {
                loadComplete = true;
            });

            // 로드 완료 대기
            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // 씬 전환
        SceneManager.LoadScene(gameScene);

        if (onLoadComplete != null)
        {
            onLoadComplete();
        }
    }

    #endregion


    #region 로딩 화면 관리

    private void UpdateLoadingScreenCamera()
    {
        if (loadingScreenCanvas != null)
        {
            // 현재 씬의 메인 카메라 찾기
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                loadingScreenCanvas.worldCamera = mainCamera;
                loadingScreenCanvas.planeDistance = 1f; // 필요한 경우 거리 조정
            }
        }
    }

    // 로딩 화면을 표시하거나 숨기는 함수
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);

            // 로딩 화면 표시 중에는 게임 시간 정지
            if (!isDeletingData)
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(loadingScreen);
            }
        }
    }

    #endregion


    #region 데이터 저장 및 로드

    // 전체 게임 상태를 저장하는 함수
    public void SaveGameState()
    {
        // 데이터 삭제 중이거나 게임 종료 중일 때는 저장 중지
        if (!isLoggedIn || isDeletingData || (GameManager.Instance != null && GameManager.Instance.isQuiting)) return;
        if (isSaving) return;

        CompleteGameState gameState = new CompleteGameState();
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;
            string data = saveable.GetSaveData();

            cachedData[componentType] = data;
            gameState.AddComponentData(typeName, data);
        }

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(jsonData);
    }

    // 동기식 저장 함수 (종료 시 사용)
    public void SaveGameStateSync(SaveCompletedCallback callback = null)
    {
        if (!isLoggedIn || isDeletingData)
        {
            callback?.Invoke(false);
            return;
        }

        CompleteGameState gameState = new CompleteGameState();
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;
            string data = saveable.GetSaveData();

            cachedData[componentType] = data;
            gameState.AddComponentData(typeName, data);
        }

        string jsonData = JsonUtility.ToJson(gameState);

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Last saved: {DateTime.Now.ToString()}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                        bool success = saveStatus == SavedGameRequestStatus.Success;

                        callback?.Invoke(success);
                    });
                }
                else
                {
                    callback?.Invoke(false);
                }
            });

        // 저장이 너무 오래 걸리는 경우를 대비한 타임아웃 처리
        StartCoroutine(SaveTimeout(callback));
    }

    // 저장 타임아웃을 처리하는 코루틴
    private IEnumerator SaveTimeout(SaveCompletedCallback callback)
    {
        // 최대 3초 대기
        yield return new WaitForSeconds(2.0f);

        // 아직 콜백이 호출되지 않았다면 호출
        if (callback != null) callback(true);
    }

    // 클라우드에 데이터를 저장하는 함수
    private void SaveToCloud(string jsonData)
    {
        isSaving = true;

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Last saved: " + DateTime.Now.ToString())
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                        isSaving = false;
                    });
                }
                else
                {
                    isSaving = false;
                }
            });
    }

    // 전체 게임 상태를 로드하는 함수
    public void LoadGameState(Action onComplete = null)
    {
        if (!isLoggedIn || isDeletingData)
        {
            onComplete?.Invoke();
            return;
        }

        // 이미 데이터가 로드된 상태면 콜백만 호출
        if (isDataLoaded)
        {
            onComplete?.Invoke();
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    saveGameClient.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success)
                        {
                            if (data == null || data.Length == 0)
                            {
                                loadedGameState = new CompleteGameState();
                                isDataLoaded = true;
                                cachedData.Clear();
                            }
                            else
                            {
                                string jsonData = Encoding.UTF8.GetString(data);
                                loadedGameState = JsonUtility.FromJson<CompleteGameState>(jsonData);
                                isDataLoaded = true;
                                CacheLoadedData();
                            }
                        }
                        onComplete?.Invoke();
                    });
                }
                else
                {
                    onComplete?.Invoke();
                }
            });
    }

    // 로드된 데이터를 캐시에 저장하는 함수
    private void CacheLoadedData()
    {
        if (!isDataLoaded || loadedGameState == null) return;

        cachedData.Clear();
        foreach (var component in loadedGameState.components)
        {
            Type componentType = Type.GetType(component.path);
            if (componentType != null)
            {
                cachedData[componentType] = component.data;
            }
        }
    }

    // 로드된 게임 상태를 적용하는 함수
    public void ApplyLoadedGameState()
    {
        if (!isDataLoaded) return;

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();

            if (cachedData.TryGetValue(componentType, out string componentData))
            {
                saveable.LoadFromData(componentData);
            }
        }
    }

    #endregion


    #region 데이터 삭제

    // 저장된 게임 데이터를 삭제하는 함수
    public void DeleteGameData(Action<bool> onComplete = null)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(false);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    // 빈 데이터로 덮어쓰기
                    CompleteGameState emptyState = new CompleteGameState();
                    string emptyJson = JsonUtility.ToJson(emptyState);
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Data deleted: {DateTime.Now.ToString()}")
                        .Build();
                    
                    saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                    {
                        bool success = saveStatus == SavedGameRequestStatus.Success;
                        if (success)
                        {
                            // 캐시된 데이터도 초기화
                            loadedGameState = null;
                            isDataLoaded = false;
                            cachedData.Clear();
                        }
                        onComplete?.Invoke(success);
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // 삭제 버튼 찾아서 설정하는 함수
    private void FindAndSetupDeleteButton()
    {
        GameObject buttonObj = GameObject.Find("Canvas/Main UI Panel/Top Simple Button Panel/Delete Data Button");
        if (buttonObj != null)
        {
            deleteDataButton = buttonObj.GetComponent<Button>();
            if (deleteDataButton != null)
            {
                deleteDataButton.onClick.RemoveAllListeners();
                deleteDataButton.onClick.AddListener(DeleteGameDataAndQuit);
            }
        }
        else
        {
            deleteDataButton = null;
        }
    }

    // 게임 데이터 삭제 후 앱 종료하는 함수 (버튼에 연결할 함수)
    public void DeleteGameDataAndQuit()
    {
        if (!isLoggedIn) return;

        isDeletingData = true;
        Time.timeScale = 0f;

        // 먼저 현재 씬의 모든 컴포넌트 초기화
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        // 클라우드 데이터 삭제
        StartCoroutine(DeleteDataWithConfirmation());
    }

    // 데이터 삭제 확인 코루틴 추가
    private IEnumerator DeleteDataWithConfirmation()
    {
        bool deleteCompleted = false;
        bool deleteSuccess = false;

        // 삭제 시도
        DeleteGameData((success) => {
            deleteCompleted = true;
            deleteSuccess = success;
        });

        // 삭제 완료 대기 (최대 3초)
        float waitTime = 0;
        while (!deleteCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // 삭제 후 로컬 데이터 초기화 확인
        loadedGameState = null;
        isDataLoaded = false;
        cachedData.Clear();

        // 삭제 확인을 위한 추가 저장 (빈 데이터)
        CompleteGameState emptyState = new CompleteGameState();
        string emptyJson = JsonUtility.ToJson(emptyState);
        SaveToCloud(emptyJson);

        // 저장 완료 대기 후 게임 종료
        yield return new WaitForSecondsRealtime(2.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // 지연 후 앱 종료하는 코루틴
    private IEnumerator QuitGameAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    #endregion


    #region OnApplication

    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 홈 버튼으로 나가면 자동 저장 (백그라운드로 전환)
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 다른 앱으로 전환시 자동 저장
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !CanSkipAutoSave())
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 즉시 동기식 저장 함수 (비정상 종료 대비)
    private void SaveGameStateSyncImmediate()
    {
        if (!isLoggedIn) return;

        CompleteGameState gameState = new CompleteGameState();
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;
            string data = saveable.GetSaveData();

            cachedData[componentType] = data;
            gameState.AddComponentData(typeName, data);
        }

        // 즉시 저장을 위한 동기 방식 시도
        string jsonData = JsonUtility.ToJson(gameState);
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Emergency save: {DateTime.Now.ToString()}")
                        .Build();
                   
                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => { });
                }
            });
    }

    #endregion


}
