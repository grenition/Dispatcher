using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement), typeof(CharacterAnimations))]
public class PlayerController : MonoBehaviour
{
    //public values
    public static PlayerController Instance { get; private set; }
    public static bool Alive
    {
        get
        {
            if (Instance != null)
                return Instance.alive;
            return false;
        }
    }
    public float HorizontalSpeed { get => characterMovement.HorizontalSpeed; }

    //parameters
    [Header("Living")]
    [SerializeField] private bool alive = true;
    [Header("Movement")]
    [SerializeField] private float minVerticalDistanceToSplitCoinsToLayers = 3f;
    [SerializeField] private float minDistanceToMove = 1f;
    [SerializeField] private ObstaclesDetector obstaclesDetector;
    [Header("Controls")]
    [SerializeField] private float clickDelay = 0.1f;

    //local values
    private CharacterMovement characterMovement;
    private CharacterAnimations characterAnimations;
    private GridObject masterCoin;
    private GridObject targetCoin;
    private List<GridObject> coins = new List<GridObject>();
    private int playerFloor;
    private bool reservePositionIsDefined = false;
    private Vector3 reservePosition = Vector3.zero;



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
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        characterMovement = GetComponent<CharacterMovement>();
        characterAnimations = GetComponent<CharacterAnimations>();
    }
    private void Start()
    {
        if (masterCoin == null)
        {
            masterCoin = ObjectsPool.GetNewGridObject(ObjectType.coin);
            if (masterCoin != null)
                masterCoin.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        //CheckDoubleClick();
    }
    private void FixedUpdate()
    {
        playerFloor = GridInteractions.Instance.GetFloorId(transform.position);

        CheckCoins();

        if (coins.Count > 0)
        {
            if (targetCoin == null || (targetCoin != null && transform.position.x - targetCoin.transform.position.x < 0f)
                || !targetCoin.gameObject.activeSelf || targetCoin.CurrentFloor > playerFloor)
            {
                targetCoin = GetTargetCoint(playerFloor);
            }
            if (targetCoin != null)
            {
                MoveToTarget(targetCoin.transform.position);
            }
        }
        if (targetCoin == null)
        {
            if (!reservePositionIsDefined) {
                reservePosition = transform.position;
                reservePositionIsDefined = true;
            }
            MoveToTarget(reservePosition);
        }
        if (GameInput.PlacingArea.IsSinglePressed() && Time.time - GameInput.PlacingArea.PressTime > clickDelay 
            && GridInteractions.Instance.CurrentInteractableObject == null)
        {
            if (GridInteractions.Instance.CheckPlaceAvailabilityForGridObject(masterCoin, out Vector3 point))
            {
                GridObject coin = ObjectsPool.GetGridObject(ObjectType.coin);
                coin.PlaceOnAwake = false;
                GridInteractions.Instance.PlaceObjectSimple(coin, point);
            }
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
    private void CheckCoins()
    {
        if (coins.Count == 0)
        {
            targetCoin = null;
            return;
        }

        int i = 0;
        while(i < coins.Count)
        {
            float distance = transform.position.x - coins[i].transform.position.x;
            if (distance < 0 || !coins[i].gameObject.activeSelf)
            {
                if (targetCoin == coins[i])
                    targetCoin = null;
                coins.Remove(coins[i]);
                continue;
            }
            i++;
        }
    }
    private GridObject GetTargetCoint(int floorId)
    {
        foreach(var coin in coins)
        {
            if (coin.CurrentFloor <= floorId)
            {
                reservePositionIsDefined = false;
                return coin;
            }
        }
        return null;
    }
    private void OnCoinPlaced(GridObject coinObj)
    {
        if (coinObj == null || coinObj.ObjType != ObjectType.coin || coins.Contains(coinObj))
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
        if (Instance == null)
            return;
        Instance.alive = false;
        Instance.characterMovement.LockMovement = true;
        Instance.characterAnimations.DieAnimation();
    }
    public void Revive()
    {
        if (Instance == null)
            return;
        Instance.alive = true;
        Instance.characterMovement.LockMovement = false;
        Instance.characterAnimations.ResetAnimator();
    }
    #endregion
}
