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
    public bool DontDestroyOnPlatformSpawn { get => dontDestroyOnPlatformSpawn; }
    public bool Interactable { get => interactable; }
    public GridObjectType TypeOfGridObject { get => gridObjectType; }
    public bool IsPooled { get; set; }

    #endregion

    #region parameters
    [SerializeField] private GridObjectType gridObjectType = GridObjectType.physicalObject;
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool correctByGridOnAwake = true;
    [SerializeField] private GridCorrection gridCorrection = GridCorrection.left;
    [SerializeField] private bool dontDestroyOnPlatformSpawn = false;
    [SerializeField] private bool onlyVerticalMovement = false;
    [SerializeField] private ObjectType objectType = ObjectType.ramp;
    [SerializeField] private Vector3 objectSize;
    [SerializeField] private Vector3 objectCenter;
    [SerializeField] private SelectionMaterialsPreset materialsPreset;
    [SerializeField][Range(0.5f, 1.5f)] private float selectionScaleMultiplier = 0.9f;
    #endregion

    #region local values
    private Transform tr;
    private List<MeshRenderComponents> meshRendererers = new List<MeshRenderComponents>();
    private List<Collider> colliders = new List<Collider>();
    private List<GridObjectInteraction> interactions = new List<GridObjectInteraction>();
    private bool isInteracting = false;
    private GridObjectBuffer savedParameters;
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(3f, 2f, 2f));
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawCube(objectCenter + transform.position, objectSize);
    }

    private void Awake()
    {
        tr = transform;
        SaveParameters();

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
    }
    private void Start()
    {
        PlaceOnTiles(tr.position);
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            PlaceOnTiles(tr.position);
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
    //возвращает, находящиеся в объекте объекты типа GridObject
    public List<GridObject> GetNearGridObjects(Vector3 position)
    {
        List<GridObject> list = new List<GridObject>();
        foreach (var col in Physics.OverlapBox(position + objectCenter, objectSize / 2))
        {
            if(col.TryGetComponent(out GridObject obj))
                list.Add(obj);
        }
        return list;

    }
    public bool CheckEmptySpace(Vector3 position)
    {
        return GetNearGridObjects(position).Count == 0;
    }
    public bool CheckEmptySpace()
    {
        return GetNearGridObjects(tr.position).Count == 0;
    }
    #endregion
    #region global interactions
    //устанавливает объект в центр ближайшейшего тайла сетки
    public void PlaceOnTiles(Vector3 newPosition)
    {
        tr.position = GridInteractions.Instance.GetNearestTilePosition(newPosition);
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
    public void InteractWithObject(ObjectType _interactor)
    {
        foreach(var _inter in interactions)
        {
            _inter.Interact(_interactor);
        }
    }
    public bool IsInteractWithObject(ObjectType _interactor)
    {
        foreach (var _inter in interactions)
        {
            if (_inter.IsInteractsWith(_interactor))
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