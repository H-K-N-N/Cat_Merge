using UnityEngine;

public enum CharacterState
{
    isWalk,
    isFaint,
    isGetCoin,
    isGrab,
    isBattle,
    isAttack
}

public class AnimatorManager : MonoBehaviour
{
    private Animator animator;
    private CharacterState currentState;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeState(CharacterState.isWalk);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeState(CharacterState.isFaint);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeState(CharacterState.isGetCoin);
        if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeState(CharacterState.isGrab);
        if (Input.GetKeyDown(KeyCode.Alpha5)) ChangeState(CharacterState.isBattle);
        if (Input.GetKeyDown(KeyCode.Alpha6)) ChangeState(CharacterState.isAttack);
    }
    public void ChangeState(CharacterState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    private void ResetAllStateBools()
    {
        animator.SetBool("isWalk", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isGetCoin", false);
        animator.SetBool("isGrab", false);
        animator.SetBool("isBattle", false);
        animator.SetBool("isAttack", false);
    }

    private void SetBoolForState(CharacterState state)
    {
        animator.SetBool(state.ToString(), true);
    }
}

