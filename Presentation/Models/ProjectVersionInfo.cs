namespace AggregateVersions.Presentation.Models
{
    public class ProjectVersionInfo
    {
        public string? GitOnlineService { get; set; }
        public string? Username { get; set; }
        public string? AppPassword { get; set; }
        public string? RepoName { get; set; }
        public string? BranchName { get; set; }
        public string? VersionPath { get; set; }
        public string? FromVersion { get; set; }
        public string? ToVersion { get; set; }
        public string? ProjectName { get; set; }
        public string? RequestFolder { get; set; }
        public string? ClonePath { get; set; }
        public string? CreationPath { get; set; }
        public string? RootPath { get; set; }
        public List<string> DatabaseFolderNames { get; set; } = [];
    }
}
