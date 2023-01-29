using System.Linq;
using System.Threading.Tasks;

namespace CrossCutting.Interfaces
{
    public interface ISnapshotStore
    {
        IQueryable<T> AsQueryable<T>(string collection);
        Task<T> SingleOrDefaultCollection<T>(string collection);
    }
}
