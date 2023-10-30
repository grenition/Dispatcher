using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    private Animator animator;

    public float jumpGravity = -40;
    public float jumpPower = 15;
    public float realGravity = -9.8f;

    private float laneOffset = 2.7f;
    public float laneChangeSpeed = 7f;
    
    public float pointStart;
    public float pointFinish;
    public float startPositionZ;
    private bool isMoving = false;
    private Coroutine movingCoroutine;

    private bool isDead = false;
    private bool isJumping = false;
    private Rigidbody rb;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        pointFinish = transform.position.z;
        startPositionZ = transform.position.z;

        StartGame();
    }

    //void Update()
    //{
    //    if (isDead)
    //        return;
    //    if (Input.GetKeyDown(KeyCode.W) && (pointFinish - 1f > startPositionZ - laneOffset) && isMoving == false)
    //    {
    //        MoveHorizontal(-laneChangeSpeed);
    //    }
    //    if (Input.GetKeyDown(KeyCode.S) && (pointFinish + 1f < startPositionZ + laneOffset) && (isMoving == false))
    //    {
    //        MoveHorizontal(laneChangeSpeed);
    //    }

    //}
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && (pointFinish - 1f > startPositionZ - laneOffset) && isMoving == false)
        {
            MoveHorizontal(-laneChangeSpeed);
        }
        if (Input.GetKeyDown(KeyCode.S) && (pointFinish + 1f < startPositionZ + laneOffset) && (isMoving == false))
        {
            MoveHorizontal(laneChangeSpeed);
        }
    }

    public void Jump()
    {
        if (isJumping)
            return;

        animator.CrossFade("jumping", 0f);
        isJumping = true;
        rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        Physics.gravity = new Vector3(0, jumpGravity, 0);
        StartCoroutine(StopJumpCorountine());
    }

    IEnumerator StopJumpCorountine()
    {
        Debug.Log(rb.velocity.y);
        do
        {
            yield return new WaitForSeconds(0.02f);
        } while (rb.velocity.y != 0);

        isJumping = false;
        Physics.gravity = new Vector3(0, realGravity, 0);
    }

    public void StartGame()
    {
        animator.SetBool("isRunning", true);
        animator.SetBool("isDead", false);
        animator.SetBool("isJumping", false);

    }
    public void EndGame()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        animator.SetBool("isRunning", false);

    }

    void MoveHorizontal(float speed)
    {
        animator.applyRootMotion = false;
        pointStart = pointFinish;
        pointFinish += Mathf.Sign(speed) * laneOffset;

        if (isMoving)
        {
            StopCoroutine(movingCoroutine);
            animator.Play("jumping");
            isMoving = false;
        }
        animator.CrossFade("jumping", 0f);
        movingCoroutine = StartCoroutine(MoveCoroutine(speed));
    }

    IEnumerator MoveCoroutine(float vectorX)
    {
        isMoving = true;
        yield return new WaitForSeconds(0.2f);
        while (Mathf.Abs(pointStart - transform.position.z) < laneOffset)
        {
            yield return new WaitForFixedUpdate();

            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, vectorX);
            float x = Mathf.Clamp(transform.position.z, Mathf.Min(pointStart, pointFinish),
                                  Mathf.Max(pointStart, pointFinish));
            transform.position = new Vector3(transform.position.x, transform.position.y, x);
        }
        rb.velocity = Vector3.zero;
        //transform.position = new Vector3(transform.position.x, transform.position.y, pointFinish);
        if (transform.position.y > 1)
        {
            rb.velocity = new Vector3(rb.velocity.x, -10, rb.velocity.z);
        }
        isMoving = false;
    }

}

