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

    // 상수 및 변수
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

    // 추가된 변수
    private bool isSaving = false;

    // 추가된 델리게이트: 저장 완료 콜백
    public delegate void SaveCompletedCallback(bool success);

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
        if (!isLoggedIn) return;

        // 이미 저장 중이면 중복 저장 방지
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
        if (!isLoggedIn)
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
        if (!isLoggedIn) return;

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
                            string jsonData = Encoding.UTF8.GetString(data);
                            loadedGameState = JsonUtility.FromJson<CompleteGameState>(jsonData);
                            isDataLoaded = true;
                            CacheLoadedData();

                            if (SceneManager.GetActiveScene().name == gameScene)
                            {
                                ApplyLoadedGameState();
                            }
                        }
                    });
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

    #endregion

    #region 로딩 화면 관리

    // 로딩 화면을 표시하거나 숨기는 함수
    public void ShowLoadingScreen(bool show)
    {
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(show);

            // 로딩 화면 표시 중에는 게임 시간 정지
            Time.timeScale = show ? 0f : 1f;

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

    // 현재 저장에 문제가 있는 경우들 (Android)
    // 게임종료버튼으로 나가기 = 저장 O
    // 홈으로 나갔다가 다시 들어와서 게임종료버튼으로 나가기 = 저장 O
    // 홈으로 나갔다가 다시 들어와서 여러탭버튼 누르고 앱 지우기 = 저장 O
    // 홈으로 나갔다가 여러탭버튼 누르고 앱 지우기 = 저장 X
    // 실행중 여러탭버튼 누르고 앱 지우기 = 저장 X

    // 저장할 데이터들이 변경될때 저장을하는 로직을 추가하니까 안되던것들이 되지만 조건이 있음
    // 값을 변경하자마자 바로 여러탭버튼 누르고 앱 지우면 저장 X
    // 값을 변경하자마자 바로 홈으로 나가싸가 여러탭버튼 누르고 앱 지우면 저장 X
    // 값을 변경하고 2~3초는 게임에 머무르면 어떤식으로 나가든 저장 O

    // 앱 종료시 동기식 저장을 실행하는 함수
    private void OnApplicationQuit()
    {
        SaveGameStateSyncImmediate();
    }

    // 홈 버튼으로 나가면 자동 저장 (백그라운드로 전환)
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameStateSyncImmediate();
        }
    }

    // 다른 앱으로 전환시 자동 저장
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
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
