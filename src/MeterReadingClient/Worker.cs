// ReSharper disable TemplateIsNotCompileTimeConstantProblem
namespace MeterReadingClient;

using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using MeterReader.gRPC;
using static MeterReader.gRPC.MeterReaderService;

[SuppressMessage("Usage", "CA2254:Template should be a static expression")]
public class Worker : BackgroundService
{
    private readonly MeterReaderServiceClient client;
    private readonly ReadingGenerator generator;
    private readonly ILogger<Worker> logger;
    private readonly IConfiguration configuration;
    private readonly int customerId;
    
    private string token;
    private DateTime expiration;

    public Worker(
        MeterReaderServiceClient client,
        ReadingGenerator generator,
        ILogger<Worker> logger,
        IConfiguration configuration)
    {
        this.client = client;
        this.generator = generator;
        this.logger = logger;
        this.configuration = configuration;
        this.customerId = configuration.GetValue<int>("CustomerId");
        this.token = string.Empty;
        this.expiration = DateTime.MinValue;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!this.NeedsLogin() || await this.RequestTokenAsync())
            {
                var headers = new Metadata { { "Authorization", $"Bearer {this.token}" } };
                var packet = new ReadingPacket { Status = ReadingStatus.Success };
                for (int i = 0; i < 5; i++)
                {
                    var reading = await this.generator.GenerateAsync(this.customerId);
                    packet.Readings.Add(reading);
                }
    
                var result = this.client.AddReading(packet, headers, cancellationToken: stoppingToken);
                this.logger.LogInformation(result.Status == ReadingStatus.Success
                    ? "Successfully called GRPC"
                    : "Failed to call GRPC");
            }
            else
            {
                this.logger.LogInformation("Failed to get JWT token");
            }
            
            await Task.Delay(5000, stoppingToken);
        }
    }

    // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    // {
    //     while (!stoppingToken.IsCancellationRequested)
    //     {
    //         try
    //         {
    //             if (!this.NeedsLogin() || await this.RequestTokenAsync())
    //             {
    //                 var headers = new Metadata { { "Authorization", $"Bearer {this.token}" } };
    //                 var stream = this.client.AddDuplexStream(headers, cancellationToken: stoppingToken);
    //                 for (int i = 0; i < 5; i++)
    //                 {
    //                     var reading = await this.generator.GenerateAsync(this.customerId);
    //                     await stream.RequestStream.WriteAsync(reading, stoppingToken);
    //                     await Task.Delay(500, stoppingToken);
    //                 }
    //
    //                 await stream.RequestStream.CompleteAsync();
    //                 while (await stream.ResponseStream.MoveNext(stoppingToken))
    //                 {
    //                     this.logger.LogWarning($"From server: {stream.ResponseStream.Current.Message}");
    //                 }
    //                 
    //                 this.logger.LogInformation("Finished calling GRPC");
    //             }
    //             else
    //             {
    //                 this.logger.LogInformation("Failed to get JWT token");
    //             }
    //         }
    //         catch (RpcException ex)
    //         {
    //             this.logger.LogError(ex.Message);
    //         }
    //         
    //         await Task.Delay(5000, stoppingToken);
    //     }
    // }

    private bool NeedsLogin()
        => string.IsNullOrWhiteSpace(this.token) || this.expiration < DateTime.UtcNow;

    private async Task<bool> RequestTokenAsync()
    {
        try
        {
            var request = new TokenRequest
            {
                Username = this.configuration["Settings:Username"],
                Password = this.configuration["Settings:Password"]
            };

            var result = await this.client.GenerateTokenAsync(request);
            if (result.Success)
            {
                this.token = result.Token;
                this.expiration = result.Expiration.ToDateTime();
            }

            return true;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex.Message);
        }

        return false;
    }
}