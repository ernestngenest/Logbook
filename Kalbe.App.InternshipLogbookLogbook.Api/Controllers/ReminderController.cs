using Hangfire;
using Microsoft.AspNetCore.Mvc;


namespace Kalbe.App.InternshipLogbookLogbook.Api.Controllers
{
    [ApiController]
    [Route("Reminder")]
    public class ReminderController : Controller
    {
        private readonly IBackgroundJobClient _jobClient;
        public ReminderController(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }
        [HttpPost]
        [Route("FireAndForget")]
        public Task<string>FireAndForget([FromBody]string message)
        {
            _jobClient.Enqueue(()=>Console.WriteLine(message));
            return Task.FromResult(message);
        }

        [HttpPost]
        [Route("Continuation")]
        public Task<string> Continuation([FromBody] string message)
        { 
            var jobid = _jobClient.Schedule(() => Console.WriteLine(message),TimeSpan.FromMinutes(1));

            _jobClient.ContinueJobWith(jobid, () => Console.WriteLine(message));
            return Task.FromResult(message);
        } 
    }
}
