using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    [SerializeField] private GridObject gridObjectBase;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform coinModel;
    [SerializeField] private float translationFromOtherRowTime = 0.5f;
    [SerializeField] private string exitTransitionTriggerName = "Exit";
    [SerializeField] private string idleStateName = "Idle";
    [SerializeField] private float lifetimeAfterTaking = 1f;
    private void OnEnable()
    {
        if (gridObjectBase == null)
            enabled = false;
        else
            gridObjectBase.OnOtherObjectInRowDeleted += OnOtherObjectInRowDeleted;
    }
    private void OnDisable()
    {
        if (gridObjectBase != null)
            gridObjectBase.OnOtherObjectInRowDeleted -= OnOtherObjectInRowDeleted;
    }
    private void OnOtherObjectInRowDeleted(Vector3 objPosition)
    {
        animator.CrossFade(idleStateName, 0f);
        StartCoroutine(TranslationEnumerator(objPosition, transform.position, translationFromOtherRowTime));
    }
    private IEnumerator TranslationEnumerator(Vector3 fromPosition, Vector3 toPosition, float time)
    {
        float startTime = Time.time;
        float t = 0f;
        while (t < 1f)
        {
            t = (Time.time - startTime) / time;
            Vector3 position = coinModel.position;
            position.z = Mathf.Lerp(fromPosition.z, toPosition.z, t);
            coinModel.position = position;
            yield return null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out PlayerController player))
        {
            StartCoroutine(TakingCoinEnumarator());
        }
    }
    private IEnumerator TakingCoinEnumarator()
    {
        if (animator != null)
            animator.SetTrigger(exitTransitionTriggerName);
        yield return new WaitForSeconds(lifetimeAfterTaking);
        ObjectsPool.StopGridObject(gridObjectBase);
    }
}
