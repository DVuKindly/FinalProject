using Microsoft.AspNetCore.Mvc;
using Project.Business.Service.Events;
using Project.Business.Model;
using Project.Data.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project.Business.Service.Paginiated;
using Microsoft.AspNetCore.Authorization;

namespace Project.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        public static int PAGE_SIZE = 2;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
     


        public async Task<IActionResult> CreateEvent([FromBody] EventCreationViewModel eventViewModel)
        {
            if (eventViewModel == null)
                return BadRequest("Event data cannot be null.");

            try
            {
                var success = await _eventService.AddEventAsync(eventViewModel);
                if (success)
                {
                    return CreatedAtAction(nameof(GetEventById), new { id = eventViewModel.EventId }, eventViewModel);
                }
                else
                {
                    return BadRequest("Failed to create event. Please check the data and try again.");
                }
            }
            catch (Exception ex)
            {
               
                return StatusCode(500, "Internal server error. Please contact support. và "+ex.Message);
            }
        }


        [HttpPut("{id}")]



        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] EventUpdateViewModel eventEntity)
        {
            if (eventEntity == null || id != eventEntity.EventId)
                return BadRequest("Event data is invalid.");

            try
            {
                await _eventService.UpdateEventAsync(eventEntity);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Event not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
    


        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                await _eventService.DeleteEventAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Event not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
      


        public async Task<IActionResult> GetEventById(Guid id)
        {
            try
            {
                var eventEntity = await _eventService.GetEventByIdAsync(id);
                if (eventEntity == null)
                    return NotFound("Event not found.");

                return Ok(eventEntity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
       


        public async Task<IActionResult> GetAllEvents(int page = 1)
        {
            try
            {
                var events = await _eventService.GetAllEventsAsync();

                // Paginate the result
                var paginatedEvents = PaginatedList<Event>.Create(events.AsQueryable(), page, PAGE_SIZE);

                return Ok(paginatedEvents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("search")]
    


        public async Task<IActionResult> SearchEvents([FromQuery] string searchTerm, int page = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    return BadRequest("Search term cannot be empty.");

                var events = await _eventService.SearchEventsAsync(searchTerm);

                // Paginate the result
                var paginatedEvents = PaginatedList<Event>.Create(events.AsQueryable(), page, PAGE_SIZE);

                return Ok(paginatedEvents);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
