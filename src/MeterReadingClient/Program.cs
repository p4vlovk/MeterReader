using Grpc.Net.ClientFactory;
using MeterReadingClient;
using MeterReadingClient.Interceptors;
using static MeterReader.gRPC.MeterReaderService;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) => services
        .AddHostedService<Worker>()
        .AddTransient<ReadingGenerator>()
        .AddGrpcClient<MeterReaderServiceClient>(options => options
            .Address = new Uri(hostContext.Configuration["ServiceUrl"]))
        .AddInterceptor(InterceptorScope.Client, serviceProvider =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<TracerInterceptor>>();
            
            return new TracerInterceptor(logger);
        }))
        // .ConfigurePrimaryHttpMessageHandler(() =>
        // {
        //     var certificate = new X509Certificate2(
        //         hostContext.Configuration["Settings:Certificate:Name"],
        //         hostContext.Configuration["Settings:Certificate:Password"]);
        //
        //     var handler = new HttpClientHandler();
        //     handler.ClientCertificates.Add(certificate);
        //
        //     return handler;
        // }))
    .Build();

await host.RunAsync();