using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessager : MonoBehaviour
{ 
    private static GameObject _messagePanel;
    public GameObject MessagePanel;

    public void Start() => _messagePanel = MessagePanel;

    public static void Message(string message)
    {
        _messagePanel.GetComponentInChildren<Text>().text = message;
        _messagePanel.SetActive(true);
    }

    public void CloseButtonOnClick()
    {
        _messagePanel.SetActive(false);
        _messagePanel.GetComponentInChildren<Text>().text = "";
    }
}
