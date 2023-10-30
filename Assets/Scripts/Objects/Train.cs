using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(GridObject))]
public class Train : MonoBehaviour
{
    public bool Interacted { get; set; }
    public ObjectType InteractsWith { get => interactsWith; }

    [SerializeField] private ObjectType interactsWith;

    [Header("changing models")]
    [SerializeField] private GameObject defaultModel;
    [SerializeField] private GameObject changedModel;

    private GridObject gridObject;
    private void Awake()
    {
        changedModel.SetActive(false);
        defaultModel.SetActive(true);
    }
    private void OnEnable()
    {
        gridObject = GetComponent<GridObject>();
        gridObject.OnInteractionWithObject += Interact;
    }
    private void OnDisable()
    {
        gridObject.OnInteractionWithObject -= Interact;
    }
    private void Interact(ObjectType objectType)
    {
        if (objectType != interactsWith || Interacted)
            return;

        defaultModel.SetActive(false);
        changedModel.SetActive(true);
        Interacted = true;

    }
}
