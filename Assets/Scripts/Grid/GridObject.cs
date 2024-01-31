using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void ObjectTypeEventHandler(ObjectType objType);
public enum ObjectType
{
    hammer,
    key,
    ramp,
    spring,
    train,
    coin
}

public enum GridObjectSelection
{
    defaultMaterial,
    greenMaterial,
    redMaterial
}
public enum GridCorrection
{
    right,
    left
}
public enum GridObjectType
{
    physicalObject,
    action
}
public class MeshRenderComponents
{
    public Renderer renderer;
    public Material startMaterial;
}
public struct GridObjectBuffer
{
    public bool interactable;
    public Vector3 position;
    public Vector3 localScale;
    public Transform parent;
}
[RequireComponent(typeof(BoxCollider))]
public class GridObject : MonoBehaviour
{
    #region public values
    public bool IsInterating 
    { 
        get => isInteracting; 
        set
        {
            foreach (var col in colliders)
                col.enabled = !value;
            if (!value)
                SetMaterial(GridObjectSelection.defaultMaterial);
            isInteracting = value;
        } 
    }
    public ObjectType ObjType { get => objectType; }
    public bool OnlyVerticalMovement { get => onlyVerticalMovement; }
    public bool CanBePlacedOnOtherObjects { get => canBePlacedOnOtherObjects; }
    public bool DontDestroyOnPlatformSpawn { get => dontDestroyOnPlatformSpawn; }
    public bool Interactable { get => interactable; }
    public GridObjectType TypeOfGridObject { get => gridObjectType; }
    public bool IsPooled { get => isPooled; set { isPooled = value; } }
    public float VericalOffset { get => verticalOffset; }
    public bool CanPlaceObjectsOnRoof { get => canPlaceOtherObjectsOnRoof; }
    public bool OneInRow { get => oneInRow; }
    public List<GridObjectInteraction> Interactions { get => interactions; }
    public float DestroyBlockTime { get; private set; }
    public int CurrentFloor { get => currentFloor; }
    public bool PlaceOnAwake { get => placeOnAwake; set { placeOnAwake = value; } }
    public bool CorrectByGridWhileMoves { get => correctByGridWhileMoves; }
    public bool FastPlacing { get => fastPlacing; }
    public Transform FastPlace { get => fastPlace; }
    public Action<Vector3> OnOtherObjectInRowDeleted;

    #endregion

    #region parameters
    [SerializeField] private GridObjectType gridObjectType = GridObjectType.physicalObject;
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool correctByGridOnAwake = true;
    [SerializeField] private bool dontDestroyOnPlatformSpawn = false;
    [SerializeField] private bool onlyVerticalMovement = false;
    [SerializeField] private bool canBePlacedOnOtherObjects = false;
    [SerializeField] private bool canPlaceOtherObjectsOnRoof = true;
    [SerializeField] private bool oneInRow = false;
    [SerializeField] private bool destroyOtherObjectsInRow = false;
    [SerializeField] private bool correctByGridWhileMoves = true;
    [SerializeField] private bool fastPlacing = false;
    [SerializeField] private Transform fastPlace;
    [SerializeField] private float destroyByOtherObjectInRowThreshold = 0.5f;
    [SerializeField] private ObjectType objectType = ObjectType.ramp;
    [SerializeField] private Vector3 objectSize;
    [SerializeField] private Vector3 objectCenter;
    [SerializeField] private SelectionMaterialsPreset materialsPreset;
    [SerializeField] private float verticalOffset = 0f;
    #endregion

    #region local values
    private Transform tr;
    private List<MeshRenderComponents> meshRendererers = new List<MeshRenderComponents>();
    private List<Collider> colliders = new List<Collider>();
    private List<GridObjectInteraction> interactions = new List<GridObjectInteraction>();
    private bool isInteracting = false;
    private GridObjectBuffer savedParameters;
    private bool isPooled = false;
    private int currentFloor;
    private bool placeOnAwake = true;
    private Vector3 boxScale = Vector3.one;
    private Vector3 boxOffset = Vector3.zero;
    [SerializeField] private BoxCollider boxCollider;
    #endregion

#if UNITY_EDITOR
    public bool visualizeGrid = true;
    private void OnDrawGizmos()
    {
        if (visualizeGrid)
        {
            Vector3 pos = transform.position;
            for (int y = -1; y < 2; y++)
            {
                for (int x = -10; x < 10; x++)
                {
                    Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
                    Vector3 _center = new Vector3(-x * 3, -2f, -y * 3) + pos;
                    Gizmos.DrawCube(_center, new Vector3(3, 1f, 3));
                }
            }
        }
        if (Application.isPlaying || !transform.hasChanged)
            return;
        Vector3 position = transform.position;
        transform.position = GridInteractions.ConvertToGridPosition(position);
        transform.hasChanged = false;

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(3f, 2f, 2f));
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);

        if (boxCollider != null)
        {
            Vector3 scale = new Vector3
            {
                x = transform.lossyScale.x * boxCollider.size.x,
                y = transform.lossyScale.y * boxCollider.size.y,
                z = transform.lossyScale.z * boxCollider.size.z
            };
            scale = transform.TransformDirection(scale);
            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Abs(scale.y);
            scale.z = Mathf.Abs(scale.z);

            Vector3 offsetPosition = new Vector3
            {
                x = transform.lossyScale.x * boxCollider.center.x,
                y = transform.lossyScale.y * boxCollider.center.y,
                z = transform.lossyScale.z * boxCollider.center.z
            };
            Gizmos.DrawCube(transform.position + transform.TransformDirection(offsetPosition), scale);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position - Vector3.up * verticalOffset, 0.1f);
    }
#endif
    private void Awake()
    {
        tr = transform;
        boxCollider = GetComponent<BoxCollider>();
        SaveParameters();

        #region Getting components
        foreach (var mesh in GetComponentsInChildren<Renderer>(true))
        {
            MeshRenderComponents comp = new MeshRenderComponents
            {
                renderer = mesh,
                startMaterial = mesh.material
            };
            meshRendererers.Add(comp);
        }
        foreach(var col in GetComponentsInChildren<Collider>(true))
        {
            colliders.Add(col);
        }
        foreach(var _inter in GetComponentsInChildren<GridObjectInteraction>(true))
        {
            interactions.Add(_inter);
        }
        #endregion
    }
    private void Start()
    {
        if(placeOnAwake)
            PlaceOnTiles(tr.position, !canBePlacedOnOtherObjects);
        DestroyBlockTime = Time.time + destroyByOtherObjectInRowThreshold;

        #region Setting values
        boxScale = new Vector3
        {
            x = transform.lossyScale.x * boxCollider.size.x,
            y = transform.lossyScale.y * boxCollider.size.y,
            z = transform.lossyScale.z * boxCollider.size.z
        };
        boxScale = transform.TransformDirection(boxScale);
        boxScale.x = Mathf.Abs(boxScale.x);
        boxScale.y = Mathf.Abs(boxScale.y);
        boxScale.z = Mathf.Abs(boxScale.z);

        boxOffset = new Vector3
        {
            x = transform.lossyScale.x * boxCollider.center.x,
            y = transform.lossyScale.y * boxCollider.center.y,
            z = transform.lossyScale.z * boxCollider.center.z
        };
        boxOffset = transform.TransformDirection(boxOffset);
        #endregion
    }
    private void OnEnable()
    {
        tr = transform;

        //if (!IsInterating && GridInteractions.Instance != null)
        //    PlaceOnTiles(transform.position);
    }
    private void OnDisable()
    {
        isInteracting = false;
    }
    #region local functions
    private void SaveParameters()
    {
        savedParameters.interactable = interactable;
        savedParameters.position = transform.position;
        savedParameters.parent = transform.parent;
        savedParameters.localScale = transform.localScale;
    }
    #endregion

    #region cheking objects in local region
    private Collider[] OverlapBox(Vector3 _position)
    {
        return Physics.OverlapBox(_position + boxOffset, boxScale / 2f);
    }
    //возвращает, находящиеся в объекте объекты типа GridObject
    public List<GridObject> GetNearGridObjects(Vector3 position)
    {
        List<GridObject> list = new List<GridObject>();

        foreach (var col in OverlapBox(position))
        {
            if (col.TryGetComponent(out GridObject obj))
            {
                if (obj.ObjType == ObjectType.coin && ObjType != ObjectType.coin)
                    continue;
                list.Add(obj);
            }
        }
        return list;
    }
    public List<GridObject> GetNearGridObjects(Vector3 position, out bool isContainsPlayer)
    {
        isContainsPlayer = false;
        List<GridObject> list = new List<GridObject>();
        foreach (var col in OverlapBox(position))
        {
            if (col.gameObject == gameObject)
                continue;
            if (col.GetComponent<PlayerController>())
                isContainsPlayer = true;
            if (col.TryGetComponent(out GridObject obj))
            {
                if (obj.ObjType == ObjectType.coin && ObjType != ObjectType.coin)
                    continue;
                list.Add(obj);
            }
        }
        return list;
    }
    public bool CheckEmptySpace(Vector3 position)
    {
        bool _emptySpace = GetNearGridObjects(position, out bool _isContainsPlayer).Count == 0;
        bool _rowIsAvailable = true;
        if (OneInRow)
        {
            List<GridObject> objectsInRow = GetObjectsInRow(position);
            if(!destroyOtherObjectsInRow && objectsInRow.Count != 0)
                _rowIsAvailable = false;
            else if (destroyOtherObjectsInRow)
            {
                foreach(var _obj in objectsInRow)
                    if(Time.time < _obj.DestroyBlockTime)
                    {
                        _rowIsAvailable = false;
                        break;
                    }
            }
        }
        return _emptySpace && _rowIsAvailable && !_isContainsPlayer;
    }
    public bool CheckEmptySpace()
    {
        return CheckEmptySpace(tr.position);
    }
    public List<GridObject> GetObjectsInRow(Vector3 position, bool ignoreInstance = true)
    {
        position = GridInteractions.Instance.GetNearestTilePosition(position);
        List<GridObject> _objects = new List<GridObject>();
        Collider[] colls = Physics.OverlapBox(position, new Vector3(GridInteractions.tileSize.x / 2f, 100f, 100f), Quaternion.identity);
        foreach (var col in colls)
        {
            if (col.TryGetComponent(out GridObject newObj))
            {
                if (ignoreInstance && col.gameObject == gameObject)
                    continue;
                _objects.Add(newObj);
            }
        }
        return _objects;
    }
    public List<GridObject> GetObjectsInRow(Vector3 position, ObjectType objectsOfType, bool ignoreInstance = true)
    {
        position = GridInteractions.Instance.GetNearestTilePosition(position);
        List<GridObject> _objects = new List<GridObject>();
        Collider[] colls = Physics.OverlapBox(position, new Vector3(GridInteractions.tileSize.x / 2f, 100f, 100f), Quaternion.identity);
        foreach (var col in colls)
        {
            if (col.TryGetComponent(out GridObject newObj))
            {
                if (ignoreInstance && col.gameObject == gameObject)
                    continue;
                if (newObj.ObjType == objectsOfType)
                    _objects.Add(newObj);
            }
        }
        return _objects;
    }
    public bool IsOneInRow(Vector3 position)
    {
        return GetObjectsInRow(position, ObjectType.coin).Count == 0;
    }
    #endregion
    #region global interactions
    //устанавливает объект в центр ближайшейшего тайла сетки
    public void PlaceOnTiles(Vector3 newPosition, bool applyOffset = false)
    {
        Vector3 position = GridInteractions.Instance.GetNearestTilePosition(newPosition);
        if (applyOffset && GridInteractions.Instance && GridInteractions.Instance.StandForGridObjects) 
        {
            position.y = verticalOffset + GridInteractions.Instance.StandForGridObjects.position.y; 
        }
        tr.position = position;

        if(OneInRow && destroyOtherObjectsInRow)
        {
            bool otherObjectDeleted = false;
            Vector3 otherObjectPosition = Vector3.zero;
            foreach (var _obj in GetObjectsInRow(position, ObjType))
            {
                otherObjectDeleted = true;
                otherObjectPosition = _obj.transform.position;
                _obj.DestroyObject();
            }
            if (otherObjectDeleted)
                OnOtherObjectInRowDeleted?.Invoke(otherObjectPosition);
        }

        currentFloor = GridInteractions.Instance.GetFloorId(transform.position - Vector3.up * verticalOffset);
    }
    public void SetMaterial(GridObjectSelection selection)
    {
        Material mat = null;
        switch (selection)
        {
            case GridObjectSelection.defaultMaterial:
                mat = null;
                break;
            case GridObjectSelection.greenMaterial:
                mat = materialsPreset.greenMaterial;
                break;
            case GridObjectSelection.redMaterial:
                mat = materialsPreset.redMaterial;
                break;
        }
        foreach(var mesh in meshRendererers)
        {
            if (mat == null)
                mesh.renderer.material = mesh.startMaterial;
            else
                mesh.renderer.material = mat;
        }
    }
    public void InteractWithObject(GridObject gridObject)
    {
        if (gridObject == null)
            return;
        foreach (var _inter in interactions)
        {
            _inter.Interact(gridObject.ObjType);
        }
    }
    public bool IsInteractsWithObject(GridObject gridObject)
    {
        if (gridObject == null)
            return false;
        foreach (var _inter in interactions)
        {
            if (_inter.IsInteractsWith(gridObject.ObjType))
                return true;
        }
        return false;
    }
    public void LockReplacing(bool lockState = true)
    {
        interactable = !lockState;
    }
    public void DestroyObject()
    {
        if (IsPooled)
        {
            ObjectsPool.StopGridObject(this);
        }
        else
            Destroy(gameObject);
    }
    public void ResetObject()
    {
        gameObject.SetActive(false);
        IsInterating = false;
        interactable = savedParameters.interactable;
        transform.parent = savedParameters.parent;
        transform.position = savedParameters.position;
        transform.localScale = savedParameters.localScale;
        gameObject.SetActive(true);
    }
    #endregion
}