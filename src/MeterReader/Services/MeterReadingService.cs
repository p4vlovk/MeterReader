namespace MeterReader.Services;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MeterReader.gRPC;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using static MeterReader.gRPC.MeterReaderService;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
// [Authorize(AuthenticationSchemes = CertificateAuthenticationDefaults.AuthenticationScheme)]
public class MeterReadingService : MeterReaderServiceBase
{
    private readonly IReadingRepository repository;
    private readonly ILogger<MeterReadingService> logger;
    private readonly JwtTokenValidationService tokenService;

    public MeterReadingService(
        IReadingRepository repository,
        ILogger<MeterReadingService> logger,
        JwtTokenValidationService tokenService)
    {
        this.repository = repository;
        this.logger = logger;
        this.tokenService = tokenService;
    }
    
    [AllowAnonymous]
    public override async Task<TokenResponse> GenerateToken(TokenRequest request, ServerCallContext context)
    {
        var credentials = new CredentialModel
        {
            UserName = request.Username,
            Passcode = request.Password
        };

        var result = await this.tokenService.GenerateTokenModelAsync(credentials);

        return result.Success
            ? new TokenResponse
            {
                Success = true,
                Token = result.Token,
                Expiration = Timestamp.FromDateTime(result.Expiration)
            }
            : new TokenResponse
            {
                Success = false
            };
    }

    public override async Task<StatusMessage> AddReading(ReadingPacket request, ServerCallContext context)
    {
        if (request.Status == ReadingStatus.Success)
        {
            foreach (var reading in request.Readings)
            {
                var readingValue = new MeterReading
                {
                    CustomerId = reading.CustomerId,
                    Value = reading.ReadingValue,
                    ReadingDate = reading.ReadingTime.ToDateTime()
                };
                
                this.logger.LogInformation("Adding {ReadingValue}", reading.ReadingValue);
                this.repository.AddEntity(readingValue);
            }

            if (await this.repository.SaveAllAsync())
            {
                this.logger.LogInformation("Successfully saved new readings");
                return new StatusMessage
                {
                    Message = "Successfully added to the database",
                    Status = ReadingStatus.Success
                };
            }
        }
        
        this.logger.LogError("Failed to save new readings");
        return new StatusMessage
        {
            Message = "Failed to store readings in database",
            Status = ReadingStatus.Success
        };
    }

    public override async Task AddDuplexStream(
        IAsyncStreamReader<ReadingMessage> requestStream,
        IServerStreamWriter<ErrorMessage> responseStream,
        ServerCallContext context)
    {
        while (await requestStream.MoveNext())
        {
            var message = requestStream.Current;

            if (message.ReadingValue < 500)
            {
                await responseStream.WriteAsync(new ErrorMessage
                {
                    Message = $"Value less than 500. Value: {message.ReadingValue}"
                });
            }
            
            var readingValue = new MeterReading
            {
                CustomerId = message.CustomerId,
                Value = message.ReadingValue,
                ReadingDate = message.ReadingTime.ToDateTime()
            };
            
            this.logger.LogInformation("Adding {ReadingValue} from stream", message.ReadingValue);
            this.repository.AddEntity(readingValue);
            await this.repository.SaveAllAsync();
        }

        // return new Empty();
    }
}