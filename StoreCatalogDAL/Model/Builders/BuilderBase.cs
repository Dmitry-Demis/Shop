using System.Reflection;

namespace StoreCatalogDAL.Model.Builders
{
    public abstract class BuilderBase<T, TBuilder>
        where T : class, new()
        where TBuilder : BuilderBase<T, TBuilder>, new()
    {
        protected T Entity { get; private init; } = new();

        // Создание нового объекта
        public static TBuilder Create()
        {
            var builder = new TBuilder
            {
                Entity = new T()
            };
            return builder;
        }

        // Установка значения свойства
        public TBuilder SetProperty(string propertyName, object? value) // Указываем, что value может быть null
        {
            var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"Property '{propertyName}' not found on type {typeof(T).Name}");

            // Проверка совместимости типов
            if (value != null && !property.PropertyType.IsInstanceOfType(value))
                throw new ArgumentException($"Cannot assign value of type {value.GetType().Name} to property {propertyName} of type {property.PropertyType.Name}.");

            // Если value равно null, проверяем, поддерживает ли свойство null
            if (value == null && property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
            {
                throw new ArgumentException($"Property '{propertyName}' does not accept null values.");
            }

            property.SetValue(Entity, value);
            return (TBuilder)this; // Возвращаем текущий строитель
        }

        // Метод для инициализации объекта данными из существующего объекта
        public TBuilder FromExisting(T existingEntity)
        {
            // Копируем значения свойств из существующего объекта в новый объект
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(existingEntity);
                SetProperty(property.Name, value); // Используем SetProperty для копирования значений
            }
            return (TBuilder)this; // Возвращаем текущий строитель
        }

        // Строим конечный объект
        public T Build() => Entity;
    }
}
