using InventorySyetem1.Models;

namespace InventorySyetem1.Utils;

public interface INotifier
{
    void SendNotification(string recipient, string message);
}