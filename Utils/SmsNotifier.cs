using InventorySyetem1.Models;

namespace InventorySyetem1.Utils;

public class SmsNotifier: INotifier
{
    public void SendNotification(string recipient, string message)
    {
        Console.WriteLine($"發送簡訊至{recipient}: {message}");
    }
}