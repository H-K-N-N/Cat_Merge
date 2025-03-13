using System;
using System.Collections.Generic;
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

    // 씬 로드 완료 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 게임 씬이 로드되면 데이터 적용
        if (scene.name == gameScene)
        {
            ShowLoadingScreen(true);
            StartCoroutine(ApplyDataAndShowScreenCoroutine());
        }
    }

    private System.Collections.IEnumerator ApplyDataAndShowScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ApplyDataAndShowScreen();
    }

    // 게임 종료 시 자동 저장
    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    // 앱이 백그라운드로 가면 자동 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveGameState();
        }
    }

    #endregion

    #region 구글 로그인 및 UI

    // logText 찾아서 설정하는 함수
    private void UpdateLogText()
    {
        logText = GameObject.Find("Canvas/Title UI/Log Text")?.GetComponent<TextMeshProUGUI>();
    }

    public void GPGS_LogIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    // 구글 로그인
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

    // 전체 게임 상태 저장
    public void SaveGameState()
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

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(jsonData);
    }

    // 클라우드에 데이터 저장
    private void SaveToCloud(string jsonData)
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
                        .WithUpdatedDescription("Last saved: " + DateTime.Now.ToString())
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) => { });
                }
            });
    }

    // 전체 게임 상태 로드
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

    // 로드된 데이터를 캐시에 저장
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

    // 로드된 게임 상태 적용
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

    // 로딩 화면 표시/숨김 처리
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

    // 데이터 적용 후 화면 표시
    private void ApplyDataAndShowScreen()
    {
        ApplyLoadedGameState();
        StartCoroutine(HideLoadingScreenCoroutine());
    }

    private System.Collections.IEnumerator HideLoadingScreenCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        ShowLoadingScreen(false);
    }

    #endregion

}
