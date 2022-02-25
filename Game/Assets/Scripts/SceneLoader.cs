using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private readonly string[] sceneNames = new string[4]{ "Options", "Statistics", "About", "Game" };
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void DropdownLoadScene(int dropdownOption)
    {
        var sceneName = sceneNames[dropdownOption];
        SceneManager.LoadScene(sceneName);
    }
}