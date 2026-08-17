using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;
using System.Collections;

// 구글 광고 관련 스크립트
public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // 실제 광고 ID
#if UNITY_ANDROID
    // 광고 게시 중단으로 인해 테스트 광고로 대체
    //private string _productionAdUnitId = "ca-app-pub-6387288948977074/4325853233";
    private string _productionAdUnitId = "ca-app-pub-6387288948977074/5875297683";
    private string _testAdUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    // IPHONE은 지원하지 않으므로 둘다 테스트로 대체
    private string _productionAdUnitId = "ca-app-pub-3940256099942544/6978759866";
    private string _testAdUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _productionAdUnitId = "unused";
    private string _testAdUnitId = "unused";
#endif

    private string _adUnitId;
    private bool isInitialized = false;
    private int retryAttempt;
    private const int MAX_RETRY_ATTEMPTS = 3;

    private RewardedAd rewardedAd;
    private bool isRewardEarned = false;
    private bool isLoadingAd = false;

    #endregion


    #region Unity Methods

    public void Awake()
    {
#if UNITY_EDITOR
        _adUnitId = _testAdUnitId;
#else
        _adUnitId = _productionAdUnitId;
#endif

        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            isInitialized = true;

            StartCoroutine(InitialAdLoadRoutine());
        });
    }

    private IEnumerator InitialAdLoadRoutine()
    {
        int initialLoadAttempts = 0;
        const int MAX_INITIAL_ATTEMPTS = 3;

        while (initialLoadAttempts < MAX_INITIAL_ATTEMPTS)
        {
            if (!isLoadingAd && (rewardedAd == null || !rewardedAd.CanShowAd()))
            {
                LoadRewardedAd();
                yield return new WaitForSeconds(2f);
            }
            else if (rewardedAd != null && rewardedAd.CanShowAd())
            {
                break;
            }
            initialLoadAttempts++;
        }
    }

    public void Start()
    {
        retryAttempt = 0;
    }

    #endregion


    #region Load Ad

    public void LoadRewardedAd()
    {
        if (!isInitialized || isLoadingAd)
        {
            Debug.Log("AdMob SDK가 아직 초기화되지 않았습니다.");
            return;
        }

        isLoadingAd = true;

        // 새로운 광고를 로드하기 전에 이전 광고 정리
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log($"보상형 전면 광고를 로딩합니다. 시도 #{retryAttempt + 1}");

        var adRequest = new AdRequest();
        //adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(_adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                isLoadingAd = false;

                if (error != null || ad == null)
                {
                    Debug.Log($"보상형 전면 광고 로딩 실패. 에러: {error?.GetMessage()}");

                    // 재시도 로직
                    if (retryAttempt < MAX_RETRY_ATTEMPTS)
                    {
                        retryAttempt++;
                        float delay = Mathf.Pow(2, retryAttempt);
                        Debug.Log($"{delay}초 후 재시도합니다...");
                        Invoke(nameof(LoadRewardedAd), delay);
                    }
                    else
                    {
                        Debug.Log("최대 재시도 횟수를 초과했습니다.");
                        ShopManager.Instance?.ResetAdState();
                    }
                    return;
                }

                Debug.Log("보상형 전면 광고가 성공적으로 로딩되었습니다.");
                rewardedAd = ad;
                retryAttempt = 0;

                // 광고 이벤트 핸들러 등록
                RegisterEventHandlers(rewardedAd);
            });
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        // 광고가 시작될 때 보상 플래그 초기화
        ad.OnAdFullScreenContentOpened += () =>
        {
            isRewardEarned = false;
            Debug.Log("광고가 표시되었습니다.");
        };

        // 광고가 닫힐 때
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("광고가 닫혔습니다.");
            if (!isRewardEarned)
            {
                ShopManager.Instance?.ResetAdState();
            }
            LoadRewardedAd();
        };

        // 광고 표시 실패시
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.Log($"광고 표시 실패: {error.GetMessage()}");
            LoadRewardedAd();
        };

        // 보상 획득시
        ad.OnAdPaid += (AdValue adValue) =>
        {
            string msg = string.Format("광고 보상이 지급되었습니다. (통화: {0}, 값: {1}, 정밀도: {2})", adValue.CurrencyCode, adValue.Value, adValue.Precision);
            Debug.Log(msg);
        };
    }

    #endregion


    #region Show Ad

    // CashForAd 광고 (ShopManager)
    public void ShowRewardedCashForAd()
    {
        if (!isInitialized)
        {
            Debug.Log("AdMob SDK가 아직 초기화되지 않았습니다.");
            ShopManager.Instance?.ResetAdState();
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            try
            {
                rewardedAd.Show((Reward reward) =>
                {
                    isRewardEarned = true;  // 보상 획득 시 플래그 설정
                    Debug.Log($"CashForAd 보상이 지급됩니다: {reward.Type}");
                    ShopManager.Instance.OnCashForAdRewardComplete();
                });
            }
            catch (Exception e)
            {
                Debug.Log($"광고 표시 중 오류 발생: {e.Message}");
                ShopManager.Instance?.ResetAdState();
                LoadRewardedAd();
            }
        }
        else
        {
            Debug.Log("광고가 준비되지 않았습니다. 새로운 광고를 로드합니다.");
            ShopManager.Instance?.ResetAdState();
            LoadRewardedAd();
        }
    }

    // DoubleCoinForAd 광고 (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        if (!isInitialized)
        {
            Debug.Log("AdMob SDK가 아직 초기화되지 않았습니다.");
            ShopManager.Instance?.ResetAdState();
            return;
        }

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            try
            {
                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"DoubleCoinForAd 보상이 지급됩니다: {reward.Type}");
                    isRewardEarned = true;
                    ShopManager.Instance.OnDoubleCoinAdRewardComplete();
                    // OnAdFullScreenContentClosed already reloads the next ad; avoid loading twice per ad watch.
                });
            }
            catch (Exception e)
            {
                Debug.Log($"광고 표시 중 오류 발생: {e.Message}");
                ShopManager.Instance?.ResetAdState();
                LoadRewardedAd();
            }
        }
        else
        {
            Debug.Log("광고가 준비되지 않았습니다. 새로운 광고를 로드합니다.");
            ShopManager.Instance?.ResetAdState();
            LoadRewardedAd();
        }
    }

    #endregion


}
