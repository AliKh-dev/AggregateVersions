using AggregateVersions.Domain.Entities;
using AggregateVersions.Domain.Interfaces;
using AggregateVersions.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;

namespace AggregateVersions.Presentation.Controllers
{
    [Route("[controller]")]
    public class VersionsController(IProjectsService projectService,
                                    IOperationsService operationsService, // in not necessarily
                                    IDataBasesService dataBasesService,
                                    IApplicationsService applicationsService, // in not necessarily
                                    IConfiguration configuration) : Controller
    {
        [Route("/")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.Projects = await projectService.GetAll();

            ProjectVersionInfo projectVersionInfo = new();

            return View(projectVersionInfo);
        }


        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Index(ProjectVersionInfo projectVersionInfo)
        {
            #region Bad Request
            if (string.IsNullOrEmpty(projectVersionInfo.GitOnlineService))
                return BadRequest("Git Online service can't be empty.");

            else if (string.IsNullOrEmpty(projectVersionInfo.Username))
                return BadRequest("Username can't be empty.");

            else if (string.IsNullOrEmpty(projectVersionInfo.AppPassword))
                return BadRequest("Password can't be empty.");

            else if (string.IsNullOrEmpty(projectVersionInfo.RepoName))
                return BadRequest("Repository name must be provided.");

            else if (string.IsNullOrEmpty(projectVersionInfo.BranchName))
                return BadRequest("Branch name must be provided.");

            else if (projectVersionInfo.ProjectName == null)
                return BadRequest("Project name must be provided.");
            #endregion

            string rootPath = CreateFilesDirectoryInProject();

            (string filesPath, string clonePath, string requestFolderName) = CreateEachRequestDirectory(rootPath, projectVersionInfo.ProjectName);

            try
            {
                List<string> operationFolderNames = GetOperationFolderNames(Path.Combine(clonePath, projectVersionInfo.VersionPath ?? ""));
                List<string> applicationFolderNames = GetApplicationFolderNames(Path.Combine(clonePath, projectVersionInfo.VersionPath ?? ""));

                CloneRepository(projectVersionInfo.GitOnlineService, projectVersionInfo.RepoName, clonePath,
                                    projectVersionInfo.BranchName, projectVersionInfo.Username, projectVersionInfo.AppPassword);

                CreateOperationFolder(filesPath, operationFolderNames);

                await CreateDatabaseFolders(filesPath, projectVersionInfo.ProjectName);

                CreateApplicationFolders(filesPath, applicationFolderNames);

                await ChooseSubFolder(filesPath, clonePath, operationFolderNames, applicationFolderNames, projectVersionInfo.VersionPath ?? "", projectVersionInfo.ProjectName, projectVersionInfo.FromVersion, projectVersionInfo.ToVersion);

                DataBaseFolderVersion(filesPath);

                ApplicationMergeFiles(filesPath);

                byte[] fileContent = ZipLocalFolder(filesPath);

                DeleteRequestDirectory(rootPath, requestFolderName);

                return File(fileContent, "application/zip", fileDownloadName: "Operation.zip");
            }
            catch (Exception e)
            {
                DeleteRequestDirectory(rootPath, requestFolderName);
                return BadRequest(e.ToString());
            }
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> GetRepositories([FromBody] RepoInfoModel repoInfo)
        {
            if (repoInfo.GitService != null &&
                repoInfo.GitService.Equals("bitbucket", StringComparison.OrdinalIgnoreCase))
            {
                if (repoInfo.Username != null && repoInfo.AppPassword != null)
                    return Json(await GetBitbucketRepositories(repoInfo.Username, repoInfo.AppPassword));
                throw new InvalidOperationException("For bitbucket, you must enter username and password");
            }

            else
                throw new InvalidOperationException("Repositories wasn't loaded");
        }


        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> GetBranches([FromBody] RepoInfoModel repoInfo)
        {
            if (repoInfo.GitService != null &&
                repoInfo.GitService.Equals("bitbucket", StringComparison.OrdinalIgnoreCase) &&
                repoInfo.RepoName != null)
            {
                if (repoInfo.Username != null && repoInfo.AppPassword != null)
                    return Json(await GetBitbucketBranches(repoInfo.RepoName, repoInfo.Username, repoInfo.AppPassword));

                else
                    throw new InvalidOperationException("For bitbucket, you must enter username and password");
            }

            else
                throw new InvalidOperationException("Branches wasn't loaded");
        }



        #region Private Methods
        private string CreateFilesDirectoryInProject()
        {
            string? currentDirectory = configuration["PathClone"] ?? "C:\\";

            string filesPath = Path.Combine(currentDirectory, "Files");

            if (!Directory.Exists(filesPath))
                Directory.CreateDirectory(filesPath);

            return filesPath;
        }

        private static (string, string, string) CreateEachRequestDirectory(string filesPath, string projectName)
        {
            string requestFolderName = projectName + DateTime.Now.Millisecond;

            string creationPath = Path.Combine(filesPath, requestFolderName, "Creation");
            string clonePath = Path.Combine(filesPath, requestFolderName, "Clone");

            if (!Directory.Exists(creationPath))
                Directory.CreateDirectory(creationPath);

            if (!Directory.Exists(clonePath))
                Directory.CreateDirectory(clonePath);

            Console.WriteLine($"Clone path: {clonePath}");
            Console.WriteLine($"Creation path: {creationPath}");
            Console.WriteLine($"Request path: {Path.Combine(requestFolderName)}");

            return (creationPath, clonePath, requestFolderName);
        }

        private void CloneRepository(string gitOnlineService, string repoName, string clonePath, string branch, string username, string appPassword)
        {
            try
            {
                switch (gitOnlineService)
                {
                    case "bitbucket":
                        BitbucketCloneRepository(repoName, clonePath, branch, username, appPassword);
                        break;
                    default:
                        throw new ArgumentException("Git online service must be match.", nameof(gitOnlineService));
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void BitbucketCloneRepository(string repoName, string clonePath, string branch, string username, string appPassword)
        {
            try
            {
                string? repoUrl = Convert.ToString(configuration.GetValue(typeof(string), "BitbucketUrlRepository")) ??
                                      throw new InvalidOperationException("Bitbucket repository url didn't provided in appsettings.");

                repoUrl = repoUrl.Replace("{0}", repoName);
                repoUrl = repoUrl.Replace("{1}", Uri.EscapeDataString(username));
                repoUrl = repoUrl.Replace("{2}", Uri.EscapeDataString(appPassword));

                string command = $"git -c http.sslVerify=false clone -b {branch} {repoUrl} {clonePath}";
                string secondCommand = $"git - c http.sslVerify = false clone --verbose - b {branch} {repoUrl} {clonePath}";
                string thirdCommand = $"git clone -c http.sslVerify=false --single-branch -b {branch} {repoUrl} {clonePath}";
                string forthCommand = $"git -c http.sslVerify=false -c http.lowSpeedLimit=0 -c http.lowSpeedTime=999999 -c http.timeout=600 clone -b {branch} {repoUrl} {clonePath}";

                RunBashCommand(command);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in clone: {e.Message}");
                Console.WriteLine($"Inner exceptoin in clone: {e.InnerException}");
                throw;
            }
        }

        private static void RunBashCommand(string command)
        {
            try
            {
                Process process = new();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                if (!process.Start())
                    throw new InvalidOperationException("Can't run command on cmd.");

                string result = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                Console.WriteLine($"Output: {result}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in run bash: {e.Message}");
                Console.WriteLine($"Inner exceptoin in run bash: {e.InnerException}");
                throw;
            }
        }

        private static void CreateOperationFolder(string localPath, List<string> operationFolderNames)
        {
            try
            {
                List<string> localFolderSubDirectories = operationFolderNames;

                foreach (string localFolderSubDirectory in localFolderSubDirectories)
                    Directory.CreateDirectory(Path.Combine(localPath, localFolderSubDirectory));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CreateDatabaseFolders(string localPath, string projectName)
        {
            try
            {
                string[] databaseSubDirectories = await GetDatabaseFolderNames(projectName);

                foreach (string databaseSubDirectory in databaseSubDirectories)
                {
                    string path = Path.Combine(localPath, "DataBases", databaseSubDirectory);

                    if (Directory.Exists(path))
                        Directory.Delete(path, true);

                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string[]> GetDatabaseFolderNames(string projectName)
        {
            try
            {
                Project project = await projectService.GetByName(projectName) ??
                                            throw new NullReferenceException($"Can't {projectName}");


                List<DataBase>? dataBases = await dataBasesService.GetByProjectID(project.ID) ??
                                                    throw new NullReferenceException($"Can't find any database based on {projectName}");

                return dataBases.Select(op => op.Name).ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void CreateApplicationFolders(string localPath, List<string> applicationFolderNames)
        {
            try
            {
                List<string> applicationsSubDirectories = applicationFolderNames;

                foreach (string applicationsSubDirectory in applicationsSubDirectories)
                {
                    string path = Path.Combine(localPath, "Applications", applicationsSubDirectory);

                    if (Directory.Exists(path))
                        Directory.Delete(path, true);

                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ChooseSubFolder(string localPath, string clonePath, List<string> operationFolderNames, List<string> applicationFolderNames, string versionsPath, string projectName, string? fromVersion, string? toVersion)
        {
            try
            {
                string path = Directory.GetDirectories(clonePath).FirstOrDefault(path => path.Contains(versionsPath, StringComparison.OrdinalIgnoreCase), string.Empty);

                string startDirectory = Path.Combine(path, fromVersion ?? "");
                string endDirectory = Path.Combine(path, toVersion ?? "");

                List<string> orderedDirectories = [.. Directory.GetDirectories(path).OrderBy(name => name)];

                List<string> versionDirectories = GetDirectoriesInRange(orderedDirectories, startDirectory, endDirectory);


                foreach (string versionDirectory in versionDirectories)
                {
                    string[] versionSubDirectories = Directory.GetDirectories(versionDirectory);

                    foreach (string versionSubDirectory in versionSubDirectories)
                    {
                        bool find = false;

                        foreach (string operation in operationFolderNames)
                        {
                            if (operation.Contains(Path.GetFileName(versionSubDirectory), StringComparison.OrdinalIgnoreCase) && nameof(AggregateApplicationsFolders).Contains(operation))
                            {
                                AggregateApplicationsFolders(
                                    versionSubDirectory,
                                    Path.Combine(localPath,
                                                 operationFolderNames.First(tmp => tmp == operation)),
                                                 applicationFolderNames,
                                                 projectName);
                                find = true;
                            }

                            else if (operation.Contains(Path.GetFileName(versionSubDirectory), StringComparison.OrdinalIgnoreCase) && nameof(AggregateDataBasesFolders).Contains(operation))
                            {
                                await AggregateDataBasesFolders(
                                    versionSubDirectory,
                                    Path.Combine(localPath,
                                                 operationFolderNames.First(tmp => tmp == operation)),
                                                 projectName);
                                find = true;
                            }

                            else if (operation.Contains(Path.GetFileName(versionSubDirectory), StringComparison.OrdinalIgnoreCase) && nameof(AggregateReportsFolders).Contains(operation))
                            {
                                AggregateReportsFolders(
                                    versionSubDirectory,
                                    Path.Combine(localPath,
                                                 operationFolderNames.First(tmp => tmp == operation)));
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
            catch (Exception e)
            {
                Console.WriteLine("In ChooseSubFolder Exception Raised.");
                Console.WriteLine(e.ToString());
                Console.WriteLine(e.InnerException);
                throw;
            }
        }

        private static List<string> GetDirectoriesInRange(List<string> directories, string start, string end)
        {
            try
            {
                int startIndex = directories.IndexOf(start);
                int endIndex = directories.IndexOf(end);

                if (startIndex == -1)
                    throw new ArgumentException("Start directory not found.");

                else if (endIndex == -1)
                    throw new ArgumentException("End directory not found.");

                else if (endIndex < startIndex)
                    throw new ArgumentException("End directory must come after start directory.");


                return directories.ToList().GetRange(startIndex, endIndex - startIndex + 1);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void AggregateApplicationsFolders(string srcPath, string destPath, List<string> applicationFolderNames, string projectName)
        {
            try
            {
                string[] applications = Directory.GetDirectories(srcPath);

                foreach (string application in applications)
                {

                    string applicationDestPath = Path.Combine(destPath,
                                                              applicationFolderNames.FirstOrDefault(app => Path.GetFileName(application).Contains(app,
                                                                StringComparison.OrdinalIgnoreCase), Path.GetFileName(application) + "(Unknown)"));

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
            catch (Exception)
            {
                throw;
            }
        }

        private async Task AggregateDataBasesFolders(string srcPath, string destPath, string projectName)
        {
            try
            {
                string[] databases = Directory.GetDirectories(srcPath);

                string[] databaseFolderNames = await GetDatabaseFolderNames(projectName);

                foreach (string database in databases)
                {
                    string databaseName = Path.GetFileName(database);

                    int slashIndex = databaseName.LastIndexOf('-');

                    string databaseDirectoryName;

                    if (slashIndex != -1)
                        databaseDirectoryName = databaseName[(slashIndex + 1)..];
                    else
                        databaseDirectoryName = databaseName;

                    string databasesDestinationPath = Path.Combine(destPath,
                                                                   databaseFolderNames.FirstOrDefault(db => db.Contains(databaseDirectoryName) ||
                                                                                                            databaseDirectoryName.Contains(db),
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
            catch (Exception)
            {
                throw;
            }
        }

        private static void AggregateReportsFolders(string srcPath, string destPath)
        {
            try
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
            catch (Exception)
            {
                throw;
            }
        }

        private static void AggregateUnknownFolders(string srcPath, string destPath)
        {
            try
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
            catch (Exception)
            {
                throw;
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
            try
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
            catch (Exception)
            {
                throw;
            }
        }

        private static void ApplicationMergeFiles(string localPath)
        {
            try
            {
                string[] applicationDirectories = Directory.GetDirectories(Path.Combine(localPath, "Applications"));

                foreach (string applicationDirectory in applicationDirectories)
                {
                    string[] applicationFiles = Directory.GetFiles(applicationDirectory);

                    if (applicationFiles.Length < 2)
                        continue;

                    Dictionary<string, int> extensions = [];

                    foreach (string applicationFile in applicationFiles)
                    {
                        string fileExtension = Path.GetFileName(applicationFile)[(Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-') + 1)..];

                        if (!extensions.ContainsKey(fileExtension))
                            extensions[fileExtension] = 1;
                        else
                            extensions[fileExtension]++;
                    }

                    if (!extensions.Values.Any(value => value >= 2))
                        continue;

                    foreach (string extension in extensions.Keys)
                    {
                        StringBuilder fileContent = new();
                        string fileName = "";

                        foreach (string applicationFile in applicationFiles)
                        {
                            string appFileExtension = Path.GetFileName(applicationFile)[(Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-') + 1)..];

                            fileName = Path.GetFileName(applicationFile)[..Path.GetFileNameWithoutExtension(applicationFile).IndexOf('-')];

                            if (appFileExtension == extension)
                            {
                                if (extension.Contains("txt"))
                                {
                                    fileContent.Append(System.IO.File.ReadAllText(applicationFile));
                                    fileContent.Append("\n\n**********************************************************\n\n");
                                    System.IO.File.Delete(applicationFile);
                                }
                                else if (extension.Contains("json"))
                                {
                                    StringBuilder content = new(System.IO.File.ReadAllText(applicationFile));

                                    content.Remove(0, 2);
                                    content.Remove(content.Length - 2, 2);
                                    content.Append(',');

                                    fileContent.Append(content);
                                    System.IO.File.Delete(applicationFile);
                                }
                            }
                        }

                        if (extension.Contains("json"))
                        {
                            fileContent.Remove(fileContent.Length - 1, 1);
                            fileContent.Insert(0, '{');
                            fileContent.Append('}');
                        }

                        bool merged = extensions[extension] >= 2;
                        if (merged)
                        {
                            using StreamWriter writer = System.IO.File.CreateText(Path.Combine(applicationDirectory, string.Concat(fileName, "(M)", "-", extension)));
                            writer.Write(fileContent);
                        }
                        else
                        {
                            using StreamWriter writer = System.IO.File.CreateText(Path.Combine(applicationDirectory, string.Concat(fileName, "-", extension)));
                            writer.Write(fileContent);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static byte[] ZipLocalFolder(string filesPath)
        {
            string zipFilePath = filesPath + ".zip";

            if (System.IO.File.Exists(zipFilePath))
                System.IO.File.Delete(zipFilePath);

            ZipFile.CreateFromDirectory(filesPath, zipFilePath);

            return System.IO.File.ReadAllBytes(zipFilePath);
        }

        private static void DeleteRequestDirectory(string rootPath, string requestFolderName)
        {
            string requestDirectoryPath = Path.Combine(rootPath, requestFolderName);

            foreach (string file in Directory.EnumerateFiles(requestDirectoryPath, "*", SearchOption.AllDirectories))
                System.IO.File.SetAttributes(file, FileAttributes.Normal);

            if (Directory.Exists(requestDirectoryPath))
                Directory.Delete(requestDirectoryPath, true);
        }

        private async Task<List<string>> GetBitbucketBranches(string repoName, string? username, string? appPassword)
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

        private async Task<List<string>> GetBitbucketRepositories(string? username, string? appPassword)
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
        #endregion

        private List<string> GetOperationFolderNames(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            List<string> operations = [];

            foreach (string folder in folders)
                operations.AddRange(Directory.GetDirectories(folder));

            operations = operations.DistinctBy(op => Path.GetFileName(op)).ToList();

            FolderNameConfig folderNameConfig = new();
            configuration.GetSection("Operations").Bind(folderNameConfig.FolderNames);

            operations = ClassifyFolderNames(operations, folderNameConfig).Values.Distinct().ToList();

            return operations;
        }

        private static Dictionary<string, string> ClassifyFolderNames(List<string> folderNames, FolderNameConfig folderNameConfig)
        {
            Dictionary<string, string> classifiedNames = [];

            foreach (string folder in folderNames)
            {
                string? matchedCategory = GetSimilarFolderName(folder, folderNameConfig);

                if (string.IsNullOrEmpty(matchedCategory))
                    matchedCategory = "Unknown";

                classifiedNames[folder] = matchedCategory;
            }

            return classifiedNames;
        }

        private static string? GetSimilarFolderName(string folderName, FolderNameConfig folderNameConfig, string? category = null)
        {
            if (category == null)
                return folderNameConfig.FolderNames
                                       .FirstOrDefault(op => op.Value.Any(alias => folderName.Contains(alias, StringComparison.OrdinalIgnoreCase)))
                                       .Key;

            else if (!folderNameConfig.FolderNames.ContainsKey(category))
                return null;

            return folderNameConfig.FolderNames[category]
                                   .FirstOrDefault(alias => folderName.Contains(alias, StringComparison.OrdinalIgnoreCase)) != null
                                   ? category : null;
        }

        private List<string> GetApplicationFolderNames(string path)
        {
            string[] folders = Directory.GetDirectories(path); // list of versions
            List<string> applications = [];

            FolderNameConfig folderNameConfig = new();
            configuration.GetSection("Operations").Bind(folderNameConfig.FolderNames);

            foreach (string folder in folders) // each of version
            {
                List<string> applicationFolders = Directory.GetDirectories(folder)
                            .Where(dir => GetSimilarFolderName(Path.GetFileName(dir), folderNameConfig, "Applications") == "Applications").ToList();

                foreach (string applicationFolder in applicationFolders)
                    applications.AddRange(Directory.GetDirectories(applicationFolder));
            }

            applications = applications.Select(app => Path.GetFileName(app)).Distinct().ToList();

            return applications;
        }

    }
}


