using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using testMufg.Data;
using testMufg.Models;
using OfficeOpenXml;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace testMufg.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        //Question No 09
        [HttpGet("top3customers")]
        public async Task<IActionResult> GetTopCustomersByMonth(int year, int month)
        {
            try
            {
                var topCustomers = await (from c in _context.Customer
                                          join p in _context.Purchase on c.CustomerCode equals p.CustomerCode
                                          where p.PurchaseDate.Year == year && p.PurchaseDate.Month == month
                                          group p by new
                                          {
                                              c.CustomerCode,
                                              c.CustomerName,
                                              c.CustomerAddress
                                          } into g
                                          orderby g.Sum(p => p.Price) descending
                                          select new
                                          {
                                              g.Key.CustomerCode,
                                              g.Key.CustomerName,
                                              g.Key.CustomerAddress,
                                              TotalPrice = g.Sum(p => p.Price),
                                              PurchasePeriod = $"{g.Select(x => x.PurchaseDate).FirstOrDefault():MMM-yy}"
                                          }).Take(3).ToListAsync();

                if (topCustomers == null || !topCustomers.Any())
                    return NotFound("No customers found for the specified month and year.");

                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the top customers. Please try again later.");
            }
        }


        //Question No 10
        [HttpPost("add-transaction")]
        public async Task<IActionResult> AddTransaction([FromBody] TransactionRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                    return BadRequest("Amount must be greater than zero.");

                var parameters = new[]
                {
                    new SqlParameter("@TellerID", request.TellerID),
                    new SqlParameter("@CustomerCode", request.CustomerCode),
                    new SqlParameter("@TransactionType", request.TransactionType),
                    new SqlParameter("@Currency", request.Currency),
                    new SqlParameter("@Denomination", request.Denomination),
                    new SqlParameter("@Amount", request.Amount),
                    new SqlParameter("@BankPIC", request.BankPIC)
                };

                await _context.Database.ExecuteSqlRawAsync("EXEC sp_AddTransaction @TellerID, @CustomerCode, @TransactionType, @Currency, @Denomination, @Amount, @BankPIC", parameters);

                return Ok("Transaction added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the transaction.");
            }
        }

        [HttpGet("search-transactions")]
        public async Task<IActionResult> SearchTransactions([FromQuery] TransactionFilter filter)
        {
            var parameters = new[]
            {
                new SqlParameter("@TransactionType", filter.TransactionType ?? (object)DBNull.Value),
                new SqlParameter("@StartDate", filter.StartDate ?? (object)DBNull.Value),
                new SqlParameter("@EndDate", filter.EndDate ?? (object)DBNull.Value),
                new SqlParameter("@Currency", filter.Currency ?? (object)DBNull.Value),
                new SqlParameter("@CustomerCode", filter.CustomerCode ?? (object)DBNull.Value)
            };

            var transactions = await _context.CashTransaction
                .FromSqlRaw("EXEC sp_GetDailyTransactions @TransactionType, @StartDate, @EndDate, @Currency, @CustomerCode", parameters)
                .ToListAsync();

            return Ok(transactions);
        }

        [HttpGet("export-transactions")]
        public async Task<IActionResult> ExportTransactions([FromQuery] TransactionFilter filter)
        {
            var transactionsResult = await SearchTransactions(filter);

            if (transactionsResult is OkObjectResult okResult && okResult.Value is List<CashTransaction> transactions)
            {
                if (transactions == null || !transactions.Any())
                    return NotFound("No transactions found for the given filter.");

                // Buat workbook baru
                using var workbook = new XSSFWorkbook(); // Membuat workbook baru
                var worksheet = workbook.CreateSheet("Transactions"); // Membuat worksheet baru

                // Buat header
                var headerRow = worksheet.CreateRow(0);
                headerRow.CreateCell(0).SetCellValue("No");
                headerRow.CreateCell(1).SetCellValue("Customer Code");
                headerRow.CreateCell(2).SetCellValue("Transaction Type");
                headerRow.CreateCell(3).SetCellValue("Currency");
                headerRow.CreateCell(4).SetCellValue("Amount");
                headerRow.CreateCell(5).SetCellValue("Transaction Date");
                headerRow.CreateCell(6).SetCellValue("Bank PIC");

                // Mengisi data
                int row = 1;
                foreach (var transaction in transactions)
                {
                    var dataRow = worksheet.CreateRow(row);
                    dataRow.CreateCell(0).SetCellValue(transaction.TransactionID);
                    dataRow.CreateCell(1).SetCellValue((double)transaction.CustomerCode);
                    dataRow.CreateCell(2).SetCellValue(transaction.TransactionType);
                    dataRow.CreateCell(3).SetCellValue(transaction.Currency);
                    dataRow.CreateCell(4).SetCellValue((double)transaction.Amount); // Pastikan untuk mengonversi ke double jika diperlukan
                    dataRow.CreateCell(5).SetCellValue(transaction.TransactionDate.ToString("yyyy-MM-dd HH:mm:ss")); // Format tanggal
                    dataRow.CreateCell(6).SetCellValue(transaction.BankPIC);
                    row++;
                }

                // Simpan ke MemoryStream
                using var stream = new MemoryStream();
                workbook.Write(stream); // Tulis workbook ke stream
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Transactions{DateTime.Now}.xlsx");
            }

            return StatusCode(500, "An error occurred while retrieving transactions.");
        }

    }

}
