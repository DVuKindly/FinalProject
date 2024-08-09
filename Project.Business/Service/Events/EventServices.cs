using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Business.Infrastructure;
using Project.Business.Model;
using Project.Business.Service.Events;
using Project.Data.Entity;

namespace Project.Business.Service.Events
{
    public class EventServices : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EventServices> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventServices(IUnitOfWork unitOfWork, ILogger<EventServices> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor; // Khởi tạo HttpContextAccessor
        }

        public async Task<bool> AddEventAsync(EventCreationViewModel eventViewModel)
        {
            if (eventViewModel == null)
            {
                _logger.LogError("Event creation view model is null.");
                return false;
            }

            try
            {
                var createdBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                User creator = _unitOfWork.UserRepository.GetQuery(x => x.UserName == createdBy).First();

                if (string.IsNullOrEmpty(createdBy))
                {
                    _logger.LogError("Current user is not authenticated or HttpContext is not available.");
                    return false;
                }

                var eventEntity = new Event
                {
                    EventId = Guid.NewGuid(),
                    EventName = eventViewModel.EventName,
                    Channel = eventViewModel.Channel,
                    StartDate = eventViewModel.StartDate,
                    EndDate = eventViewModel.EndDate,
                    Partner = eventViewModel.Partner,
                    Target = eventViewModel.Target,
                    TotalParticipants = eventViewModel.TotalParticipants,
                    Notes = eventViewModel.Notes,
                    Status = eventViewModel.Status,
                    CreatedAt = DateTime.Now,
                    CreatedBy = creator.Id,
                    Creator = creator

                };

                _unitOfWork.EventRepository.Add(eventEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Event created: {eventEntity.EventName} by {createdBy}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating event: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException?.Message}");
                return false;
            }
        }


        public async Task UpdateEventAsync(EventUpdateViewModel eventViewModel)
        {
            if (eventViewModel == null)
            {
                _logger.LogError("Event entity is null.");
                throw new ArgumentNullException(nameof(eventViewModel));
            }

            try
            {
                var updatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

              

                if (string.IsNullOrEmpty(updatedBy))
                {
                    _logger.LogError("Current user is not authenticated or HttpContext is not available.");
                    throw new KeyNotFoundException("Current user is not authenticated or HttpContext is not available.");
                }
                User creator = _unitOfWork.UserRepository.GetQuery(x => x.UserName == updatedBy).First();




                var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(eventViewModel.EventId);
                if (eventEntity == null)
                {
                    _logger.LogWarning($"Event not found: {eventViewModel.EventId}");
                    throw new KeyNotFoundException("Event not found.");
                }

                if (eventEntity.IsDeleted)
                {
                    _logger.LogWarning($"Event has been deleted: {eventViewModel.EventId}");
                    throw new InvalidOperationException("Cannot update a deleted event.");
                }

                // Update event fields
                eventEntity.EventName = eventViewModel.EventName;
                eventEntity.Channel = eventViewModel.Channel;
                eventEntity.StartDate = eventViewModel.StartDate;
                eventEntity.EndDate = eventViewModel.EndDate;
                eventEntity.Partner = eventViewModel.Partner;
                eventEntity.Target = eventViewModel.Target;
                eventEntity.TotalParticipants = eventViewModel.TotalParticipants;
                eventEntity.Notes = eventViewModel.Notes;
                eventEntity.Status = eventViewModel.Status;
                eventEntity.UpdatedAt = DateTime.Now;
                eventEntity.UpdatedBy = creator.Id;

                _unitOfWork.EventRepository.Update(eventEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Event updated: {eventEntity.EventName} by {updatedBy}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating event: {ex.Message}");
                throw;
            }
        }



        public async Task DeleteEventAsync(Guid eventId)
        {
            try
            {
                var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                {
                    _logger.LogWarning($"Event not found: {eventId}");
                    throw new KeyNotFoundException("Event not found.");
                }

                if (eventEntity.IsDeleted)
                {
                    _logger.LogWarning($"Event has already been deleted: {eventId}");
                    throw new InvalidOperationException("Cannot delete an event that has already been deleted.");
                }

                var deletedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

                if (string.IsNullOrEmpty(deletedBy))
                {
                    _logger.LogError("Current user is not authenticated or HttpContext is not available.");
                    throw new KeyNotFoundException("Current user is not authenticated or HttpContext is not available.");
                }
                User creator = _unitOfWork.UserRepository.GetQuery(x => x.UserName == deletedBy).First();

                eventEntity.DeletedAt = DateTime.UtcNow;
                eventEntity.DeletedBy = creator.Id;
                eventEntity.IsDeleted = true;

                _unitOfWork.EventRepository.Update(eventEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Event deleted: {eventEntity.EventName} by {deletedBy}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting event: {ex.Message}");
                throw;
            }
        }







        public async Task<Event> GetEventByIdAsync(Guid eventId)
        {
            try
            {
                var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                {
                    _logger.LogWarning($"Event not found: {eventId}");
                    throw new KeyNotFoundException("Event not found.");
                }

                if (eventEntity.IsDeleted)
                {
                    _logger.LogWarning($"Event has been deleted: {eventId}");
                    throw new InvalidOperationException("Cannot retrieve a deleted event.");
                }

                return eventEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting event: {ex.Message}");
                throw;
            }
        }



        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            try
            {
                return await _unitOfWork.EventRepository.GetQuery()
                    .Where(e => !e.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all events: {ex.Message}");
                throw;
            }
        }


        public async Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));

                return await _unitOfWork.EventRepository.GetQuery()
                    .Where(e => !e.IsDeleted &&
                                (e.EventName.ToLower().Contains(searchTerm.ToLower()) ||
                                 e.Notes.ToLower().Contains(searchTerm.ToLower())))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching events: {ex.Message}");
                throw;
            }
        }



    }
}
