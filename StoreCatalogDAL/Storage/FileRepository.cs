using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using StoreCatalogDAL.Model;

namespace StoreCatalogDAL.Storage
{
    internal class FileRepository<T>(string filePath) : IRepository<T>
       where T : class, IEntity, new()
    {
        private readonly string _filePath = filePath ?? throw new ArgumentException("File path must not be null or empty.", nameof(filePath));
        private readonly CsvConfiguration _csvConfig = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim
        };
        public IQueryable<T> Items
        {
            get
            {
                if (!File.Exists(_filePath))
                    return Enumerable.Empty<T>().AsQueryable();
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, _csvConfig);
                var records = csv.GetRecords<T>().ToList();
                return records.AsQueryable();
            }
        }
        public async Task<T?> GetAsync(int id, CancellationToken cancel = default)
        {
            var items = await ReadAllAsync(cancel).ConfigureAwait(false);
            return items.SingleOrDefault(item => item.Id == id);
        }

        public async Task<T> AddAsync(T item, CancellationToken cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);
            var items = await ReadAllAsync(cancel).ConfigureAwait(false);
            var newId = items.Count != 0 ? items.Max(existing => existing.Id) + 1 : 1;
            item.Id = newId;
            items.Add(item);
            await WriteAllAsync(items, cancel).ConfigureAwait(false);
            return item;
        }


        public async Task UpdateAsync(T item, CancellationToken cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);
            var items = await ReadAllAsync(cancel).ConfigureAwait(false);
            var index = items.FindIndex(existing => existing.Id == item.Id);
            items[index] = (index == -1) ? throw new InvalidOperationException($"Item with ID {item.Id} not found.") : item;
            await WriteAllAsync(items, cancel).ConfigureAwait(false);
        }

        public async Task RemoveAsync(int id, CancellationToken cancel = default)
        {
            var items = await ReadAllAsync(cancel).ConfigureAwait(false);
            var item = items.SingleOrDefault(existing => existing.Id == id) ?? throw new InvalidOperationException($"Item with ID {id} not found.");
            items.Remove(item);
            await WriteAllAsync(items, cancel).ConfigureAwait(false);
        }

        private async Task<List<T>> ReadAllAsync(CancellationToken cancel = default)
        {
            if (!File.Exists(_filePath))
                return [];
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, _csvConfig);
            return await Task.Run(() => csv.GetRecords<T>().ToList(), cancel).ConfigureAwait(false);
        }

        private async Task WriteAllAsync(List<T> items, CancellationToken cancel = default)
        {
            using var writer = new StreamWriter(_filePath, false);
            using var csv = new CsvWriter(writer, _csvConfig);
            await Task.Run(() =>
            {
                csv.WriteRecords(items);
            }, cancel).ConfigureAwait(false);
        }
        public async Task<IEnumerable<T>?> GetAllAsync(CancellationToken cancel = default) => await ReadAllAsync(cancel).ConfigureAwait(false);
    }
}
