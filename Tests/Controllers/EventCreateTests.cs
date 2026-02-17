using INF_SP.Controllers;
using INF_SP.Models;
using INF_SP.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;

namespace INF_SP.Tests.Controllers
{
    public class EventCreateTests
    {
        [Fact]
        public void Create_GET_WhenUserLoggedIn_ShouldReturnView()
        {
            // Arrange
            var context = TestHelper.GetInMemoryDbContext();
            var controller = TestHelper.CreateControllerWithSession<EventsController>(context, "1", "Organizer");

            // Act
            var result = controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Create_POST_WithInvalidModelState_ShouldReturnViewWithModel()
        {
            // Arrange
            var context = TestHelper.GetInMemoryDbContext();
            var controller = TestHelper.CreateControllerWithSession<EventsController>(context);
            
            var newEvent = new Event
            {
                Title = "Test Event",
                Description = "Description",
                EventDate = DateTime.UtcNow.AddDays(10),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                Location = "Location",
                Capacity = 50,
                Category = "Workshop"
            };

            // Manually add a model state error
            controller.ModelState.AddModelError("Title", "Title is required");

            // Act
            var result = controller.Create(newEvent).Result;

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeEquivalentTo(newEvent);
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_WithEmptyTitle_ShouldFailValidation()
        {
            // Arrange
            var context = TestHelper.GetInMemoryDbContext();
            var controller = TestHelper.CreateControllerWithSession<EventsController>(context);

            var newEvent = new Event
            {
                Title = "", // Empty title
                Description = "Description",
                EventDate = DateTime.UtcNow.AddDays(10),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                Location = "Location",
                Capacity = 50,
                Category = "Conference"
            };

            // Manually trigger validation
            controller.ModelState.Clear();
            controller.TryValidateModel(newEvent);

            // Act
            var result = controller.Create(newEvent).Result;

            // Assert
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_WithZeroCapacity_ShouldFailValidation()
        {
            // Arrange
            var context = TestHelper.GetInMemoryDbContext();
            var controller = TestHelper.CreateControllerWithSession<EventsController>(context);

            var newEvent = new Event
            {
                Title = "Invalid Capacity Event",
                Description = "Capacity is zero",
                EventDate = DateTime.UtcNow.AddDays(10),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(12),
                Location = "Location",
                Capacity = 0, // Invalid capacity
                Category = "Conference"
            };

            // Manually trigger validation
            controller.ModelState.Clear();
            controller.TryValidateModel(newEvent);

            // Act
            var result = controller.Create(newEvent).Result;

            // Assert
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_POST_WithStartTimeAfterEndTime_ShouldFailValidation()
        {
            // Arrange
            var context = TestHelper.GetInMemoryDbContext();
            var controller = TestHelper.CreateControllerWithSession<EventsController>(context);

            var newEvent = new Event
            {
                Title = "Invalid Time Event",
                Description = "Start time is after end time",
                EventDate = DateTime.UtcNow.AddDays(10),
                StartTime = TimeSpan.FromHours(15), // 3 PM
                EndTime = TimeSpan.FromHours(10),   // 10 AM
                Location = "Location",
                Capacity = 50,
                Category = "Workshop"
            };

            // Manually trigger validation
            controller.ModelState.Clear();
            controller.TryValidateModel(newEvent);

            // Act
            var result = controller.Create(newEvent).Result;

            // Assert
            controller.ModelState.IsValid.Should().BeFalse();
        }
    }
}