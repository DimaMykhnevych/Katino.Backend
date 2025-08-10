namespace Katino.Application.DTOs.Category;

public class GetCategoryDto
{
    public IEnumerable<CategoryDto> Categories { get; set; }
    public int ResultsAmount { get; set; }
}
