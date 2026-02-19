using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventRegistrationSystem.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
        Task SendWithAttachmentAsync(string to, string subject, string body, Stream attachmentStream, string fileName, string contentType);
    }
}