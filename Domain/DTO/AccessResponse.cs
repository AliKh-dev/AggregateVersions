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

        public Access ToAccess()
        {
            return new Access()
            {
                ID = ID,
                Guid = Guid,
                CreationDate = CreationDate,
                LastModifiedDate = LastModifiedDate,
                ModifiedBy = ModifiedBy,
                CreatedBy = CreatedBy,
                Title = Title,
                Key = Key,
                ParentId = ParentId,
                ApplicationId = ApplicationId,
                TypeId = TypeId,
                IsEnable = IsEnable,
                DisplayCode = DisplayCode,
                IsApi = IsApi,
                IsSharedInSubsystems = IsSharedInSubsystems,
                CommonnessStatus = CommonnessStatus,
                IsDeleted = IsDeleted
            };
        }
    }

    public static class AccessExportResponseExtension
    {
        public static AccessResponse ToAccessResponse(this Access access)
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
