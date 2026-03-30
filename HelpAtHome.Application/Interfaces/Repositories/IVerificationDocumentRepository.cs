using HelpAtHome.Core.Entities;

namespace HelpAtHome.Application.Interfaces.Repositories
{
    public interface IVerificationDocumentRepository : IGenericRepository<VerificationDocument>
    {
        Task<(IEnumerable<VerificationDocument> Items, int Total)> GetPendingPagedAsync(int page, int size);
    }
}
