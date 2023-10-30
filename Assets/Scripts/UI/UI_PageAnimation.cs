using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PageAnimation : MonoBehaviour
{
    [SerializeField] private float delay = 0f;
    [SerializeField] private float animationTime = 0.5f;

    private Transform tr;
    private Vector3 savedPosition = Vector3.zero;
    private Vector2 savedResolution = Vector2.zero;

    bool isFirstStart = true;

    private void OnEnable()
    {
        tr = transform;
        savedPosition = tr.position;
        if (isFirstStart)
        {
            savedResolution = new Vector2(Screen.width, Screen.height);
            isFirstStart = false;
        }
        tr.position = savedPosition + tr.right * savedResolution.x;
        StartCoroutine(TransitionCoroutine(savedPosition));
    }
    private void OnDisable()
    {
        tr.position = savedPosition;
    }
    public void CloseWithAnimation()
    {
        if (!gameObject.activeSelf)
            return;

        StopAllCoroutines();
        Vector3 targetPosition = savedPosition - tr.right * Screen.width;
        StartCoroutine(TransitionCoroutine(targetPosition, true));
    }

    private IEnumerator TransitionCoroutine(Vector3 targetPosition, bool disableOnEnd = false)
    {
        yield return new WaitForSeconds(delay);

        float t = 0f;
        float startTime = Time.time;
        Vector3 startPosition = tr.position;
        while (t < 1f)
        {
            t = (Time.time - startTime) / animationTime;
            tr.position = Vector3.Lerp(tr.position, targetPosition, t);
            yield return null;
        }

        if (disableOnEnd)
            gameObject.SetActive(false);
    }
}
