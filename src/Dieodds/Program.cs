namespace Dieodds;

public static class Program
{
    public static int Main(string[] args)
    {
        IIo io = new RealIo();
        return Cli.Run(args, io);
    }

    private sealed class RealIo : IIo
    {
        private static readonly System.Text.UTF8Encoding Utf8 = new(false);

        public byte[] ReadStdin()
        {
            using var input = Console.OpenStandardInput();
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            return buffer.ToArray();
        }

        public void Out(string text)
        {
            Write(Console.OpenStandardOutput(), text);
        }

        public void Err(string text)
        {
            Write(Console.OpenStandardError(), text);
        }

        private static void Write(Stream output, string text)
        {
            var bytes = Utf8.GetBytes(text);
            output.Write(bytes);
            output.Flush();
        }
    }
}

