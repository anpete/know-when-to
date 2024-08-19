using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace KnowWhenTo.Cli;

[UsedImplicitly]
public class SimulationSettings : CommandSettings
{
    [CommandArgument(0, "<HANDS>")]
    [Description("The hands of the players. E.g. AhKd JsJc")]
    public string[] Hands { get; [UsedImplicitly] set; } = [];

    [CommandOption("-i|--iterations <ITERATIONS>")]
    [Description("Number of iterations")]
    public long Iterations { get; [UsedImplicitly] set; } = 100_000_000;

    [CommandOption("-b|--board <BOARD>")]
    [Description("The board cards. E.g. Ac3d7h")]
    public string? Board { get; [UsedImplicitly] set; }

    [CommandOption("-t|--threads <THREADS>")]
    [Description("Number of threads")]
    public int Threads { get; [UsedImplicitly] set; } = Environment.ProcessorCount - 1;
}

[UsedImplicitly]
internal class SimulationCommand : AsyncCommand<SimulationSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SimulationSettings settings)
    {
        var players = settings.Hands.Select(Player.Parse).ToArray();
        var board = Card.ParseBoard(settings.Board ?? "");

        // Run a single iteration to validate
        MonteCarlo.Simulate(players, board, _ => false);

        AnsiConsole.MarkupLine(
            $"[bold dodgerblue1]\nSimulating ~{settings.Iterations:N0} hands using {settings.Threads} threads.\n[/]");

        var table = new Table();

        table
            .AddColumn("[bold teal]Hand[/]")
            .AddColumn("[bold teal]Equity[/]")
            .AddColumn("[bold teal]Pots won[/]", c => c.RightAligned())
            .AddColumn("[bold teal]Win %[/]")
            .AddColumn("[bold teal]Pots tied[/]")
            .AddColumn("[bold teal]Tie %[/]");

        table.BorderColor(Color.Grey50);
        table.RoundedBorder();

        foreach (var p in players)
        {
            table.AddRow(p.Format(), "", "", "", "", "");
        }

        var sync = new object();
        var running = true;
        var iterations = 0;
        var sw = Stopwatch.StartNew();

        await AnsiConsole
            .Live(table)
            .StartAsync(
                async ctx =>
                {
                    var results = new SimulationResult[players.Length];

                    for (var i = 0; i < settings.Threads; i++)
                    {
                        _ = Task.Run(
                            () =>
                            {
                                MonteCarlo.Simulate(
                                    players,
                                    board,
                                    r =>
                                    {
                                        lock (sync)
                                        {
                                            for (var j = 0; j < r.Length; j++)
                                            {
                                                results[j].Won += r[j].Won;
                                                results[j].Tied += r[j].Tied;
                                            }

                                            iterations += MonteCarlo.ProgressIterations;

                                            // ReSharper disable once AccessToModifiedClosure
                                            return running;
                                        }
                                    });
                            });
                    }

                    while (running)
                    {
                        if (iterations > 0)
                        {
                            lock (sync)
                            {
                                for (var i = 0; i < results.Length; i++)
                                {
                                    var r = results[i];

                                    table.UpdateCell(i, 1, $"{(r.Won + r.Tied) / iterations * 100:F3}%");
                                    table.UpdateCell(i, 2, $"{r.Won:N0}");
                                    table.UpdateCell(i, 3, $"{r.Won / (decimal)iterations * 100:F3}%");
                                    table.UpdateCell(i, 4, $"{r.Tied:F3}");
                                    table.UpdateCell(i, 5, $"{r.Tied / iterations * 100:F3}%");
                                }

                                if (iterations >= settings.Iterations)
                                {
                                    running = false;
                                }
                            }

                            ctx.Refresh();
                        }

                        await Task.Delay(500);
                    }
                });

        sw.Stop();

        AnsiConsole.MarkupLine(
            $"[bold yellow]\nSimulated {iterations:N0} hands in {sw.Format()} " +
            $"(~{iterations / sw.Elapsed.TotalSeconds:N0} hands/s)\n[/]");

        return 0;
    }
}

public static class Extensions
{
    public static string Format(this Player player)
    {
        return $"{player.Card1.Format()}{player.Card2.Format()}";
    }

    private static string Format(this Card card)
    {
        return $"[bold {(card % 4) switch
        {
            3 => "red",
            2 => "blue",
            1 => "green",
            0 => "black",
            _ => throw new UnreachableException()
        }} on white]{card}[/]";
    }
}

internal static class StopwatchExtensions
{
    public static string Format(this Stopwatch stopwatch)
    {
        var elapsed = stopwatch.Elapsed;

        return elapsed.TotalMilliseconds < 1000 ? $"{elapsed.TotalMilliseconds:F2}ms" : $"{elapsed.TotalSeconds:F2}s";
    }
}

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<SimulationCommand>();

        app.Configure(config => { config.SetApplicationName("eval-hands"); });

        return await app.RunAsync(args);
    }
}