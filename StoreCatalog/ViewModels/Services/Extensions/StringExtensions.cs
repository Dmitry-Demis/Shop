namespace StoreCatalogPresentation.ViewModels.Services.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrWhiteSpace(params string?[] strings) =>
            strings.All(str => !string.IsNullOrWhiteSpace(str));
    }

}
