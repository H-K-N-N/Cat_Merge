using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;
using System.Text;
using System;
using System.Linq;

// 저장/로드가 필요한 컴포넌트에 적용할 인터페이스
public interface ISaveable
{
    string GetSaveData();
    void LoadFromData(string data);
}

[System.Serializable]
public class CompleteGameState
{
    public Dictionary<string, string> componentData = new Dictionary<string, string>();
}

public class GoogleManager : MonoBehaviour
{
    public static GoogleManager Instance { get; private set; }

    private TextMeshProUGUI logText;                        // 임시 로그인 로그 텍스트 (나중에는 삭제할 예정)

    private const string fileName = "GameCompleteState";    // 저장할 파일명
    private bool isLoggedIn = false;                        // 로그인 여부 확인

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
    }

    public void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        UpdateLogText();  // logText 찾기
        GPGS_LogIn();
    }

    // 임시 logText 찾아서 설정하는 함수
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
            if (logText != null)
            {
                logText.text = "로그인 실패";
            }
            isLoggedIn = false;
        }
    }

    // ======================================================================================================================

    // 전체 게임 상태 저장
    public void SaveGameState()
    {
        if (!isLoggedIn) return;

        CompleteGameState gameState = new CompleteGameState();

        // Scene에서 ISaveable 인터페이스를 구현한 모든 컴포넌트 찾기
        ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

        foreach (ISaveable saveable in saveables)
        {
            // 각 컴포넌트의 고유 식별자로 MonoBehaviour의 전체 경로 사용
            MonoBehaviour mb = (MonoBehaviour)saveable;
            string path = GetGameObjectPath(mb.gameObject);
            gameState.componentData[path] = saveable.GetSaveData();
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
                        .WithUpdatedDescription("Last saved: " + DateTime.Now.ToString())
                        .Build();

                    saveGameClient.CommitUpdate(game, update, data, (saveStatus, savedGame) =>
                    {
                        Debug.Log(saveStatus == SavedGameRequestStatus.Success ? "게임 상태 저장 성공" : "게임 상태 저장 실패");
                    });
                }
                else
                {
                    Debug.Log("파일을 열 수 없음 (저장 실패)");
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
                            CompleteGameState gameState = JsonUtility.FromJson<CompleteGameState>(jsonData);

                            if (gameState != null)
                            {
                                // Scene의 모든 ISaveable 컴포넌트 찾기
                                ISaveable[] saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>().ToArray();

                                foreach (ISaveable saveable in saveables)
                                {
                                    MonoBehaviour mb = (MonoBehaviour)saveable;
                                    string path = GetGameObjectPath(mb.gameObject);

                                    if (gameState.componentData.TryGetValue(path, out string componentData))
                                    {
                                        saveable.LoadFromData(componentData);
                                    }
                                }
                                Debug.Log("게임 상태 로드 성공");
                            }
                        }
                        else
                        {
                            Debug.Log("데이터 읽기 실패");
                        }
                    });
                }
                else
                {
                    Debug.Log("파일을 열 수 없음 (로드 실패)");
                }
            });
    }

    // GameObject의 전체 경로를 얻는 헬퍼 함수
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }

    // 게임 종료 시 자동 저장
    private void OnApplicationQuit()
    {
        SaveGameState();
    }

    // 앱이 백그라운드로 가면 자동 저장
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveGameState();
    }

    // ======================================================================================================================


}