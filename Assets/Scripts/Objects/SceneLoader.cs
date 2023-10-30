using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static void ReloadCurrentScene()
    {
        print("restart");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
