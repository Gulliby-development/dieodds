<p align="center"><img src="docs/logo.svg" width="96" alt="" /></p>

# dieodds

dieodds is an offline exact-probability calculator for tabletop and board-game players and designers. It evaluates dice expressions without rolling dice, using exact integer counts to report summary statistics, distributions, target chances, and comparisons.

[![CI](https://github.com/Gulliby-development/dieodds/actions/workflows/ci.yml/badge.svg)](https://github.com/Gulliby-development/dieodds/actions/workflows/ci.yml) [![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Install

Install the .NET 10 SDK, then build a Release version from the repository root:

```sh
dotnet build -c Release
```

The executable is `src/Dieodds/bin/Release/net10.0/dieodds` on macOS and Linux, or `src/Dieodds/bin/Release/net10.0/dieodds.exe` on Windows. You can also invoke the assembly on any supported platform:

```sh
dotnet src/Dieodds/bin/Release/net10.0/dieodds.dll COMMAND
```

Replace `COMMAND` with one of the command forms below.

`dieodds --version` prints `dieodds 0.1.0`; the version is the `<Version>` in `Directory.Build.props` at the repository root.

## Usage

```text
usage: dieodds stats EXPR
       dieodds chart [--width N] EXPR
       dieodds chance EXPR OP N
       dieodds versus A B
```

Each expression is one command-line argument; quote it in your shell if it contains spaces or shell-special characters. Use `-` instead of an expression to read one expression from standard input.

For example, `dieodds stats 2d6+3` prints:

```text
expression: 2d6+3
outcomes: 36
min: 5
max: 15
mean: 10.00
variance: 5.83
median: 10
mode: 10
```

Other command-line forms include `dieodds chart --width 8 2d6`, `dieodds chance 2d6 ge 7`, and `dieodds versus d20+5 2d10+8`.

## Expressions

Expressions support integer constants, dice, parentheses, addition, subtraction, and multiplication, with spaces or tabs allowed between grammar items but not inside them. A single leading minus negates the first term. Addition and subtraction are evaluated left to right; multiplication is also left to right and must have a NUMBER literal on at least one side of each `*` (a group or dice term is not a NUMBER literal).

A dice term has the form `[COUNT]dSIDES`, optionally followed by one modifier (`!` or `rN`), then optionally by `kh[K]` or `kl[K]`. COUNT defaults to 1; K defaults to 1. `F` is a fudge die with faces -1, 0, and 1; `%` means 100 sides. A plain numbered die has 2–100 sides. For `rN`, an initial result at or below N is rolled once more and the second result stands; N is from 1 to one less than the number of sides. For `!`, a maximum face is rolled again, and a second maximum may be rolled once more; there are no further rolls. Keep-high or keep-low sums the K highest or lowest results among COUNT independent dice. Counts are limited to 1–20 without modifiers and 1–10 with modifiers; modified numbered dice have 2–20 sides, and fudge dice cannot explode or reroll. K is from 1 to COUNT, and an expression may contain at most three dice terms and at most 200 characters. NUMBER constants have one to four digits with no leading zero; dice counts and modifier values are digits subject to their stated ranges, with no leading zero.

Plain numbered dice have equal-weight faces. Reroll and explosion modifiers produce their specified weighted faces; all valid dice outcomes have exact integer weights. dieodds counts independent dice sequences as weighted outcomes with arbitrary-precision integers, so totals are not capped. Probabilities remain exact internally. Means, variances, and percentages are displayed to two decimal places and rounded half away from zero; rounding changes only the displayed decimals, not the exact internal counts. For standard input, `-` reads strict UTF-8, skips one leading byte-order mark, ignores blank lines, and requires exactly one nonblank expression.

## Commands

`stats EXPR` reports the expression, weighted outcome total, minimum, maximum, exact mean and variance (displayed to two decimal places), median, and all tied modes.

`chart [--width N] EXPR` prints each possible value, its probability, and a proportional bar. The width defaults to 40 and must be an integer from 1 to 80.

`chance EXPR OP N` reports the probability of the expression meeting the comparison. OP is one of `ge`, `gt`, `le`, `lt`, `eq`, or `ne`; N is an integer from -100000 to 100000.

`versus A B` reports the probabilities that A beats B, ties B, or loses to B. Each side is one expression argument or `-` for standard input; standard input can be used for only one side.

## Exit codes

Exit code 0 means success. Exit code 1 means a `chance` comparison matched no outcomes; the zero-probability result is still printed. Exit code 2 means invalid commands, arguments, expressions, or input.

## Development

From the repository root, use these checks:

```sh
dotnet build
dotnet format --verify-no-changes
dotnet test
```

## License

MIT, see [LICENSE](LICENSE).

---

Built with BAKED
