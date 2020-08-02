using Newtonsoft.Json;

namespace BookShop.DataProcessor.ExportDto
{
    public class ExportAuthorBooksDto
    {
        [JsonProperty("BookName")]
        public string Name { get; set; }

        [JsonProperty("BookPrice")]
        public string Price { get; set; }
    }
}
