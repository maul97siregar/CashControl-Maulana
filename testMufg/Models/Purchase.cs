using System.ComponentModel.DataAnnotations;

namespace testMufg.Models
{
    public class Purchase
    {
        [Key]
        public int ID { get; set; }
        public decimal CustomerCode { get; set; }
        public string Item { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}
