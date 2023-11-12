using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectsPoolUnit
{
    public ObjectType type = ObjectType.ramp;
    public GridObject prefab;
    public int count = 10;

    public List<GridObject> bufferedObjects = new List<GridObject>();
    public GridObject GetGridObject()
    {
        if (prefab == null && bufferedObjects.Count == 0)
            return null;
        if (bufferedObjects.Count == 0)
        {
            GridObject obj = ObjectsPool.SpawnAndSetupGridObject(prefab, true);
            bufferedObjects.Add(obj);
            obj.IsPooled = true;
            return obj;
        }
        for(int i = bufferedObjects.Count - 1; i >= 0; i--)
        {
            if (!bufferedObjects[i].gameObject.activeSelf)
            {
                GridObject obj = bufferedObjects[i];
                obj.gameObject.SetActive(true);
                bufferedObjects.CycleMoveDown();
                obj.ResetObject();
                return obj;
            }
        }
        GridObject _obj = bufferedObjects[bufferedObjects.Count - 1];
        _obj.gameObject.SetActive(true);
        _obj.ResetObject();
        bufferedObjects.CycleMoveDown();
        return _obj;
    }
}
public class ObjectsPool : MonoBehaviour
{
    //public values
    public static ObjectsPool Instance
    {
        get
        {
            if(instance == null)
            {
                return FindObjectOfType<ObjectsPool>();
            }
            return instance;
        }
    }
    private static ObjectsPool instance;
    public bool IsInitialized { get; private set; }
    public static event GridObjectEventHandler OnPooledObjectGetted;

    //parameters
    [SerializeField] private ObjectsPoolUnit[] objectUnits;

    #region base
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(this);
            return;
        }
        InitializePools();
    }
    private void InitializePools()
    {
        foreach(var unit in objectUnits)
        {
            //clearing unit
            foreach (var obj in unit.bufferedObjects)
                Destroy(obj.gameObject);
            unit.bufferedObjects.Clear();

            if (unit.prefab == null)
                continue;

            //spawning objects
            for(int i = 0; i < unit.count; i++)
            {
                GridObject obj = SpawnAndSetupGridObject(unit.prefab);
                unit.bufferedObjects.Add(obj);
                obj.IsPooled = true;
            }
        }
    }
    #endregion
    #region global interactions
    public static GridObject GetNewGridObject(ObjectType type)
    {
        if (Instance == null)
            return null;
        foreach (var unit in instance.objectUnits)
        {
            if (unit.type == type && unit.prefab != null)
                return Instantiate(unit.prefab);
        }
        return null;
    }
    public static GridObject GetGridObject(ObjectType type)
    {
        if (Instance == null)
            return null;
        foreach(var unit in instance.objectUnits)
        {
            if (unit.type != type)
                continue;
            GridObject obj = unit.GetGridObject();
            if (obj != null)
                OnPooledObjectGetted?.Invoke(obj);
            return obj;
        }
        return null;
    }
    public static void StopGridObject(GridObject obj)
    {
        obj.gameObject.SetActive(false);
        if (Instance != null)
            obj.transform.parent = Instance.transform;
    }
    public static GridObject SpawnAndSetupGridObject(GridObject prefab, bool activeState = false)
    {
        GridObject obj = Instantiate(prefab);
        obj.gameObject.SetActive(activeState);
        if (Instance != null)
            obj.transform.parent = Instance.transform;
        return obj;
    }
    public static List<GridObject> GetActiveGridObjectOfType(ObjectType type)
    {
        List<GridObject> objects = new List<GridObject>();
        if (Instance == null)
            return objects;
        foreach(var unit in instance.objectUnits)
        {
            if (unit.type != type)
                continue;
            foreach (var obj in unit.bufferedObjects)
                if (obj.gameObject.activeSelf)
                    objects.Add(obj);
        }
        return objects;
    }
    #endregion
}
