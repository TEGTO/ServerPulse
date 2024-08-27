using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Services.Interfaces
{
    public interface ISlotDataPicker
    {
        Task<SlotData> GetSlotDataAsync(string key, CancellationToken cancellationToken);
    }
}