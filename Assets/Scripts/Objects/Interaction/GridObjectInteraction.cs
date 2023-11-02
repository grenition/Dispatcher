using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObjectInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected ObjectType interactsWith = ObjectType.hammer;

    protected bool interacted = false;

    public bool IsInteractsWith(ObjectType interactor)
    {
        return interactor == interactsWith && !interacted;
    }
    public virtual void Interact(ObjectType interactor)
    {

    }
}
