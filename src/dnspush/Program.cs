using System;
using McMaster.Extensions.CommandLineUtils;

namespace dnspush
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();
            var optionProvider = app.Option("-p|--provider <PROVIDER URL>", "The subject", CommandOptionType.SingleValue);
            var optionHost = app.Option<int>("-h|--host <DNS HOST>", "dfgdfgdfg", CommandOptionType.SingleValue);

            app.OnExecuteAsync(async cancellationToken =>
            {
                var subject = optionProvider.HasValue()
                    ? optionProvider.Value()
                    : "world";

                var count = optionHost.HasValue() ? optionHost.ParsedValue : 1;
                for (var i = 0; i < count; i++)
                {
                    Console.WriteLine($"Hello {subject}!");
                }
                return 0;
            });

            return app.Execute(args);
        }
    }
}
