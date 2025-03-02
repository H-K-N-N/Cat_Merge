using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using TMPro;

public class GoogleManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI logText;

    private void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        GPGS_LogIn();
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
            string displayName = PlayGamesPlatform.Instance.GetUserDisplayName();
            string userID = PlayGamesPlatform.Instance.GetUserId();

            logText.text = "로그인 성공 : " + displayName + " / " + userID;
            //Debug.Log("Success");
        }
        else
        {
            logText.text = "로그인 실패";
            //Debug.Log("Fail");
            //GameManager.instance.DataSave_Scr.LoadUserDataFun();
        }
    }

    /////////////////////////////////////////////////////////////////////////////

    // 데이터 저장
    /*
    public void SaveData() // 외부에서 접근할 함수
    {
        OpenSaveGame();
    }

    private void OpenSaveGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        // 데이터 접근
        saveGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            onsavedGameOpend);
    }


    private void onsavedGameOpend(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            var update = new SavedGameMetadataUpdate.Builder().Build();

            //json
            var json = JsonUtility.ToJson("저장하려는 데이터!");
            byte[] data = Encoding.UTF8.GetBytes(json);

            // 저장 함수 실행
            saveGameClient.CommitUpdate(game, update, data, OnSavedGameWritten);
        }
        else
        {
            Debug.Log("Save No.....");
        }
    }

    // 저장 확인 
    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // 저장완료부분
            Debug.Log("Save End");
        }
        else
        {
            Debug.Log("Save nonononononono...");
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////

    // 데이터 로드
    /*
    public void LoadData()
    {
        OpenLoadGame();
    }

    private void OpenLoadGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        saveGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            LoadGameData);
    }

    private void LoadGameData(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("!! GoodLee");

            // 데이터 로드
            saveGameClient.ReadBinaryData(data, onSavedGameDataRead);
        }
        else
        {
            Debug.Log("?? no");
        }
    }

    // 불러온 데이터 처리 
    private void onSavedGameDataRead(SavedGameRequestStatus status, byte[] loadedData)
    {
        string data = System.Text.Encoding.UTF8.GetString(loadedData);

        if (data == "")
        {
            SaveData();
        }
        else
        {
            // 불러온 데이터를 따로 처리해주는 부분 필요!
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////

    // 데이터 삭제
    /*
    public void DeleteData()
    {
        DeleteGameData();
    }

    private void DeleteGameData()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        savedGameClient.OpenWithAutomaticConflictResolution(fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            DeleteSaveGame);
    }


    private void DeleteSaveGame(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status == SavedGameRequestStatus.Success)
        {
            savedGameClient.Delete(data);

        }
        else
        {
        }
    }
    */

    /////////////////////////////////////////////////////////////////////////////


}