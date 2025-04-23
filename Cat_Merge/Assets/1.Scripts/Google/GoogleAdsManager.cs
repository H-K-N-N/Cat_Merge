using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;

public class GoogleAdsManager : MonoBehaviour
{


    #region Variables

    // 테스트 광고를 위한 광고 단위 ID 설정
#if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
    private string _adUnitId = "unused";
#endif

    private RewardedInterstitialAd rewardedInterstitialAd;
    private bool isInitialized = false;

    #endregion


    #region Unity Methods

    public void Awake()
    {
        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            isInitialized = true;
        });
    }

    public void Start()
    {
        LoadRewardedInterstitialAd();
    }

    #endregion


    #region Load Ad

    // 네트워크 및 초기화 상태 체크 함수 추가
    private bool CheckAdAvailability()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //Debug.LogWarning("네트워크 연결 불가");
            ShopManager.Instance.ResetAdState();
            return false;
        }

        if (!isInitialized)
        {
            //Debug.LogWarning("광고 SDK가 초기화되지 않음");
            ShopManager.Instance.ResetAdState();
            return false;
        }

        return true;
    }

    public void LoadRewardedInterstitialAd()
    {
        if (!CheckAdAvailability()) return;

        // 새로운 광고를 로드하기 전에 이전 광고 정리
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        //Debug.Log("보상형 전면 광고를 로딩");

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedInterstitialAd.Load(_adUnitId, adRequest,
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    //Debug.Log("보상형 전면 광고 로딩 실패. 에러: " + error);
                    ShopManager.Instance.ResetAdState();
                    return;
                }

                //Debug.Log("보상형 전면 광고가 성공적으로 로딩되었습니다. 응답: " + ad.GetResponseInfo());
                rewardedInterstitialAd = ad;
                RegisterEventHandlers(ad);
            });
    }

    // 광고 이벤트 핸들러 등록
    private void RegisterEventHandlers(RewardedInterstitialAd ad)
    {
        // 광고 표시 실패
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            //Debug.LogError($"광고 표시 실패: {error.GetMessage()}");
            ShopManager.Instance.ResetAdState();
            LoadRewardedInterstitialAd();
        };

        // 광고가 표시됨
        ad.OnAdFullScreenContentOpened += () =>
        {
            //Debug.Log("광고가 표시됨");
        };

        // 광고가 닫힘
        ad.OnAdFullScreenContentClosed += () =>
        {
            //Debug.Log("광고가 닫힘");
            LoadRewardedInterstitialAd();
        };

        // 광고 클릭
        ad.OnAdClicked += () =>
        {
            //Debug.Log("광고 클릭됨");
        };

        // 광고 노출 기록
        ad.OnAdImpressionRecorded += () =>
        {
            //Debug.Log("광고 노출이 기록됨");
        };

        // 광고 수익 발생
        ad.OnAdPaid += (AdValue adValue) =>
        {
            //Debug.Log($"광고 수익 발생: 통화={adValue.CurrencyCode}, 금액={adValue.Value}");
        };
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ShopManager.Instance.ResetAdState();
        }
    }

    #endregion


    #region Show Ad

    // CashForAd 광고 (ShopManager)
    public void ShowRewardedCashForAd()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                ShopManager.Instance.OnCashForAdRewardComplete();

                LoadRewardedInterstitialAd();
            });
        }
        else
        {
            LoadRewardedInterstitialAd();
        }
    }

    // DoubleCoinForAd 광고 (ShopManager)
    public void ShowRewardedDoubleCoinForAd()
    {
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                ShopManager.Instance.OnDoubleCoinAdRewardComplete();

                LoadRewardedInterstitialAd();
            });
        }
        else
        {
            LoadRewardedInterstitialAd();
        }
    }

    #endregion


}
