using Common.Books.Data.Entities;
using Common.Books.Models;

namespace Common.Books.Extensions;

public static class EntityExtensions
{
    public static PageDto ToPageDto(this Page? page, Dictionary<string, string>? metaData = null)
    {
        var pageDto = new PageDto
        {
            Id = page.Id,
            BookId = page.BookId,
            PageNumber = page.PageNumber,
            Content = page.Content,
            Chapter = page.Chapter,
            PageType = page.PageType,
            Prompt = page.Prompt,
            PageUrl = page.PageUrl,
            ImageUrl = page.ImageUrl,
            MetaData = metaData ?? new Dictionary<string, string>()
        };

        return pageDto;
    }
}