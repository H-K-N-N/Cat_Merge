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
    isAttack
}

[System.Serializable]
public struct GradeOverrideData
{
    public int grade;
    public AnimatorOverrideController overrideController;
}

public class AnimatorManager : MonoBehaviour
{
    private Animator animator;
    public int catGrade;

    [Header("등급별 애니메이터 오버라이드 리스트")]
    public List<GradeOverrideData> overrideDataList;
    public Dictionary<int, AnimatorOverrideController> overrideDict;

    private CatState currentState;


    void Awake()
    {
        animator = GetComponent<Animator>();

        // 딕셔너리 초기화
        overrideDict = new Dictionary<int, AnimatorOverrideController>();
        foreach (var data in overrideDataList)
        {
            if (!overrideDict.ContainsKey(data.grade))
            {
                overrideDict.Add(data.grade, data.overrideController);
            }
        }

        // TitleScene인 경우 catGrade를 사용한 등급 적용
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            ApplyAnim(catGrade);
        }
    }

    public void ChangeState(CatState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    private void ResetAllStateBools()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isGetCoin", false);
        animator.SetBool("isGrab", false);
        animator.SetBool("isBattle", false);
        animator.SetBool("isAttack", false);
    }

    private void SetBoolForState(CatState state)
    {
        animator.SetBool(state.ToString(), true);
    }

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

    public void ApplyAnim(int grade)
    {
        ApplyAnimatorOverride(grade);
    }


}

