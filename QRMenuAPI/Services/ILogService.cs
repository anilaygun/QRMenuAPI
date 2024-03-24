using QRMenuAPI.Models;

namespace QRMenuAPI.Services
{
    public interface ILogService
    {
        Task LogAsync(LogEntry entry);
    }
}
