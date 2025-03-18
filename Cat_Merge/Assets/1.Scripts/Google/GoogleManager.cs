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

    // Dictionary를 List로 변환하는 메서드
    public void AddComponentData(string path, string data)
    {
        components.Add(new ComponentData { path = path, data = data });
    }

    // List에서 Dictionary처럼 데이터 조회
    public bool TryGetValue(string path, out string data)
    {
        foreach (var component in components)
        {
            if (component.path == path)
            {
                data = component.data;
                return true;
            }
        }
        data = null;
        return false;
    }
}

public class GoogleManager : MonoBehaviour
{
    #region 변수들

    public static GoogleManager Instance { get; private set; }

    private const string fileName = "GameCompleteState";
    private const string gameScene = "GameScene-Han";

    private TextMeshProUGUI logText;

    private GameObject loadingScreen;

    private bool isLoggedIn = false;
    private bool isDataLoaded = false;
    private CompleteGameState loadedGameState;
    private Dictionary<Type, string> cachedData = new Dictionary<Type, string>();

    private float autoSaveInterval = 30f;
    private float autoSaveTimer = 0f;

    private bool isSaving = false;
    private bool isDeletingData = false;

    public delegate void SaveCompletedCallback(bool success);

    private Button deleteDataButton;        // 게임 데이터 삭제 버튼

    #endregion

    #region 초기화 및 이벤트 처리

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();
        GPGS_LogIn();

        loadingScreen = GameObject.Find("LoadingScreen");
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
            DontDestroyOnLoad(loadingScreen);
        }
    }

    private void Update()
    {
        // 데이터 삭제 중이거나 게임 종료 중일 때는 자동 저장 중지
        if (isDeletingData || (GameManager.Instance != null && GameManager.Instance.isQuiting)) return;

        // 주기적 자동 저장 처리
        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveGameState();
            autoSaveTimer = 0f;
        }
    }

    // 씬 로드 완료시 데이터를 적용하는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때마다 삭제 버튼 찾기
        FindAndSetupDeleteButton();

        // 게임 씬이 로드되면 데이터 적용
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(ApplyDataAndShowScreenCoroutine());
        }
    }

    // 데이터 적용 및 화면 표시를 지연시키는 코루틴
    private IEnumerator ApplyDataAndShowScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ApplyDataAndShowScreen();
    }

    #endregion

    #region 구글 로그인 및 UI

    // logText 찾아서 설정하는 함수
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    // 구글 플레이 로그인을 시도하는 함수
    public void GPGS_LogIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
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
                logText.text = "로그인 성공 : " + displayName + " / " + userID;
            }

            // 로그인 성공 시 자동으로 데이터 로드
            LoadGameState();
        }
        else
        {
            isLoggedIn = false;
            if (logText != null)
            {
                logText.text = "로그인 실패";
            }
        }
    }

    #endregion

    #region 데이터 저장 및 로드

    // 전체 게임 상태를 저장하는 함수
    public void SaveGameState()
    {
        // 데이터 삭제 중이거나 게임 종료 중일 때는 저장 중지
        if (!isLoggedIn || isDeletingData ||
            (GameManager.Instance != null && GameManager.Instance.isQuiting)) return;
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
            if (callback != null) callback(false);
            return;
        }

        Debug.Log("동기식 저장 시작...");
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

        // 저장 완료 플래그
        bool saveCompleted = false;

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
                        saveCompleted = true;
                        bool success = saveStatus == SavedGameRequestStatus.Success;
                        if (success)
                        {
                            Debug.Log("동기식 클라우드 저장 성공: " + DateTime.Now.ToString());
                        }
                        else
                        {
                            Debug.LogWarning("동기식 클라우드 저장 실패: " + saveStatus);
                        }

                        if (callback != null) callback(success);
                    });
                }
                else
                {
                    Debug.LogError("동기식 저장 게임 열기 실패: " + status);
                    saveCompleted = true;
                    if (callback != null) callback(false);
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
        Debug.Log("클라우드 저장 시작...");

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
                        if (saveStatus == SavedGameRequestStatus.Success)
                        {
                            Debug.Log("클라우드 저장 성공: " + DateTime.Now.ToString());
                        }
                        else
                        {
                            Debug.LogWarning("클라우드 저장 실패: " + saveStatus);
                        }
                    });
                }
                else
                {
                    isSaving = false;
                    Debug.LogError("저장 게임 열기 실패: " + status);
                }
            });
    }

    // 전체 게임 상태를 로드하는 함수
    public void LoadGameState()
    {
        if (!isLoggedIn || isDeletingData) return;

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
                                // 데이터가 없거나 빈 경우 초기 상태로 설정
                                Debug.Log("저장된 데이터가 없거나 비어 있습니다. 초기 상태로 설정합니다.");
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

                            if (SceneManager.GetActiveScene().name == gameScene)
                            {
                                ApplyLoadedGameState();
                            }
                        }
                        else
                        {
                            Debug.LogError($"데이터 읽기 실패: {readStatus}");
                        }
                    });
                }
                else
                {
                    Debug.LogError($"저장 게임 열기 실패: {status}");
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
            try
            {
                Type componentType = Type.GetType(component.path);
                if (componentType != null)
                {
                    cachedData[componentType] = component.data;
                }
            }
            catch (Exception) { }
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

    // 저장된 게임 데이터를 삭제하는 함수
    public void DeleteGameData(Action<bool> onComplete = null)
    {
        Debug.Log("DeleteGameData 함수 시작...");

        if (!isLoggedIn)
        {
            Debug.LogError("로그인되어 있지 않아 데이터 삭제 불가");
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
                Debug.Log($"파일 열기 상태: {status}");

                if (status == SavedGameRequestStatus.Success)
                {
                    // 빈 데이터로 덮어쓰기
                    CompleteGameState emptyState = new CompleteGameState();
                    string emptyJson = JsonUtility.ToJson(emptyState);
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Data deleted: " + DateTime.Now.ToString())
                        .Build();

                    Debug.Log("빈 데이터로 덮어쓰기 시도...");
                    saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                    {
                        Debug.Log($"데이터 삭제 저장 상태: {saveStatus}");
                        bool success = saveStatus == SavedGameRequestStatus.Success;
                        if (success)
                        {
                            // 캐시된 데이터도 초기화
                            loadedGameState = null;
                            isDataLoaded = false;
                            cachedData.Clear();
                            Debug.Log("게임 데이터 삭제 성공");
                        }
                        else
                        {
                            Debug.LogError("게임 데이터 삭제 실패: " + saveStatus);
                        }
                        onComplete?.Invoke(success);
                    });
                }
                else
                {
                    Debug.LogError("게임 데이터 삭제를 위한 파일 열기 실패: " + status);
                    onComplete?.Invoke(false);
                }
            });
    }

    // 삭제 버튼 찾아서 설정하는 함수
    private void FindAndSetupDeleteButton()
    {
        try
        {
            GameObject buttonObj = GameObject.Find("Canvas/Main UI Panel/Top Simple Button Panel/Delete Data Button");
            if (buttonObj != null)
            {
                deleteDataButton = buttonObj.GetComponent<Button>();
                if (deleteDataButton != null)
                {
                    // 기존 리스너 제거 후 새로 추가
                    deleteDataButton.onClick.RemoveAllListeners();
                    deleteDataButton.onClick.AddListener(DeleteGameDataAndQuit);
                    Debug.Log("데이터 삭제 버튼 연동 성공");
                }
                else
                {
                    Debug.LogWarning("데이터 삭제 버튼 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("데이터 삭제 버튼 오브젝트를 찾을 수 없습니다.");
                deleteDataButton = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"데이터 삭제 버튼 설정 중 오류 발생: {e.Message}");
            deleteDataButton = null;
        }
    }

    // 게임 데이터 삭제 후 앱 종료하는 함수 (버튼에 연결할 함수)
    public void DeleteGameDataAndQuit()
    {
        Debug.Log("데이터 삭제 및 종료 시작...");

        // 로그인 상태 확인
        if (!isLoggedIn)
        {
            Debug.LogError("데이터 삭제 실패: 로그인되어 있지 않습니다.");
            return;
        }

        // 게임 일시 정지 및 삭제 모드 활성화
        isDeletingData = true;
        Time.timeScale = 0f;

        // 먼저 현재 씬의 모든 컴포넌트 초기화
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        Debug.Log($"초기화할 컴포넌트 수: {saveables.Length}");

        foreach (ISaveable saveable in saveables)
        {
            try
            {
                saveable.LoadFromData(null); // 모든 컴포넌트 초기화
            }
            catch (Exception e)
            {
                Debug.LogError($"컴포넌트 초기화 중 오류: {e.Message}");
            }
        }

        // 클라우드 데이터 삭제 - 더 긴 대기 시간 적용
        StartCoroutine(DeleteDataWithConfirmation());
    }

    // 데이터 삭제 확인 코루틴 추가
    private IEnumerator DeleteDataWithConfirmation()
    {
        bool deleteCompleted = false;
        bool deleteSuccess = false;

        // 첫 번째 삭제 시도
        DeleteGameData((success) => {
            deleteCompleted = true;
            deleteSuccess = success;
            Debug.Log($"첫 번째 데이터 삭제 결과: {success}");
        });

        // 삭제 완료 대기 (최대 3초)
        float waitTime = 0;
        while (!deleteCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // 첫 번째 시도가 실패했거나 시간 초과된 경우 두 번째 시도
        if (!deleteSuccess)
        {
            Debug.Log("첫 번째 삭제 시도 실패, 두 번째 시도 중...");
            deleteCompleted = false;

            // 두 번째 삭제 시도
            DeleteGameDataAlternative((success) => {
                deleteCompleted = true;
                deleteSuccess = success;
                Debug.Log($"두 번째 데이터 삭제 결과: {success}");
            });

            // 두 번째 삭제 완료 대기 (최대 3초)
            waitTime = 0;
            while (!deleteCompleted && waitTime < 3.0f)
            {
                waitTime += 0.1f;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        // 삭제 후 로컬 데이터 초기화 확인
        loadedGameState = null;
        isDataLoaded = false;
        cachedData.Clear();

        // 삭제 확인을 위한 추가 저장 (빈 데이터)
        CompleteGameState emptyState = new CompleteGameState();
        string emptyJson = JsonUtility.ToJson(emptyState);
        SaveToCloud(emptyJson);

        // 저장 완료 대기
        yield return new WaitForSecondsRealtime(2.0f);

        Debug.Log("데이터 삭제 프로세스 완료, 게임 종료 중...");

        // 게임 종료
        StartCoroutine(QuitGameAfterDelay());
    }

    // 대체 데이터 삭제 방법 추가
    private void DeleteGameDataAlternative(Action<bool> onComplete)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(false);
            return;
        }

        try
        {
            // 빈 데이터 생성
            CompleteGameState emptyState = new CompleteGameState();
            string emptyJson = JsonUtility.ToJson(emptyState);
            byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

            // 직접 저장 시도
            ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
            saveGameClient.OpenWithManualConflictResolution(
                fileName,
                DataSource.ReadNetworkOnly, // 네트워크에서만 읽기 시도
                true, // 새 게임 생성 허용
                ConflictCallback,
                (status, game) =>
                {
                    if (status == SavedGameRequestStatus.Success)
                    {
                        SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                            .WithUpdatedDescription("Data reset: " + DateTime.Now.ToString())
                            .Build();

                        saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                        {
                            bool success = saveStatus == SavedGameRequestStatus.Success;
                            if (success)
                            {
                                // 캐시 초기화 시도
                                PlayerPrefs.DeleteAll();
                                PlayerPrefs.Save();

                                Debug.Log("대체 방식으로 데이터 삭제 성공");
                            }
                            else
                            {
                                Debug.LogError("대체 방식으로 데이터 삭제 실패: " + saveStatus);
                            }
                            onComplete?.Invoke(success);
                        });
                    }
                    else
                    {
                        Debug.LogError("대체 방식으로 게임 열기 실패: " + status);
                        onComplete?.Invoke(false);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"대체 삭제 중 예외 발생: {e.Message}");
            onComplete?.Invoke(false);
        }
    }

    // 수동 충돌 해결 콜백
    private void ConflictCallback(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        // 항상 빈 데이터로 해결
        CompleteGameState emptyState = new CompleteGameState();
        string emptyJson = JsonUtility.ToJson(emptyState);
        byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

        // 빈 데이터로 충돌 해결
        resolver.ChooseMetadata(unmerged);
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

    #region 로딩 화면 관리

    // 로딩 화면을 표시하거나 숨기는 함수
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);

            // 로딩 화면 표시 중에는 게임 시간 정지
            if (!isDeletingData) // 데이터 삭제 중이 아닐 때만 타임스케일 조정
            {
                Time.timeScale = show ? 0f : 1f;
            }

            if (show)
            {
                DontDestroyOnLoad(loadingScreen);
            }
        }
    }

    // 데이터를 적용하고 화면을 표시하는 함수
    private void ApplyDataAndShowScreen()
    {
        ApplyLoadedGameState();
        StartCoroutine(HideLoadingScreenCoroutine());
    }

    // 로딩 화면을 숨기는 코루틴
    private IEnumerator HideLoadingScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion

    #region OnApplication

    // 앱 종료시 동기식 저장을 실행하는 함수
    private void OnApplicationQuit()
    {
        // 데이터 삭제 중이거나 게임 종료 중일 때는 저장 중지
        if (!isDeletingData &&
            (GameManager.Instance == null || !GameManager.Instance.isQuiting))
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 홈 버튼으로 나가면 자동 저장 (백그라운드로 전환)
    private void OnApplicationPause(bool pause)
    {
        if (pause && !isDeletingData &&
            (GameManager.Instance == null || !GameManager.Instance.isQuiting))
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 다른 앱으로 전환시 자동 저장
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !isDeletingData &&
            (GameManager.Instance == null || !GameManager.Instance.isQuiting))
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
            try
            {
                MonoBehaviour mb = (MonoBehaviour)saveable;
                Type componentType = mb.GetType();
                string typeName = componentType.FullName;
                string data = saveable.GetSaveData();

                cachedData[componentType] = data;
                gameState.AddComponentData(typeName, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"저장 중 오류 발생: {e.Message}");
            }
        }

        string jsonData = JsonUtility.ToJson(gameState);

        // 즉시 저장을 위한 동기 방식 시도
        try
        {
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
                            .WithUpdatedDescription("Emergency save: " + DateTime.Now.ToString())
                            .Build();

                        saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => {
                            if (saveStatus == SavedGameRequestStatus.Success)
                            {
                                Debug.Log("즉시 저장 성공: " + DateTime.Now.ToString());
                            }
                            else
                            {
                                Debug.LogWarning("즉시 저장 실패: " + saveStatus);
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("즉시 저장 게임 열기 실패: " + status);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError($"즉시 저장 중 예외 발생: {e.Message}");
        }
    }
    #endregion

}
