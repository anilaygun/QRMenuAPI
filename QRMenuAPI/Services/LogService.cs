using QRMenuAPI.Data;
using QRMenuAPI.Models;

namespace QRMenuAPI.Services
{
    public class LogService : ILogService
    {
        private readonly AppDbContext _context;

        public LogService(AppDbContext context)
        {
            _context = context;
        }
        public async Task LogAsync(LogEntry entry)
        {
            _context.LogEntries.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}
