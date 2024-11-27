using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GymBooking.Data;
using GymBooking.Models;
using GymBooking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymBooking.Controllers
{
    [Authorize]
    public class GymClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GymClassesController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context
        )
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Get the logged-in user's ID

            // Await the ToListAsync call to ensure we get the actual list of GymClassViewModel
            var gymClasses = await _context
                .GymClasses.Select(g => new GymClassViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    StartTime = g.StartTime,
                    Duration = g.Duration,
                    Description = g.Description,
                    IsBooked = g.AttendingMembers.Any(am => am.ApplicationUserId == userId),
                })
                .ToListAsync(); // Await here to get the actual list of gym classes

            return View(gymClasses); // Pass the list of gym classes to the view
        }

        //BOOKING PASS
        public async Task<IActionResult> BookingToggle(int id)
        {
            // Check if the gym class exists
            var gymClass = await _context
                .GymClasses.Include(g => g.AttendingMembers)
                .ThenInclude(am => am.ApplicationUser) // Load related ApplicationUser data
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gymClass == null)
                return NotFound();

            // Get the logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null)
                return NotFound();

            // Check if the user is already booked
            var existingBooking = gymClass.AttendingMembers.FirstOrDefault(am =>
                am.ApplicationUserId == userId
            );

            if (existingBooking != null)
            {
                // If already booked, unbook by removing from the join table
                _context.Entry(existingBooking).State = EntityState.Deleted;
                TempData["BookingMessage"] = "You have successfully unbooked from the class.";
            }
            else
            {
                // If not booked, create a new booking
                var newBooking = new ApplicationUserGymClass
                {
                    GymClassId = gymClass.Id,
                    ApplicationUserId = user.Id,
                };
                _context.Add(newBooking);
                TempData["BookingMessage"] = "You have successfully booked the class.";
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the Details view for the specific gym class
            return RedirectToAction(nameof(Details), new { id = gymClass.Id });
        }

        // GET: GymClasses
        public async Task<IActionResult> History()
        {
            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //get current date
            var currentDateTime = DateTime.Now;

            // Fetch classes that the user has booked
            var history = await _context
                .GymClasses.Include(g => g.AttendingMembers)
                .ThenInclude(am => am.ApplicationUser)
                .Where(g =>
                    g.StartTime < currentDateTime
                    && g.AttendingMembers.Any(am => am.ApplicationUserId == userId)
                )
                .ToListAsync();
            return View(history);
        }

        public async Task<IActionResult> BookedClasses()
        {
            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //get current date
            var currentDateTime = DateTime.Now;

            // Fetch classes that the user has booked
            var bookedClasses = await _context
                .GymClasses.Include(g => g.AttendingMembers)
                .ThenInclude(am => am.ApplicationUser)
                .Where(g =>
                    g.StartTime > currentDateTime
                    && g.AttendingMembers.Any(am => am.ApplicationUserId == userId)
                )
                .ToListAsync();

            return View(bookedClasses);
        }

        // GET: GymClasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the gym class with its attending members and their information
            var gymClass = await _context
                .GymClasses.Include(g => g.AttendingMembers)
                .ThenInclude(am => am.ApplicationUser) // Load related ApplicationUser data
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gymClass == null)
            {
                return NotFound();
            }

            return View(gymClass);
        }

        [Authorize(Roles = "Admin")]
        // GET: GymClasses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GymClasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GymClass gymClass)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gymClass);
                TempData["CreateMessage"] = "You have Succesfully Created a Gym Class";

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gymClass);
        }

        [Authorize(Roles = "Admin")]
        // GET: GymClasses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymClass = await _context.GymClasses.FindAsync(id);
            if (gymClass == null)
            {
                return NotFound();
            }
            return View(gymClass);
        }

        // POST: GymClasses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,StartTime,Duration,Description")] GymClass gymClass
        )
        {
            if (id != gymClass.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymClassExists(gymClass.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gymClass);
        }

        [Authorize(Roles = "Admin")]
        // GET: GymClasses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymClass = await _context.GymClasses.FirstOrDefaultAsync(m => m.Id == id);
            if (gymClass == null)
            {
                return NotFound();
            }

            return View(gymClass);
        }

        // POST: GymClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gymClass = await _context.GymClasses.FindAsync(id);
            if (gymClass != null)
            {
                _context.GymClasses.Remove(gymClass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GymClassExists(int id)
        {
            return _context.GymClasses.Any(e => e.Id == id);
        }
    }
}
