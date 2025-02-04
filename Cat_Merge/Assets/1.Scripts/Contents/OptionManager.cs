using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// OptionManager Script
public class OptionManager : MonoBehaviour
{
    // Singleton Instance
    public static OptionManager Instance { get; private set; }

    // 사운드 컨트롤 내부 클래스
    public class SoundController
    {
        private AudioSource audioSource;
        private Slider volumeSlider;
        private AudioClip soundClip;

        public SoundController(GameObject parent, Slider slider, bool loop, AudioClip clip)
        {
            audioSource = parent.AddComponent<AudioSource>();
            audioSource.loop = loop;
            volumeSlider = slider;
            soundClip = clip;

            audioSource.clip = soundClip;

            if (volumeSlider != null)
            {
                volumeSlider.value = 0.1f;
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }
        }

        // 볼륨 설정 함수
        public void SetVolume(float volume)
        {
            bool isBgm = (audioSource == Instance.bgmController.audioSource);

            if ((isBgm && !Instance.isBgmOn) || (!isBgm && !Instance.isSfxOn))
            {
                audioSource.volume = 0f;
            }
            else
            {
                audioSource.volume = volume;
            }

            Instance.SetSoundToggleImage(isBgm);
        }

        // 사운드 재생
        public void Play()
        {
            audioSource.Play();
        }

        // 사운드 정지
        public void Stop()
        {
            audioSource.Stop();
        }
    }

    // ======================================================================================================================

    [Header("---[OptionManager]")]
    [SerializeField] private Button optionButton;               // 옵션 버튼
    [SerializeField] private Image optionButtonImage;           // 옵션 버튼 이미지
    [SerializeField] private GameObject optionMenuPanel;        // 옵션 메뉴 Panel
    [SerializeField] private Button optionBackButton;           // 옵션 뒤로가기 버튼
    private ActivePanelManager activePanelManager;              // ActivePanelManager

    [SerializeField] private GameObject[] mainOptionMenus;      // 메인 옵션 메뉴 Panels
    [SerializeField] private Button[] subOptionMenuButtons;     // 서브 옵션 메뉴 버튼 배열

    // ======================================================================================================================

    [Header("---[Sub Menu UI Color]")]
    private string activeColorCode = "#5f5f5f";                 // 활성화상태 Color
    private string inactiveColorCode = "#FFFFFF";               // 비활성화상태 Color

    // ======================================================================================================================
    // [All]

    private float onX = 65f, offX = -65f;                       // 핸들 버튼 x좌표
    private float moveDuration = 0.2f;                          // 토글 애니메이션 지속 시간

    // ======================================================================================================================
    // [Sound]

    [Header("---[BGM]")]
    [SerializeField] private Slider bgmSlider;                  // 배경음 사운드 조절 슬라이더
    [SerializeField] private Button bgmSoundToggleButton;       // 배경음 On/Off 버튼
    [SerializeField] private RectTransform bgmSoundHandle;      // 배경음 토글 핸들
    [SerializeField] private Image bgmOnOffImage;               // 배경음 On/Off 이미지
    private SoundController bgmController;                      // 배경음 컨트롤러
    private Coroutine bgmToggleCoroutine;                       // 배경음 토글 애니메이션 코루틴
    private bool isBgmOn = true;                                // 배경음 활성화 여부

    [Header("---[SFX]")]
    [SerializeField] private Slider sfxSlider;                  // 효과음 볼륨 조절 슬라이더
    [SerializeField] private Button sfxSoundToggleButton;       // 효과음 On/Off 버튼
    [SerializeField] private RectTransform sfxSoundHandle;      // 효과음 토글 핸들
    [SerializeField] private Image sfxOnOffImage;               // 효과음 On/Off 이미지
    private SoundController sfxController;                      // 효과음 컨트롤러
    private Coroutine sfxToggleCoroutine;                       // 효과음 토글 애니메이션 코루틴
    private bool isSfxOn = true;                                // 효과음 활성화 여부

    [Header("---[Common]")]
    private Sprite soundOnImage;                                // 사운드 On 이미지
    private Sprite soundOffImage;                               // 사운드 Off 이미지

    // ======================================================================================================================
    // [Display]

    [Header("---[Effect]")]
    [SerializeField] private Button effecctToggleButton;        // 이펙트 On/Off 버튼
    [SerializeField] private RectTransform  effectHandle;       // 이펙트 토글 핸들
    private Coroutine effectToggleCoroutine;                    // 이펙트 토글 애니메이션 코루틴
    private bool isEffectOn = true;                             // 이펙트 활성화 여부

    [Header("---[Screen Shaking]")]
    [SerializeField] private Button shakingToggleButton;        // 흔들림 On/Off 버튼
    [SerializeField] private RectTransform shakingHandle;       // 흔들림 토글 핸들
    private Coroutine shakingToggleCoroutine;                   // 흔들림 토글 애니메이션 코루틴
    private bool isShakingOn = true;                            // 흔들림 활성화 여부

    [Header("---[Saving Mode]")]
    [SerializeField] private Button savingToggleButton;         // 절전모드 On/Off 버튼
    [SerializeField] private RectTransform savingHandle;        // 절전모드 토글 핸들
    private Coroutine savingToggleCoroutine;                    // 절전모드 토글 애니메이션 코루틴
    private bool isSavingOn = true;                             // 절전모드 활성화 여부

    // ======================================================================================================================

    // Enum으로 메뉴 타입 정의 (서브 메뉴를 구분하기 위해 사용)
    private enum OptionMenuType
    {
        Sound,                                  // 사운드 메뉴
        Display,                                // 화면 메뉴
        System,                                 // 시스템 메뉴
        End                                     // Enum의 끝
    }
    private OptionMenuType activeMenuType;      // 현재 활성화된 메뉴 타입

    // ======================================================================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        optionMenuPanel.SetActive(false);

        InitializeOptionManager();
    }

    private void Start()
    {
        activePanelManager = FindObjectOfType<ActivePanelManager>();
        activePanelManager.RegisterPanel("OptionMenu", optionMenuPanel, optionButtonImage);
    }

    // ======================================================================================================================

    // 모든 OptionManager 시작 함수들 모음
    private void InitializeOptionManager()
    {
        InitializeOptionButton();
        InitializeSubMenuButtons();

        InitializeSoundControllers();
        InitializeDisplayControllers();
    }

    // OptionButton 초기 설정 함수
    private void InitializeOptionButton()
    {
        optionButton.onClick.AddListener(() => activePanelManager.TogglePanel("OptionMenu"));
        optionBackButton.onClick.AddListener(() => activePanelManager.ClosePanel("OptionMenu"));
    }

    // Sound 초기 설정 함수
    private void InitializeSoundControllers()
    {
        // Audio 초기화
        AudioClip bgmClip = Resources.Load<AudioClip>("Audios/BGM_Sound");
        AudioClip sfxClip = Resources.Load<AudioClip>("Audios/SFX_Sound");
        bgmController = new SoundController(gameObject, bgmSlider, true, bgmClip);
        sfxController = new SoundController(gameObject, sfxSlider, false, sfxClip);

        bgmController.SetVolume(bgmSlider.value);
        sfxController.SetVolume(sfxSlider.value);

        bgmController.Play();

        // Image 초기화
        soundOnImage = Resources.Load<Sprite>("Sprites/Cats/1");
        soundOffImage = Resources.Load<Sprite>("Sprites/Cats/2");
        bgmOnOffImage.sprite = soundOnImage;
        sfxOnOffImage.sprite = soundOnImage;

        // 
        bgmSoundToggleButton.onClick.AddListener(() => ToggleSound(true));
        sfxSoundToggleButton.onClick.AddListener(() => ToggleSound(false));

        UpdateToggleUI(isBgmOn, true, true);
        UpdateToggleUI(isSfxOn, false, true);
    }

    // Display 초기 설정 함수
    private void InitializeDisplayControllers()
    {
        // 이펙트 토글 버튼 초기화
        effecctToggleButton.onClick.AddListener(() => ToggleEffect());
        UpdateEffectToggleUI(isEffectOn, true);

        // 화면 흔들림 토글 버튼 초기화 
        shakingToggleButton.onClick.AddListener(() => ToggleShaking());
        UpdateShakingToggleUI(isShakingOn, true);

        // 절전모드 토글 버튼 초기화
        savingToggleButton.onClick.AddListener(() => ToggleSaving());
        UpdateSavingToggleUI(isSavingOn, true);
    }

    // ======================================================================================================================
    // [서브 메뉴]

    // 서브 메뉴 버튼 초기화 및 클릭 이벤트 추가 함수
    private void InitializeSubMenuButtons()
    {
        for (int i = 0; i < (int)OptionMenuType.End; i++)
        {
            int index = i;
            subOptionMenuButtons[index].onClick.AddListener(() => ActivateMenu((OptionMenuType)index));
        }

        ActivateMenu(OptionMenuType.Sound);
    }

    // 선택한 서브 메뉴를 활성화하는 함수
    private void ActivateMenu(OptionMenuType menuType)
    {
        activeMenuType = menuType;

        for (int i = 0; i < mainOptionMenus.Length; i++)
        {
            mainOptionMenus[i].SetActive(i == (int)menuType);
        }

        UpdateSubMenuButtonColors();
    }

    // 서브 메뉴 버튼 색상을 업데이트하는 함수
    private void UpdateSubMenuButtonColors()
    {
        for (int i = 0; i < subOptionMenuButtons.Length; i++)
        {
            UpdateSubButtonColor(subOptionMenuButtons[i].GetComponent<Image>(), i == (int)activeMenuType);
        }
    }

    // 서브 메뉴 버튼 색상을 활성 상태에 따라 업데이트하는 함수
    private void UpdateSubButtonColor(Image buttonImage, bool isActive)
    {
        string colorCode = isActive ? activeColorCode : inactiveColorCode;
        if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
        {
            buttonImage.color = color;
        }
    }

    // ======================================================================================================================
    // [사운드 설정]

    // 사운드 On/Off 버튼 함수
    public void ToggleSound(bool isBgm)
    {
        if (isBgm)
        {
            isBgmOn = !isBgmOn;

            SetSoundToggleImage(true);
        }
        else
        {
            isSfxOn = !isSfxOn;

            SetSoundToggleImage(false);
        }

        UpdateToggleUI(isBgm ? isBgmOn : isSfxOn, isBgm);
    }

    // 사운드 On/Off 이미지 변경 함수
    private void SetSoundToggleImage(bool isBgm)
    {
        bool isOn = isBgm ? isBgmOn : isSfxOn;
        float volume = isBgm ? bgmSlider.value : sfxSlider.value;
        Image targetImage = isBgm ? bgmOnOffImage : sfxOnOffImage;

        if (!isOn || volume == 0)
        {
            targetImage.sprite = soundOffImage;
        }
        else
        {
            targetImage.sprite = soundOnImage;
        }
    }

    // 사운드 버튼 UI 업데이트 함수
    private void UpdateToggleUI(bool state, bool isBgm, bool instant = false)
    {
        float targetX = state ? onX : offX;
        float targetVolume = state ? (isBgm ? bgmSlider.value : sfxSlider.value) : 0.0f;
        RectTransform soundHandle = isBgm ? bgmSoundHandle : sfxSoundHandle;
        SoundController controller = isBgm ? bgmController : sfxController;

        if (instant)
        {
            soundHandle.anchoredPosition = new Vector2(targetX, soundHandle.anchoredPosition.y);
            controller.SetVolume(targetVolume);
        }
        else
        {
            ref Coroutine toggleCoroutine = ref (isBgm ? ref bgmToggleCoroutine : ref sfxToggleCoroutine);

            StopAndStartCoroutine(ref toggleCoroutine, AnimateToggle(soundHandle, targetX, controller, targetVolume));
        }
    }

    // 코루틴 정지 후 실행시키는 함수
    private void StopAndStartCoroutine(ref Coroutine coroutine, IEnumerator routine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(routine);
    }

    // 사운드 On/Off 버튼 애니메이션 코루틴
    private IEnumerator AnimateToggle(RectTransform handle, float targetX, SoundController controller, float targetVolume)
    {
        float elapsedTime = 0f;
        float startX = handle.anchoredPosition.x;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            handle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), handle.anchoredPosition.y);
            yield return null;
        }

        handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
        controller.SetVolume(targetVolume);
    }

    // ======================================================================================================================
    // [디스플레이 설정]

    // 이펙트 On/Off 토글 함수
    public void ToggleEffect()
    {
        isEffectOn = !isEffectOn;
        UpdateEffectToggleUI(isEffectOn);
    }

    // 이펙트 토글 UI 업데이트 함수
    private void UpdateEffectToggleUI(bool state, bool instant = false)
    {
        float targetX = state ? onX : offX;

        if (instant)
        {
            effectHandle.anchoredPosition = new Vector2(targetX, effectHandle.anchoredPosition.y);
        }
        else
        {
            StopAndStartCoroutine(ref effectToggleCoroutine, AnimateDisplayToggle(effectHandle, targetX));
        }
    }

    // 화면 흔들림 On/Off 토글 함수
    public void ToggleShaking()
    {
        isShakingOn = !isShakingOn;
        UpdateShakingToggleUI(isShakingOn);
    }

    // 화면 흔들림 토글 UI 업데이트 함수
    private void UpdateShakingToggleUI(bool state, bool instant = false)
    {
        float targetX = state ? onX : offX;

        if (instant)
        {
            shakingHandle.anchoredPosition = new Vector2(targetX, shakingHandle.anchoredPosition.y);
        }
        else
        {
            StopAndStartCoroutine(ref shakingToggleCoroutine, AnimateDisplayToggle(shakingHandle, targetX));
        }
    }

    // 절전모드 On/Off 토글 함수
    public void ToggleSaving()
    {
        isSavingOn = !isSavingOn;
        UpdateSavingToggleUI(isSavingOn);
    }

    // 절전모드 토글 UI 업데이트 함수
    private void UpdateSavingToggleUI(bool state, bool instant = false)
    {
        float targetX = state ? onX : offX;

        if (instant)
        {
            savingHandle.anchoredPosition = new Vector2(targetX, savingHandle.anchoredPosition.y);
        }
        else
        {
            StopAndStartCoroutine(ref savingToggleCoroutine, AnimateDisplayToggle(savingHandle, targetX));
        }
    }

    // 디스플레이 토글 애니메이션 코루틴
    private IEnumerator AnimateDisplayToggle(RectTransform handle, float targetX)
    {
        float elapsedTime = 0f;
        float startX = handle.anchoredPosition.x;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            handle.anchoredPosition = new Vector2(Mathf.Lerp(startX, targetX, t), handle.anchoredPosition.y);
            yield return null;
        }

        handle.anchoredPosition = new Vector2(targetX, handle.anchoredPosition.y);
    }

}
