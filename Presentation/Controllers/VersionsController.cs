using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Presentation.Models;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class VersionsController(IProjectsService projectService,
                                    IOperationsService operationsService,
                                    IDataBasesService dataBasesService,
                                    IApplicationsService applicationsService,
                                    IConfiguration configuration) : Controller
    {
        [Route("/")]
        [Route("[action]")]
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Projects = projectService.GetAll().Result.ToList();

            ProjectVersionInfo projectVersionInfo = new();

            return View(projectVersionInfo);
        }


        [Route("[action]")]
        [HttpPost]
        public IActionResult Index(ProjectVersionInfo projectVersionInfo)
        {
            if (projectVersionInfo.RepoUrl == null)
                return BadRequest("Repository Url is null here.");

            string filesPath = CreateFilesDirectoryInProject();

            (string localPath, string clonePath, string requestFolderName) = CreateEachRequestDirectory(filesPath, projectVersionInfo.ProjectName!);

            if (projectVersionInfo.Username != null && projectVersionInfo.AppPassword != null)
                CloneRepository(projectVersionInfo.GitOnlineService!, projectVersionInfo.RepoUrl, clonePath, projectVersionInfo.BranchName!, projectVersionInfo.Username, projectVersionInfo.AppPassword);
            else
                CloneRepository(projectVersionInfo.GitOnlineService!, projectVersionInfo.RepoUrl, clonePath, projectVersionInfo.BranchName!);

            CreateLocalFolder(localPath, projectVersionInfo.ProjectName!);

            CreateDatabaseFolders(localPath, projectVersionInfo.ProjectName!);

            CreateApplicationFolders(localPath, projectVersionInfo.ProjectName!);

            if (projectVersionInfo.FromVersion != null && projectVersionInfo.ToVersion != null)
                ChooseSubFolder(localPath, clonePath, projectVersionInfo.VersionPath!, projectVersionInfo.FromVersion, projectVersionInfo.ToVersion, projectVersionInfo.ProjectName!);
            else
                ChooseSubFolder(localPath, clonePath, projectVersionInfo.VersionPath!, projectVersionInfo.ProjectName!);

            DataBaseFolderVersion(localPath);

            ApplicationMergeFiles(localPath);

            byte[] fileContent = ZipLocalFolder(localPath);

            DeleteRequestDirectory(filesPath, requestFolderName);

            return File(fileContent, "application/zip", fileDownloadName: "Operation.zip");
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> GetRepositories([FromBody] RepoInfoModel repoInfo)
        {
            if (repoInfo.GitService != null)
            {
                if (repoInfo.GitService.Equals("bitbucket", StringComparison.OrdinalIgnoreCase))
                {
                    if (repoInfo.Username != null && repoInfo.AppPassword != null)
                        return Json(await GetBitbucketRepositories(repoInfo.Username, repoInfo.AppPassword));
                    else
                        return Json(await GetBitbucketRepositories());
                }

                else
                    throw new InvalidOperationException("repsitories wasn't loaded");
            }

            else
                throw new InvalidOperationException("repsitories wasn't loaded");
        }


        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> GetBranches([FromBody] RepoInfoModel repoInfo)
        {
            if (repoInfo.GitService != null)
            {
                if (repoInfo.GitService.Equals("github", StringComparison.OrdinalIgnoreCase) && repoInfo.RepoUrl != null)
                {
                    if (repoInfo.Username != null && repoInfo.AppPassword != null)
                        return Json(await GetGitHubBranches(repoInfo.RepoUrl, repoInfo.Username, repoInfo.AppPassword));
                    else
                        return Json(await GetGitHubBranches(repoInfo.RepoUrl));
                }

                else if (repoInfo.GitService.Equals("bitbucket", StringComparison.OrdinalIgnoreCase) && repoInfo.RepoName != null)
                {
                    if (repoInfo.Username != null && repoInfo.AppPassword != null)
                        return Json(await GetBitbucketBranches(repoInfo.RepoName, repoInfo.Username, repoInfo.AppPassword));
                    else
                        return Json(await GetBitbucketBranches(repoInfo.RepoName));
                }

                else
                    throw new InvalidOperationException("branch wasn't loaded");
            }

            else
                throw new InvalidOperationException("branch wasn't loaded");
        }



        #region Private Methods

        private static void ApplicationMergeFiles(string localPath)
        {
            string[] applicationDirectories = Directory.GetDirectories(Path.Combine(localPath, "Applications"));

            foreach (string applicationDirectory in applicationDirectories)
            {
                string[] applicationFiles = Directory.GetFiles(applicationDirectory);


                if (applicationFiles.Length > 2)
                {
                    List<string> fileExtensions = [];

                    foreach (string applicationFile in applicationFiles)
                    {
                        string fileExtension = Path.GetFileName(applicationFile)[(Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-') + 1)..];

                        if (!fileExtensions.Contains(fileExtension))
                            fileExtensions.Add(fileExtension);
                    }

                    foreach (string fileExtension in fileExtensions)
                    {
                        StringBuilder fileContent = new();
                        string fileName = "";

                        foreach (string applicationFile in applicationFiles)
                        {
                            string appFileExtension = Path.GetFileName(applicationFile)[(Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-') + 1)..];

                            fileName = Path.GetFileName(applicationFile)[..Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-')];

                            if (appFileExtension == fileExtension)
                            {
                                if (fileExtension.Contains("txt"))
                                {
                                    fileContent.Append(System.IO.File.ReadAllText(applicationFile));
                                    fileContent.Append("\n\n**********************************************************\n\n");
                                }
                                else
                                {
                                    StringBuilder content = new(System.IO.File.ReadAllText(applicationFile));

                                    content.Remove(0, 2);
                                    content.Remove(content.Length - 2, 2);
                                    content.Append(',');

                                    fileContent.Append(content);
                                }
                                System.IO.File.Delete(applicationFile);
                            }
                        }

                        if (!fileExtension.Contains("txt"))
                        {
                            fileContent.Remove(fileContent.Length - 1, 1);
                            fileContent.Insert(0, '{');
                            fileContent.Append('}');
                        }


                        using StreamWriter writer = System.IO.File.CreateText(Path.Combine(applicationDirectory, string.Concat(fileName, "(Merge)", "-", fileExtension)));
                        writer.WriteAsync(fileContent);
                    }
                }
            }

        }

        private void CreateApplicationFolders(string localPath, string projectName)
        {
            string[] applicationsSubDirectories = GetApplicationFolderNames(projectName);
            foreach (string applicationsSubDirectory in applicationsSubDirectories)
            {
                string path = Path.Combine(localPath, "Applications", applicationsSubDirectory);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                Directory.CreateDirectory(path);
            }
        }

        private static void DataBaseFolderVersion(string localPath)
        {
            string[] dataBaseDirectories = Directory.GetDirectories(Path.Combine(localPath, "DataBases"));

            VersionFolder(dataBaseDirectories);

            foreach (string dataBaseDirectory in dataBaseDirectories)
                VersionFolder(Directory.GetDirectories(dataBaseDirectory));
        }

        private static void VersionFolder(string[] directories)
        {
            foreach (string directory in directories)
            {
                string[] files = Directory.GetFiles(directory);

                List<char> versions = [];

                foreach (string file in files)
                {
                    string? fileName = Path.GetFileName(file);

                    if (fileName != null && char.IsNumber(fileName.First()) && !versions.Contains(fileName.First()))
                        versions.Add(fileName.First());
                }

                if (versions.Count > 1)
                {
                    foreach (char version in versions)
                    {
                        Directory.CreateDirectory(Path.Combine(directory, version.ToString()));

                        foreach (string file in files)
                        {
                            string? fileName = Path.GetFileName(file);

                            if (fileName != null && fileName.First() == version)
                                Directory.Move(file, Path.Combine(directory, version.ToString(), fileName));
                        }
                    }
                }
            }
        }

        private async static Task<List<string>> GetGitHubBranches(string repoUrl, string? username = null, string? appPassword = null)
        {
            string apiUrl = repoUrl.Replace("https://github.com/", "https://api.github.com/repos/");

            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(appPassword))
            {
                var credentials = $"{username}:{appPassword}";
                var byteArray = Encoding.UTF8.GetBytes(credentials);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            HttpResponseMessage response = await client.GetAsync($"{apiUrl}/branches");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            JArray branches = JArray.Parse(responseBody);

            return branches.Select(branch => branch["name"]!.ToString()).ToList();
        }

        private async Task<List<string>> GetBitbucketBranches(string repoName, string? username = null, string? appPassword = null)
        {
            string apiUrl = configuration["BitbucketUrlBranches"] ?? "";
            apiUrl = apiUrl.Replace("{0}", repoName);

            using HttpClient client = new();
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(appPassword))
            {
                var byteArray = new UTF8Encoding().GetBytes($"{username}:{appPassword}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject branches = JObject.Parse(responseBody);

            return branches["values"]!.Select(branch => branch["displayId"]!.ToString()).ToList();
        }

        private async Task<List<string>> GetBitbucketRepositories(string? username = null, string? appPassword = null)
        {
            string apiUrl = configuration["BitbucketUrlRepositories"] ?? "";

            using HttpClient client = new();
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(appPassword))
            {
                var byteArray = new UTF8Encoding().GetBytes($"{username}:{appPassword}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject branches = JObject.Parse(responseBody);

            return branches["values"]!.Select(repository => repository["name"]!.ToString()).ToList();
        }

        private static void DeleteRequestDirectory(string filesPath, string requestFolderName)
        {
            string requestDirectoryPath = Path.Combine(filesPath, requestFolderName);

            Directory.GetFiles(requestDirectoryPath, "*", SearchOption.AllDirectories)
             .ToList()
             .ForEach(file => new FileInfo(file) { IsReadOnly = false });

            if (Directory.Exists(requestDirectoryPath))
                Directory.Delete(requestDirectoryPath, true);
        }

        private static string CreateFilesDirectoryInProject()
        {
            string currentDirectory = Directory.GetCurrentDirectory();

            string filesPath = Path.Combine(currentDirectory, "Files");

            Directory.CreateDirectory(filesPath);

            return filesPath;
        }

        private static (string, string, string) CreateEachRequestDirectory(string filesPath, string projectName)
        {
            DateTime dateTime = DateTime.Now;

            string requestFolderName = projectName + dateTime.Millisecond;

            string creationPath = Path.Combine(filesPath, requestFolderName, "Creation");
            string clonePath = Path.Combine(filesPath, requestFolderName, "Clone");

            Directory.CreateDirectory(creationPath);
            Directory.CreateDirectory(clonePath);

            return (creationPath, clonePath, requestFolderName);
        }

        private string[] GetLocalFolderNames(string projectName)
        {
            if (projectService.GetByName(projectName).Result == null)
                return [];
            else
                return operationsService.GetByProjectID(projectService.GetByName(projectName).Result!.ID).Result!.Select(op => op.Name).ToArray();
        }

        private string[] GetDatabaseFolderNames(string projectName)
        {
            if (projectService.GetByName(projectName).Result == null)
                return [];
            else
                return dataBasesService.GetByProjectID(projectService.GetByName(projectName).Result!.ID).Result!.Select(op => op.Name).ToArray();
        }

        private string[] GetApplicationFolderNames(string projectName)
        {
            if (projectService.GetByName(projectName).Result == null)
                return [];
            else
                return applicationsService.GetByProjectID(projectService.GetByName(projectName).Result!.ID).Result!.Select(op => op.Name).ToArray();
        }

        private void CreateLocalFolder(string localPath, string projectName)
        {
            Directory.CreateDirectory(localPath);

            string[] localFolderSubDirectories = GetLocalFolderNames(projectName)!;

            foreach (string localFolderSubDirectory in localFolderSubDirectories)
                Directory.CreateDirectory(Path.Combine(localPath, localFolderSubDirectory));
        }

        private void CreateDatabaseFolders(string localPath, string projectName)
        {
            string[] databaseSubDirectories = GetDatabaseFolderNames(projectName)!;

            foreach (string databaseSubDirectory in databaseSubDirectories)
            {
                string path = Path.Combine(localPath, "DataBases", databaseSubDirectory);

                if (Directory.Exists(path))
                    Directory.Delete(path, true);

                Directory.CreateDirectory(path);
            }
        }

        private static ReadOnlySpan<string> GetDirectoriesInRange(string[] directories, string start, string end)
        {
            ReadOnlySpan<string> span = new(directories);

            int startIndex = span.IndexOf(start);
            int endIndex = span.IndexOf(end);

            if (startIndex == -1)
                throw new ArgumentException("Start directory not found.");

            else if (endIndex == -1)
                throw new ArgumentException("End directory not found.");

            else if (endIndex < startIndex)
                throw new ArgumentException("End directory must come after start directory.");


            return span.Slice(startIndex, endIndex - startIndex + 1);
        }

        private void ChooseSubFolder(string localPath, string clonePath, string versionsPath, string fromVersion, string toVersion, string projectName)
        {
            string[] localFolderNames = GetLocalFolderNames(projectName);

            string startDirectory = Path.Combine(clonePath, versionsPath, fromVersion);
            string endDirectory = Path.Combine(clonePath, versionsPath, toVersion);

            ReadOnlySpan<string> versionDirectories = GetDirectoriesInRange(Directory.GetDirectories(Path.Combine(clonePath, versionsPath)), startDirectory, endDirectory);

            foreach (string versionDirectory in versionDirectories)
            {
                string[] versionSubDirectories = Directory.GetDirectories(versionDirectory);

                foreach (string versionSubDirectory in versionSubDirectories)
                {
                    bool find = false;
                    string[] operations = operationsService.GetAll().Result.Select(op => op.Name).ToArray();
                    foreach (string operation in operations)
                    {
                        if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateApplicationsFolders).Contains(operation))
                        {
                            AggregateApplicationsFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)),
                                             projectName);
                            find = true;
                        }

                        else if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateDataBasesFolders).Contains(operation))
                        {
                            AggregateDataBasesFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)),
                                             projectName);
                            find = true;
                        }

                        else if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateReportsFolders).Contains(operation))
                        {
                            AggregateReportsFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)));
                            find = true;
                        }
                    }
                    if (!find)
                        AggregateUnknownFolders(
                            versionSubDirectory,
                            Path.Combine(localPath, "Unknown"));

                }
            }
        }

        private void ChooseSubFolder(string localPath, string clonePath, string versionsPath, string projectName)
        {
            string[] localFolderNames = GetLocalFolderNames(projectName);

            string[] versionDirectories = Directory.GetDirectories(Path.Combine(clonePath, versionsPath));

            foreach (string versionDirectory in versionDirectories)
            {
                string[] versionSubDirectories = Directory.GetDirectories(versionDirectory);

                foreach (string versionSubDirectory in versionSubDirectories)
                {
                    bool find = false;
                    string[] operations = operationsService.GetAll().Result.Select(op => op.Name).ToArray();
                    foreach (string operation in operations)
                    {
                        if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateApplicationsFolders).Contains(operation))
                        {
                            AggregateApplicationsFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)),
                                             projectName);
                            find = true;
                        }

                        else if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateDataBasesFolders).Contains(operation))
                        {
                            AggregateDataBasesFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)),
                                             projectName);
                            find = true;
                        }

                        else if (Path.GetFileName(versionSubDirectory).Contains(operation) && nameof(AggregateReportsFolders).Contains(operation))
                        {
                            AggregateReportsFolders(
                                versionSubDirectory,
                                Path.Combine(localPath,
                                             localFolderNames.First(tmp => tmp == operation)));
                            find = true;
                        }
                    }
                    if (!find)
                        AggregateUnknownFolders(
                            versionSubDirectory,
                            Path.Combine(localPath, "Unknown"));

                }
            }
        }

        private void AggregateApplicationsFolders(string srcPath, string destPath, string projectName)
        {
            string[] applications = Directory.GetDirectories(srcPath);

            foreach (string application in applications)
            {
                string applicationDestPath = Path.Combine(destPath,
                                                          GetApplicationFolderNames(projectName)
                                                          .FirstOrDefault(app => app.Contains(Path.GetFileName(application)),
                                                                          Path.GetFileName(application) + "(Unknown)"));

                if (!Directory.Exists(applicationDestPath))
                    Directory.CreateDirectory(applicationDestPath);

                string[] applicationFiles = Directory.GetFiles(application);

                foreach (string applicationFile in applicationFiles)
                {
                    string applicationFileDestPath = Path.Combine(applicationDestPath, Path.GetFileName(applicationFile));

                    if (Directory.Exists(applicationFileDestPath))
                        Directory.Delete(applicationFileDestPath, true);

                    Directory.Move(applicationFile, applicationFileDestPath);
                }
            }
        }

        private void AggregateDataBasesFolders(string srcPath, string destPath, string projectName)
        {
            string[] databases = Directory.GetDirectories(srcPath);

            foreach (string database in databases)
            {
                string databaseName = Path.GetFileName(database);

                int slashIndex = databaseName.LastIndexOf('-');

                string databaseDirectoryName;

                if (slashIndex != -1)
                    databaseDirectoryName = databaseName[(slashIndex + 1)..];
                else
                    databaseDirectoryName = databaseName;

                string databasesDestinationPath = Path.Combine(destPath, GetDatabaseFolderNames(projectName)
                    .FirstOrDefault(databases => databases.Contains(databaseDirectoryName),
                                    databaseDirectoryName + "(Unknown)"));

                if (!Directory.Exists(databasesDestinationPath))
                    Directory.CreateDirectory(databasesDestinationPath);

                string[] databaseSubDirectories = Directory.GetDirectories(database);

                foreach (string databaseSubDirectory in databaseSubDirectories)
                {
                    string databaseSubDirectoryDestinationPath;

                    if (databaseSubDirectory.Contains("rollback", StringComparison.CurrentCultureIgnoreCase))
                        databaseSubDirectoryDestinationPath = Path.Combine(databasesDestinationPath, "Rollback");

                    else
                    {
                        string databaseSubDirectoryName = string.Concat(Path.GetFileName(databaseSubDirectory), "(Unknown)");
                        databaseSubDirectoryDestinationPath = Path.Combine(databasesDestinationPath, databaseSubDirectoryName);
                    }


                    if (!Directory.Exists(databaseSubDirectoryDestinationPath))
                        Directory.CreateDirectory(databaseSubDirectoryDestinationPath);

                    string[] subDirectoryFiles = Directory.GetFiles(databaseSubDirectory);

                    foreach (string subDirectoryFile in subDirectoryFiles)
                    {
                        string subDirectoryFileDestinationPath = Path.Combine(databaseSubDirectoryDestinationPath, Path.GetFileName(subDirectoryFile));

                        if (System.IO.File.Exists(subDirectoryFileDestinationPath))
                            System.IO.File.Delete(subDirectoryFileDestinationPath);
                        System.IO.File.Move(subDirectoryFile, subDirectoryFileDestinationPath);
                    }
                }

                string[] databaseFiles = Directory.GetFiles(database);

                foreach (string databaseFile in databaseFiles)
                {
                    string fileDestinationPath = Path.Combine(databasesDestinationPath, Path.GetFileName(databaseFile));

                    if (System.IO.File.Exists(fileDestinationPath))
                        System.IO.File.Delete(fileDestinationPath);
                    System.IO.File.Move(databaseFile, fileDestinationPath);
                }
            }
        }

        private static void AggregateReportsFolders(string srcPath, string destPath)
        {
            string[] reports = Directory.GetDirectories(srcPath);

            foreach (string report in reports)
            {
                string reportDestinationPath = Path.Combine(destPath, Path.GetFileName(report));

                if (Directory.Exists(reportDestinationPath))
                    Directory.Delete(reportDestinationPath, true);
                Directory.Move(report, reportDestinationPath);
            }
        }

        private static void AggregateUnknownFolders(string srcPath, string destPath)
        {
            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            string[] unknownsFiles = Directory.GetFiles(srcPath);
            string[] unknownsFolders = Directory.GetDirectories(srcPath);

            if (!Directory.Exists(Path.Combine(destPath, Path.GetFileName(srcPath))))
                Directory.CreateDirectory(Path.Combine(destPath, Path.GetFileName(srcPath)));

            foreach (string unknownFiles in unknownsFiles)
            {
                string unknownDestinationPath = Path.Combine(destPath, Path.GetFileName(srcPath), Path.GetFileName(unknownFiles));

                if (Directory.Exists(unknownDestinationPath))
                    Directory.Move(unknownFiles, unknownDestinationPath + "(Copy)");

                Directory.Move(unknownFiles, unknownDestinationPath);
            }

            foreach (string unknownFolder in unknownsFolders)
            {
                string unknownDestinationPath = Path.Combine(destPath, Path.Combine(Path.GetFileName(unknownFolder)));

                Directory.Move(unknownFolder, unknownDestinationPath);
            }
        }

        private static byte[] ZipLocalFolder(string localPath)
        {
            string zipFilePath = localPath + ".zip";

            if (System.IO.File.Exists(zipFilePath))
                System.IO.File.Delete(zipFilePath);

            ZipFile.CreateFromDirectory(localPath, zipFilePath);

            return System.IO.File.ReadAllBytes(zipFilePath);
        }

        private static void CloneRepository(string gitOnlineService, string repoUrl, string clonePath, string branch)
        {
            switch (gitOnlineService)
            {
                case "github":
                    GithubCloneRepository(repoUrl, clonePath, branch);
                    break;
                case "bitbucket":
                    BitbucketCloneRepository(repoUrl, clonePath, branch);
                    break;
                default:
                    break;
            }
        }

        private static void CloneRepository(string gitOnlineService, string repoUrl, string clonePath, string branch, string username, string appPassword)
        {
            switch (gitOnlineService)
            {
                case "github":
                    GithubCloneRepository(repoUrl, clonePath, branch, username, appPassword);
                    break;
                case "bitbucket":
                    BitbucketCloneRepository(repoUrl, clonePath, branch, username, appPassword);
                    break;
                default:
                    break;
            }
        }

        private static void BitbucketCloneRepository(string repositoryUrl, string clonePath, string branch)
        {
            CloneOptions cloneOptions = new() { BranchName = branch };

            Repository.Clone(repositoryUrl, clonePath, cloneOptions);
        }

        private static void BitbucketCloneRepository(string repositoryUrl, string clonePath, string branch, string username, string appPassword)
        {
            CloneOptions cloneOptions = GetCloneOptionsWithCredentials(username, appPassword);
            cloneOptions.BranchName = branch;

            Repository.Clone(repositoryUrl, clonePath, cloneOptions);
        }

        private static void GithubCloneRepository(string repositoryUrl, string clonePath, string branch)
        {
            CloneOptions cloneOptions = new() { BranchName = branch };

            Repository.Clone(repositoryUrl, clonePath, cloneOptions);
        }

        private static void GithubCloneRepository(string repositoryUrl, string clonePath, string branch, string username, string appPassword)
        {
            CloneOptions cloneOptions = GetCloneOptionsWithCredentials(username, appPassword);
            cloneOptions.BranchName = branch;

            Repository.Clone(repositoryUrl, clonePath, cloneOptions);
        }

        private static CloneOptions GetCloneOptionsWithCredentials(string username, string appPassword)
        {
            CloneOptions cloneOptions = new();
            cloneOptions.FetchOptions.CredentialsProvider = (_url, _user, _cre) => new UsernamePasswordCredentials
            {
                Username = username,
                Password = appPassword
            };

            return cloneOptions;
        }

        #endregion
    }
}
