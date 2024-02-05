using System.ComponentModel.DataAnnotations;

namespace Common.Books.Data.Entities;

public class BaseEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
}