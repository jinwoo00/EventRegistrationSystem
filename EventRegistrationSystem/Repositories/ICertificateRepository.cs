using EventRegistrationSystem.DTOs;
using EventRegistrationSystem.Models;

namespace EventRegistrationSystem.Repositories
{
    public interface ICertificateRepository
    {
        // ========== LIST & PAGINATION ==========
        Task<PagedResult<CertificateListItemDTO>> GetPagedAsync(
            int? eventId = null,
            string? search = null,
            int page = 1,
            int pageSize = 10);

        // ========== SINGLE CERTIFICATE ==========
        Task<Certificate?> GetByIdAsync(int id);

        // ========== GENERATION ==========
        Task<bool> GenerateCertificateAsync(int eventId, string userId);
        Task<int> GeneratePendingCertificatesAsync(int? eventId = null);

        // ========== REVOKE ==========
        Task<bool> RevokeAsync(int id);

        // ========== EXISTENCE CHECK ==========
        Task<bool> ExistsAsync(int eventId, string userId);

        // ========== STATS ==========
        Task<int> GetTotalIssuedAsync(int? eventId = null);
        Task<int> GetPendingCountAsync(int? eventId = null);
        Task<int> GetIssuedTodayAsync(int? eventId = null);

        // ========== CERTIFICATE TEMPLATE MANAGEMENT ==========
        Task<CertificateTemplate?> GetTemplateByEventAsync(int eventId);
        Task<bool> UploadTemplateAsync(int eventId, string fileName, string filePath, string userId);
        Task<bool> DeleteTemplateAsync(int eventId);

        // ========== PARTICIPANTS FOR SENDING ==========
        Task<List<ParticipantCertificateDTO>> GetCheckedInParticipantsAsync(int eventId);

        // ========== MARK CERTIFICATE AS SENT ==========
        Task<bool> MarkCertificateSentAsync(int registrationId, string email);

        // ========== BULK SEND (DELEGATE‑BASED) ==========
        Task<int> SendBulkCertificatesAsync(int eventId, string templatePath, Func<string, string, string, Stream, Task> sendEmailAction);
        Task<int> GetUserCertificateCountAsync(string userId);
        Task<List<UserCertificateDTO>> GetUserCertificatesAsync(string userId);
        Task<bool> ApproveCertificateAsync(int certificateId);
        Task<int> ApproveAllPendingAsync(int eventId);
        Task<List<Certificate>> GetPendingCertificatesAsync(int eventId);
        // ========== PARTICIPANTS FOR SENDING (PAGED) ==========
        Task<PagedResult<ParticipantCertificateDTO>> GetPagedCheckedInParticipantsAsync(
            int eventId,
            int page = 1,
            int pageSize = 10);

    }
}