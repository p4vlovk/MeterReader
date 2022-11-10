namespace MeterReaderLib.Models;

using System.ComponentModel.DataAnnotations;

public class CredentialModel
{
    [Required] public string? UserName { get; set; }
    [Required] public string? Passcode { get; set; }
}