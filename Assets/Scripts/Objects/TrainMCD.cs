using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMCD : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private Vector3 targetLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 cornerOffset = Vector3.zero;

    private Vector3 localCoordinates = Vector3.zero;

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.DrawSphere(transform.position + targetLocalPosition, 0.2f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + cornerOffset, 0.2f);
        }
    }
    private void Update()
    {
        if((transform.position + cornerOffset).x > GridInteractions.Instance.InteractionBegin.position.x)
        {
            if(Vector3.Distance(localCoordinates, targetLocalPosition) > 0.2f)
            {
                Vector3 direction = (targetLocalPosition - localCoordinates).normalized * speed * Time.deltaTime;
                localCoordinates += direction;
                transform.Translate(direction);
            }
        }
    }
}
