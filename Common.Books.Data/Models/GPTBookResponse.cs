using Newtonsoft.Json;

namespace Common.Books.Data.Models;

public class GPTBookResponse
{
    [JsonProperty("title")] public string Title { get; set; }

    [JsonProperty("pages")] public List<GptPage> Pages { get; set; } = new();
    
}

public class GptPage
{
    [JsonProperty("chapter")] public string Chapter { get; set; }

    [JsonProperty("pageNumber")] public int PageNumber { get; set; }

    [JsonProperty("content")] public string Content { get; set; }
}