using FluentValidation;

namespace Common.Books.Models;

public class BookRequestDto
{
    public required int Age { get; set; }
    public required string Name { get; set; }
    public required string Gender { get; set; }
    public required string FavoriteColor { get; set; }
    public required string Ethnicity { get; set; }
    public required string RequestedBy { get; set; }
}


public class BookRequestDtoValidator : AbstractValidator<BookRequestDto> 
{
    public BookRequestDtoValidator() 
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Ethnicity).NotNull().NotEmpty();
        RuleFor(x => x.Gender).NotNull().NotEmpty();
        RuleFor(x => x.FavoriteColor).NotNull().NotEmpty();
        RuleFor(x => x.Age).InclusiveBetween(1, 150);
    }
}