using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Json;
using Vortex.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font;
using System.IO;

namespace Vortex.Controllers
{
    public class RevenueController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://localhost:7161/api/Revenue/";

        public RevenueController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("APIClient");
        }

        // Hiển thị bảng
        public async Task<IActionResult> Index(string groupBy = "day", DateTime? fromDate = null, DateTime? toDate = null)
        {
            ViewBag.GroupByList = new List<SelectListItem>
            {
                new SelectListItem { Value = "day", Text = "Ngày", Selected = groupBy=="day" },
                new SelectListItem { Value = "month", Text = "Tháng", Selected = groupBy=="month" },
                new SelectListItem { Value = "year", Text = "Năm", Selected = groupBy=="year" }
            };

            var url = $"{_baseUrl}summary?groupBy={groupBy}";
            if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return View(new List<RevenueViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<RevenueViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RevenueViewModel>();

            // Chuyển Date sang giờ Việt Nam
            foreach (var item in data)
            {
                item.Date = item.Date.ToLocalTime();
            }

            ViewBag.GroupBy = groupBy;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            return View(data);
        }

        // Xuất PDF
        [HttpPost]
        public async Task<IActionResult> ExportPdf(string groupBy = "day", DateTime? fromDate = null, DateTime? toDate = null)
        {
            var url = $"{_baseUrl}summary?groupBy={groupBy}";
            if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return BadRequest("Không lấy được dữ liệu");

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<RevenueViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<RevenueViewModel>();

            foreach (var item in data)
            {
                item.Date = item.Date.ToLocalTime();
            }

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Load font Unicode (Roboto)
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Roboto-Regular.ttf");
                var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                // Header
                document.Add(new Paragraph("Báo cáo doanh thu").SetFont(font).SetFontSize(20));
                document.Add(new Paragraph($"Từ {fromDate?.ToString("dd/MM/yyyy") ?? "N/A"} đến {toDate?.ToString("dd/MM/yyyy") ?? "N/A"}").SetFont(font));
                document.Add(new Paragraph(" ")); // khoảng trắng

                // Table
                var table = new Table(4);
                table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Doanh thu").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số đơn").SetFont(font)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Số sản phẩm").SetFont(font)));

                string dateFormat = groupBy switch
                {
                    "day" => "dd/MM/yyyy",
                    "month" => "MM/yyyy",
                    "year" => "yyyy",
                    _ => "dd/MM/yyyy"
                };

                foreach (var item in data)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Date.ToString(dateFormat)).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalRevenue.ToString("N0", CultureInfo.InvariantCulture)).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalOrders.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalProductsSold.ToString()).SetFont(font)));
                }

                document.Add(table);
                document.Close();
            }

            return File(ms.ToArray(), "application/pdf", "RevenueReport.pdf");
        }
    }
}
