using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameNumber
{
    public bool Valid => _digits.Length == 4 && _digits.GroupBy(digit => digit).Where(digit => digit.Count() > 1).Count() == 0 ? true : false;
    private byte[] _digits = new byte[4];

    public GameNumber() : this(Enumerable.Range(1, 9999)
            .Select(number => new GameNumber(number.ToString("D4")))
            .Where(number => number.Valid)
            .Select(number => number.ToString())
            .ElementAt(new Random().Next(0, 5040))) { }
    public GameNumber(string number)
    {
        try
        {
            _digits = number
                .Select(digit => byte.Parse(digit.ToString()))
                .ToArray();
        }
        catch { }
    }

    public ValueTuple<byte, byte> Compare(GameNumber comparedNumber)
    {
        var cattle = (byte)_digits
            .Intersect(comparedNumber._digits)
            .Count();
        var bulls = (byte)_digits
            .Select((digit, index) => (digit, index))
            .Intersect(comparedNumber._digits.Select((digit, index) => (digit, index)))
            .Count();
        return (bulls, (byte)(cattle - bulls));
    }
    public bool Contains(byte digit) => _digits.Contains(digit);
    public bool Contains(byte[] digits) => _digits.Any(digit => digits.Contains(digit));
    public byte ContainCount(byte[] digits) => (byte)_digits.Count(digit => digits.Contains(digit));
    public override string ToString()
    {
        return string.Join("", _digits);
    }
}
