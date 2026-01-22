using System.Runtime.CompilerServices;
using UnityEngine;

public class Animation_State_Controller : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
       // Debug.Log(animator);
    }

    public void SetIsWalking(bool val)
    {
        if (val)
        {
            SetIsAiming(false);
        }
        animator.SetBool("isWalking?", val);
    }

    public bool GetIsWalking()
    {
        return animator.GetBool("isWalking?");
    }

    public void SetIsRunning(bool val)
    {
        
        animator.SetBool("isRunning", val);
    }

    public bool GetIsRunning()
    {
        return animator.GetBool("isRunning");
    }

    public void SetIsAiming(bool val)
    {
        animator.SetBool("isAiming", val);
    }

    public bool GetIsAiming()
    {
        return animator.GetBool("isAiming");
    }

    public void SetIsFiring(bool val)
    {
        animator.SetBool("isFiring", val);
    }

    public bool GetIsFiring()
    {
        return animator.GetBool("isFiring");
    }

    
 };
