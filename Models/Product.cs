using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int quantity { get; set; }
        public string ImageUrl { get; set; }
        public string Color { get; set; }
        public string Tag { get; set; }
        public int Rating { get; set; } 
        public int CategoryID { get; set; }
        [JsonIgnore]
        public Category Category { get; set; }
    }
}
