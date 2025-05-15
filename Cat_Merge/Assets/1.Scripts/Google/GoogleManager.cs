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
    public long timestamp;

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

// 구글 로그인 & 저장 및 로드 관련 스크립트
public class GoogleManager : MonoBehaviour
{


    #region Variables

    public static GoogleManager Instance { get; private set; }

    private Button deleteDataButton;                                // 게임 데이터 삭제 버튼 (나중에 삭제 예정)
    private const string SAVE_FILE_NAME = "GoogleCloudSaveState";   // 파일 이름
    private const string GAME_SCENE = "GameScene-Han";              // GameScene 이름
    private const float AUTO_SAVE_INTERVAL = 30f;                   // 자동 저장 간격
    private float autoSaveTimer = 0f;                               // 자동 저장 타이머

    [HideInInspector] public bool isLoggedIn = false;               // 구글 로그인 여부
    [HideInInspector] public bool isDeleting = false;               // 현재 데이터 삭제중 여부
    [HideInInspector] public bool isPlayedGame = false;             // 첫 게임 실행 여부
    private bool isSaving = false;                                  // 저장 중인지 확인하는 플래그
    private bool isGameStarting = false;                            // 게임 시작 중인지 확인하는 플래그

    private Vector2 gameStartPosition;                              // 게임 시작 터치 위치를 저장할 변수

    private const string encryptionKey = "CatMergeGame_EncryptionKey";  // 암호화 키
    private const string FIRST_PLAY_KEY = "IsFirstPlay";                // 첫 실행 체크용 키


    private const string SAVE_VERSION_KEY = "SaveVersion";          // 저장 버전 키
    private const int CURRENT_SAVE_VERSION = 2;                     // 현재 저장 버전 (1: 이전 버전, 2: 새로운 버전)

    private const int MAX_BACKUP_COUNT = 3;                         // 최대 백업 파일 수
    private const int MAX_SAVE_SIZE_MB = 3;                         // 최대 저장 용량 (MB)
    private const string BACKUP_FILE_PREFIX = "Backup_";            // 백업 파일 접두사
    private const string NETWORK_STATUS_KEY = "NetworkStatus";      // 네트워크 상태 키

    private bool isNetworkAvailable = false;                        // 네트워크 사용 가능 여부
    private Queue<CompleteGameState> backupStates;                  // 백업 상태 큐
    private DateTime lastNetworkCheck = DateTime.MinValue;          // 마지막 네트워크 체크 시간
    private const float NETWORK_CHECK_INTERVAL = 30f;               // 네트워크 체크 간격

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
        StartCoroutine(InitializeGoogleLogin());

        // 백업 시스템 초기화
        backupStates = new Queue<CompleteGameState>();

        // 네트워크 상태 모니터링 시작
        StartCoroutine(MonitorNetworkStatus());

        // 저장된 네트워크 상태 복원
        isNetworkAvailable = PlayerPrefs.GetInt(NETWORK_STATUS_KEY, 1) == 1;
    }

    private void Update()
    {
        if (CanSkipAutoSave())
        {
            return;
        }

        autoSaveTimer += Time.deltaTime;
        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            //Debug.Log($"[저장] {AUTO_SAVE_INTERVAL}초 경과로 자동 저장 시작");
            SaveAllGameData();
            autoSaveTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    #endregion


    #region Initialize

    // 구글 플레이 서비스를 초기화하는 함수
    private void InitializeGooglePlay()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        CheckAndMigrateData();
    }

    // 저장 버전 확인 및 마이그레이션 함수
    private void CheckAndMigrateData()
    {
        // PlayerPrefs에 저장된 데이터가 있는지 먼저 확인
        bool hasExistingData = false;
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (ISaveable saveable in saveables)
        {
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string typeName = mb.GetType().FullName;
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(typeName, "")))
            {
                hasExistingData = true;
                break;
            }
        }

        // 실제 데이터가 있을 때만 마이그레이션 검사
        if (hasExistingData)
        {
            int savedVersion = PlayerPrefs.GetInt(SAVE_VERSION_KEY, 1);
            if (savedVersion < CURRENT_SAVE_VERSION)
            {
                //Debug.Log($"[마이그레이션] 이전 버전({savedVersion})의 데이터 발견. 마이그레이션 시작");
                MigrateOldData();
                PlayerPrefs.SetInt(SAVE_VERSION_KEY, CURRENT_SAVE_VERSION);
                PlayerPrefs.Save();
                //Debug.Log("[마이그레이션] 데이터 마이그레이션 완료");
            }
        }
        else
        {
            // 새로운 시작이므로 현재 버전으로 설정
            PlayerPrefs.SetInt(SAVE_VERSION_KEY, CURRENT_SAVE_VERSION);
            PlayerPrefs.Save();
        }
    }

    private void MigrateOldData()
    {
        try
        {
            // 1. 기존 데이터 백업
            Dictionary<string, string> oldData = new Dictionary<string, string>();
            var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

            foreach (ISaveable saveable in saveables)
            {
                MonoBehaviour mb = (MonoBehaviour)saveable;
                string typeName = mb.GetType().FullName;
                string data = PlayerPrefs.GetString(typeName, "");

                if (!string.IsNullOrEmpty(data))
                {
                    oldData[typeName] = data;
                    //Debug.Log($"[마이그레이션] {typeName} 데이터 백업");
                }
            }

            // 2. 새로운 형식으로 변환
            if (oldData.Count > 0)
            {
                CompleteGameState newGameState = new CompleteGameState();

                foreach (var kvp in oldData)
                {
                    string encryptedData = IsValidBase64(kvp.Value) ? kvp.Value : EncryptData(kvp.Value);
                    newGameState.AddComponentData(kvp.Key, encryptedData);
                }

                newGameState.UpdateTimestamp();

                // 3. 새로운 형식으로 저장
                if (isLoggedIn)
                {
                    SaveToCloud(JsonUtility.ToJson(newGameState));
                    //Debug.Log("[마이그레이션] 클라우드에 새로운 형식으로 저장");
                }
                else
                {
                    SaveToPlayerPrefs(newGameState);
                    //Debug.Log("[마이그레이션] PlayerPrefs에 새로운 형식으로 저장");
                }
            }
        }
        catch (Exception e)
        {
            //Debug.LogError($"[마이그레이션] 데이터 마이그레이션 중 오류 발생: {e.Message}");
        }
    }

    #endregion


    #region Scene Management

    // 씬 로드 시 필요한 설정을 하는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndSetupDeleteButton();
        LoadingScreen.Instance?.UpdateLoadingScreenCamera();
    }

    #endregion


    #region Google Login & Game Start Setting

    // 구글 로그인을 시도하는 코루틴
    private IEnumerator InitializeGoogleLogin()
    {
        //Debug.Log("[초기화] 구글 로그인 시도");
        bool loginComplete = false;
        PlayGamesPlatform.Instance.Authenticate((status) => {
            ProcessAuthentication(status);
            loginComplete = true;
            //Debug.Log($"[초기화] 구글 로그인 결과: {status}");
        });

        float waitTime = 0;
        while (!loginComplete && waitTime < 5f)
        {
            waitTime += Time.deltaTime;
            yield return null;
        }

        //// 초기 isPlayedGame 상태를 로컬 저장소 기준으로 설정
        //isPlayedGame = PlayerPrefs.HasKey(FIRST_PLAY_KEY);
        //Debug.Log($"[초기화] 첫 실행 상태 확인: {!isPlayedGame}");
    }

    // 구글 인증 결과를 처리하는 함수
    internal void ProcessAuthentication(SignInStatus status)
    {
        isLoggedIn = (status == SignInStatus.Success);
    }

    // 게임 시작 버튼 클릭 시 호출되는 함수 (버튼에 연결)
    public void OnGameStartButtonClick()
    {
        if (isGameStarting) return;

        TitleManager titleManager = FindObjectOfType<TitleManager>();
        titleManager?.OnGameStart();

        Vector2 touchPosition = Input.mousePosition;
        StartCoroutine(StartGameWithLoad(touchPosition));
    }

    // 게임 시작 시 데이터를 로드하는 코루틴
    private IEnumerator StartGameWithLoad(Vector2 touchPosition)
    {
        isGameStarting = true;
        gameStartPosition = touchPosition;

        // 로딩 화면 표시
        LoadingScreen.Instance.Show(true, touchPosition);
        yield return new WaitForSecondsRealtime(LoadingScreen.Instance.animationDuration);

        // 씬 전환
        SceneManager.LoadScene(GAME_SCENE);
        yield return new WaitForEndOfFrame();

        // GameManager가 초기화될 때까지 대기
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
            //Debug.LogError("[로드] GameManager를 찾을 수 없음");
            yield break;
        }

        // 모든 컴포넌트가 초기화될 때까지 추가 대기
        yield return new WaitForSecondsRealtime(0.5f);

        UnencryptedData();

        //Debug.Log("[로드] 게임 데이터 로드 시작");

        CompleteGameState cloudState = null;
        CompleteGameState localState = null;

        // 로컬 데이터 먼저 로드
        if (PlayerPrefs.HasKey(FIRST_PLAY_KEY))
        {
            localState = LoadLocalStateFromPlayerPrefs();
            //Debug.Log($"[로드] 로컬 데이터 타임스탬프: {localState?.timestamp}");
        }

        // 구글 로그인 상태면 클라우드 데이터 로드 시도
        if (isLoggedIn)
        {
            //Debug.Log("[로드] Google Cloud 데이터 로드 시도");
            bool loadComplete = false;

            LoadFromCloud((success, state) => {
                if (success)
                {
                    cloudState = state;
                    //Debug.Log($"[로드] 클라우드 데이터 타임스탬프: {cloudState?.timestamp}");
                }
                loadComplete = true;
            });

            float waitTime = 0;
            while (!loadComplete && waitTime < 5f)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
        }

        // 데이터 동기화 결정
        CompleteGameState finalState = null;

        if (cloudState != null && localState != null)
        {
            // 둘 다 있는 경우 - 타임스탬프 비교
            if (cloudState.timestamp >= localState.timestamp)
            {
                //Debug.Log("[로드] 클라우드 데이터가 더 최신이므로 클라우드 데이터 사용");
                finalState = cloudState;
            }
            else
            {
                //Debug.Log("[로드] 로컬 데이터가 더 최신이므로 로컬 데이터 사용");
                finalState = localState;
                // 더 최신 데이터를 클라우드에 동기화
                if (isLoggedIn && isNetworkAvailable)
                {
                    SaveToCloud(JsonUtility.ToJson(localState));
                }
            }
        }
        else
        {
            // 둘 중 하나만 있는 경우
            finalState = cloudState ?? localState;
            //Debug.Log($"[로드] 사용 가능한 데이터 사용: {(cloudState != null ? "클라우드" : "로컬")}");
        }

        // 최종 선택된 데이터 적용
        if (finalState != null)
        {
            ApplyGameState(finalState);

            // 유효한 데이터가 로드되었으므로 첫 게임이 아님을 표시
            isPlayedGame = true;
            if (!PlayerPrefs.HasKey(FIRST_PLAY_KEY))
            {
                PlayerPrefs.SetInt(FIRST_PLAY_KEY, 1);
                PlayerPrefs.Save();
            }
            
            //Debug.Log($"[로드] 최종 데이터 적용 완료 (타임스탬프: {finalState.timestamp})");
        }
        else
        {
            isPlayedGame = false;
        }

        // 잠시 대기
        yield return new WaitForSecondsRealtime(0.5f);
        LoadingScreen.Instance.Show(false, gameStartPosition);

        // 첫 게임 여부 확인 및 처리
        if (!isPlayedGame)
        {
            //Debug.Log("[초기화] 첫 게임 실행 - 튜토리얼 시작");
            StartCoroutine(gameManager.ShowFirstGamePanel());
        }

        isGameStarting = false;
        //Debug.Log("[로드] 게임 데이터 로드 완료");
    }

    // PlayerPrefs에서 전체 게임 상태를 로드하는 함수
    private CompleteGameState LoadLocalStateFromPlayerPrefs()
    {
        CompleteGameState state = new CompleteGameState();
        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            if (saveable == null) continue;

            string typeName = saveable.GetType().FullName;
            string encryptedData = PlayerPrefs.GetString(typeName, "");

            if (!string.IsNullOrEmpty(encryptedData))
            {
                state.AddComponentData(typeName, encryptedData);
            }
        }

        string timestampStr = PlayerPrefs.GetString("LastSaveTime", "0");
        state.timestamp = long.Parse(timestampStr);

        return state.components.Count > 0 ? state : null;
    }

    // 게임 상태를 적용하는 함수
    private void ApplyGameState(CompleteGameState state)
    {
        if (state == null) return;

        var saveables = Resources.FindObjectsOfTypeAll<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            string typeName = saveable.GetType().FullName;
            if (state.TryGetValue(typeName, out string encryptedData))
            {
                string decryptedData = DecryptData(encryptedData);
                saveable.LoadFromData(decryptedData);
            }
        }
    }

    #endregion


    #region Save System

    // 강제로 게임 데이터를 저장하는 함수
    public void ForceSaveAllData()
    {
        if (isDeleting) return;
        //Debug.Log("[저장] 강제 저장 요청됨");
        SaveAllGameData();
        autoSaveTimer = 0f;
    }

    // 전체 게임 데이터를 저장하는 함수
    private void SaveAllGameData()
    {
        if (isSaving)
        {
            //Debug.Log("[저장] 이미 저장 중이므로 스킵");
            return;
        }
        isSaving = true;

        try
        {
            CompleteGameState gameState = new CompleteGameState();
            var saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            int componentCount = 0;

            foreach (var saveable in saveables)
            {
                if (saveable == null) continue;

                string typeName = saveable.GetType().FullName;
                string data = saveable.GetSaveData();

                if (!string.IsNullOrEmpty(data))
                {
                    string encryptedData = EncryptData(data);
                    gameState.AddComponentData(typeName, encryptedData);
                    componentCount++;
                }
            }

            gameState.UpdateTimestamp();

            // 데이터 무결성 검증
            if (!ValidateGameState(gameState))
            {
                //Debug.LogError("[저장] 데이터 무결성 검증 실패");
                if (!RestoreFromBackup())
                {
                    //Debug.LogError("[저장] 백업 복구 실패");
                }
                return;
            }

            // 백업 생성
            CreateBackup(gameState);

            // 클라우드 저장
            if (isLoggedIn && isNetworkAvailable)
            {
                //Debug.Log($"[저장] Google Cloud 저장 시작 - 컴포넌트 {componentCount}개");
                SaveToCloud(JsonUtility.ToJson(gameState));
            }
            else
            {
                //Debug.Log($"[저장] PlayerPrefs 저장 시작 - 컴포넌트 {componentCount}개");
                SaveToPlayerPrefs(gameState);
            }

            // 오래된 백업 정리
            CleanupOldBackups();
        }
        catch (Exception e)
        {
            //Debug.LogError($"[저장] 저장 중 오류 발생: {e.Message}");
            if (!RestoreFromBackup())
            {
                //Debug.LogError("[저장] 백업 복구 실패");
            }
        }
        finally
        {
            isSaving = false;
        }

        //Debug.Log("[저장] 저장 완료");
    }

    // PlayerPrefs에 게임 데이터를 저장하는 함수
    private void SaveToPlayerPrefs(CompleteGameState gameState)
    {
        try
        {
            foreach (var component in gameState.components)
            {
                PlayerPrefs.SetString(component.path, component.data);
            }
            PlayerPrefs.SetString("LastSaveTime", gameState.timestamp.ToString());
            PlayerPrefs.Save();
            //Debug.Log("[저장] PlayerPrefs 저장 성공");
        }
        catch (Exception e)
        {
            //Debug.LogError($"[저장] PlayerPrefs 저장 실패: {e.Message}");
        }
    }

    // 구글 클라우드에 게임 데이터를 저장하는 함수
    private void SaveToCloud(string jsonData)
    {
        if (!isLoggedIn) return;

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
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

                    saveGameClient.CommitUpdate(game, update, data, (commitStatus, savedGame) =>
                    {
                        //Debug.Log($"[저장] Google Cloud 저장 결과: {commitStatus}");
                    });
                }
                else
                {
                    //Debug.LogError($"[저장] Google Cloud 저장 실패: {status}");
                }
            });
    }

    #endregion


    #region Load System

    // 구글 클라우드에서 게임 데이터를 로드하는 함수
    private void LoadFromCloud(Action<bool, CompleteGameState> onComplete)
    {
        if (!isLoggedIn)
        {
            onComplete?.Invoke(false, null);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
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
                                string jsonData = Encoding.UTF8.GetString(data);
                                CompleteGameState loadedState = JsonUtility.FromJson<CompleteGameState>(jsonData);

                                if (ValidateGameState(loadedState))
                                {
                                    onComplete?.Invoke(true, loadedState);
                                    return;
                                }
                            }
                            catch (Exception e)
                            {
                                //Debug.LogError($"[로드] 클라우드 데이터 파싱 실패: {e.Message}");
                            }
                        }
                        onComplete?.Invoke(false, null);
                    });
                }
                else
                {
                    onComplete?.Invoke(false, null);
                }
            });
    }

    #endregion


    #region Encryption

    // 데이터를 암호화하는 함수
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

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception e)
        {
            //Debug.LogError($"[암호화 오류] {e.Message}");
            return data;
        }
    }

    // 데이터를 복호화하는 함수
    private string DecryptData(string encryptedData)
    {
        if (string.IsNullOrEmpty(encryptedData)) return encryptedData;

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
            byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] decryptedBytes = new byte[encryptedBytes.Length];

            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(encryptedBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception e)
        {
            //Debug.LogError($"[복호화 오류] {e.Message}");
            return encryptedData;
        }
    }

    // 암호화되지 않은 데이터를 암호화하는 함수
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
                SaveToPlayerPrefs(new CompleteGameState { components = new List<ComponentData> { new ComponentData { path = typeName, data = EncryptData(data) } } });
            }
        }
    }

    // Base64 형식이 유효한지 확인하는 함수
    private bool IsValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0)
            return false;

        foreach (char c in base64String)
        {
            if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z') && (c < '0' || c > '9') && c != '+' && c != '/' && c != '=')
                return false;
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

    #endregion


    #region Application Lifecycle

    // 앱이 일시정지될 때 호출되는 함수
    private void OnApplicationPause(bool pause)
    {
        if (pause && !CanSkipAutoSave())
        {
            SaveAllGameData();
        }
    }

    // 앱이 종료될 때 호출되는 함수
    private void OnApplicationQuit()
    {
        if (!CanSkipAutoSave())
        {
            SaveAllGameData();
        }
    }

    // 자동 저장을 건너뛸 수 있는지 확인하는 함수
    private bool CanSkipAutoSave()
    {
        return isDeleting ||
            (GameManager.Instance != null && GameManager.Instance.isQuiting) ||
            (BattleManager.Instance != null && BattleManager.Instance.IsBattleActive);
    }

    #endregion


    #region Delete System

    // 삭제 버튼을 찾고 설정하는 함수
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

    // 게임 데이터를 삭제하고 게임을 종료하는 함수
    public void DeleteGameDataAndQuit()
    {
        isDeleting = true;
        Time.timeScale = 0f;

        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>().ToArray();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadFromData(null);
        }

        StartCoroutine(DeleteDataWithConfirmation());
    }

    // 데이터 삭제를 확인하고 처리하는 코루틴
    private IEnumerator DeleteDataWithConfirmation()
    {
        bool deleteCompleted = false;
        bool deleteSuccess = false;

        DeleteGameData((success) => {
            deleteCompleted = true;
            deleteSuccess = success;
        });

        float waitTime = 0;
        while (!deleteCompleted && waitTime < 3.0f)
        {
            waitTime += 0.1f;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(1.0f);
        StartCoroutine(QuitGameAfterDelay());
    }

    // 게임 데이터를 삭제하는 함수
    private void DeleteGameData(Action<bool> onComplete = null)
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        if (!isLoggedIn)
        {
            onComplete?.Invoke(true);
            return;
        }

        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;
        saveGameClient.OpenWithAutomaticConflictResolution(
            SAVE_FILE_NAME,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    CompleteGameState emptyState = new CompleteGameState();
                    string emptyJson = JsonUtility.ToJson(emptyState);
                    byte[] emptyData = Encoding.UTF8.GetBytes(emptyJson);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription($"Data deleted: {DateTime.Now}")
                        .Build();

                    saveGameClient.CommitUpdate(game, update, emptyData, (saveStatus, savedGame) =>
                    {
                        onComplete?.Invoke(saveStatus == SavedGameRequestStatus.Success);
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
    }

    // 지연 후 게임을 종료하는 코루틴
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


    #region Network Monitoring

    // 네트워크 상태를 모니터링하는 코루틴
    private IEnumerator MonitorNetworkStatus()
    {
        while (true)
        {
            if ((DateTime.Now - lastNetworkCheck).TotalSeconds >= NETWORK_CHECK_INTERVAL)
            {
                lastNetworkCheck = DateTime.Now;
                bool previousState = isNetworkAvailable;
                isNetworkAvailable = Application.internetReachability != NetworkReachability.NotReachable;

                if (previousState != isNetworkAvailable)
                {
                    PlayerPrefs.SetInt(NETWORK_STATUS_KEY, isNetworkAvailable ? 1 : 0);
                    PlayerPrefs.Save();

                    if (isNetworkAvailable)
                    {
                        StartCoroutine(SyncDataWithCloud());
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // 클라우드와 데이터 동기화하는 함수
    private IEnumerator SyncDataWithCloud()
    {
        if (!isLoggedIn || !isNetworkAvailable) yield break;

        //Debug.Log("[동기화] 클라우드 동기화 시작");
        bool syncComplete = false;

        LoadFromCloud((success, state) => {
            if (success)
            {
                SaveAllGameData();
            }
            syncComplete = true;
        });

        while (!syncComplete)
        {
            yield return null;
        }

        //Debug.Log("[동기화] 클라우드 동기화 완료");
    }

    #endregion


    #region Data Integrity and Backup

    // 데이터 무결성 검증 함수
    private bool ValidateGameState(CompleteGameState state)
    {
        if (state == null) return false;

        // 타임스탬프 검증
        if (state.timestamp <= 0) return false;

        // 기본 데이터 존재 여부 검증
        if (state.components == null || state.components.Count == 0) return false;

        // 데이터 크기 검증
        string jsonData = JsonUtility.ToJson(state);
        float dataSizeMB = Encoding.UTF8.GetByteCount(jsonData) / (1024f * 1024f);
        if (dataSizeMB > MAX_SAVE_SIZE_MB) return false;

        return true;
    }

    // 백업 생성 함수
    private void CreateBackup(CompleteGameState state)
    {
        if (!ValidateGameState(state)) return;

        backupStates.Enqueue(state);
        while (backupStates.Count > MAX_BACKUP_COUNT)
        {
            backupStates.Dequeue();
        }

        // 로컬에도 백업 저장
        string backupJson = JsonUtility.ToJson(state);
        string backupKey = $"{BACKUP_FILE_PREFIX}{state.timestamp}";
        PlayerPrefs.SetString(backupKey, backupJson);
        PlayerPrefs.Save();
    }

    // 백업에서 복구하는 함수
    private bool RestoreFromBackup()
    {
        if (backupStates.Count == 0) return false;

        CompleteGameState backupState = backupStates.Last();
        if (!ValidateGameState(backupState)) return false;

        //Debug.Log("[복구] 백업에서 데이터 복구 시작");

        try
        {
            var saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            foreach (var saveable in saveables)
            {
                string typeName = saveable.GetType().FullName;
                if (backupState.TryGetValue(typeName, out string encryptedData))
                {
                    string decryptedData = DecryptData(encryptedData);
                    saveable.LoadFromData(decryptedData);
                }
            }

            //Debug.Log("[복구] 백업에서 데이터 복구 완료");
            return true;
        }
        catch (Exception e)
        {
            //Debug.LogError($"[복구] 백업 복구 실패: {e.Message}");
            return false;
        }
    }

    // 오래된 백업 정리하는 함수
    private void CleanupOldBackups()
    {
        var keys = new List<string>();
        var timestamps = new List<long>();

        // PlayerPrefs에서 백업 키 찾기
        foreach (string key in PlayerPrefs.GetString("").Split('\0'))
        {
            if (key.StartsWith(BACKUP_FILE_PREFIX))
            {
                keys.Add(key);
                long timestamp = long.Parse(key.Substring(BACKUP_FILE_PREFIX.Length));
                timestamps.Add(timestamp);
            }
        }

        // 오래된 백업 삭제
        if (keys.Count > MAX_BACKUP_COUNT)
        {
            var sortedIndices = timestamps
                .Select((t, i) => new { Timestamp = t, Index = i })
                .OrderByDescending(x => x.Timestamp)
                .Skip(MAX_BACKUP_COUNT)
                .Select(x => x.Index);

            foreach (int index in sortedIndices)
            {
                PlayerPrefs.DeleteKey(keys[index]);
            }
            PlayerPrefs.Save();
        }
    }

    #endregion


}
