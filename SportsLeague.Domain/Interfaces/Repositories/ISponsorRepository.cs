using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.Domain.Interfaces
{
    public interface ISponsorRepository : IGenericRepository<Sponsor>
    {
        Task<bool> ExistsByNameAsync(string name);
    }
}