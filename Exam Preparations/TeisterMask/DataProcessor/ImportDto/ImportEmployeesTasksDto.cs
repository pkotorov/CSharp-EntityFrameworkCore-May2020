using Newtonsoft.Json;

namespace TeisterMask.DataProcessor.ImportDto
{
    public class ImportEmployeesTasksDto
    {
        [JsonProperty("Tasks")]
        public int[] TaskId { get; set; }
    }
}
