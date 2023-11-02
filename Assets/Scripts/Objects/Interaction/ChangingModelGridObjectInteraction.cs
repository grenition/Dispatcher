using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingModelGridObjectInteraction : GridObjectInteraction
{
    [SerializeField] private GameObject startModel;
    [SerializeField] private GameObject endModel;

    private void Awake()
    {
        if (endModel != null)
            endModel.SetActive(false);
    }

    public override void Interact(ObjectType interactor)
    {
        if (!IsInteractsWith(interactor) || startModel == null || endModel == null)
            return;
        startModel.SetActive(false);
        endModel.SetActive(true);

        interacted = true;
    }
}
