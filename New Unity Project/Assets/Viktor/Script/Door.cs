using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] Animator animator;

    public void Interact()
    {
        animator.SetBool("isOpen", !animator.GetBool("isOpen"));
    }
}
