using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    //public values
    public PlatformGenerationType PlatformType { get => platformType; }
    public bool IsPooling { get; set; }

    //parameters
    [Header("Platfrom parameters")]
    [SerializeField] private Transform begin;
    [SerializeField] private Transform end;
    [SerializeField] private PlatformGenerationType platformType = PlatformGenerationType.tonnels;

    //local values
    private Transform tr;

    //local functions
    private void Awake()
    {
        tr = transform;
    }
    private Vector3 CalculatePositionRelativeAnotherPlatform(Vector3 otherPlatformEnd)
    {
        Vector3 displacement = otherPlatformEnd - GetBeginPoint();
        return tr.position + displacement;
    }

    //public functions
    public Vector3 GetBeginPoint()
    {
        if (begin != null)
            return begin.position;
        return tr.position;
    }
    public Vector3 GetEndPoint()
    {
        if (end != null)
            return end.position;
        return tr.position;
    }
    public bool IsPlatformActive()
    {
        return gameObject.activeSelf;
    }
    public void EnablePlatform(Vector3 otherPlatformEnd)
    {
        gameObject.SetActive(true);
        tr.position = CalculatePositionRelativeAnotherPlatform(otherPlatformEnd);
    }
    public void DisablePlatform(bool destroy = true)
    {
        foreach(var obj in GetComponentsInChildren<GridObject>())
        {
            if (!obj.DontDestroyOnPlatformSpawn)
                obj.DestroyObject();
        }

        if (!IsPooling && destroy)
        {
            Destroy(gameObject);
            return;
        }

        gameObject.SetActive(false);
        tr.parent = PlatformsPool.PlatformsParent;
    }
    public void Move(Vector3 direction)
    {
        tr.Translate(direction);
    }
}
