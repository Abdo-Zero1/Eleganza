using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Models
{
    public class Cart
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }    
        public string ApplicationUserId { get; set; }
        [JsonIgnore]
        public ApplicationUser ApplicationUser { get; set; }
        public int Count { get; set; }
    }
}
