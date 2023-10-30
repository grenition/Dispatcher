using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ObjectTypeEventHandler(ObjectType objType);

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
public class GridObject : MonoBehaviour
{
    public Transform Tr { get => tr; }
    public bool IsInterating { get; set; }
    public ObjectType ObjType { get => objectType; }
    public bool OnlyVerticalMovement { get => onlyVerticalMovement; }
    public bool DontDestroyOnPlatformSpawn { get => dontDestroyOnPlatformSpawn; }
    public bool Interactable { get => interactable; }
    public GridObjectType TypeOfGridObject { get => gridObjectType; }
    public event ObjectTypeEventHandler OnInteractionWithObject;

    [SerializeField] private GridObjectType gridObjectType = GridObjectType.physicalObject;
    [SerializeField] private bool interactable = true;
    [SerializeField] private bool correctByGridOnAwake = true;
    [SerializeField] private GridCorrection gridCorrection = GridCorrection.left;
    [SerializeField] private bool dontDestroyOnPlatformSpawn = false;
    [SerializeField] private bool onlyVerticalMovement = false;
    [SerializeField] private bool destroyAfterBadTranslation = true;
    [SerializeField] private bool isInventoryObject = false;
    [SerializeField] private ObjectType objectType = ObjectType.ramp;

    [SerializeField] private Vector3 objectSize;
    [SerializeField] private Vector3 objectCenter;

    [SerializeField] private Material greenSelectionMaterial;
    [SerializeField] private Material redSelectionMaterial;

    private Transform tr;
    private List<PlatformGridTile> occupedTiles = new List<PlatformGridTile>();
    [SerializeField] private List<MeshRenderComponents> meshRendererers = new List<MeshRenderComponents>();

    private void Awake()
    {
        tr = transform;

        foreach(var mesh in GetComponentsInChildren<Renderer>(true))
        {
            MeshRenderComponents comp = new MeshRenderComponents
            {
                renderer = mesh,
                startMaterial = mesh.material
            };
            meshRendererers.Add(comp);
        }

        CorrectByGrid();
    }
    private void OnEnable()
    {
        if (!IsInterating)
            PlaceOnTiles(transform.position);
    }
    public void CorrectByGrid()
    {
        if (gridObjectType == GridObjectType.action)
            return;

        float maxX = -999999f;
        PlatformGridTile maxTile = null;

        float minX = 999999f;
        PlatformGridTile minTile = null;

        foreach (var col in Physics.OverlapBox(tr.position + objectCenter, objectSize / 2))
        {
            if (col.TryGetComponent(out PlatformGridTile tile))
            {
                if(tile.transform.position.x > maxX)
                {
                    maxX = tile.transform.position.x;
                    maxTile = tile;
                }
                if(tile.transform.position.x < minX)
                {
                    minX = tile.transform.position.x;
                    minTile = tile;
                }
            }
        }

        if(maxTile != null && gridCorrection == GridCorrection.left)
        {
            tr.parent = maxTile.transform;
            tr.localPosition = Vector3.zero;
        }
        else if(minTile != null && gridCorrection == GridCorrection.right)
        {
            tr.parent = minTile.transform;
            tr.localPosition = Vector3.zero;
        }
    }
    public bool PlaceOnTiles(Vector3 newPosition)
    {
        if (gridObjectType == GridObjectType.action)
            return false;
        List<PlatformGridTile> tiles = new List<PlatformGridTile>();
        foreach(var col in Physics.OverlapBox(newPosition + objectCenter, objectSize / 2))
        {
            if(col.TryGetComponent(out PlatformGridTile tile))
            {
                if (tile.IsOccupied)
                    return false;
                tiles.Add(tile);
            }
        }
        ClearTiles();
        foreach (var tile in tiles)
            tile.CurrentObject = this;
        transform.position = newPosition;
        if (tiles.Count > 0)
            transform.parent = tiles[0].transform;
        occupedTiles = tiles;
        return true;
    }
    public void ClearTiles()
    {
        foreach(var tile in occupedTiles)
        {
            tile.CurrentObject = null;
        }
    }
    public bool CheckEmptySpace(Vector3 position)
    {
        foreach (var col in Physics.OverlapBox(position + objectCenter, objectSize / 2))
        {
            if (col.TryGetComponent(out PlatformGridTile tile))
            {
                if (tile.IsOccupied)
                    return false;
            }
        }
        return true;
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
                mat = greenSelectionMaterial;
                break;
            case GridObjectSelection.redMaterial:
                mat = redSelectionMaterial;
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
    public void DestroyMe(bool returnToInventory = true)
    {
        if (!destroyAfterBadTranslation && gridObjectType != GridObjectType.action)
            return;

        if (isInventoryObject && returnToInventory)
        {
            Inventory.AddObjectToInventory(objectType);
        }
        Destroy(gameObject);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawCube(objectCenter + transform.position, objectSize);
    }
    public void SetInventoryObject(bool active = true)
    {
        isInventoryObject = active;
        dontDestroyOnPlatformSpawn = !active;
        destroyAfterBadTranslation = active;
    }
    public void InteractWithObject(ObjectType _objectType)
    {
        OnInteractionWithObject?.Invoke(_objectType);
    }

    public bool IsIntertactWith(ObjectType _objectType)
    {
        if (TryGetComponent(out Train train))
        {
            return (train.InteractsWith == _objectType && !train.Interacted);
        }
        return false;
    }
}