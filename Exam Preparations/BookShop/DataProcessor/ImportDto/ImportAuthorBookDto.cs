namespace BookShop.DataProcessor.ImportDto
{
    using Newtonsoft.Json;

    public class ImportAuthorBookDto
    {
        [JsonProperty("Id")]
        public int? BookId { get; set; }
    }
}
