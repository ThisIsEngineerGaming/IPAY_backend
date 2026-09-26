using System.Collections.Generic;

namespace ExamTest.Domain.Entities.Shop
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }
}
