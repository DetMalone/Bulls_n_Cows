using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void DropdownLoadScene(int dropdownOption)
    {
        switch (dropdownOption)
        {
            case 0:
                SceneManager.LoadScene("Statistics"); break;
            case 1:
                SceneManager.LoadScene("About"); break;
            default:
                SceneManager.LoadScene("Game"); break;
        }
    }
}