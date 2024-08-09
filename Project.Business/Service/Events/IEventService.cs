using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Project.Business.Model; // Assuming this is where the EventCreationViewModel is defined
using Project.Data.Entity;   // Assuming this is where the Event class is defined

namespace Project.Business.Service.Events
{
    public interface IEventService
    {
        Task<bool> AddEventAsync(EventCreationViewModel eventViewModel);
        Task UpdateEventAsync(EventUpdateViewModel eventEntity);
        Task DeleteEventAsync(Guid eventId);
        Task<Event> GetEventByIdAsync(Guid eventId);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task<IEnumerable<Event>> SearchEventsAsync(string searchTerm);
    }
}
