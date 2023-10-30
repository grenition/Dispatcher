using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMCD : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private Transform start;
    [SerializeField] private Transform end;

    [SerializeField] private Transform train;

    private void Start()
    {
        train.position = start.position;
    }
    private void Update()
    {
        if(train.position.x > GridInteractions.Instance.InteractionBegin.position.x)
        {
            if(Vector3.Distance(train.position, end.position) > 0.5f)
            {
                train.Translate((end.position - train.position).normalized * speed * Time.deltaTime);
            }
        }
    }
}
