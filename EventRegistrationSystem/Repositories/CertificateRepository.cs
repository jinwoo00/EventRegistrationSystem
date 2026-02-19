using Microsoft.EntityFrameworkCore;
using EventRegistrationSystem.Data;
using EventRegistrationSystem.Models;
using EventRegistrationSystem.DTOs;

namespace EventRegistrationSystem.Repositories
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CertificateRepository(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ----- Templates -----
        public async Task<CertificateTemplate?> GetTemplateByEventAsync(int eventId)
        {
            return await _context.CertificateTemplates
                .FirstOrDefaultAsync(t => t.EventId == eventId);
        }

        public async Task<bool> UploadTemplateAsync(int eventId, string fileName, string filePath, string userId)
        {
            var existing = await GetTemplateByEventAsync(eventId);
            if (existing != null)
            {
                // Delete old file
                if (File.Exists(existing.FilePath))
                    File.Delete(existing.FilePath);
                _context.CertificateTemplates.Remove(existing);
                await _context.SaveChangesAsync();
            }

            var template = new CertificateTemplate
            {
                EventId = eventId,
                FileName = fileName,
                FilePath = filePath,
                UploadedAt = DateTime.Now,
                UploadedBy = userId
            };
            _context.CertificateTemplates.Add(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTemplateAsync(int eventId)
        {
            var template = await GetTemplateByEventAsync(eventId);
            if (template == null) return false;
            if (File.Exists(template.FilePath))
                File.Delete(template.FilePath);
            _context.CertificateTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        // ----- Participants -----
        public async Task<List<ParticipantCertificateDTO>> GetCheckedInParticipantsAsync(int eventId)
        {
            var participants = await _context.Registrations
                .Where(r => r.EventId == eventId && r.IsCheckedIn)
                .Include(r => r.User)
                .OrderBy(r => r.CheckedInAt)
                .Select(r => new ParticipantCertificateDTO
                {
                    RegistrationId = r.Id,
                    AttendeeName = r.User != null ? r.User.FullName : null,
                    AttendeeEmail = r.User != null ? r.User.Email : null,
                    CheckedInAt = r.CheckedInAt ?? r.RegisteredAt,
                    HasCertificate = _context.Certificates.Any(c => c.EventId == eventId && c.UserId == r.UserId),
                    CertificateSentAt = _context.Certificates
                        .Where(c => c.EventId == eventId && c.UserId == r.UserId)
                        .Select(c => c.SentAt)
                        .FirstOrDefault()
                })
                .ToListAsync();
            return participants;
        }

        // ----- Mark as sent -----
        public async Task<bool> MarkCertificateSentAsync(int registrationId, string email)
        {
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.Id == registrationId);
            if (registration == null || registration.UserId == null)
                return false;

            var existing = await _context.Certificates
                .FirstOrDefaultAsync(c => c.EventId == registration.EventId && c.UserId == registration.UserId);

            if (existing != null)
            {
                existing.SentAt = DateTime.Now;
                existing.SentToEmail = email;
            }
            else
            {
                var cert = new Certificate
                {
                    EventId = registration.EventId,
                    UserId = registration.UserId,
                    CertificateNumber = GenerateCertificateNumber(),
                    IssuedAt = DateTime.Now,
                    SentAt = DateTime.Now,
                    SentToEmail = email,
                    IsTemplateBased = true
                };
                _context.Certificates.Add(cert);
            }
            await _context.SaveChangesAsync();
            return true;
        }

        // ----- Bulk send (delegates email sending) -----
        public async Task<int> SendBulkCertificatesAsync(int eventId, string templatePath, Func<string, string, string, Stream, Task> sendEmailAction)
        {
            var participants = await GetCheckedInParticipantsAsync(eventId);
            var unsent = participants.Where(p => !p.HasCertificate).ToList();

            int sentCount = 0;
            foreach (var p in unsent)
            {
                var registration = await _context.Registrations
                    .Include(r => r.User)
                    .Include(r => r.Event)
                    .FirstOrDefaultAsync(r => r.Id == p.RegistrationId);
                if (registration?.User == null) continue;

                using var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
                await sendEmailAction(registration.User.Email!, registration.Event!.Title, registration.User.FullName ?? registration.User.Email!, stream);
                await MarkCertificateSentAsync(registration.Id, registration.User.Email!);
                sentCount++;
            }
            return sentCount;
        }

        private string GenerateCertificateNumber()
        {
            return $"CERT-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20);
        }
        public async Task<PagedResult<CertificateListItemDTO>> GetPagedAsync(
            int? eventId = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Certificates
                .Include(c => c.Event)
                .Include(c => c.User)
                .AsNoTracking()
                .AsQueryable();

            if (eventId.HasValue)
                query = query.Where(c => c.EventId == eventId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(c =>
                    (c.User != null && (c.User.FullName != null && c.User.FullName.ToLower().Contains(search) ||
                                        c.User.Email != null && c.User.Email.ToLower().Contains(search))) ||
                    (c.Event != null && c.Event.Title.ToLower().Contains(search)) ||
                    (c.CertificateNumber != null && c.CertificateNumber.ToLower().Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.IssuedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CertificateListItemDTO
                {
                    Id = c.Id,
                    EventTitle = c.Event != null ? c.Event.Title : null,
                    AttendeeName = c.User != null ? c.User.FullName : null,
                    AttendeeEmail = c.User != null ? c.User.Email : null,
                    CertificateNumber = c.CertificateNumber ?? string.Empty,
                    IssuedAt = c.IssuedAt,
                    FilePath = c.FilePath,
                    EventId = c.EventId,
                    UserId = c.UserId,
                    SentAt = c.SentAt,
                    SentToEmail = c.SentToEmail,
                    IsTemplateBased = c.IsTemplateBased
                })
                .ToListAsync();

            return new PagedResult<CertificateListItemDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        public async Task<Certificate?> GetByIdAsync(int id)
        {
            return await _context.Certificates
                .Include(c => c.Event)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<bool> GenerateCertificateAsync(int eventId, string userId)
        {
            if (await ExistsAsync(eventId, userId))
                return false;

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
            if (registration == null || !registration.IsCheckedIn)
                return false;

            var certificate = new Certificate
            {
                EventId = eventId,
                UserId = userId,
                CertificateNumber = GenerateCertificateNumber(),
                IssuedAt = DateTime.Now,
                FilePath = null,
                IsTemplateBased = true,
                SentAt = null,
                SentToEmail = null
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int> GeneratePendingCertificatesAsync(int? eventId = null)
        {
            var query = _context.Registrations
                .Where(r => r.IsCheckedIn)
                .Where(r => !_context.Certificates.Any(c => c.EventId == r.EventId && c.UserId == r.UserId));

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            var pending = await query.ToListAsync();
            foreach (var reg in pending)
            {
                await GenerateCertificateAsync(reg.EventId, reg.UserId);
            }
            return pending.Count;
        }
        public async Task<bool> RevokeAsync(int id)
        {
            var cert = await _context.Certificates.FindAsync(id);
            if (cert == null) return false;

            _context.Certificates.Remove(cert);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ExistsAsync(int eventId, string userId)
        {
            return await _context.Certificates
                .AnyAsync(c => c.EventId == eventId && c.UserId == userId);
        }
        public async Task<int> GetTotalIssuedAsync(int? eventId = null)
        {
            var query = _context.Certificates.AsQueryable();
            if (eventId.HasValue)
                query = query.Where(c => c.EventId == eventId.Value);
            return await query.CountAsync();
        }
        public async Task<int> GetPendingCountAsync(int? eventId = null)
        {
            var query = _context.Registrations
                .Where(r => r.IsCheckedIn)
                .Where(r => !_context.Certificates.Any(c => c.EventId == r.EventId && c.UserId == r.UserId));

            if (eventId.HasValue)
                query = query.Where(r => r.EventId == eventId.Value);

            return await query.CountAsync();
        }
        public async Task<int> GetIssuedTodayAsync(int? eventId = null)
        {
            var today = DateTime.Today;
            var query = _context.Certificates.Where(c => c.IssuedAt.Date == today);
            if (eventId.HasValue)
                query = query.Where(c => c.EventId == eventId.Value);
            return await query.CountAsync();
        }
        public async Task<int> GetUserCertificateCountAsync(string userId)
        {
            return await _context.Certificates
                .Where(c => c.UserId == userId)
                .CountAsync();
        }
        public async Task<List<UserCertificateDTO>> GetUserCertificatesAsync(string userId)
        {
            return await _context.Certificates
                .Include(c => c.Event)
                .Where(c => c.UserId == userId && c.IsApproved) // only approved
                .OrderByDescending(c => c.IssuedAt)
                .Select(c => new UserCertificateDTO
                {
                    Id = c.Id,
                    EventTitle = c.Event != null ? c.Event.Title : string.Empty,
                    EventStartDate = c.Event != null ? c.Event.StartDate : DateTime.MinValue,
                    IssuedAt = c.IssuedAt,
                    CertificateNumber = c.CertificateNumber,
                    FilePath = c.FilePath,
                    IsApproved = c.IsApproved
                })
                .ToListAsync();
        }
        public async Task<bool> ApproveCertificateAsync(int certificateId, IWebHostEnvironment env)
        {
            var cert = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == certificateId);
            if (cert == null) return false;

            // Generate PDF
            var pdfBytes = CertificateGenerator.GenerateCertificate(
                cert.User?.FullName ?? "Attendee",
                cert.Event?.Title ?? "Event",
                cert.Event?.StartDate ?? DateTime.Now,
                cert.CertificateNumber
            );

            // Save to wwwroot/certificates
            var certDir = Path.Combine(env.WebRootPath, "certificates");
            Directory.CreateDirectory(certDir);
            var fileName = $"cert-{cert.CertificateNumber}.pdf";
            var filePath = Path.Combine(certDir, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

            cert.FilePath = $"/certificates/{fileName}";
            cert.IsApproved = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveCertificateAsync(int certificateId)
        {
            try
            {
                var cert = await _context.Certificates
                    .Include(c => c.User)
                    .Include(c => c.Event)
                    .FirstOrDefaultAsync(c => c.Id == certificateId);
                if (cert == null) return false;

                // Generate PDF
                var pdfBytes = CertificateGenerator.GenerateCertificate(
                    cert.User?.FullName ?? "Attendee",
                    cert.Event?.Title ?? "Event",
                    cert.Event?.StartDate ?? DateTime.Now,
                    cert.CertificateNumber
                );

                // Save to wwwroot/certificates
                var certDir = Path.Combine(_webHostEnvironment.WebRootPath, "certificates");
                Directory.CreateDirectory(certDir);
                var fileName = $"cert-{cert.CertificateNumber}.pdf";
                var filePath = Path.Combine(certDir, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                cert.FilePath = $"/certificates/{fileName}";
                cert.IsApproved = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"ApproveCertificateAsync error: {ex}");
                return false;
            }
        }

        public async Task<int> ApproveAllPendingAsync(int eventId)
        {
            var pending = await _context.Certificates
                .Include(c => c.User)
                .Include(c => c.Event)
                .Where(c => c.EventId == eventId && !c.IsApproved)
                .ToListAsync();

            int count = 0;
            foreach (var cert in pending)
            {
                if (await ApproveCertificateAsync(cert.Id)) // reuse the single approval method
                    count++;
            }
            return count;
        }

        public async Task<List<Certificate>> GetPendingCertificatesAsync(int eventId)
        {
            return await _context.Certificates
                .Include(c => c.User)
                .Where(c => c.EventId == eventId && !c.IsApproved)
                .ToListAsync();
        }
        public async Task<PagedResult<ParticipantCertificateDTO>> GetPagedCheckedInParticipantsAsync(
    int eventId,
    int page = 1,
    int pageSize = 10)
        {
            // First, check total checked‑in count for this event
            var totalCheckedIn = await _context.Registrations
                .CountAsync(r => r.EventId == eventId && r.IsCheckedIn);
            Console.WriteLine($"Event {eventId} has {totalCheckedIn} checked‑in participants.");

            var query = _context.Registrations
                .Where(r => r.EventId == eventId && r.IsCheckedIn)
                .Include(r => r.User)
                .OrderBy(r => r.CheckedInAt ?? r.RegisteredAt)
                .Select(r => new ParticipantCertificateDTO
                {
                    RegistrationId = r.Id,
                    AttendeeName = r.User != null ? r.User.FullName : null,
                    AttendeeEmail = r.User != null ? r.User.Email : null,
                    CheckedInAt = r.CheckedInAt ?? r.RegisteredAt,
                    HasCertificate = _context.Certificates.Any(c => c.EventId == eventId && c.UserId == r.UserId),
                    CertificateSentAt = _context.Certificates
                        .Where(c => c.EventId == eventId && c.UserId == r.UserId)
                        .Select(c => c.SentAt)
                        .FirstOrDefault()
                });

            var totalCount = await query.CountAsync();
            Console.WriteLine($"TotalCount from query: {totalCount}");

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Console.WriteLine($"Retrieved {items.Count} items for page {page}.");

            return new PagedResult<ParticipantCertificateDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}