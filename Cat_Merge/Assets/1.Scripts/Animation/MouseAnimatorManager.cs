using UnityEngine;

public enum MouseState
{
    isIdle,
    isFaint,
    isAttack1,
    isAttack2,
    isAttack3,
}

public class MouseAnimatorManager : MonoBehaviour
{


    #region Variables

    private Animator animator;
    private MouseState currentState;

    #endregion


    #region Unity Methods

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    #endregion


    #region State Management

    // 쥐 상태 변경 및 애니메이션 적용 함수
    public void ChangeState(MouseState newState)
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
        animator.SetBool("isFaint", false);
        animator.SetBool("isAttack1", false);
        animator.SetBool("isAttack2", false);
        animator.SetBool("isAttack3", false);
    }

    // 특정 상태의 bool 값을 true로 설정하는 함수
    private void SetBoolForState(MouseState state)
    {
        animator.SetBool(state.ToString(), true);
    }

    #endregion


}