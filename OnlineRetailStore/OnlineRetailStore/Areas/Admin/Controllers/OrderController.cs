using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineRetailStore.Data;

namespace OnlineRetailStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // បង្ហាញបញ្ជី Orders របស់អតិថិជនទាំងអស់
        public async Task<IActionResult> Index(string statusFilter)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
            }

            ViewBag.CurrentStatus = statusFilter ?? "All";
            return View(await ordersQuery.ToListAsync());
        }

        // មើលព័ត៌មានលម្អិតរបស់ Order នីមួយៗ
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ប្តូរស្ថានភាព Order (Pending / Paid / Shipped / Completed)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            order.Status = status;

            // កែសម្រួលស្ថានភាព Payment ឱ្យស៊ីគ្នា
            if (order.Payment != null)
            {
                if (status == "Paid" || status == "Completed")
                {
                    order.Payment.PaymentStatus = "Completed";
                    order.Payment.PaymentDate ??= DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}