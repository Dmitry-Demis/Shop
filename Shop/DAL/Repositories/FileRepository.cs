using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using Shop.DAL.Models;

namespace Shop.DAL.Repositories
{
    public class FileRepository<T> : IRepository<T>
        where T : class, IEntity, new()
    {
        private readonly string _filePath;
        private readonly CsvConfiguration _csvConfig;

        public FileRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be null or empty.", nameof(filePath));

            _filePath = filePath;

            // Настройка для CsvHelper
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                TrimOptions = TrimOptions.Trim
            };
        }

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

        public async Task<T?> GetAsync(int id, CancellationToken Cancel = default)
        {
            var items = await ReadAllAsync(Cancel).ConfigureAwait(false);
            return items.SingleOrDefault(item => item.Id == id);
        }

        public async Task<T> AddAsync(T item, CancellationToken Cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);

            var items = await ReadAllAsync(Cancel).ConfigureAwait(false);
            items.Add(item);

            await WriteAllAsync(items, Cancel).ConfigureAwait(false);

            return item;
        }

        public async Task UpdateAsync(T item, CancellationToken Cancel = default)
        {
            ArgumentNullException.ThrowIfNull(item);

            var items = await ReadAllAsync(Cancel).ConfigureAwait(false);
            var index = items.FindIndex(existing => existing.Id == item.Id);

            if (index == -1)
                throw new InvalidOperationException($"Item with ID {item.Id} not found.");

            items[index] = item;

            await WriteAllAsync(items, Cancel).ConfigureAwait(false);
        }

        public async Task RemoveAsync(int id, CancellationToken Cancel = default)
        {
            var items = await ReadAllAsync(Cancel).ConfigureAwait(false);
            var item = items.SingleOrDefault(existing => existing.Id == id);

            if (item == null)
                throw new InvalidOperationException($"Item with ID {id} not found.");

            items.Remove(item);

            await WriteAllAsync(items, Cancel).ConfigureAwait(false);
        }

        private async Task<List<T>> ReadAllAsync(CancellationToken Cancel = default)
        {
            if (!File.Exists(_filePath))
                return [];

            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, _csvConfig);

            return await Task.Run(() => csv.GetRecords<T>().ToList(), Cancel).ConfigureAwait(false);
        }

        private async Task WriteAllAsync(List<T> items, CancellationToken Cancel = default)
        {
            using var writer = new StreamWriter(_filePath, false);
            using var csv = new CsvWriter(writer, _csvConfig);

            await Task.Run(() =>
            {
                csv.WriteRecords(items);
            }, Cancel).ConfigureAwait(false);
        }
    }

}
