namespace KnowWhenTo;

public readonly record struct Player
{
    public Card Card1 { get; private init; }
    public Card Card2 { get; private init; }

    public static Player Parse(string cards)
    {
        if (cards.Length != 4)
        {
            throw new ArgumentOutOfRangeException(nameof(cards));
        }

        var card1 = Card.Parse(cards[..2]);
        var card2 = Card.Parse(cards[2..]);

        ArgumentOutOfRangeException.ThrowIfEqual(card1, card2);

        return new Player { Card1 = card1, Card2 = card2 };
    }

    public override string ToString()
    {
        return $"{Card1}{Card2}";
    }
}