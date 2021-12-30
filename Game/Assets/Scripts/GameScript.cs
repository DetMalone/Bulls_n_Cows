using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    private Game _game;

    public GameObject Canvas;

    public GameObject VersionNumberText;

    public GameObject WinChanceText;

    private Vector3 _graphicRowShift = new Vector3(0, -140, 0);
    private List<List<GameObject>> _graphicRows;
    public List<GameObject> GraphicRow;
    public GameObject PlayerAlternativeInput;

    public GameObject BotGuessInput;

    private Color _digitRed = new Color(240 / 255f, 190 / 255f, 190 / 255f);
    private Color _digitGreen = new Color(190 / 255f, 230 / 255f, 190 / 255f);
    private Color _digitGrey = new Color(200 / 255f, 200 / 255f, 200 / 255f);
    private List<Color> _colorOrderList;
    private Color _digitWhite = new Color(250 / 255f, 250 / 255f, 240 / 255f);
    private List<GameObject> _digitButtons;
    public GameObject DigitButton;

    public GameObject StartGamePanel;
    public GameObject GameLogPanel;
    public GameObject EndGamePanel;

    public void Start()
    {
        CheckVersion();

        _digitButtons = new List<GameObject>();
        _graphicRows = new List<List<GameObject>>();

        RestartButtonOnClick();
    }

    private void Move()
    {
        try
        {
            var playerSupposedNumber = _graphicRows.Last().Find(element => new Regex("PlayerInput").IsMatch(element.name)).GetComponent<InputField>().text;
            var botBulls = _graphicRows.Last().Find(element => new Regex("BotBulls").IsMatch(element.name)).GetComponent<Text>().text;
            var botCows = _graphicRows.Last().Find(element => new Regex("BotCows").IsMatch(element.name)).GetComponent<Text>().text;

            _game.Move(playerSupposedNumber, byte.Parse(botBulls), byte.Parse(botCows));
            SetInteractableTurnGraphic(false);

            if (!_game.IsEnded)
            {
                AddTurnGraphic();
            }
        }
        catch (Exception ex)
        {
            ErrorMessager.Message(ex.Message);
        }
    }

    private void SetInteractableTurnGraphic(bool isInteractable)
    {
        _graphicRows
            .Last()
            .Where(element => new Regex("PlayerInput").IsMatch(element.name))
            .ToList()
            .ForEach(element => element.GetComponent<InputField>().interactable = isInteractable);
        _graphicRows
            .Last()
            .Where(element => new Regex("Button").IsMatch(element.name))
            .ToList()
            .ForEach(element => element.GetComponent<Button>().interactable = isInteractable);
    }
    private void AddDigitButtonsGraphic()
    {
        _digitButtons = Enumerable.Range(0, 10).Select(digit => Instantiate(DigitButton, GameLogPanel.transform)).ToList();
        Enumerable.Range(0, 10).ToList().ForEach(digit => _digitButtons[digit].GetComponentInChildren<Text>().text = digit.ToString());
        _digitButtons.ForEach(digit => digit.transform.localPosition += new Vector3(digit.GetComponent<RectTransform>().rect.width, 0, 0) * int.Parse(digit.GetComponentInChildren<Text>().text));
        _digitButtons.ForEach(digit => digit.GetComponent<Button>().onClick.AddListener(delegate { DigitButtonOnClick(digit); }));
    }
    private void AddTurnGraphic()
    {
        WinChanceText.GetComponent<Text>().text = $"Win {Math.Round(_game.WinChance * 100d, 2)}%";

        List<GameObject> graphicElements = GraphicRow.Select(element => Instantiate(element, GameLogPanel.transform)).ToList();
        graphicElements.ForEach(element => element.transform.localPosition += _graphicRowShift * _game.TurnCount);
        graphicElements.Find(element => new Regex("TurnNumber").IsMatch(element.name)).GetComponent<Text>().text = (_game.TurnCount + 1).ToString();
        graphicElements.Find(element => new Regex("BullsPlus").IsMatch(element.name)).GetComponent<Button>().onClick.AddListener(BullsPlusButtonOnClick);
        graphicElements.Find(element => new Regex("CowsPlus").IsMatch(element.name)).GetComponent<Button>().onClick.AddListener(CowsPlusButtonOnClick);
        _graphicRows.Add(graphicElements);

        try
        {
            _game.Hint();
            if (BotGuessInput.activeSelf == true)
            {
                _graphicRows.Last().FindAll(element => new Regex("Plus").IsMatch(element.name)).ForEach(element => element.GetComponent<Button>().interactable = false);
                var botGuessedNumber = new GameNumber(BotGuessInput.GetComponent<InputField>().text);
                var botSupposedNumber = new GameNumber(_graphicRows.Last().Find(element => new Regex("BotInput").IsMatch(element.name)).GetComponent<InputField>().text);
                (byte bulls, byte cows) botAnswer = botGuessedNumber.Compare(botSupposedNumber);
                _graphicRows.Last().Find(element => new Regex("BotBulls").IsMatch(element.name)).GetComponent<Text>().text = botAnswer.bulls.ToString();
                _graphicRows.Last().Find(element => new Regex("BotCows").IsMatch(element.name)).GetComponent<Text>().text = botAnswer.cows.ToString();
            }
        }
        catch (Exception ex)
        {
            BackButtonOnClick();
            ErrorMessager.Message(ex.Message);
        }
    }
    private void ClearTurnGraphic()
    {
        _graphicRows.Last().ForEach(element => Destroy(element));
        _graphicRows.Remove(_graphicRows.Last());
    }
    private void Clear()
    {
        if (_graphicRows != null && _graphicRows.Count != 0)
            Enumerable.Range(0, _graphicRows.Count).ToList().ForEach(rowIndex => ClearTurnGraphic());
        _digitButtons.ForEach(button => Destroy(button));
        _digitButtons.Clear();
        _game = new Game(); 
        InitializeGameEvents();
    }
    private void SetGameButtonInteractable(bool isInteractable)
    {
        Canvas.GetComponentsInChildren<Button>()
            .Where(button => !new Regex("Menu|End|Guess|Random|Cancel|Close").IsMatch(button.name))
            .ToList()
            .ForEach(button => button.interactable = isInteractable);
    }

    private void Save(StatisticsFieldName name)
    {
        int oldCount = PlayerPrefs.GetInt(SaveTag.GetTag(name), 0);
        PlayerPrefs.SetInt(SaveTag.GetTag(name), ++oldCount);
    }
    private void SaveScore(StatisticsFieldName name, int score)
    {
        float oldScore;
        int oldCount = PlayerPrefs.GetInt(SaveTag.GetTag(name), 0);
        switch (name)
        {
            case StatisticsFieldName.WinCount:
                oldScore = PlayerPrefs.GetFloat(SaveTag.GetTag(StatisticsFieldName.WinAvgScore), 0f);
                PlayerPrefs.SetFloat(SaveTag.GetTag(StatisticsFieldName.WinAvgScore), (oldScore * oldCount + score) / ++oldCount);
                break;
            case StatisticsFieldName.LoseCount:
                oldScore = PlayerPrefs.GetFloat(SaveTag.GetTag(StatisticsFieldName.LoseAvgScore), 0f);
                PlayerPrefs.SetFloat(SaveTag.GetTag(StatisticsFieldName.LoseAvgScore), (oldScore * oldCount + score) / ++oldCount);
                break;
            case StatisticsFieldName.DrawCount:
                oldScore = PlayerPrefs.GetFloat(SaveTag.GetTag(StatisticsFieldName.DrawAvgScore), 0f);
                PlayerPrefs.SetFloat(SaveTag.GetTag(StatisticsFieldName.DrawAvgScore), (oldScore * oldCount + score) / ++oldCount);
                break;
        }    
    }
    public void LoadGameOptions()
    {
        SetColorOrder();
        PlayerAlternativeInput.SetActive(OptionsScript.GetOptionTag(OptionTag.BottomInput) == "True");
    }
    private void CheckVersion()
    {
        var oldVersion = PlayerPrefs.GetString("version", "");
        var versionText = VersionNumberText.GetComponent<Text>();
        if (oldVersion != versionText.text && versionText.text.Split('-').Length > 1 && versionText.text.Split('-')[1] == "upd")
            SaveTag.DeleteAllTags();
        PlayerPrefs.SetString("version", versionText.text);
    }
    private void SetColorOrder()
    {
        var colorOption = OptionsScript.GetOptionTag(OptionTag.Color).Split(',');
        var _colorList = new List<(int, Color)>();
        if (colorOption[0] != "-1") _colorList.Add((int.Parse(colorOption[0]), _digitGreen));
        if (colorOption[1] != "-1") _colorList.Add((int.Parse(colorOption[1]), _digitRed));
        if (colorOption[2] != "-1") _colorList.Add((int.Parse(colorOption[2]), _digitGrey));
        _colorOrderList = new List<Color>();
        _colorOrderList = _colorList.OrderBy(clr => clr.Item1).Select(clr => clr.Item2).ToList();
    }

    public void RestartButtonOnClick()
    {
        SetGameButtonInteractable(false);
        StartGamePanel.SetActive(true);
    }
    public void BackButtonOnClick()
    {
        try
        {
            _game = _game.Back();
            ClearTurnGraphic();
            SetInteractableTurnGraphic(true);
        }
        catch (Exception ex)
        {
            ErrorMessager.Message(ex.Message);
        }
    }
    public void GoButtonOnClick()
    {
        Move();
        PlayerAlternativeInput.GetComponent<InputField>().text = "";
    }
    public void EndRestartButtonOnClick()
    {
        EndGamePanel.GetComponentInChildren<Text>().text = "";
        EndGamePanel.SetActive(false);
        SetGameButtonInteractable(true);
        RestartButtonOnClick();
    }

    public void BullsPlusButtonOnClick()
    {
        var BotBullsText = _graphicRows.Last().Find(element => new Regex(@"BotBullsText").IsMatch(element.name)).GetComponent<Text>();
        BotBullsText.text = ((int.Parse(BotBullsText.text) + 1) % 5).ToString();
    }
    public void CowsPlusButtonOnClick()
    {
        var BotCowsText = _graphicRows.Last().Find(element => new Regex(@"BotCowsText").IsMatch(element.name)).GetComponent<Text>();
        BotCowsText.text = ((int.Parse(BotCowsText.text) + 1) % 5).ToString();
    }

    public void DigitButtonOnClick(GameObject button)
    {
        if (_colorOrderList.Count > 0)
        {
            var buttonColor = button.GetComponent<Graphic>().color;
            if (buttonColor == _digitWhite)
            {
                button.GetComponent<Graphic>().color = _colorOrderList[0];
            }
            else
            {
                for (int i = 0; i < _colorOrderList.Count; i++)
                {
                    if (buttonColor == _colorOrderList[i])
                    {
                        if (i + 1 < _colorOrderList.Count)
                        {
                            button.GetComponent<Graphic>().color = _colorOrderList[i + 1];
                            break;
                        }
                        else
                        {
                            button.GetComponent<Graphic>().color = _digitWhite;
                            break;
                        }
                    }
                }
            }
        }
    }

    public void PlayerAlternativeInputAwake()
    {
        PlayerAlternativeInput.GetComponent<InputField>().onValueChanged.AddListener(text => PlayerAlternativeInputOnValueChanged(text));
    }
    public void PlayerAlternativeInputOnValueChanged(string text)
    {
        var lastPlayerInput = _graphicRows.Last().Find(element => new Regex("PlayerInput").IsMatch(element.name));
        if (lastPlayerInput.GetComponent<InputField>().interactable)
        {
            lastPlayerInput.GetComponent<InputField>().text = text;
        }
    }

    public void RandomButtonOnClick()
    {
        StartGamePanel.GetComponentInChildren<InputField>().text = new GameNumber().ToString();
    }
    public void GuessButtonOnClick()
    {
        ClearGameState();
        if (new GameNumber(StartGamePanel.GetComponentInChildren<InputField>().text).Valid)
        {
            BotGuessInput.GetComponent<InputField>().text = StartGamePanel.GetComponentInChildren<InputField>().text;
            BotGuessInput.SetActive(true);
            StartGameState();
        }
        else
        {
            ErrorMessager.Message("This number is invalid. Try again.");
            RestartButtonOnClick();
        }
    }
    public void CancelButtonOnClick()
    {
        ClearGameState();
        StartGameState();
    }

    private void ClearGameState()
    {
        StartGamePanel.SetActive(false);
        BotGuessInput.SetActive(false);
        SetGameButtonInteractable(true);
        Clear();
    }
    private void StartGameState()
    {
        PlayerAlternativeInputAwake();
        LoadGameOptions();
        AddTurnGraphic();
        AddDigitButtonsGraphic();
    }

    private void InitializeGameEvents()
    {
        _game.Hinted += (number) => _graphicRows
              .Last()
              .Find(element => new Regex("BotInput").IsMatch(element.name))
              .GetComponent<InputField>().text = number;
        _game.Answered += (answer) =>
        {
            _graphicRows.Last().Find(element => new Regex("PlayerBulls").IsMatch(element.name)).GetComponent<Text>().text = answer.Bulls.ToString();
            _graphicRows.Last().Find(element => new Regex("PlayerCows").IsMatch(element.name)).GetComponent<Text>().text = answer.Cows.ToString();
        };
        _game.Ended += (isWin) =>
        {
            string endingText;
            switch (isWin)
            {
                case (true, false):
                    endingText = "Right, Bot guessed {1}.\nYou won this game.\nYour score {0}.";
                    SaveScore(StatisticsFieldName.WinCount, _game.TurnCount);
                    Save(StatisticsFieldName.WinCount); break;
                case (false, true):
                    endingText = "Wrong, Bot guessed {1}.\nYou lost this game.\nBot score {0}.";
                    SaveScore(StatisticsFieldName.LoseCount, _game.TurnCount);
                    Save(StatisticsFieldName.LoseCount); break;
                case (true, true):
                    endingText = "Right, Bot guessed {1}.\nIt's Draw.\nYour score {0}.";
                    SaveScore(StatisticsFieldName.DrawCount, _game.TurnCount);
                    Save(StatisticsFieldName.DrawCount); break;
                default:
                    endingText = "Something went wrong."; break;
            }
            EndGamePanel.GetComponentInChildren<Text>().text = string.Format(endingText, _game.TurnCount, _game.GuessedNumber);
            EndGamePanel.SetActive(true);
            SetGameButtonInteractable(false);
            SetInteractableTurnGraphic(false);
            Save(StatisticsFieldName.GameCount);
        };
    }
}