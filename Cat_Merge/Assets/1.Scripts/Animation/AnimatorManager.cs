using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum CatState
{
    isIdle,
    isWalk,
    isFaint,
    isGetCoin,
    isGrab,
    isBattle,
    isAttack,
    isPick
}

[System.Serializable]
public struct GradeOverrideData
{
    public int grade;
    public AnimatorOverrideController overrideController;
}

public class AnimatorManager : MonoBehaviour
{


    #region Variables

    private Animator animator;
    public int catGrade;

    [Header("등급별 애니메이터 오버라이드 리스트")]
    public List<GradeOverrideData> overrideDataList;
    public Dictionary<int, AnimatorOverrideController> overrideDict;

    private CatState currentState;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        InitializeAnimator();
        InitializeOverrideDict();
        ApplyTitleSceneAnimation();
    }

    #endregion


    #region Initialize

    // 애니메이터 컴포넌트 초기화 함수
    private void InitializeAnimator()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // 오버라이드 딕셔너리 초기화 함수
    private void InitializeOverrideDict()
    {
        overrideDict = new Dictionary<int, AnimatorOverrideController>();
        foreach (var data in overrideDataList)
        {
            if (!overrideDict.ContainsKey(data.grade))
            {
                overrideDict.Add(data.grade, data.overrideController);
            }
        }
    }

    // 타이틀 씬에서의 애니메이션 적용 함수
    private void ApplyTitleSceneAnimation()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            ApplyAnim(catGrade);
        }
    }

    #endregion


    #region State Management

    // 고양이 상태 변경 및 애니메이션 적용 함수
    public void ChangeState(CatState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    // 모든 상태 bool 값 초기화 함수
    private void ResetAllStateBools()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isGetCoin", false);
        animator.SetBool("isGrab", false);
        animator.SetBool("isBattle", false);
        animator.SetBool("isAttack", false);
        animator.SetBool("isPick", false);
    }

    // 특정 상태의 bool 값을 true로 설정하는 함수
    private void SetBoolForState(CatState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    #endregion


    #region Animation Override

    // 특정 등급의 애니메이터 오버라이드 컨트롤러 적용 함수
    public void ApplyAnimatorOverride(int grade)
    {
        if (overrideDict.ContainsKey(grade))
        {
            if (!animator.enabled) animator.enabled = true;
            animator.runtimeAnimatorController = overrideDict[grade];
        }
        else
        {
            animator.enabled = false;
        }
    }

    // 애니메이션 적용 함수
    public void ApplyAnim(int grade)
    {
        ApplyAnimatorOverride(grade);
    }

    #endregion


}
