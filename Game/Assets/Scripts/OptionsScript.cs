using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScript : MonoBehaviour
{
    private Color _redColor = new Color(240 / 255f, 190 / 255f, 190 / 255f);
    private Color _greenColor = new Color(190 / 255f, 230 / 255f, 190 / 255f);
    private Color _greyColor = new Color(200 / 255f, 200 / 255f, 200 / 255f);
    private Color _deltaColor = new Color(50 / 255f, 50 / 255f, 50 / 255f, 0);
    private Dictionary<ColorName, Color> _colorDictionary;
    private (ColorName Color, string Index)[] _buttonColors;
    private List<ColorName> _activeButtons;

    public GameObject[] ColorButtons;
    public GameObject InputOptionCheckbox;

    public void Start()
    {
        _activeButtons = new List<ColorName>();
        _colorDictionary = new Dictionary<ColorName, Color>() { { ColorName.Green, _greenColor }, { ColorName.Red, _redColor }, { ColorName.Grey, _greyColor } };
        _buttonColors = new (ColorName Color, string Index)[3] { (ColorName.Green, "-1"), (ColorName.Red, "-1"), (ColorName.Grey, "-1") };
        LoadColorOption();
        SetActiveButtonsList();
        SetColorButtonsState();

        InitializeInputOption();
        InputOptionCheckbox.GetComponent<Toggle>().onValueChanged.AddListener(value => SaveInputOption(value));
    }

    private void ClickButton(int index)
    {
        if (_buttonColors[index].Index == "-1")
        {
            _activeButtons.Add(_buttonColors[index].Color);
            SetButtonsOrder();
        }
        else
        {
            _activeButtons.Remove(_buttonColors[index].Color);
            SetButtonsOrder();
        }
    }
    private void SetActiveButtonsList()
    {
        _activeButtons.Clear();
        _buttonColors
            .OrderBy(btn => int.Parse(btn.Index))
            .Where(btn => btn.Index != "-1")
            .ToList()
            .ForEach(btn => _activeButtons.Add(btn.Color));
    }
    private void SetButtonsOrder()
    {
        _buttonColors = _buttonColors.Select(btn => (btn.Color, "-1")).ToArray();
        for (int i = 0; i < _activeButtons.Count; i++)
            for (int j = 0; j < _buttonColors.Length; j++)
                if (_buttonColors[j].Color == _activeButtons[i])
                    _buttonColors[j].Index = (i + 1).ToString();
    }

    public void ColorOptionButtonOnClick(GameObject button)
    {
        for (int i = 0; i < ColorButtons.Length; i++)
        {
            if (ColorButtons[i] == button)
            {
                ClickButton(i);
                SetColorButtonsState();
            }
        }
        SaveColorOption();
    }

    public void SetColorButtonsState()
    {
        for (int i = 0; i < ColorButtons.Length; i++)
        {
            ColorButtons[i].GetComponent<Graphic>().color = _buttonColors[i].Index != "-1" ? _colorDictionary[_buttonColors[i].Color] : _colorDictionary[_buttonColors[i].Color] - _deltaColor;
            ColorButtons[i].GetComponentInChildren<Text>().text = _buttonColors[i].Index != "-1" ? _buttonColors[i].Index : "";
        }
    }

    public void LoadColorOption()
    {
        var colorOption = GetOptionTag(OptionTag.Color).Split(',');
        for (int i = 0; i < _buttonColors.Length; i++)
            _buttonColors[i].Index = colorOption[i];
    }
    public void SaveColorOption()
    {
        string colorOption = _buttonColors[0].Index;
        for (int i = 1; i < _buttonColors.Length; i++)
            colorOption += "," + _buttonColors[i].Index;
        SetOptionTag(OptionTag.Color, colorOption);
    }

    private void InitializeInputOption() => InputOptionCheckbox.GetComponent<Toggle>().isOn = LoadInputOption();

    public bool LoadInputOption() => GetOptionTag(OptionTag.BottomInput) == "False" ? false : true;
    public void SaveInputOption(bool value) => SetOptionTag(OptionTag.BottomInput, value ? "True" : "False");

    public static string GetOptionTag(OptionTag optionTag)
    {
        var tag = typeof(OptionTag).GetEnumName(optionTag);
        if (optionTag == OptionTag.Color)
            return PlayerPrefs.HasKey(tag) ? PlayerPrefs.GetString(tag) : "2,1,-1";
        else
            return PlayerPrefs.HasKey(tag) ? PlayerPrefs.GetString(tag) : "False";
    }
    public static void SetOptionTag(OptionTag optionTag, string value)
    {
        var tag = typeof(OptionTag).GetEnumName(optionTag);
        PlayerPrefs.SetString(tag, value);
    }
}

public enum OptionTag
{
    Color,
    BottomInput
}

public enum ColorName
{
    Green,
    Red,
    Grey
}
