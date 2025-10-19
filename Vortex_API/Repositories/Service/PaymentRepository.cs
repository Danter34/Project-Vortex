using System.Security.Cryptography;
using System.Text;
using Vortex_API.Data;
using Vortex_API.Model.Domain;
using Vortex_API.Repositories.Interface;
using Vortex_API.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;

namespace Vortex_API.Repositories.Service
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public PaymentRepository(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<PaymentResponse> CreatePayment(PaymentRequest dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            var momoConfig = _config.GetSection("MomoSettings");
            var endpoint = momoConfig["Endpoint"];
            var partnerCode = momoConfig["PartnerCode"];
            var accessKey = momoConfig["AccessKey"];
            var secretKey = momoConfig["SecretKey"];
            var returnUrl = momoConfig["ReturnUrl"];
            var notifyUrl = momoConfig["NotifyUrl"];

            string orderInfo = $"Thanh toán đơn hàng #{order.Id}";
            string amount = order.TotalAmount.ToString("0");
            string momoOrderId = DateTime.Now.Ticks.ToString(); // orderId MoMo dùng
            string requestId = momoOrderId;
            string requestType = "captureWallet";

            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData=&ipnUrl={notifyUrl}&orderId={momoOrderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            string signature = CreateSignature(rawHash, secretKey);

            var body = new
            {
                partnerCode,
                partnerName = "Vortex",
                storeId = "VortexShop",
                requestId,
                amount,
                orderId = momoOrderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                requestType,
                extraData = "",
                signature
            };

            using var client = new HttpClient();
            var response = await client.PostAsync(endpoint,
                new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"));

            var json = await response.Content.ReadAsStringAsync();
            dynamic momoResponse = JsonConvert.DeserializeObject(json)!;

            Console.WriteLine("Momo response: " + json);

            var payment = new Payment
            {
                OrderId = order.Id,
                MomoOrderId = momoOrderId, // lưu lại để truy xuất khi MoMo redirect
                Amount = order.TotalAmount,
                PaymentMethod = "MoMo",
                Status = "Đang xử lý",
                PayUrl = momoResponse.payUrl
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                PayUrl = payment.PayUrl!,
                Status = payment.Status
            };
        }
        public async Task<bool> HandleMomoReturn(string orderId, string resultCode)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.MomoOrderId == orderId);

            if (payment == null) return false;

            if (resultCode == "0") 
            {
                payment.Status = "Đã thanh toán";

                var order = payment.Order;
                if (order != null)
                {
                    order.Status = "Đang xử lý";

                    foreach (var item in order.Items)
                    {
                        var product = item.Product;
                        if (product != null)
                        {
                            if (product.StockQuantity < item.Quantity)
                                throw new Exception($"Sản phẩm '{product.Title}' không đủ hàng (còn {product.StockQuantity}).");

                            product.StockQuantity -= item.Quantity;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }

            payment.Status = "Thanh toán thất bại";
            await _context.SaveChangesAsync();
            return false;
        }

        public async Task<bool> HandleMomoNotify(string orderId, string resultCode)
        {
            return await HandleMomoReturn(orderId, resultCode);
        }

        private string CreateSignature(string rawData, string secretKey)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
        public async Task CheckFailedPaymentsAsync()
        {
            var now = DateTime.Now;

            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.Status == "Đang xử lý" || p.Status == "Thanh toán thất bại")
                .ToListAsync();

            var expiredPayments = payments
                .Where(p => p.Status == "Thanh toán thất bại" ||
                            (p.Status == "Đang xử lý" && (now - p.CreatedAt).TotalMinutes > 2))
                .ToList();

            foreach (var payment in expiredPayments)
            {
                if (payment.Order != null)
                {
                    payment.Order.Status = "Thanh toán thất bại";
                    payment.Status = "Thanh toán thất bại";
                }
            }

            await _context.SaveChangesAsync();
        }
        public async Task<PaymentResponse> CreateVnPayPayment(PaymentRequest dto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);
            if (order == null) throw new Exception("Order not found");

            var config = _config.GetSection("VnPaySettings");
            string tmnCode = config["TmnCode"];
            string hashSecret = config["HashSecret"];
            string baseUrl = config["BaseUrl"];
            string returnUrl = config["ReturnUrl"];

            var vnpParams = new SortedDictionary<string, string>()
    {
        {"vnp_Version", "2.1.0"},
        {"vnp_Command", "pay"},
        {"vnp_TmnCode", tmnCode},
        {"vnp_Amount", ((long)(order.TotalAmount * 100)).ToString()}, // nhân 100
        {"vnp_CurrCode", "VND"},
        {"vnp_TxnRef", DateTime.Now.Ticks.ToString()},
        {"vnp_OrderInfo", $"Thanh toán đơn hàng #{order.Id}"},
        {"vnp_OrderType", "other"},
        {"vnp_Locale", "vn"},
        {"vnp_ReturnUrl", returnUrl},
        {"vnp_IpAddr", "127.0.0.1"},
        {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")}
    };

            var rawData = string.Join("&", vnpParams
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(hashSecret)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                secureHash = BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
            }

            var query = string.Join("&", vnpParams
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));

            string paymentUrl = $"{baseUrl}?{query}&vnp_SecureHash={secureHash}";

            Console.WriteLine("==== RAWDATA (encoded) ====");
            Console.WriteLine(rawData);
            Console.WriteLine("==== SECURE HASH ====");
            Console.WriteLine(secureHash);
            Console.WriteLine("==== FINAL URL ====");
            Console.WriteLine(paymentUrl);

            var payment = new Payment
            {
                OrderId = order.Id,
                MomoOrderId = vnpParams["vnp_TxnRef"],
                Amount = order.TotalAmount,
                PaymentMethod = "VNPay",
                Status = "Đang xử lý",
                PayUrl = paymentUrl
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                Amount = payment.Amount,
                PayUrl = payment.PayUrl!,
                Status = payment.Status
            };
        }
        public async Task<bool> HandleVnPayReturn(IDictionary<string, string> queryParams)
        {
            if (!queryParams.TryGetValue("vnp_TxnRef", out string? orderId) ||
                !queryParams.TryGetValue("vnp_ResponseCode", out string? responseCode))
                return false;

            var payment = await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.MomoOrderId == orderId);

            if (payment == null) return false;

            if (responseCode == "00") // thành công
            {
                payment.Status = "Đã thanh toán";
                if (payment.Order != null)
                {
                    payment.Order.Status = "Đang xử lý";
                    foreach (var item in payment.Order.Items)
                    {
                        if (item.Product != null)
                        {
                            if (item.Product.StockQuantity < item.Quantity)
                                throw new Exception($"Sản phẩm '{item.Product.Title}' không đủ hàng.");
                            item.Product.StockQuantity -= item.Quantity;
                        }
                    }
                }
            }
            else
            {
                payment.Status = "Thanh toán thất bại";
                if (payment.Order != null)
                    payment.Order.Status = "Thanh toán thất bại";
            }

            await _context.SaveChangesAsync();
            return responseCode == "00";
        }


        public static string CreateRequestUrl(Dictionary<string, string> vnp_Params, string vnp_HashSecret, string vnp_Url)
        {
            // Sắp xếp theo key tăng dần
            var sorted = vnp_Params.OrderBy(k => k.Key);

            //  Tạo chuỗi dữ liệu để hash
            var signData = string.Join("&", sorted
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

            //  Tạo secure hash
            string signValue;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
            {
                byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                signValue = BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
            }

            // Tạo URL đầy đủ
            var query = string.Join("&", sorted
                .Where(p => !string.IsNullOrEmpty(p.Value))
                .Select(p => $"{p.Key}={WebUtility.UrlEncode(p.Value)}"));

            string paymentUrl = $"{vnp_Url}?{query}&vnp_SecureHash={signValue}";
            return paymentUrl;
        }

    }
}
