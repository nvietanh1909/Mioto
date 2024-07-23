using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mioto.Models
{
    public static class GoogleCalendarService
    {
        private const string ApplicationName = "Mioto";
        private const string CredentialsFilePath = "C:\\Json\\client_secret_721365774128-ic21v6701mm4e6nq2qf5h4m42kmop2s6.apps.googleusercontent.com.json";

        public static async Task<CalendarService> GetCalendarServiceAsync()
        {
            UserCredential credential;
            using (var stream = new FileStream(CredentialsFilePath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None);
            }

            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        // Thêm sự kiện vào lịch Google
        public static async Task<Event> AddEventAsync(string summary, string location, string description, DateTime startDateTime, DateTime endDateTime)
        {
            var service = await GetCalendarServiceAsync();

            var newEvent = new Event()
            {
                Summary = summary,
                Location = location,
                Description = description,
                Start = new EventDateTime()
                {
                    DateTime = startDateTime,
                    TimeZone = "Asia/Ho_Chi_Minh",
                },
                End = new EventDateTime()
                {
                    DateTime = endDateTime,
                    TimeZone = "Asia/Ho_Chi_Minh",
                }
            };

            var calendarId = "primary"; // Sử dụng lịch chính của người dùng
            var insertRequest = service.Events.Insert(newEvent, calendarId);
            var createdEvent = await insertRequest.ExecuteAsync();

            return createdEvent;
        }

        // Cập nhật sự kiện
        public static async Task<Event> UpdateEventAsync(string eventId, string summary, string location, string description, DateTime startDateTime, DateTime endDateTime)
        {
            var service = await GetCalendarServiceAsync();

            var eventToUpdate = await service.Events.Get("primary", eventId).ExecuteAsync();
            if (eventToUpdate == null)
            {
                throw new Exception("Event not found.");
            }

            eventToUpdate.Summary = summary;
            eventToUpdate.Location = location;
            eventToUpdate.Description = description;
            eventToUpdate.Start = new EventDateTime()
            {
                DateTime = startDateTime,
                TimeZone = "Asia/Ho_Chi_Minh",
            };
            eventToUpdate.End = new EventDateTime()
            {
                DateTime = endDateTime,
                TimeZone = "Asia/Ho_Chi_Minh",
            };

            var updateRequest = service.Events.Update(eventToUpdate, "primary", eventId);
            var updatedEvent = await updateRequest.ExecuteAsync();

            return updatedEvent;
        }

        // Xóa sự kiện
        public static async Task DeleteEventAsync(string eventId)
        {
            var service = await GetCalendarServiceAsync();
            var deleteRequest = service.Events.Delete("primary", eventId);
            await deleteRequest.ExecuteAsync();
        }

        // Lấy danh sách sự kiện
        public static async Task<IList<Event>> ListEventsAsync(DateTime timeMin, DateTime timeMax)
        {
            var service = await GetCalendarServiceAsync();

            var request = service.Events.List("primary");
            request.TimeMin = timeMin;
            request.TimeMax = timeMax;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            return events.Items;
        }
    }
}
