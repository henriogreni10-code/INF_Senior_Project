using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using INF_SP.Data;
using INF_SP.Models;

namespace INF_SP.Controllers
{
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Messages (Inbox for all users)
        public async Task<IActionResult> Index()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);

            // Get all messages sent TO this user
            var receivedMessages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Vendor)
                .Where(m => m.RecipientId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            // Get all messages sent BY this user
            var sentMessages = await _context.Messages
                .Include(m => m.Recipient)
                .Include(m => m.Vendor)
                .Where(m => m.SenderId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            ViewBag.UnreadCount = receivedMessages.Count(m => !m.IsRead);
            ViewBag.SentMessages = sentMessages;

            return View(receivedMessages);
        }

        // GET: Messages/Compose
        public async Task<IActionResult> Compose(int? recipientId, int? vendorId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            if (recipientId.HasValue)
            {
                var recipient = await _context.Users.FindAsync(recipientId.Value);
                ViewBag.RecipientName = recipient?.Name;
                ViewBag.RecipientId = recipientId.Value;
            }

            if (vendorId.HasValue)
            {
                var vendor = await _context.Vendors
                    .Include(v => v.User)
                    .FirstOrDefaultAsync(v => v.Id == vendorId.Value);
                ViewBag.VendorName = vendor?.BusinessName;
                ViewBag.VendorId = vendorId.Value;
                ViewBag.RecipientId = vendor?.UserId;
                ViewBag.RecipientName = vendor?.User?.Name;
            }

            return View();
        }

        // POST: Messages/Send
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int recipientId, string messageText, int? vendorId, int? eventId)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var senderId = int.Parse(userIdString);

            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var message = new Message
            {
                SenderId = senderId,
                RecipientId = recipientId,
                VendorId = vendorId,
                EventId = eventId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message sent successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Messages/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int originalMessageId, string messageText)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var senderId = int.Parse(userIdString);

            var originalMessage = await _context.Messages.FindAsync(originalMessageId);
            
            if (originalMessage == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(messageText))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var replyMessage = new Message
            {
                SenderId = senderId,
                RecipientId = originalMessage.SenderId, // Send back to original sender
                VendorId = originalMessage.VendorId,
                EventId = originalMessage.EventId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Messages.Add(replyMessage);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reply sent successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Messages/MarkAsRead
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

            if (message.RecipientId != userId)
            {
                return Forbid();
            }

            message.IsRead = true;
            _context.Update(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Messages/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int messageId)
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

            // Only recipient can delete
            if (message.RecipientId != userId)
            {
                return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Message deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}