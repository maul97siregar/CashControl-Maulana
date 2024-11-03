using System.ComponentModel.DataAnnotations;

namespace testMufg.Models
{
    public class Customer
    {
        [Key]
        public decimal CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public int CustomerType { get; set; }
        public string CustomerAddress { get; set; }
    }
}
