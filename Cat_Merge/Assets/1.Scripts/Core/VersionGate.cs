using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

// 원격 version.json 을 조회해서 강제 업데이트 여부를 판단하는 클래스
// 반드시 fail-open 으로 동작한다: 오프라인/타임아웃/404/JSON 오류/버전값 파싱 실패 등
// 어떤 이유로든 확인에 실패하면 무조건 게임을 그대로 진행시킨다.
//
// TitleScene의 "VersionGate" 오브젝트에 이 스크립트가 붙어있고, 아래 필드들은
// 그 하위에 미리 만들어둔 "VersionGateCanvas" 패널(Delete Yes/No Panel과 동일한
// 구조)을 그대로 연결해서 쓴다. 이 오브젝트 자체는 Active 상태여야 하고,
// VersionGateCanvas 패널은 평소엔 비활성화 상태여야 한다(둘 다 이 스크립트가 관리).
public class VersionGate : MonoBehaviour
{

    #region Variables

    [Header("---[Panel References]")]
    [SerializeField] private GameObject versionGateCanvas;   // 업데이트 안내 패널 전체 (평소엔 비활성화)
    [SerializeField] private TextMeshProUGUI explainText;    // 설명 텍스트 (Explain Text)
    [SerializeField] private Button quitButton;               // 종료하기 버튼
    [SerializeField] private Button updateButton;              // 업데이트 버튼

    // 원격 설정 파일 주소
    private const string VERSION_URL = "https://puddingnote.github.io/catmergegame/version.json";

    // 네트워크 요청 타임아웃 (초)
    private const int REQUEST_TIMEOUT_SECONDS = 5;

    // 원격 파일 조회에 실패했을 때 등, message 값이 비어있을 경우 대신 보여줄 기본 문구
    private const string DEFAULT_MESSAGE = "새로운 버전이 나왔습니다.\n업데이트 후 이용해 주세요.";

    // 차단 화면이 떠 있는 동안에만 true. 이 동안엔 뒤로가기를 종료로만 처리한다.
    private bool isBlocking = false;

    // 업데이트 버튼을 눌렀을 때 열어줄 스토어 주소 (버전 확인 완료 후 채워짐)
    private string pendingStoreUrl;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        if (versionGateCanvas != null)
        {
            versionGateCanvas.SetActive(false);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        if (updateButton != null)
        {
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
        }
    }

    private void Start()
    {
        StartCoroutine(CheckVersion());
    }

    private void Update()
    {
        // 차단 화면이 떠 있을 때만, 뒤로가기(안드로이드 Back = Escape)를 종료로 처리한다.
        // 차단 중이 아닐 때는 아무것도 하지 않아서 다른 뒤로가기 처리와 충돌하지 않는다.
        if (isBlocking && Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
    }

    #endregion


    #region Version Check

    // 원격 version.json 을 조회해서 강제 업데이트 여부를 확인하는 코루틴
    private IEnumerator CheckVersion()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(VERSION_URL))
        {
            request.timeout = REQUEST_TIMEOUT_SECONDS;

            yield return request.SendWebRequest();

            // 네트워크 오류, 타임아웃, 404 등 -> 무조건 통과
            if (request.result != UnityWebRequest.Result.Success)
            {
                //Debug.Log($"[VersionGate] 버전 확인 실패, 통과: {request.error}");
                yield break;
            }

            VersionGateData data = null;
            try
            {
                data = JsonUtility.FromJson<VersionGateData>(request.downloadHandler.text);
            }
            catch (Exception)
            {
                // JSON 문법 오류 -> 무조건 통과
                data = null;
            }

            if (data == null || string.IsNullOrWhiteSpace(data.minVersion))
            {
                // minVersion 이 비어있거나 파싱 안 됨 -> 무조건 통과
                yield break;
            }

            // AppVersion.IsAtLeast 자체도 파싱 실패시 항상 true(통과)를 반환한다
            bool isVersionOk = AppVersion.IsAtLeast(Application.version, data.minVersion);
            if (!isVersionOk)
            {
                ShowUpdateRequiredView(data);
            }
        }
    }

    #endregion


    #region Blocking UI

    // 씬에 미리 만들어둔 VersionGateCanvas 패널을 채우고 띄우는 함수
    private void ShowUpdateRequiredView(VersionGateData data)
    {
        isBlocking = true;
        pendingStoreUrl = data.storeUrl;

        if (explainText != null)
        {
            explainText.text = string.IsNullOrWhiteSpace(data.message) ? DEFAULT_MESSAGE : data.message;
        }

        if (versionGateCanvas != null)
        {
            versionGateCanvas.SetActive(true);
        }
    }

    // [업데이트] 버튼 클릭 함수 - 스토어로 이동. 눌러도 이 창은 닫지 않는다(업데이트 전까지 계속 막혀 있어야 함)
    private void OnUpdateButtonClicked()
    {
        if (!string.IsNullOrWhiteSpace(pendingStoreUrl))
        {
            Application.OpenURL(pendingStoreUrl);
        }
    }

    // 게임 종료 함수 (에디터에서는 플레이 모드만 정지)
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

}

// version.json 파싱용 데이터 클래스
[Serializable]
public class VersionGateData
{
    public string minVersion;
    public string storeUrl;
    public string message;
}
