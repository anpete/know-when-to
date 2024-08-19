# know-when-to

You better know when to hold'em!

This repo contains a pretty fast (~95M hands/s on an M3 MacBook) Texas Hold'em hand evaluator.

What is a hand evaluator? It's a small program that given one or more sets of starting hands, and 0-5 board cards, will determine the equity of each hand - Equity is the percentage of the time that a hand will win the pot.

The evaluator is written in C# using .NET Core and uses a Monte Carlo simulation to determine the equity of each hand. Relative hand strength is determined by simulating a large number of hands and counting the number of times each hand wins. Hand ranks are determined using the lookup table approach. The file `HandRanks.dat` contains the precomputed hand ranks.

There is a CLI which will allow you to evaluate hands from the command line. E.g.:

```
./eval-hands AcKc QcQh 2h7d JcTd --board AhKd

Simulating ~100,000,000 hands using 10 threads.

╭──────┬─────────┬─────────────┬─────────┬───────────┬────────╮
│ Hand │ Equity  │    Pots won │ Win %   │ Pots tied │ Tie %  │
├──────┼─────────┼─────────────┼─────────┼───────────┼────────┤
│ AcKc │ 80.769% │ 117,784,625 │ 80.730% │ 57045.250 │ 0.039% │
│ QcQh │ 4.247%  │   6,139,192 │ 4.208%  │ 57045.250 │ 0.039% │
│ 2h7d │ 2.322%  │   3,330,708 │ 2.283%  │ 57045.250 │ 0.039% │
│ JcTd │ 12.662% │  18,417,294 │ 12.623% │ 57045.250 │ 0.039% │
╰──────┴─────────┴─────────────┴─────────┴───────────┴────────╯

Simulated 146,900,000 hands in 1.54s (~95,578,334 hands/s)
```
