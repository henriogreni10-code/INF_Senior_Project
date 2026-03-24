using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using INF_SP.Data;
using INF_SP.Models;

namespace INF_SP.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events (Browse all events for attendees)
        // GET: Events (Browse all events with search and filters)
        public async Task<IActionResult> Index(string searchString, string location, DateTime? eventDate, string category)
        {
            var events = from e in _context.Events
                         where e.EventDate >= DateTime.UtcNow
                         select e;

            // Search by title or description
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.Title.Contains(searchString) ||
                                        e.Description.Contains(searchString));
            }

            // Filter by location
            if (!string.IsNullOrEmpty(location))
            {
                events = events.Where(e => e.Location.Contains(location));
            }

            // Filter by date
            if (eventDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date == eventDate.Value.Date);
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                events = events.Where(e => e.Category == category);
            }

            // Store filter values in ViewBag for the view
            ViewBag.SearchString = searchString;
            ViewBag.Location = location;
            ViewBag.EventDate = eventDate;
            ViewBag.Category = category;

            return View(await events
                .Include(e => e.Organizer)
                .Include(e => e.Bookings)
                .OrderBy(e => e.EventDate)
                .ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Bookings)
                .Include(e => e.Ratings)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if current user is already registered
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdString))
            {
                var userId = int.Parse(userIdString);
                ViewBag.IsRegistered = await _context.Bookings
                    .AnyAsync(b => b.EventId == id && b.AttendeeId == userId);
                
                ViewBag.UserRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId);
            }

            return View(eventItem);
        }

        // GET: Events/MyEvents (Organizer's events)
        public async Task<IActionResult> MyEvents()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var events = await _context.Events
                .Where(e => e.OrganizerId == userId)
                .Include(e => e.Bookings)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            return View(events);
        }

        // GET: Events/MyRegistrations (Attendee's registered events)
        public async Task<IActionResult> MyRegistrations()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var bookings = await _context.Bookings
                .Where(b => b.AttendeeId == userId)
                .Include(b => b.Event)
                .ThenInclude(e => e.Organizer)
                .OrderByDescending(b => b.Event.EventDate)
                .ToListAsync();

            return View(bookings);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventItem)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            eventItem.OrganizerId = int.Parse(userIdString);
            eventItem.EventDate = DateTime.SpecifyKind(
            eventItem.EventDate, DateTimeKind.Utc);

            if (ModelState.IsValid)
            {
                _context.Add(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(MyEvents));
            }

            return View(eventItem);
        }
        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            eventItem.EventDate = DateTime.SpecifyKind(
            eventItem.EventDate, DateTimeKind.Utc);

            // Check if current user is the organizer
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || eventItem.OrganizerId != int.Parse(userIdString))
            {
                return Forbid();
            }

            return View(eventItem);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || eventItem.OrganizerId != int.Parse(userIdString))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Event updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyEvents));
            }
            return View(eventItem);
        }

        // POST: Events/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || eventItem.OrganizerId != int.Parse(userIdString))
            {
                return Forbid();
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyEvents));
        }

        // POST: Events/RSVP/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RSVP(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var eventItem = await _context.Events.Include(e => e.Bookings).FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if already registered
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.EventId == id && b.AttendeeId == userId);

            if (existingBooking != null)
            {
                TempData["Error"] = "You are already registered for this event.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Check capacity
            var currentBookings = eventItem.Bookings?.Count ?? 0;
            if (currentBookings >= eventItem.Capacity)
            {
                TempData["Error"] = "This event is at full capacity.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var booking = new Booking
            {
                EventId = id,
                AttendeeId = userId
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Successfully registered for the event!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Events/CancelRSVP/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRSVP(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.EventId == id && b.AttendeeId == userId);

            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Successfully cancelled your registration.";
            return RedirectToAction(nameof(MyRegistrations));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
        // GET: Events/Attendees/5 (View registered attendees for an event)
        public async Task<IActionResult> Attendees(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdString);

            var eventItem = await _context.Events
                .Include(e => e.Organizer)
                .Include(e => e.Bookings)
                    .ThenInclude(b => b.Attendee)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if current user is the organizer
            if (eventItem.OrganizerId != userId)
            {
                TempData["Error"] = "You don't have permission to view attendees for this event.";
                return RedirectToAction(nameof(MyEvents));
            }

            return View(eventItem);
        }

        // POST: Events/RateEvent
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RateEvent(int eventId, int stars, string comment)
{
    var userIdString = HttpContext.Session.GetString("UserId");
    if (string.IsNullOrEmpty(userIdString))
    {
        return RedirectToAction("Login", "Account");
    }

    var userId = int.Parse(userIdString);

    // Check if user is registered for this event
    var booking = await _context.Bookings
        .FirstOrDefaultAsync(b => b.EventId == eventId && b.AttendeeId == userId);

    if (booking == null)
    {
        TempData["Error"] = "You must be registered for this event to rate it.";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    // Check if event has already happened
    var eventItem = await _context.Events.FindAsync(eventId);
    if (eventItem != null && eventItem.EventDate > DateTime.UtcNow)
    {
        TempData["Error"] = "You can only rate events that have already occurred.";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    // Check if user already rated this event
    var existingRating = await _context.Ratings
        .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

    if (existingRating != null)
    {
        // Update existing rating
        existingRating.Stars = stars;
        existingRating.Comment = comment ?? string.Empty;
        existingRating.CreatedAt = DateTime.UtcNow;
        _context.Update(existingRating);
        TempData["Success"] = "Your rating has been updated!";
    }
    else
    {
        // Create new rating
        var rating = new Rating
        {
            EventId = eventId,
            UserId = userId,
            Stars = stars,
            Comment = comment ?? string.Empty
        };
        _context.Ratings.Add(rating);
        TempData["Success"] = "Thank you for rating this event!";
    }

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id = eventId });
}

    }
}