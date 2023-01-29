using Domain.Entities;

namespace Domain.Repositories
{
    /// Deixei public para realizar os testes, sem os testes poderia ser trocado para internal para não ser possível acessar de fora do Domain.
    public interface IBbqRepository : IStreamRepository<Bbq>
    {
    }
}