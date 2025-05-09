using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
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
    public long timestamp;  // 타임스탬프 추가

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

    public void UpdateTimestamp()
    {
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private Button deleteDataButton;                        // 게임 데이터 삭제 버튼 (나중에 삭제 예정)

    private const string fileName = "GoogleCloudSaveState"; // 파일 이름
    private const string gameScene = "GameScene-Han";       // GameScene 이름
    private const float autoSaveInterval = 30f;             // 주기적 자동 저장 시간
    private float autoSaveTimer = 0f;                       // 자동 저장 시간 계산 타이머

    [HideInInspector] public bool isLoggedIn = false;       // 구글 로그인 여부
    [HideInInspector] public bool isDeleting = false;       // 현재 데이터 삭제중 여부
    private bool isSaving = false;                          // 저장 중인지 확인하는 플래그
    private bool isGameStarting = false;                    // 게임 시작 중인지 확인하는 플래그

    private Vector2 gameStartPosition;                      // 게임 시작 터치 위치를 저장할 변수

    private const string encryptionKey = "CatMergeGame_EncryptionKey";  // 암호화 키

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

    private void Start()
    {
        InitializeGooglePlay();
        UnencryptedData();
        StartCoroutine(GPGS_Login());
    }

    private void Update()
    {
        if (CanSkipAutoSave()) return;

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= autoSaveInterval)
        {
            SaveToCloudWithLocalData();
            autoSaveTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion


    #region Initialize

    // Google Play 관련 세팅 함수
    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    #endregion


    #region Scene Management

    // 씬 로드 완료시 데이터를 적용하는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드될 때 삭제 버튼 찾기
        FindAndSetupDeleteButton();

        // 카메라 업데이트는 항상 실행
        LoadingScreen.Instance?.UpdateLoadingScreenCamera();
    }

    #endregion


    #region Google Login & Game Start Setting

    // 구글 로그인 코루틴
    private IEnumerator GPGS_Login()
    {
        // 구글 로그인 시도
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
        });

        // 로그인 완료 대기 (최대 5초)
        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // 데이터 동기화 및 로드
        if (isLoggedIn)
        {
            bool syncComplete = false;
            SynchronizeData((success) => {
                if (success)
                {
                    LoadFromLocalPlayerPrefs(gameObject);
                }
                else
                {
                    Debug.LogWarning("클라우드 동기화 실패. 로컬 데이터만 사용합니다.");
                    LoadFromLocalPlayerPrefs(gameObject);
                }
                syncComplete = true;
            });

            // 동기화 완료 대기 (최대 5초)
            waitTime = 0;
            while (!syncComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // 비로그인 상태: 로컬 데이터만 로드
            LoadFromLocalPlayerPrefs(gameObject);
        }
    }

    // 구글 로그인 결과를 처리하는 함수
    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            isLoggedIn = true;
        }
        else
        {
            isLoggedIn = false;
        }
    }

    // 게임 시작 버튼에 연결할 public 함수
    public void OnGameStartButtonClick()
    {
        // 이미 게임 시작 중이면 무시
        if (isGameStarting) return;

        // TitleManager 찾아서 게임 시작 알림
        TitleManager titleManager = FindObjectOfType<TitleManager>();
        titleManager?.OnGameStart();

        // 터치 위치 가져오기
        Vector2 touchPosition = Input.mousePosition;
        StartCoroutine(StartGameWithLoad(touchPosition));
    }

    // 게임 시작 버튼을 누르면 시작되는 게임 세팅 코루틴
    private IEnumerator StartGameWithLoad(Vector2 touchPosition)
    {
        isGameStarting = true;
        gameStartPosition = touchPosition;

        // 1. 로딩 화면 시작
        LoadingScreen.Instance.Show(true, touchPosition);

        // 2. 로딩 애니메이션 완료 대기
        yield return new WaitForSecondsRealtime(LoadingScreen.Instance.animationDuration);

        // 3. 씬 전환
        SceneManager.LoadScene(gameScene);

        // 4. 씬 로드 완료 대기
        yield return new WaitForEndOfFrame();

        // 4-1. GameManager 찾을 때까지 대기
        int maxAttempts = 10;
        int attempts = 0;
        GameManager gameManager = null;

        while (gameManager == null && attempts < maxAttempts)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                attempts++;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        if (gameManager == null)
        {
            yield break;
        }

        // 5. 데이터 로드 및 적용
        bool loadComplete = false;
        if (isLoggedIn)
        {
            // 로그인 상태: 클라우드 데이터 로드 시도
            LoadFromCloud((success) => {
                if (!success)
                {
                    // 클라우드 로드 실패시 로컬 데이터 사용
                    LoadFromLocalPlayerPrefs(gameManager.gameObject);
                }
                loadComplete = true;
            });
        }
        else
        {
            // 비로그인 상태: 로컬 데이터만 로드
            LoadFromLocalPlayerPrefs(gameManager.gameObject);
            loadComplete = true;
        }

        // 5-1. 로드 완료 대기
        float waitTime = 0;
        while (!loadComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        // 5-2. 데이터 적용
        ApplyLoadedGameState(gameManager.gameObject);

        // 6. 로딩 화면 숨기기
        yield return new WaitForSecondsRealtime(0.5f);
        LoadingScreen.Instance.Show(false, gameStartPosition);

        // 7. 첫게임 판별
        bool hasAnyData = CheckForAnyData(gameManager);
        if (!hasAnyData)
        {
            StartCoroutine(gameManager.ShowFirstGamePanel());
        }

        isGameStarting = false;
    }

    // 어떤 데이터라도 존재하는지 확인하는 함수 (첫 게임인지 판별)
    private bool CheckForAnyData(GameManager gameManager)
    {
        var saveables = gameManager.gameObject.GetComponents<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                return true;
            }
        }

        return false;
    }

    #endregion


    #region Data Save and Load System

    // 자동 저장을 스킵해도 되는지 판별하는 함수
    private bool CanSkipAutoSave()
    {
        return isDeleting || (GameManager.Instance != null && GameManager.Instance.isQuiting);
    }

    // 문자열 암호화 함수
    private string EncryptData(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        try
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptedBytes = new byte[dataBytes.Length];

            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            string result = Convert.ToBase64String(encryptedBytes);

            // 결과가 원본과 같은지 검증
            if (result == data)
            {
                Debug.LogError("[암호화 실패] 암호화된 결과가 원본과 동일합니다!");
            }

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[암호화 오류] {e.Message}\n{e.StackTrace}");
            return data;
        }
    }

    // 문자열 복호화 함수
    private string DecryptData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData)) return encryptedData;

        try
        {
            // 데이터가 Base64 형식인지 확인 (암호화된 데이터인지 확인)
            if (!IsValidBase64(encryptedData))
            {
                return encryptedData;
            }

            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            string result = Encoding.UTF8.GetString(decryptedBytes);

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[복호화 오류] {e.Message}\n{e.StackTrace}");
            return encryptedData;
        }
    }

    // 문자열이 유효한 Base64 형식인지 확인하는 함수
    private bool IsValidBase64(string base64String)
    {
        // Base64 문자열은 길이가 4의 배수여야 하며, 특정 문자만 포함해야 함
        if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
        {
            return false;
        }

        // Base64에 사용되는 문자만 포함하는지 확인 (A-Z, a-z, 0-9, +, /, =)
        foreach (char c in base64String)
        {
            if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '+' && c != '/' && c != '=')
            {
                return false;
            }
        }

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // 모든 암호화되지 않은 데이터를 암호화하는 함수
    private void UnencryptedData()
    {
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data) && !IsValidBase64(data))
            {
                SaveToPlayerPrefs(typeName, data);
            }
        }
    }

    // PlayerPrefs에 암호화된 데이터 저장하는 함수
    public void SaveToPlayerPrefs(string key, string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            // 원본 데이터와 암호화된 데이터
            string encryptedData = EncryptData(data);

            // 암호화가 제대로 되었는지 확인
            bool isEncrypted = IsValidBase64(encryptedData) && encryptedData != data;

            // 암호화가 실패했다면 다시 시도
            if (!isEncrypted)
            {
                encryptedData = ForcedEncrypt(data);
            }

            PlayerPrefs.SetString(key, encryptedData);
            PlayerPrefs.Save();
        }
    }

    // 강제 암호화 함수 (문제 해결용)
    private string ForcedEncrypt(string data)
    {
        if (string.IsNullOrEmpty(data)) return data;

        try
        {
            // 명시적으로 단계별로 암호화 진행
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] encryptedBytes = new byte[dataBytes.Length];

            // XOR 암호화
            for (int i = 0; i < dataBytes.Length; i++)
            {
                encryptedBytes[i] = (byte)(dataBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            // Base64로 인코딩
            string result = Convert.ToBase64String(encryptedBytes);
            Debug.Log($"강제 암호화 결과: 길이={result.Length}, 시작={result.Substring(0, Math.Min(10, result.Length))}");
            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"강제 암호화 에러: {e.Message}");
            // 암호화 실패 시 기본 Base64 인코딩이라도 수행
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
        }
    }

    // PlayerPrefs에서 데이터 로드하는 함수
    private void LoadFromLocalPlayerPrefs(GameObject gameManagerObject)
    {
        // GameManager 오브젝트에서 모든 컴포넌트 가져오기
        var allComponents = gameManagerObject.GetComponents<MonoBehaviour>();

        List<ISaveable> saveables = new List<ISaveable>();
        foreach (var component in allComponents)
        {
            if (component is ISaveable saveable)
            {
                saveables.Add(saveable);
            }
        }

        if (saveables.Count == 0)
        {
            return;
        }

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            Type componentType = mb.GetType();
            string typeName = componentType.FullName;

            // PlayerPrefs에서 직접 암호화된 원본 데이터 가져오기
            string rawData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(rawData))
            {
                // 복호화 진행
                string decryptedData = DecryptData(rawData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    // 로드된 데이터를 적용하는 함수
    private void ApplyLoadedGameState(GameObject gameManagerObject)
    {
        var saveables = gameManagerObject.GetComponents<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;

            // PlayerPrefs에서 직접 암호화된 원본 데이터 가져오기
            string rawData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(rawData))
            {
                // 복호화 진행
                string decryptedData = DecryptData(rawData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    // 모든 PlayerPrefs 데이터를 구글 클라우드에 저장하는 함수
    // 30초마다 자동저장, 수동 저장하기버튼, 게임 종료, 비정상적인 종료
    public void SaveToCloudWithLocalData()
    {
        if (!isLoggedIn || isSaving) return;

        isSaving = true;
        CompleteGameState gameState = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = saveable.GetSaveData();

            if (!string.IsNullOrEmpty(data))
            {
                gameState.AddComponentData(typeName, data);
            }
        }

        gameState.UpdateTimestamp();
        SaveLocalTimestamp(gameState.timestamp);

        string jsonData = JsonUtility.ToJson(gameState);
        SaveToCloud(EncryptData(jsonData));
        isSaving = false;
    }

    // 구글 클라우드에 데이터 저장하는 함수
    private void SaveToCloud(string jsonData)
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
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);
                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Last saved: {DateTime.Now}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, null);
                }
            });
    }

    // 구글 클라우드에서 데이터 로드하는 함수
    private void LoadFromCloud(Action<bool> onComplete)
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
                    saveGameClient.ReadBinaryData(game, (readStatus, data) =>
                    {
                        if (readStatus == SavedGameRequestStatus.Success && data != null && data.Length > 0)
                        {
                            string encryptedJsonData = Encoding.UTF8.GetString(data);
                            string decryptedJsonData = DecryptData(encryptedJsonData);
                            CompleteGameState loadedState = JsonUtility.FromJson<CompleteGameState>(decryptedJsonData);

                            foreach (var component in loadedState.components)
                            {
                                SaveToPlayerPrefs(component.path, component.data);
                            }

                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            onComplete?.Invoke(false);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // 여러 ISaveable 컴포넌트를 한 번에 저장하는 함수
    public void SaveAllSaveables(ISaveable[] saveables)
    {
        foreach (ISaveable saveable in saveables)
        {
            if (saveable == null) continue;

            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = saveable.GetSaveData();

            if (!string.IsNullOrEmpty(data))
            {
                SaveToPlayerPrefs(typeName, data);
            }
        }
    }

    // 즉시 클라우드 저장 실행 함수 (동기식)
    public void SaveGameStateSyncImmediate()
    {
        SaveToCloudWithLocalData();
    }

    #endregion


    #region Data Remove System

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
        isDeleting = true;
        Time.timeScale = 0f;

        // 먼저 현재 씬의 모든 컴포넌트 초기화
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        // 클라우드 및 로컬 데이터 삭제
        StartCoroutine(DeleteDataWithConfirmation());
    }

    // 데이터 삭제 확인 코루틴
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

        // 저장 완료 대기 후 게임 종료
        yield return new WaitForSecondsRealtime(1.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // 저장된 게임 데이터를 삭제하는 함수
    private void DeleteGameData(Action<bool> onComplete = null)
    {
        // PlayerPrefs 데이터 삭제
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 로그인 상태가 아니면 로컬 삭제만 하고 종료
        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
            return;
        }

        // 클라우드 데이터 삭제
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
                            //cachedData.Clear();
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

    // 앱을 나가면 자동 저장
    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    // 홈 버튼으로 나가면 자동 저장 (백그라운드로 전환)
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    // 다른 앱으로 전환시 자동 저장
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !CanSkipAutoSave())
        {
            SaveToCloudWithLocalData();
        }
    }

    #endregion


    #region Data Synchronization

    private const string LOCAL_TIMESTAMP_KEY = "LocalDataTimestamp";
    private CompleteGameState cachedLocalState;

    // 로컬 데이터의 타임스탬프 저장
    private void SaveLocalTimestamp(long timestamp)
    {
        PlayerPrefs.SetString(LOCAL_TIMESTAMP_KEY, timestamp.ToString());
        PlayerPrefs.Save();
    }

    // 로컬 데이터의 타임스탬프 로드
    private long LoadLocalTimestamp()
    {
        string timestampStr = PlayerPrefs.GetString(LOCAL_TIMESTAMP_KEY, "0");
        return long.Parse(timestampStr);
    }

    // 로컬 데이터를 CompleteGameState로 변환
    private CompleteGameState GetLocalGameState()
    {
        if (cachedLocalState != null)
            return cachedLocalState;

        CompleteGameState localState = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            string data = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(data))
            {
                localState.AddComponentData(typeName, data);
            }
        }

        localState.timestamp = LoadLocalTimestamp();
        cachedLocalState = localState;
        return localState;
    }

    // 클라우드 데이터를 로컬에 적용
    private void ApplyCloudToLocal(CompleteGameState cloudState)
    {
        foreach (var component in cloudState.components)
        {
            SaveToPlayerPrefs(component.path, component.data);
        }
        SaveLocalTimestamp(cloudState.timestamp);
        cachedLocalState = null; // 캐시 무효화
    }

    // 로컬 데이터를 클라우드에 적용
    private void ApplyLocalToCloud()
    {
        var localState = GetLocalGameState();
        localState.UpdateTimestamp();
        SaveLocalTimestamp(localState.timestamp);
        string jsonData = JsonUtility.ToJson(localState);
        SaveToCloud(EncryptData(jsonData));
        cachedLocalState = null; // 캐시 무효화
    }

    // 데이터 동기화 처리
    public void SynchronizeData(Action<bool> onComplete)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
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
                        if (readStatus == SavedGameRequestStatus.Success && data != null && data.Length > 0)
                        {
                            try
                            {
                                string encryptedJsonData = Encoding.UTF8.GetString(data);
                                string decryptedJsonData = DecryptData(encryptedJsonData);
                                CompleteGameState cloudState = JsonUtility.FromJson<CompleteGameState>(decryptedJsonData);
                                CompleteGameState localState = GetLocalGameState();

                                // 타임스탬프 비교 및 동기화
                                if (cloudState.timestamp > localState.timestamp)
                                {
                                    // 클라우드 데이터가 더 최신
                                    ApplyCloudToLocal(cloudState);
                                }
                                else if (cloudState.timestamp < localState.timestamp)
                                {
                                    // 로컬 데이터가 더 최신
                                    ApplyLocalToCloud();
                                }
                                onComplete?.Invoke(true);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"데이터 동기화 중 오류 발생: {e.Message}");
                                onComplete?.Invoke(false);
                            }
                        }
                        else
                        {
                            // 클라우드에 데이터가 없으면 로컬 데이터를 업로드
                            ApplyLocalToCloud();
                            onComplete?.Invoke(true);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    #endregion

}
