using System.Diagnostics;

namespace KnowWhenTo;

public readonly record struct Card
{
    private static readonly string[] Names =
    [
        "2c", "2d", "2h", "2s",
        "3c", "3d", "3h", "3s",
        "4c", "4d", "4h", "4s",
        "5c", "5d", "5h", "5s",
        "6c", "6d", "6h", "6s",
        "7c", "7d", "7h", "7s",
        "8c", "8d", "8h", "8s",
        "9c", "9d", "9h", "9s",
        "Tc", "Td", "Th", "Ts",
        "Jc", "Jd", "Jh", "Js",
        "Qc", "Qd", "Qh", "Qs",
        "Kc", "Kd", "Kh", "Ks",
        "Ac", "Ad", "Ah", "As"
    ];

    private readonly byte _value;

    internal Card(byte value)
    {
        Debug.Assert(value is >= 1 and <= 52);

        _value = value;
    }

    public static Card[] ParseBoard(string board)
    {
        if (string.IsNullOrWhiteSpace(board))
        {
            return [];
        }

        if (board.Length % 2 != 0 || board.Length > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(board), $"Invalid board: '{board}'.");
        }

        var cards = new Card[board.Length / 2];

        for (var i = 0; i < board.Length; i += 2)
        {
            cards[i / 2] = Parse(board[i..(i + 2)]);
        }

        return cards;
    }

    public static Card Parse(string card)
    {
        var index = Array.IndexOf(Names, card);

        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(card), $"Invalid card: '{card}'.");
        }

        return new Card((byte)(index + 1));
    }

    public static implicit operator byte(Card card)
    {
        return card._value;
    }

    public override string ToString()
    {
        return Names[_value - 1];
    }
}