using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement), typeof(CharacterAnimations))]
public class PlayerController : MonoBehaviour
{
    //public values
    public static PlayerController instance;
    public static bool Alive
    {
        get
        {
            if (instance != null)
                return instance.alive;
            return false;
        }
    }

    //parameters
    [Header("Living")]
    [SerializeField] private bool alive = true;
    [Header("Movement")]
    [SerializeField] private float minDistanceToMove = 1f;
    [SerializeField] private ObstaclesDetector obstaclesDetector;

    //local values
    private CharacterMovement characterMovement;
    private CharacterAnimations characterAnimations;
    private GridObject currentCoin;
    private List<GridObject> coins = new List<GridObject>();

    #region unity events
    private void OnEnable()
    {
        GridInteractions.OnNewGridObjectPlaced += OnCoinPlaced;
    }
    private void OnDisable()
    {
        GridInteractions.OnNewGridObjectPlaced -= OnCoinPlaced;
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        characterMovement = GetComponent<CharacterMovement>();
        characterAnimations = GetComponent<CharacterAnimations>();
    }
    private void Update()
    {
        CheckCurrentCoin();
        if (coins.Count != 0)
            MoveToTarget(coins[0].transform.position);

        if(Input.GetKeyDown(KeyCode.Mouse0) && GridInteractions.Instance.CurrentInteractableObject == null)
        {
            if (Inventory.IsObjectAvailable(ObjectType.coin))
                Inventory.StartPlacingGridObject(ObjectType.coin);
        }
    }
    #endregion
    #region local functions
    private void MoveToTarget(Vector3 target)
    {
        Vector3 dir = VectorMath.ExtractDotVector(target - transform.position, Vector3.forward);
        if (dir.magnitude < minDistanceToMove)
            return;
        if(obstaclesDetector != null)
        {
            if (obstaclesDetector.CheckEmptySpace(dir))
                return;
            dir = Vector3.ClampMagnitude(dir, obstaclesDetector.CheckingDistance);
        }
        characterMovement.GoNearestLine(transform.position + dir);
    }
    private void CheckCurrentCoin()
    {
        if (coins.Count == 0)
            return;
        float distance = transform.position.x - coins[0].transform.position.x;
        if (distance < 0)
        {
            coins.Remove(coins[0]);
        }
    }
    private void OnCoinPlaced(GridObject coinObj = default)
    {
        if (coinObj == null || coinObj.ObjType != ObjectType.coin)
            return;
        float distance = transform.position.x - coinObj.transform.position.x;
        if (distance < 0)
            return;
        for(int i = 0; i < coins.Count; i++)
        {
            float otherDistance = transform.position.x - coins[i].transform.position.x;
            if(distance < otherDistance)
            {
                coins.Insert(i, coinObj);
                return;
            }
        }
        coins.Add(coinObj);
    }
    private GridObject GetNearestCoin()
    {
        List<GridObject> objects = ObjectsPool.GetActiveGridObjectOfType(ObjectType.coin);

        GridObject minObject = null;
        float minDistance = float.MaxValue;
        float distance = 0f;
        foreach(var obj in objects)
        {
            distance = transform.position.x - obj.transform.position.x;
            if(distance > 0 && distance < minDistance)
            {
                minDistance = distance;
                minObject = obj;
            }
        }
        return minObject;
    }
    #endregion
    #region global interactions
    public static void Die()
    {
        if (instance == null)
            return;
        instance.alive = false;
        instance.characterMovement.LockMovement = true;
        instance.characterAnimations.DieAnimation();
    }
    public void Revive()
    {
        if (instance == null)
            return;
        instance.alive = true;
        instance.characterMovement.LockMovement = false;
        instance.characterAnimations.ResetAnimator();
    }
    #endregion
}
