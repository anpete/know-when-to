using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KnowWhenTo;

public readonly record struct Hand
{
    private static readonly int[] HandRanks = new int[32487834];

    private readonly int _category;
    private readonly int _rank;

    static Hand()
    {
        var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().Location).AbsolutePath);

        Debug.Assert(directory != null);

        using var dataFile = File.OpenRead(Path.Combine(directory, "HandRanks.dat"));
        using var binaryReader = new BinaryReader(dataFile);

        var span = MemoryMarshal.Cast<int, byte>(HandRanks.AsSpan());
        var length = dataFile.Read(span);

        Debug.Assert(length == span.Length);
    }

    public Hand(Card card1, Card card2, Card card3, Card card4, Card card5, Card card6, Card card7)
    {
        var value = HandRanks[53 + card1];
        value = HandRanks[value + card2];
        value = HandRanks[value + card3];
        value = HandRanks[value + card4];
        value = HandRanks[value + card5];
        value = HandRanks[value + card6];
        value = HandRanks[value + card7];

        Debug.Assert(value > 0);

        _category = value >> 12;

        Debug.Assert(_category is > 0 and < 10);

        _rank = value & 0x00000FFF;
    }

    public static bool operator >(Hand h1, Hand h2)
    {
        return h1._category > h2._category || (h1._category == h2._category && h1._rank > h2._rank);
    }

    public static bool operator <(Hand h1, Hand h2)
    {
        return h1._category < h2._category || (h1._category == h2._category && h1._rank < h2._rank);
    }
}