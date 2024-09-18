using AggregateVersions.Domain.Entities;

namespace AggregateVersions.Domain.DTO
{
    public class AccessResponse
    {
        public long ID { get; set; }
        public Guid Guid { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public long? ModifiedBy { get; set; }
        public long? CreatedBy { get; set; }
        public string? Title { get; set; }
        public string? Key { get; set; }
        public long? ParentId { get; set; }
        public long? ApplicationId { get; set; }
        public long TypeId { get; set; }
        public bool IsEnable { get; set; }
        public string? DisplayCode { get; set; }
        public bool IsApi { get; set; }
        public bool IsSharedInSubsystems { get; set; } = false;
        public int CommonnessStatus { get; set; } = 0;
        public bool? IsDeleted { get; set; } = false;
    }

    public static class AccessExportResponseExtension
    {
        public static AccessResponse ToAccessExportResponse(this Access access)
        {
            return new AccessResponse()
            {
                ID = access.ID,
                Guid = access.Guid,
                CreationDate = access.CreationDate,
                LastModifiedDate = access.LastModifiedDate,
                ModifiedBy = access.ModifiedBy,
                CreatedBy = access.CreatedBy,
                Title = access.Title,
                Key = access.Key,
                ParentId = access.ParentId,
                ApplicationId = access.ApplicationId,
                TypeId = access.TypeId,
                IsEnable = access.IsEnable,
                DisplayCode = access.DisplayCode,
                IsApi = access.IsApi,
                IsSharedInSubsystems = access.IsSharedInSubsystems,
                CommonnessStatus = access.CommonnessStatus,
                IsDeleted = access.IsDeleted
            };
        }
    }
}
