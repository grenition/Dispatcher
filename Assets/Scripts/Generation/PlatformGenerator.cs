using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformGenerationType
{
    tonnels,
    level
}
public class PlatformGenerator : MonoBehaviour
{
    //public values
    public static PlatformGenerator instance;

    public static PlatformGenerationType GenerationType
    {
        get
        {
            if (instance != null)
                return instance.generationType;
            return PlatformGenerationType.tonnels;
        }
        set
        {
            if (instance == null)
                return;
            instance.generationType = value;
        }
    }
    public static int CurrentLevelId { get; set; }
    public Transform EndArea { get => endArea; }
    public Transform BeginArea { get => beginArea; }

    //parameters
    [SerializeField] private PlatformGenerationType generationType = PlatformGenerationType.tonnels;
    [SerializeField] private float movingSpeedMultiplier = 1f;
    [SerializeField] private Transform platformsParent;
    [SerializeField] private int platformsCount = 10;
    [SerializeField] private Transform endArea;
    [SerializeField] private Transform beginArea;

    //local values
    public List<Platform> platforms = new List<Platform>();
    private Vector3 movementDirection = Vector3.right;

    //local functions
    private void OnEnable()
    {
        if (platformsParent == null || endArea == null || beginArea == null)
        {
            Debug.LogWarning("Component's on Platform Generator are not asigned");
            enabled = false;
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        MovePlatforms();
        CheckPlatforms();
        SetupPlatforms();
    }
    private void MovePlatforms()
    {
        foreach(var plat in platforms)
        {
            float mult = 1f;
            if (GameController.Instance != null)
                mult = GameController.Instance.PlatformsSpeed;
            if(plat != null)
                plat.Move(movementDirection * movingSpeedMultiplier * mult * Time.deltaTime);
        }
    }
    private void CheckPlatforms()
    {
        if (platforms.Count == 0)
            return;

        while (true)
        {
            bool flag = true;
            foreach(var pl in platforms)
            {
                if (pl.GetEndPoint().x > endArea.position.x)
                {
                    RemovePlatform(pl);
                    flag = false;
                    break;
                }
            }
            if (flag)
                break;

        }

        for (int i = platforms.Count - 1; i > 0; i--)
        {
            if(platforms[i].GetEndPoint().x > endArea.position.x)
            {
                for (int j = 0; j < platforms.Count - i; i++)
                {
                    RemoveFirstPlatform();
                }
            }
        }
    }
    private void SetupPlatforms()
    {
        while(platforms.Count < platformsCount)
        {
            if (platforms.Count > 0 && platforms[platforms.Count - 1].GetEndPoint().x < beginArea.position.x)
                return;

            Platform plat = GetNextPlatform();
            if (plat == null || platforms.Contains(plat))
            {
                return;
            }


            Vector3 otherPlatformEnd = platformsParent.position;
            if (platforms.Count > 0)
                otherPlatformEnd = platforms[platforms.Count - 1].GetEndPoint();
            plat.EnablePlatform(otherPlatformEnd);

            plat.transform.parent = platformsParent;
            plat.transform.SetAsLastSibling();
            platforms.Add(plat);
        }
    }
    private void RemoveFirstPlatform()
    {
        if (platforms.Count == 0)
            return;
        platforms[0].DisablePlatform();
        platforms.RemoveAt(0);
    }
    private void RemovePlatform(Platform pl)
    {
        pl.DisablePlatform();
        platforms.Remove(pl);
    }
    private Platform GetNextPlatform()
    {
        if (generationType != PlatformGenerationType.level)
            return PlatformsPool.GetPlatform(generationType);
        else
        {
            Platform plat = PlatformsPool.GetLevelPlatform(CurrentLevelId);
            if (plat == null)
                plat = PlatformsPool.GetPlatform(PlatformGenerationType.tonnels);
            return plat;
        }
    }
    public static void ClearAll()
    {
        if (instance == null)
            return;
        foreach(var j in instance.platforms)
        {
            Destroy(j.gameObject);
        }
        instance.platforms.Clear();
    }
    //public functions
    public static Platform GetLastPlatform()
    {
        if (instance == null)
            return null;
        return instance.platforms[instance.platforms.Count - 1];
    }
}
