// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace KnowWhenTo;

public record struct SimulationResult(long Won, decimal Tied);

public static class MonteCarlo
{
    public const int ProgressIterations = 100_000;

    private static void ThrowIfDuplicate(long deck, Card card, string arg)
    {
        if ((deck & (1L << card)) != 0)
        {
            throw new ArgumentOutOfRangeException(arg, $"Duplicate card: '{card}'.");
        }
    }

    public static void Simulate(Player[] players,
        Card[] board,
        Func<SimulationResult[], bool> progress,
        int progressIterations = ProgressIterations)
    {
        ArgumentNullException.ThrowIfNull(players);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(progressIterations);

        if (players.Length is 0 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(players), "The number of players must be between 1 and 10.");
        }

        if (board.Length > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(board), "The number of board cards must be between 0 and 5.");
        }

        var random = new Random();

        Span<SimulationResult> results = stackalloc SimulationResult[players.Length];

        var deck = 0L;

        // Remove player cards from the deck
        for (var i = 0; i < players.Length; i++)
        {
            ThrowIfDuplicate(deck, players[i].Card1, nameof(players));

            deck |= 1L << players[i].Card1;

            ThrowIfDuplicate(deck, players[i].Card2, nameof(players));

            deck |= 1L << players[i].Card2;
        }

        // Remove board cards from the deck 
        for (var i = 0; i < board.Length; i++)
        {
            ThrowIfDuplicate(deck, board[i], nameof(board));

            deck |= 1L << board[i];
        }

        var deals = 5 - board.Length;

        Span<Card> community = stackalloc Card[5];

        board.CopyTo(community);

        var iterations = 0L;

        while (true)
        {
            var iterationDeck = deck;

            // Deal the remaining board cards
            var k = deals;
            while (k > 0)
            {
                var next = (byte)random.Next(1, 53);

                if ((iterationDeck & (1L << next)) == 0)
                {
                    community[5 - k--] = new Card(next);
                    iterationDeck |= 1L << next;
                }
            }

            // Determine the winner
            var winner = -1;
            var bestHand = default(Hand);
            byte ties = 0;

            for (var j = 0; j < players.Length; j++)
            {
                var hand = new Hand(
                    players[j].Card1,
                    players[j].Card2,
                    community[0],
                    community[1],
                    community[2],
                    community[3],
                    community[4]);

                if (hand > bestHand)
                {
                    bestHand = hand;
                    winner = j;
                }
                else if (hand == bestHand)
                {
                    if (winner > -1)
                    {
                        ties |= (byte)(1 << winner);
                        winner = -1;
                    }

                    ties |= (byte)(1 << j);
                }
            }

            if (winner > -1)
            {
                results[winner].Won++;
            }
            else
            {
                var tied = byte.PopCount(ties);

                for (var j = 0; j < players.Length; j++)
                {
                    if ((ties & (1 << j)) != 0)
                    {
                        results[j].Tied += 1.0m / tied;
                    }
                }
            }

            if (++iterations % progressIterations == 0)
            {
                if (!progress(results.ToArray()))
                {
                    break;
                }

                results.Clear();
            }
        }
    }
}