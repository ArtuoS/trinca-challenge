using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossCutting
{
    public class SnapshotStore
    {
        private readonly Database _database;

        public SnapshotStore(Database database)
        {
            _database = database;
        }

        public IQueryable<T> AsQueryable<T>(string collection)
            => _database.GetContainer(collection).GetItemLinqQueryable<T>();
    }

    public static class SnapshotStoreExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> queryable)
        {
            var snapshots = new List<T>();

            var feedIterator = queryable.ToFeedIterator();
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                foreach (var snapshot in response)
                    snapshots.Add(snapshot);
            }

            return snapshots;
        }

        public static async Task<T> SingleOrDefaultAsync<T>(this IQueryable<T> queryable)
        {
            var feedIterator = queryable.ToFeedIterator();
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                return response.SingleOrDefault();
            }
            return default;
        }
    }
}