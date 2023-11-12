using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void GridObjectEventHandler(GridObject gridObject);
public class GridInteractions : MonoBehaviour
{
    public static GridInteractions Instance { get; private set; }
    public Transform InteractionEnd { get => interactionEnd; }
    public Transform InteractionBegin { get => interactionBegin; }

    //events
    public static event GridObjectEventHandler OnGridObjectPlaced;
    public static event GridObjectEventHandler OnNewGridObjectPlaced;

    //parameters
    [Header("Grid Parameters")]
    [SerializeField] private Vector2 tileSize = new Vector2(3f, 3f);
    [SerializeField] private Vector2 gridOffset = Vector2.zero;
    [Tooltip("x field: minimal vertical coordinate, y field: maximum vertical coordinate")]
    [SerializeField] private Vector2 minAndMaxVericalGridCoordinates = new Vector2(-1f, 1f);


    [Header("Legacy")]
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private LayerMask gridObjectsLayerMask;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float smoothGridTranslationMultiplier = 10f;
    [SerializeField] private Transform interactionBegin;
    [SerializeField] private Transform interactionEnd;

    //local values
    private GridObject currentObjectCopy;
    private GridObject currentObjectInstance;
    private Camera mainCamera;
    private GameObject lastObject;
    private Vector3 targetPosition;
    private Vector3 savedObjectLocalPosition;
    private Transform savedObjectParent;
    private float currentDisplacement = 0f;


    #region Grid Logic
    private void OnDrawGizmos()
    {
        for(int y = -1; y < 2; y++)
        {
            for(int x = 0; x < 50; x++)
            {
                Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                Vector3 _center = new Vector3(-x * tileSize.x + currentDisplacement, 4f, -y * tileSize.y);
                Gizmos.DrawCube(_center, new Vector3(tileSize.x, 1f, tileSize.y));
            }
        }
    }

    private void UpdateDisplacement()
    {
        currentDisplacement += GameController.Instance.PlatformsSpeed * Time.deltaTime;
        if (currentDisplacement >= 0)
            currentDisplacement -= tileSize.x;
    }
    private Vector2 GetNearestTile(Vector3 position)
    {
        Vector2 tileCoordinates = new Vector2((position.x - currentDisplacement) / tileSize.x, position.z / tileSize.y);
        tileCoordinates.y = Mathf.Clamp(tileCoordinates.y, minAndMaxVericalGridCoordinates.x, minAndMaxVericalGridCoordinates.y);
        tileCoordinates.x = Mathf.Round(tileCoordinates.x);
        tileCoordinates.y = Mathf.Round(tileCoordinates.y);
        return tileCoordinates;
    }
    public Vector3 GetNearestTilePosition(Vector3 position, bool displacementByMoving = true)
    {
        Vector2 tileCoordinates = GetNearestTile(position);

        float displacement = 0f;
        if (displacementByMoving)
            displacement = currentDisplacement;

        return new Vector3
        {
            x = tileCoordinates.x * tileSize.x + gridOffset.x + displacement,
            y = position.y,
            z = tileCoordinates.y * tileSize.y + gridOffset.y
        };
    }
    #endregion
    #region Unity Events
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        mainCamera = Camera.main;
    }
    private void OnEnable()
    {
        //подписываемся на забирание предмета из инвентаря, при его использовании
        OnGridObjectPlaced += TakeItemFromInventory;
    }
    private void OnDisable()
    {
        //отписываемся, чтобы избежать лишних проблем
        OnGridObjectPlaced -= TakeItemFromInventory;
    }
    void Update()
    {
        //эта функция обновляет коэфицент смещения, чтобы следовать движению платформ
        UpdateDisplacement();

        //при нажатии на кнопку пытаемся определить объект, который находится под курсором
        if (Input.GetKeyDown(KeyCode.Mouse0))
            DetectObject();
        
        //при отжатии кнопки пытаемся поставить объект или удаляем его
        if (!Input.GetKey(KeyCode.Mouse0) && CurrentInteractableObject != null)
        {
            PlaceCurrentObject();
        }

        //находим ближайший тайл сетки для текущего объекта
        UpdateTargetTile();
        //двигаем текущий объект к тайлу
        MoveCurrentObject();
        //меняем цвет
        SetCurrentObjectMaterial();
    }
    #endregion
    #region GridObject main
    public GridObject CurrentInteractableObject
    {
        get
        {
            return currentObjectCopy;
        }
        set
        {
            if (currentObjectCopy != null)
            {
                currentObjectCopy.DestroyObject();
                currentObjectInstance = null;
            }

            if (value != null && !value.Interactable)
                return;

            currentObjectCopy = value;
            if (currentObjectCopy != null)
            {
                currentObjectCopy.IsInterating = true;

                //смещаем объект куда-то далеко, чтобы при спавне не появлялся на экране
                float median = ((interactionBegin.position + interactionEnd.position) / 2f).x;
                currentObjectCopy.transform.position = new Vector3(median, -99999f, 99999f);
                targetPosition = new Vector3(median, -99999f, 99999f);
            }
        }
    }
    private GridObject CreateCopyOfGridObject(GridObject objectInstance, bool poolCopy = false)
    {
        //создаем копию исходного объекта на сетке
        GridObject objectCopy;
        if (!poolCopy)
            objectCopy = Instantiate(objectInstance, objectInstance.transform.position, objectInstance.transform.rotation);
        else
        {
            //использование объекта с пула
            objectCopy = ObjectsPool.GetGridObject(objectInstance.ObjType);
            objectCopy.transform.position = objectInstance.transform.position;
            objectCopy.transform.rotation = objectInstance.transform.rotation;
        }
        objectCopy.transform.parent = objectInstance.transform;
        objectCopy.transform.localScale = Vector3.one;
        return objectCopy;
    }
    private void DetectObject()
    {
        if (CurrentInteractableObject != null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, maxDistance, gridObjectsLayerMask))
        {
            if(hit.collider.TryGetComponent(out GridObject obj))
            {
                if(obj.Interactable)
                {
                    currentObjectInstance = obj;
                    CurrentInteractableObject = CreateCopyOfGridObject(obj);
                }
            }
        }
    }
    private void PlaceCurrentObject()
    {
        if (currentObjectCopy == null)
            return;

        if(currentObjectCopy.TypeOfGridObject == GridObjectType.action)
        {
            //проверяем наличие объектов внутри текущего, если находим, то применяем свойство
            List<GridObject> _objects = currentObjectCopy.GetNearGridObjects(currentObjectCopy.transform.position);
            if (_objects.Count > 0 && _objects[0].IsInteractWithObject(currentObjectCopy.ObjType))
            {
                _objects[0].InteractWithObject(currentObjectCopy.ObjType);
                SoundController.PlayAudioClip("ObjectInteraction");
                //вызываем событие
                OnGridObjectPlaced?.Invoke(currentObjectCopy);
            }
        }
        else
        {
            //ставим объекты при условии, что есть свободное пространство
            if (currentObjectCopy.CheckEmptySpace())
            {
                if (currentObjectInstance != null)
                {
                    //установка сохраненного объекта в новое место
                    currentObjectInstance.PlaceOnTiles(currentObjectCopy.transform.position);
                }
                else
                {
                    //поиск последней активной платформы на сцене, чтобы сделать новый объект ее дочерним
                    Platform plat = PlatformGenerator.GetLastPlatform();
                    if (plat == null)
                        return;

                    //спавн копии текущего интерактивного объекта и применение ему свойств статического
                    currentObjectCopy.IsInterating = false;
                    GridObject newObject = CreateCopyOfGridObject(currentObjectCopy, true);
                    newObject.LockReplacing();
                    newObject.transform.parent = plat.transform;
                    //вызываем событие
                    OnNewGridObjectPlaced?.Invoke(newObject);

                }
                //вызываем событие
                OnGridObjectPlaced?.Invoke(currentObjectCopy);
            }
        }
        CurrentInteractableObject = null;
    }
    #endregion
    #region Updating functions
    private void UpdateTargetTile()
    {
        //относительно текущего ввода находим ближайшую клетку для перемещения
        if(CurrentInteractableObject == null)
            return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, gridLayerMask))
        {
            targetPosition = GetNearestTilePosition(hit.point, true);
        }
    }
    public Vector3 GetNearestToMousePosition()
    {
        Vector3 outPosition = Vector3.zero;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, gridLayerMask))
        {
            outPosition = GetNearestTilePosition(hit.point, true);
        }
        return outPosition;
    }
    private void MoveCurrentObject()
    {
        if (CurrentInteractableObject == null)
            return;

        //телепортация объекта к цели при большом расстоянии
        if (Vector3.Distance(CurrentInteractableObject.transform.position, targetPosition) > maxDistance)
            CurrentInteractableObject.transform.position = targetPosition;

        //плавно двигаем объект к цели
        CurrentInteractableObject.transform.position = Vector3.Lerp(CurrentInteractableObject.transform.position,
            targetPosition, smoothGridTranslationMultiplier * Time.deltaTime);

        //вертикальное движение (если включено)
        if (CurrentInteractableObject.OnlyVerticalMovement)
        {
            Vector3 localPosition = CurrentInteractableObject.transform.localPosition;
            localPosition.x = savedObjectLocalPosition.x;
            CurrentInteractableObject.transform.localPosition = localPosition;
        }

        //проверка на выходы за пределы окрестности для размещения
        float posX = CurrentInteractableObject.transform.position.x;
        if (posX > interactionEnd.position.x || posX < interactionBegin.position.x)
            CurrentInteractableObject = null;
    }
    private void SetCurrentObjectMaterial()
    {
        if (CurrentInteractableObject == null)
            return;
        
        //изменяем цвет объекта, проверяя на пустоту внутри него
        GridObjectSelection _material = GridObjectSelection.redMaterial;
        if (CurrentInteractableObject.TypeOfGridObject == GridObjectType.action)
        {
            List<GridObject> gridObjects = CurrentInteractableObject.GetNearGridObjects(CurrentInteractableObject.transform.position);
            if (gridObjects.Count > 0 && gridObjects[0].IsInteractWithObject(CurrentInteractableObject.ObjType))
                _material = GridObjectSelection.greenMaterial;
        }
        else
        {
            if(CurrentInteractableObject.CheckEmptySpace())
                _material = GridObjectSelection.greenMaterial;
        }       
        CurrentInteractableObject.SetMaterial(_material);
    }
    #endregion
    #region Interaction with other scripts
    //забираем из инвентаря предмет, если только что его использовали
    private void TakeItemFromInventory(GridObject gridObject)
    {
        if (gridObject == null)
            return;
        print(gridObject.ObjType);
        Inventory.RemoveObjectFromInventory(gridObject.ObjType);
    }
    #endregion
}
