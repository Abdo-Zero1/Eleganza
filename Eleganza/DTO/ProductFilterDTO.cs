namespace Eleganza.DTO
{
    public class ProductFilterDTO
    {
        public string? SortBy { get; set; }     
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Color { get; set; }      
        public string? Tag { get; set; }
    }
}
