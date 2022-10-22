using PassPort.Models;
using PassPort.Utilities;
using System.CommandLine;

namespace PassPort.Services
{
    public class Program
    {
        public static Config? Config { get; private set; }

        private static readonly SemaphoreSlim exitSignal = new(0, 1);

        public static void Exit() => exitSignal.Release();

        public static async Task Main(string[] args)
        {
            var rootCommand = CreateRootCommand();
            await rootCommand.InvokeAsync(args);
            await exitSignal.WaitAsync();
        }

        private static RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand(description: "PassPort is a fast traffic processing server.");
            var configOption = new Option<FileInfo?>(name: "--config");
            rootCommand.AddOption(configOption);
            rootCommand.SetHandler(Initialize, configOption);
            return rootCommand;
        }

        private static void Initialize(FileInfo? configFile)
        {
            Console.WriteLine("Loading config...");
            if (configFile == null)
                throw new ArgumentNullException(nameof(configFile));
            Config = ConfigLoader.LoadConfig(configFile.FullName);

            Console.WriteLine("Initialize module...");
            foreach (var node in Config.Chains.Values)
                node.Module.Initialize(node);
        }
    }
}
