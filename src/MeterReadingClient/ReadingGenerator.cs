namespace MeterReadingClient;

using Google.Protobuf.WellKnownTypes;
using MeterReader.gRPC;

public class ReadingGenerator
{
    public Task<ReadingMessage> GenerateAsync(int customerId)
        => Task.FromResult(new ReadingMessage
        {
            CustomerId = customerId,
            ReadingTime = Timestamp.FromDateTime(DateTime.UtcNow),
            ReadingValue = new Random().Next(10000)
        });
}