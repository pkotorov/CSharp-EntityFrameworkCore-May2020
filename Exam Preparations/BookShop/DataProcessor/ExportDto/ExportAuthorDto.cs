using Newtonsoft.Json;

namespace BookShop.DataProcessor.ExportDto
{
    public class ExportAuthorDto
    {
        [JsonProperty("AuthorName")]
        public string Name { get; set; }

        public ExportAuthorBooksDto[] Books { get; set; }
    }
}
