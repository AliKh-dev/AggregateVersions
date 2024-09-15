namespace AggregateVersions.Domain.Entities
{
    public class Access
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
        public Access? Parent { get; set; }
    }
}
