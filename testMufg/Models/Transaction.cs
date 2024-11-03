using System.ComponentModel.DataAnnotations;

namespace testMufg.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class CashTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        [ForeignKey("Teller")]
        [Required]
        public int TellerID { get; set; }

        [Required]
        [Range(0, 99999999)]
        public decimal CustomerCode { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        [DataType(DataType.Currency)]
        public decimal? Denomination { get; set; }

        [DataType(DataType.Currency)]
        [Required]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string BankPIC { get; set; }

        // Navigation property for the Teller related to this transaction
        public virtual Teller Teller { get; set; }
    }

    public class Teller
    {
        public int TellerID { get; set; }

        [Required]
        [StringLength(100)]
        public string TellerName { get; set; }

        [DataType(DataType.Currency)]
        public decimal OpeningBalance { get; set; }

        [DataType(DataType.Currency)]
        public decimal ClosingBalance { get; set; }

        [Required]
        public DateTime Date { get; set; }

        // Navigation property for related CashTransactions
        public virtual ICollection<CashTransaction> CashTransactions { get; set; }
    }

    public class TransactionRequest
    {
        [Required]
        public int TellerID { get; set; }

        [Required]
        [StringLength(20)]
        public string CustomerCode { get; set; }

        [Required]
        [StringLength(10)]
        public string TransactionType { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; }

        [StringLength(10)]
        public string Denomination { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string BankPIC { get; set; }
    }

    public class TransactionFilter
    {
        public string TransactionType { get; set; }

        public string Currency { get; set; }

        public string CustomerCode { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
