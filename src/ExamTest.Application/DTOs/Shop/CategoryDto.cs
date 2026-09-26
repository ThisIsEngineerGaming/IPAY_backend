namespace ExamTest.Application.DTOs.Shop
{
    /// <summary>Category as returned by the API.</summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }

    /// <summary>Request body for creating or replacing a category. The id is assigned by the server / taken from the route.</summary>
    public class SaveCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }
}
