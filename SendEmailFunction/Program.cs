using Microsoft.Extensions.Hosting;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;

var builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<ISmtpClient, SmtpClient>();
    });

builder.Build().Run();