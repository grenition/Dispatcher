using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlatformBuffer
{
    //parameters
    public int platformsCount = 10;
    public Platform platformPrefab;

    //values
    public List<Platform> platforms = new List<Platform>();
    public PlatformGenerationType PlarformType
    {
        get
        {
            if (platformPrefab != null)
                return platformPrefab.PlatformType;
            return PlatformGenerationType.tonnels;
        }
    }

    //functions
    public void InitializeAllPlatforms()
    {
        if (platformPrefab == null)
            return;

        for (int i = 0; i < platformsCount; i++)
        {
            InitializeOnePlatform();
        }
    }
    public Platform InitializeOnePlatform()
    {
        if (platformPrefab == null)
            return null;

        Platform plat = Object.Instantiate(platformPrefab, PlatformsPool.PlatformsParent);
        plat.DisablePlatform(false);
        plat.IsPooling = true;
        platforms.Add(plat);

        return plat;
    }
    public Platform GetPlatform()
    {
        if (platformsCount == 0)
        {
            Platform plat = Object.Instantiate(platformPrefab, PlatformsPool.PlatformsParent);
            plat.DisablePlatform(false);
            return plat;
        }

        Platform platform = null;

        for(int i = 0; i < 2; i++)
        {
            if (platforms.Count != platformsCount)
                return InitializeOnePlatform();
            for (int j = 0; j < platforms.Count; j++)
            {
                if(i == 0 && !platforms[j].IsPlatformActive() || i == 1)
                {
                    platform = platforms[j];
                    platforms.CycleMoveRight();
                    return platform;
                }
            }
        }
        return platform;
    }
}
public class PlatformsPool : MonoBehaviour
{
    //public values
    public static PlatformsPool Instance
    {
        get
        {
            if(bufferedInstance == null)
            {
                bufferedInstance = FindObjectOfType<PlatformsPool>();
            }
            return bufferedInstance;
        }
    }
    public static Transform PlatformsParent
    {
        get
        {
            if (Instance == null)
                return null;
            return Instance.platformsParent;
        }
    }

    //parameters
    [SerializeField] private PlatformBuffer[] platforms;
    [SerializeField] private Platform[] levelPlatforms;

    [SerializeField] private Transform platformsParent;

    //local values
    private static PlatformsPool bufferedInstance;
    private bool initialized = false;

    //local functions
    private void Awake()
    {
        if (Instance == this)
            Initialize();
    }
    private void Initialize()
    {
        initialized = true;
    }
    private Platform FindPlatform(PlatformGenerationType type)
    {
        if (!initialized)
            Initialize();

        Platform platform = null;

        foreach (var buf in platforms)
        {
            if (buf.PlarformType == type)
                return buf.GetPlatform();
        }

        return platform;
    }

    //public functions
    public static Platform GetPlatform(PlatformGenerationType type)
    {
        if (Instance == null)
            return null;
        if (!Instance.initialized)
            Instance.Initialize();

        return Instance.FindPlatform(type);
    }
    public static bool IsLevelAvailable(int levelId)
    {
        if (Instance == null)
            return false;
        return !(levelId < 0 || levelId >= Instance.levelPlatforms.Length);
            
    }
    public static Platform GetLevelPlatform(int levelId)
    {
        if (Instance == null)
            return null;
        if (!Instance.initialized)
            Instance.Initialize();
        if (levelId >= Instance.levelPlatforms.Length || levelId < 0)
            return null;
        Platform plat = Instantiate(Instance.levelPlatforms[levelId], PlatformsPool.PlatformsParent);
        plat.DisablePlatform(false);
        return plat;
    }
}
