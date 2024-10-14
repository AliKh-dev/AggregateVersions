namespace AggregateVersions.Domain.Entities
{
    public class Project
    {
        public Guid ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<DataBase>? DataBases { get; set; }
    }
}
