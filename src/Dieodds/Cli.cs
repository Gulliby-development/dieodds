using System.Reflection;

namespace Dieodds;

public static class Cli
{
    public const string Usage =
        "usage: dieodds stats EXPR\n" +
        "       dieodds chart [--width N] EXPR\n" +
        "       dieodds chance EXPR OP N\n" +
        "       dieodds versus A B";

    private static readonly Dictionary<string, Func<IReadOnlyList<string>, IIo, int>> Commands =
        new(StringComparer.Ordinal)
        {
            ["stats"] = StatsCommand.Run,
            ["chart"] = ChartCommand.Run,
            ["chance"] = ChanceCommand.Run,
            ["versus"] = VersusCommand.Run
        };

    public static int Run(string[] args, IIo io)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--version")
            {
                io.Out($"dieodds {GetVersion()}\n");
                return 0;
            }

            if (args.Length == 0)
            {
                throw new DieException("missing command (stats, chart, chance or versus)");
            }

            if (args.Length == 1 && args[0] == "--help")
            {
                io.Out(Usage + "\n");
                return 0;
            }

            if (!Commands.TryGetValue(args[0], out var command))
            {
                throw new DieException($"unknown command {Text.Show(args[0])}");
            }

            return command(args[1..], io);
        }
        catch (DieException exception)
        {
            io.Err($"dieodds: {exception.Message}\n");
            return 2;
        }
    }

    private static string GetVersion()
    {
        var version = typeof(Cli).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (version is null)
        {
            return "dev";
        }

        var suffixIndex = version.IndexOf('+');
        return suffixIndex < 0 ? version : version[..suffixIndex];
    }
}
