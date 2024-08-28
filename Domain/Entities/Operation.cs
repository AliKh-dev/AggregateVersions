namespace AggregateVersions.Domain.Entities
{
    public class Operation
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid ProjectID { get; set; }
        public Project? Project { get; set; }
    }
}
