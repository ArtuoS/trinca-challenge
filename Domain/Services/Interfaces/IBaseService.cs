using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IBaseService<T>
    {
        Task<T> GetAsync(string id);
    }
}