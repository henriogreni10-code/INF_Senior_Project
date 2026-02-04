using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using INF_SP.Data;
using INF_SP.Models;

namespace INF_SP.Controllers
{
    public class VendorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendors (Directory)
        public async Task<IActionResult> Index(string searchString)
        {
            var vendors = from v in _context.Vendors.Include(v => v.User)
                         select v;

            if (!string.IsNullOrEmpty(searchString))
            {
                vendors = vendors.Where(v => v.BusinessName.Contains(searchString) || 
                                            v.ServiceType.Contains(searchString));
            }

            return View(await vendors.ToListAsync());
        }

        // GET: Vendors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.Vendors
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // GET: Vendors/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendor == null)
            {
                return RedirectToAction(nameof(CreateProfile));
            }

            return View(vendor);
        }

        // GET: Vendors/CreateProfile
        public IActionResult CreateProfile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var existingVendor = _context.Vendors.FirstOrDefault(v => v.UserId == userId);

            if (existingVendor != null)
            {
                return RedirectToAction(nameof(MyProfile));
            }

            return View();
        }

        // POST: Vendors/CreateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProfile(Vendor vendor)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            vendor.UserId = int.Parse(userIdString);

            if (ModelState.IsValid)
            {
                _context.Add(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyProfile));
            }
            return View(vendor);
        }

        // GET: Vendors/EditProfile
        public async Task<IActionResult> EditProfile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var vendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == userId);

            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        // POST: Vendors/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(Vendor vendor)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            if (vendor.UserId != userId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vendor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VendorExists(vendor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyProfile));
            }
            return View(vendor);
        }

        // POST: Vendors/SendMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int vendorId, string messageText)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null)
            {
                return NotFound();
            }

            var message = new Message
            {
                SenderId = int.Parse(userIdString),
                RecipientId = vendor.UserId,
                VendorId = vendorId,
                MessageText = messageText
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction(nameof(Details), new { id = vendorId });
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }
    }
}