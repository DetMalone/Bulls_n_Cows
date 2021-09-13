using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Game : ICloneable
{
    private Game _reserveCopy;
    public byte TurnCount => (byte)_turns.Count();
    private List<(GameAnswer player, GameAnswer bot)> _turns;
    private (bool player, bool bot) _isWin { set => Ended(value); }

    private List<GameNumber> _possibleBotNumbers;
    private List<byte> _notCheckedDigits;

    private GameNumber _guessedNumber;
    public string GuessedNumber => _guessedNumber.ToString();

    public Game()
    {
        _guessedNumber = new GameNumber();
        _notCheckedDigits = Enumerable.Range(0, 10).Select(n => (byte)n).ToList();
        _possibleBotNumbers = Enumerable.Range(1, 9999)
            .Select(number => new GameNumber(number.ToString("D4")))
            .Where(number => number.Valid)
            .ToList();
        _turns = new List<(GameAnswer player, GameAnswer bot)>();
    }

    public void Hint()
    {
        string hintedNumber = default;
        if (_turns.Count == 0)
        {
            Hinted(hintedNumber = new GameNumber().ToString());
        }
        else
        {
            if (_possibleBotNumbers.Count() != 0)
            {
                var max = _possibleBotNumbers.Max(number => number.ContainCount(_notCheckedDigits.ToArray()));
                var possibleHints = _possibleBotNumbers.Where(number => number.ContainCount(_notCheckedDigits.ToArray()) == max).ToList();
                Hinted(hintedNumber = possibleHints.ElementAt(new System.Random().Next(0, possibleHints.Count())).ToString());
            }
            else throw new Exception("Your answers were wrong. Try again answer to last turn or restart game, please.");
        }
        _turns.Add((new GameAnswer(), new GameAnswer(hintedNumber)));
    }
    public void Move(string playerSupposedNumber, byte botBulls, byte botCows)
    {
        (bool player, bool bot) isWin = default;
        if (botBulls == 4 && botCows == 0) isWin = (false, true);
        if (new GameNumber(playerSupposedNumber).Valid)
        {
            _reserveCopy = (Game)Clone();

            (_turns.Last().bot.Bulls, _turns.Last().bot.Cows) = (botBulls, botCows);
            _turns.Last().player.Number = new GameNumber(playerSupposedNumber);
            (_turns.Last().player.Bulls, _turns.Last().player.Cows) = _turns.Last().player.Number.Compare(_guessedNumber);
            Answered((_turns.Last().player.Bulls, _turns.Last().player.Cows));

            _possibleBotNumbers.RemoveAll(number => number.Compare(_turns.Last().bot.Number) != (botBulls, botCows));
            _notCheckedDigits.RemoveAll(digit => _turns.Last().bot.Number.Contains(digit));

            if (_turns.Last().player.Bulls == 4) isWin.player = true;
            if (isWin != default) _isWin = isWin;
        }
        else
        {
            throw new Exception("Your supposed number is invalid. Try again or restart game, please.");
        };
    }
    public Game Back()
    {
        return _reserveCopy != null ? _reserveCopy : throw new Exception("It's a start point. You can't to back.");
    }

    public object Clone()
    {
        var clone = new Game();

        clone._turns = _turns.Select(turn => ((GameAnswer)turn.player.Clone(), (GameAnswer)turn.bot.Clone())).ToList();
        clone._notCheckedDigits = _notCheckedDigits.ToList();
        clone._possibleBotNumbers = _possibleBotNumbers.Select(number => new GameNumber(number.ToString())).ToList();
        clone._guessedNumber = new GameNumber(_guessedNumber.ToString());

        clone.Hinted = (Action<string>)Hinted.Clone();
        clone.Answered = (Action<(byte Bulls, byte Cows)>)Answered.Clone();
        clone.Ended = (Action<(bool, bool)>)Ended.Clone();

        clone._reserveCopy = _reserveCopy != null ? (Game)_reserveCopy.Clone() : null;

        return clone;
    }

    public event Action<string> Hinted;
    public event Action<(byte Bulls, byte Cows)> Answered;
    public event Action<(bool, bool)> Ended;
}

public class GameAnswer : ICloneable
{
    public GameNumber Number { get; set; }
    public byte Bulls { get; set; }
    public byte Cows { get; set; }
    public GameAnswer()
    { }
    public GameAnswer(string number)
    {
        Number = new GameNumber(number);
    }
    public GameAnswer(string number, (byte bulls, byte cows) answer) : this(number)
    {
        (Bulls, Cows) = answer;
    }

    public object Clone()
    {
        var clone = new GameAnswer();
        clone.Number = Number != null ? new GameNumber(Number.ToString()) : null;
        clone.Bulls = Bulls;
        clone.Cows = Cows;
        return clone;
    }
}