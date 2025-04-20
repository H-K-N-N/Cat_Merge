using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private Animator animator;
    private MouseState currentState;


    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeState(MouseState newState)
    {
        if (currentState == newState) return;

        ResetAllStateBools();
        SetBoolForState(newState);

        currentState = newState;
    }

    private void ResetAllStateBools()
    {
        animator.SetBool("isIdle", false);
        animator.SetBool("isFaint", false);
        animator.SetBool("isAttack1", false);
        animator.SetBool("isAttack2", false);
        animator.SetBool("isAttack3", false);
    }

    private void SetBoolForState(MouseState state)
    {
        animator.SetBool(state.ToString(), true);
    }

}

