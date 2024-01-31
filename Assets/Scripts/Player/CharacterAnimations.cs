using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement), typeof(Animator))]
public class CharacterAnimations : MonoBehaviour
{
    [SerializeField] private float lerpingHorizontalMult = 5f;
    [SerializeField] private float minVerticalMagnitude = 0.97f;
    private CharacterMovement mov;
    private Animator anim;
    private float currentHorizontal = 0f;
    private bool isGrounded = false;
    
    private void Awake()
    {
        mov = GetComponent<CharacterMovement>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        currentHorizontal = Mathf.Lerp(currentHorizontal, mov.CurrentMovementData.movement.z, lerpingHorizontalMult * Time.deltaTime);
        anim.SetFloat("Horizontal", currentHorizontal);

        isGrounded = Mathf.Abs(mov.CurrentMovementData.movement.y) < minVerticalMagnitude;
        anim.SetBool("IsGrounded", mov.IsGrounded);
    }
    public void DieAnimation()
    {
        anim.SetBool("isDead", true);
    }
    public void ResetAnimator()
    {
        anim.SetBool("isDead", false);
        anim.SetFloat("Horizontal", 0f);

        anim.enabled = false;
        anim.enabled = true;
    }
}
