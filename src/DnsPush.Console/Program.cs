using System;
using DnsPush.Core.Hosts;
using DnsPush.Core.Hosts.Namecheap;
using DnsPush.Core.OptionValidators;
using McMaster.Extensions.CommandLineUtils;
using Serilog;

namespace DnsPush.Console
{
    class Program
    {
        static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
            Log.Information("The global logger has been configured");

            var app = new CommandLineApplication();

            app.HelpOption();
            var optionApiUser = app.Option
            (
                "-u|--api-user <API_USER>",
                "Username required to access the API",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            optionApiUser.Validators.Add(new ApiUserLengthValidator());
            var optionApiKey = app.Option
            (
                "-k|--api-key <API_KEY>",
                "Password required used to access the API",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            optionApiKey.Validators.Add(new ApiKeyLengthValidator());
            var optionUserName = app.Option
            (
                "-U|--user-name <USER_NAME>",
                "The Username on which a command is executed. Generally, the values of ApiUser and UserName parameters are the same.",
                CommandOptionType.SingleValue
            );
            optionUserName.Validators.Add(new UserNameLengthValidator());
            var optionClientIp = app.Option
            (
                "-p|--client-ip <CLIENT_IP>",
                "An IP address of the server from which our system receives API calls (only IPv4 can be used)",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            optionClientIp.Validators.Add(new ClientIpLengthValidator());
            optionClientIp.Validators.Add(new ClientIpFormatValidator());
            var optionSld = app.Option
            (
                "-s|--sld <SLD>",
                "Second-level domain of the record to update. In example.com, 'example' is the second-level domain of the .com TLD",
                CommandOptionType.SingleValue
            )
                .IsRequired();
                optionSld.Validators.Add(new SldLengthValidator());
            var optionTld = app.Option
            (
                "-t|--tld <TLD>",
                "Top-level domain of the record to update. In example.com, 'com' is the top-level domain",
                CommandOptionType.SingleValue
            )
                .IsRequired();
                optionTld.Validators.Add(new TldLengthValidator());
            var optionHostName = app.Option
            (
                "-h|--host-name <HOST_NAME>", "Sub-domain/hostname of the record to update",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            var optionRecordType = app.Option
            (
                "-T|--record-type <RECORD_TYPE>", "Possible values: 'A', 'CNAME'",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            optionRecordType.Validators.Add(new RecordTypeValidValuesValidator());
            var optionAddress = app.Option
            (
                "-a|--address <ADDRESS>",
                "Possible values are a URL or an IP address. The value for this parameter is based on RECORD_TYPE",
                CommandOptionType.SingleValue
            )
                .IsRequired();
            optionAddress.Validators.Add(new AddressFormatValidator());
            var optionTtl = app.Option<int>
            (
                "-l|--ttl <TTL>",
                "Time to live of the record to update. Possible values: any value between 60 to 60000",
                CommandOptionType.SingleValue
            );
            optionTtl.Validators.Add(new TtlRangeValidator());
            var optionSandbox = app.Option
            (
                "-b|--sandbox <SANDBOX>",
                "Use test server environment",
                CommandOptionType.NoValue
            );

            Log.Information("Program options configured.");

            app.OnExecuteAsync(async cancellationToken =>
            {
                var hostOptions = new NamecheapOptions
                {
                    ApiUser = optionApiUser.Value(),
                    ApiKey = optionApiKey.Value(),
                    UserName = optionUserName.HasValue() ? optionUserName.Value() : optionApiUser.Value(),
                    ClientIp = optionClientIp.Value(),
                    IsSandbox = optionSandbox.HasValue(),
                };
                Log.Information("Host options configured.");
                Log.Debug("{options}", hostOptions);
                using var host = new NamecheapHost(hostOptions);
                Log.Information("Host created");

                var updateOptions = new NamecheapUpdateRecordOptions
                {
                    Sld = optionSld.Value(),
                    Tld = optionTld.Value(),
                    HostName = optionHostName.Value(),
                    RecordType = optionRecordType.Value(),
                    Address = optionAddress.Value(),
                    Ttl = optionTtl.HasValue() ? optionTtl.ParsedValue : default,
                };
                Log.Information("Update options configured.");
                Log.Debug("{options}", updateOptions);

                UpdateRecordResult updateResult = await host.UpdateRecordAsync(updateOptions, cancellationToken);
                Log.Debug("Update completed with status: {status}.", updateResult.Success);

                int appStatusCode = updateResult.Success ? 0 : 1;
                Log.Debug("App execution complete. Exiting with status code: {status}", appStatusCode);
                return appStatusCode;
            });

            Log.Information("Executing app...");
            return app.Execute(args);
        }
    }
}
