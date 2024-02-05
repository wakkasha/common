namespace Common.Books.Interfaces;

public interface IHtmlService
{
    Task<string> GetAndApplyReplacementsAsync(string templateName, Dictionary<string, string> data);
}