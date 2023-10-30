using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInteractions : MonoBehaviour
{
    public static GridInteractions Instance { get; private set; }
    public GridObject CurrentInteractableObject
    {
        get
        {
            return currentObject;
        }
        set
        {
            if(currentObject != null)
            {
                currentObject.SetMaterial(GridObjectSelection.defaultMaterial);
                currentObject.IsInterating = false;
                currentObject.SetInventoryObject(false);
            }

            if (value != null && !value.Interactable)
                return;

            currentObject = value;

            if (currentObject != null)
            {
                savedObjectParent = currentObject.transform.parent;
                savedObjectLocalPosition = currentObject.transform.localPosition;

                currentObject.IsInterating = true;

                float median = ((interactionBegin.position + interactionEnd.position) / 2f).x;

                currentObject.transform.position = new Vector3(median, -99999f, 99999f);
                targetPosition = new Vector3(median, -99999f, 99999f);

                currentObject.ClearTiles();
            }
        }
    }
    public Transform InteractionEnd { get => interactionEnd; }
    public Transform InteractionBegin { get => interactionBegin; }

    //parameters
    [SerializeField] private LayerMask gridLayerMask;
    [SerializeField] private LayerMask gridObjectsLayerMask;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private float smoothGridTranslationMultiplier = 10f;
    [SerializeField] private Transform interactionBegin;
    [SerializeField] private Transform interactionEnd;

    //local values
    private GridObject currentObject;
    private PlatformGridTile tileForCurrentObject;

    private Camera mainCamera;
    private GameObject lastObject;
    private Vector3 targetPosition;

    private Vector3 savedObjectLocalPosition;
    private Transform savedObjectParent;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        mainCamera = Camera.main;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            DetectObject();

        if (!Input.GetKey(KeyCode.Mouse0) && CurrentInteractableObject != null)
        {
            PlaceCurrentObject();
        }
        SelectTileForCurrentObject();
        MoveCurrentObject();
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
                    CurrentInteractableObject = obj;
                }
            }
        }
    }
    private void PlaceCurrentObject()
    {
        if (currentObject == null)
            return;

        if(currentObject.TypeOfGridObject == GridObjectType.action)
        {
            bool returnToInventory = true;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, gridObjectsLayerMask))
            {
                if (hit.collider.TryGetComponent(out GridObject obj))
                {
                    SoundController.PlatAudioClip("ObjectInteraction");
                    obj.InteractWithObject(currentObject.ObjType);
                    returnToInventory = false;
                }
            }
            StopObject(true, true, returnToInventory);
        }

        if(tileForCurrentObject == null)
        {
            StopObject(true, true);
            return;
        }

        Vector3 position = tileForCurrentObject.Tr.position;
        if (currentObject.OnlyVerticalMovement)
            position = currentObject.Tr.position;

        if (currentObject.PlaceOnTiles(position))
        {
            SoundController.PlatAudioClip("PlacingObject");
            StopObject(false, false);
        }
        else
        {
            StopObject(true, true);
        }
    }
    private void StopObject(bool destroy = false, bool applySavedTransforms = false, bool returnToInventory = true)
    {
        if (currentObject == null)
            return;

        if (applySavedTransforms)
        {
            currentObject.transform.localPosition = savedObjectLocalPosition;
            currentObject.transform.parent = savedObjectParent;
        }

        if(destroy || currentObject.TypeOfGridObject == GridObjectType.action)
            currentObject.DestroyMe(returnToInventory);
        CurrentInteractableObject = null;
    }
    private void SelectTileForCurrentObject()
    {
        if(CurrentInteractableObject == null)
        {
            tileForCurrentObject = null;
            return;
        }
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, gridLayerMask))
        {
            if (CurrentInteractableObject.TypeOfGridObject == GridObjectType.action)
            {
                targetPosition = hit.point;
                if(hit.collider.gameObject != lastObject)
                {
                    GridObjectSelection selection = GridObjectSelection.redMaterial;

                    if(hit.collider.TryGetComponent(out PlatformGridTile tile))
                    {
                        if(tile.CurrentObject != null && tile.CurrentObject.ObjType == ObjectType.train 
                            && tile.CurrentObject.IsIntertactWith(CurrentInteractableObject.ObjType))
                        {
                            selection = GridObjectSelection.greenMaterial;
                        }
                    }

                    currentObject.SetMaterial(selection);
                }
            }
            else
            {

                if (hit.collider.gameObject == lastObject)
                {
                    if (tileForCurrentObject == null)
                    {
                        targetPosition = hit.point;
                        CurrentInteractableObject.SetMaterial(GridObjectSelection.redMaterial);
                    }
                    else
                    {
                        targetPosition = tileForCurrentObject.Tr.position;
                    }
                    return;
                }

                if (hit.collider.TryGetComponent(out PlatformGridTile tile))
                {
                    if (CurrentInteractableObject.CheckEmptySpace(tile.Tr.position))
                    {
                        CurrentInteractableObject.SetMaterial(GridObjectSelection.greenMaterial);
                        targetPosition = tile.Tr.position;
                        tileForCurrentObject = tile;
                    }
                    else
                        tileForCurrentObject = null;
                }
                else
                {
                    tileForCurrentObject = null;
                }
            }
            lastObject = hit.collider.gameObject;
        }
    }
    private void MoveCurrentObject()
    {
        if (CurrentInteractableObject == null)
            return;

        if (Vector3.Distance(CurrentInteractableObject.Tr.position, targetPosition) > maxDistance)
            CurrentInteractableObject.Tr.position = targetPosition;

        CurrentInteractableObject.Tr.position = Vector3.Lerp(CurrentInteractableObject.Tr.position,
            targetPosition, smoothGridTranslationMultiplier * Time.deltaTime);

        if (CurrentInteractableObject.OnlyVerticalMovement)
        {
            Vector3 localPosition = CurrentInteractableObject.Tr.localPosition;
            localPosition.x = savedObjectLocalPosition.x;
            CurrentInteractableObject.Tr.localPosition = localPosition;
        }

        float posX = CurrentInteractableObject.Tr.position.x;
        if (posX > interactionEnd.position.x || posX < interactionBegin.position.x)
            StopObject(true, true);
    }
}
