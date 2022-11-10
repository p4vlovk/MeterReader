namespace MeterReaderLib.Models;

public class TokenModel
{
    public string? Token { get; internal set; }
    public DateTime Expiration { get; internal set; }
    public bool Success { get; internal set; }
}