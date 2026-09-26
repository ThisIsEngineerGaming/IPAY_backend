namespace ExamTest.Application.DTOs.Shop
{
    /// <summary>Manufacturer as returned by the API.</summary>
    public class ManufacturerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>Request body for creating or replacing a manufacturer. The id is assigned by the server / taken from the route.</summary>
    public class SaveManufacturerDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
