using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AboutScript : MonoBehaviour
{
    public GameObject VersionNumberText;
    public void Start()
    {
        VersionNumberText.GetComponent<Text>().text = PlayerPrefs.GetString("version", "init");
    }
}


