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

        // GET: Vendors (Vendor Directory)
        public async Task<IActionResult> Index(string searchString)
        {
            var vendors = from v in _context.Vendors
                         .Include(v => v.User)
                          select v;

            if (!string.IsNullOrEmpty(searchString))
            {
                vendors = vendors.Where(v => v.BusinessName.Contains(searchString) ||
                                           v.ServiceType.Contains(searchString));
            }

            ViewBag.SearchString = searchString;
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
                .Include(v => v.User)
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

            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Vendor")
            {
                TempData["Error"] = "Only vendors can create vendor profiles.";
                return RedirectToAction("Index", "Home");
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

            var userId = int.Parse(userIdString);

            // Check if vendor profile already exists
            var existingVendor = await _context.Vendors.FirstOrDefaultAsync(v => v.UserId == userId);
            if (existingVendor != null)
            {
                TempData["Error"] = "You already have a vendor profile.";
                return RedirectToAction(nameof(MyProfile));
            }

            if (ModelState.IsValid)
            {
                vendor.UserId = userId;
                vendor.CreatedAt = DateTime.UtcNow;
                _context.Add(vendor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vendor profile created successfully!";
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
                return RedirectToAction(nameof(CreateProfile));
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
                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction(nameof(MyProfile));
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

            var senderId = int.Parse(userIdString);

            // Get vendor to find the recipient (vendor's user account)
            var vendor = await _context.Vendors
                .Include(v => v.User)
                .FirstOrDefaultAsync(v => v.Id == vendorId);

            if (vendor == null)
            {
                TempData["Error"] = "Vendor not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Details), new { id = vendorId });
            }

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = vendor.UserId, // Send to the vendor's user account
                VendorId = vendorId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction(nameof(Details), new { id = vendorId });
        }

        // GET: Vendors/Inbox (NEW - View received messages)
        public async Task<IActionResult> Inbox()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);

            // Check if user is a vendor
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Vendor")
            {
                TempData["Error"] = "Only vendors can access the inbox.";
                return RedirectToAction("Index", "Home");
            }

            // Get all messages sent TO this vendor
            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Vendor)
                .Where(m => m.RecipientId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Count unread messages
            ViewBag.UnreadCount = messages.Count(m => !m.IsRead);

            return View(messages);
        }

        // POST: Vendors/MarkAsRead (NEW - Mark message as read)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);

            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
            {
                return NotFound();
            }

            // Ensure the current user is the recipient
            if (message.RecipientId != userId)
            {
                return Forbid();
            }

            message.IsRead = true;
            _context.Update(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Inbox));
        }

        // POST: Vendors/DeleteMessage (NEW - Delete a message)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);

            var message = await _context.Messages.FindAsync(messageId);

            if (message == null)
            {
                return NotFound();
            }

            // Ensure the current user is the recipient
            if (message.RecipientId != userId)
            {
                return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message deleted successfully.";
            return RedirectToAction(nameof(Inbox));
        }

        private bool VendorExists(int id)
        {
            return _context.Vendors.Any(e => e.Id == id);
        }

        // GET: Vendors/BrowseEvents 
        public async Task<IActionResult> BrowseEvents(string searchString, string category, DateTime? eventDate)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }
        
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Vendor")
            {
                TempData["Error"] = "Only vendors can access this page.";
                return RedirectToAction("Index", "Home");
            }
        
            // Get upcoming events
            var events = _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Bookings)
                .Include(e => e.Ratings)
                .Where(e => e.EventDate >= DateTime.UtcNow) // Only upcoming events
                .AsQueryable();
        
            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.Contains(searchString) || 
                                        e.Description.Contains(searchString) ||
                                        e.Location.Contains(searchString));
            }
        
            // Apply category filter
            if (!string.IsNullOrEmpty(category))
            {
                events = events.Where(e => e.Category == category);
            }
        
            // Apply date filter
            if (eventDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date == eventDate.Value.Date);
            }
        
            // Order by date
            events = events.OrderBy(e => e.EventDate);
        
            ViewBag.SearchString = searchString;
            ViewBag.Category = category;
            ViewBag.EventDate = eventDate?.ToString("yyyy-MM-dd");
        
            return View(await events.ToListAsync());
        }
        
    }
    
}