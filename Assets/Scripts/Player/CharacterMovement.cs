using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MovementData
{
    public Vector3 movement;
}

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    //public values
    public MovementData CurrentMovementData;
    public bool LockMovement { get; set; }
    public float HorizontalSpeed { get => horizontalSpeed; }

    //parameters
    [Header("Velocity")]
    [SerializeField] private float horizontalSpeed = 2f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float highJumpForce = 10f;

    [Header("Placing on lines")]
    [SerializeField] private Vector3[] lines = {new Vector3(0f, 0f, -3f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 3f)};
    [SerializeField] private int startLine = 1;

    [Header("Optional")]
    [SerializeField] private float distanceToResetMovement = 3.1f;
    [SerializeField] private float minDistanceForHardMovement = 0.05f;
    [SerializeField] private bool hardMovement = false;
    [SerializeField] private float minMomentumToDetectFallingOrRising = 0.5f;

    //local values
    private CharacterController con;
    private int currentLine;
    private Vector3 physicsMomentum = Vector3.zero;

    #region Unity Events
    private void Awake()
    {
        con = GetComponent<CharacterController>();
    }
    private void OnEnable()
    {
        //устанавливаем стартувую линию
        currentLine = startLine;
    }
    private void Update()
    {
        UpdateLineBounds();

        //общее направление движения персонажа
        Vector3 movement = Vector3.zero;
        if(!LockMovement)
            movement += GetHorizontalVelocity() * Time.deltaTime;
        movement += GetVerticalMomentum() * Time.deltaTime;
        //применяем движение к персонажу
        con.Move(movement);
    }
    #endregion
    #region Base
    private Vector3 GetVerticalMomentum()
    {
        //длина вектора с определенным знаком, относительно vector3.up
        float dotProduct = VectorMath.GetDotProduct(physicsMomentum, Vector3.up);

        //добавляем физику
        if (!con.isGrounded)
            physicsMomentum -= Vector3.up * Time.deltaTime * gravity;
        else
        {
            //урезаем "нижнюю" часть момента
            if(dotProduct < 0f)
                physicsMomentum = Vector3.zero;
        }

        //записываем движение
        CurrentMovementData.movement.y = Mathf.Clamp(dotProduct, -1f, 1f);

        return physicsMomentum;
    }
    private Vector3 GetHorizontalVelocity()
    {
        //получаем направление движения для перемещения между линиями
        Vector3 direction = lines[currentLine] - transform.position;
        //убираем вертикальную составляющую направления
        direction = VectorMath.RemoveDotVector(direction, Vector3.up);

        //применение скорости для направления
        if (hardMovement)
        {
            //если мы очень близко к цели, то переходим на плавное движение, чтобы не возникало дрожание персонажа
            if (direction.magnitude > minDistanceForHardMovement)
                direction.Normalize();
            direction *= horizontalSpeed;
        }
        else
        {
            //просто плавное движение
            direction *= horizontalSpeed;
            direction = Vector3.ClampMagnitude(direction, horizontalSpeed);
        }

        //записываем движение
        CurrentMovementData.movement = new Vector3(direction.x / horizontalSpeed, CurrentMovementData.movement.y, direction.z / horizontalSpeed);
       
        return direction;
    }
    private void UpdateLineBounds()
    {
        //ограничения к текущей линии
        currentLine = Mathf.Clamp(currentLine, 0, lines.Length - 1);
    }
    #endregion
    #region Controls
    public void GoUpperLine()
    {
        GoLine(currentLine - 1);
    }
    public void GoLowerLine()
    {
        GoLine(currentLine + 1);
    }
    public void GoLine(int lineId)
    {
        currentLine = lineId;
    }
    public void GoNearestLine(Vector3 targetPosition)
    {
        int minLineId = 0;
        float minDistance = float.MaxValue;
        for(int i = 0; i < lines.Length; i++)
        {
            float dist = Vector3.Distance(targetPosition, lines[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                minLineId = i;
            }
        }
        GoLine(minLineId);
    }
    public void Jump(float force)
    {
        physicsMomentum += Vector3.up * force;
    }
    public void HighJump()
    {
        Jump(highJumpForce);
    }
    #endregion
}
