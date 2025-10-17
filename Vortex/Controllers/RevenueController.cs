using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text.Json;
using Vortex.Models;
using iText.Layout.Properties;
using iText.Layout.Element;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.IO.Font;
using iText.Kernel.Colors;


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

            // Tính tổng
            var totalRevenue = data.Sum(x => x.TotalRevenue);
            var totalOrders = data.Sum(x => x.TotalOrders);
            var totalProducts = data.Sum(x => x.TotalProductsSold);

            using var ms = new MemoryStream();
            using (var writer = new PdfWriter(ms))
            {
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Font Unicode
                var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Roboto-Regular.ttf");
                var font = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                var boldFontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "Roboto-Bold.ttf");
                var boldFont = PdfFontFactory.CreateFont(boldFontPath, PdfEncodings.IDENTITY_H);

                // Tiêu đề
                var title = new Paragraph("BÁO CÁO DOANH THU VORTEX PLAY")
                    .SetFont(boldFont)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFontColor(ColorConstants.BLUE);

                var dateRange = new Paragraph($"Từ {fromDate?.ToString("dd/MM/yyyy") ?? "N/A"} đến {toDate?.ToString("dd/MM/yyyy") ?? "N/A"}")
                    .SetFont(font)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.GRAY);

                document.Add(title);
                document.Add(dateRange);
                document.Add(new Paragraph(" "));

                // Tạo bảng
                var table = new Table(new float[] { 3, 3, 2, 2 });
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Header
                string[] headers = { "Ngày", "Doanh thu", "Số đơn", "Số sản phẩm" };
                foreach (var h in headers)
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(h).SetFont(boldFont).SetFontColor(ColorConstants.WHITE))
                        .SetBackgroundColor(ColorConstants.DARK_GRAY)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                        .SetPadding(5));
                }

                // Format ngày theo nhóm
                string dateFormat = groupBy switch
                {
                    "day" => "dd/MM/yyyy",
                    "month" => "MM/yyyy",
                    "year" => "yyyy",
                    _ => "dd/MM/yyyy"
                };

                // Dữ liệu
                foreach (var item in data)
                {
                    table.AddCell(new Cell().Add(new Paragraph(item.Date.ToString(dateFormat)).SetFont(font))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalRevenue.ToString("N0", CultureInfo.InvariantCulture) + " đ").SetFont(font))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalOrders.ToString()).SetFont(font))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    table.AddCell(new Cell().Add(new Paragraph(item.TotalProductsSold.ToString()).SetFont(font))
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                }

                document.Add(table);

                document.Add(new Paragraph(" "));

                // Tổng kết
                var totalSection = new Table(new float[] { 4, 4, 4 });
                totalSection.SetWidth(UnitValue.CreatePercentValue(100));

                totalSection.AddCell(new Cell().Add(new Paragraph($"Tổng doanh thu: {totalRevenue.ToString("N0")} đ").SetFont(boldFont))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5));
                totalSection.AddCell(new Cell().Add(new Paragraph($"Tổng đơn: {totalOrders}").SetFont(boldFont))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5));
                totalSection.AddCell(new Cell().Add(new Paragraph($"Tổng sản phẩm: {totalProducts}").SetFont(boldFont))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5));

                document.Add(totalSection);

        
            }

            return File(ms.ToArray(), "application/pdf", "RevenueReport.pdf");
        }

    }
}
