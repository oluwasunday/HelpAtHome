using HelpAtHome.Application.Interfaces.Repositories;
using HelpAtHome.Application.Repositories;
using HelpAtHome.Core.Entities;
using HelpAtHome.Infrastructure.Data;

namespace HelpAtHome.Infrastructure.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly AppDbContext _ctx;
        public TransactionRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
    }
}
