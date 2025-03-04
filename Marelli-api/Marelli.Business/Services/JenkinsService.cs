using Marelli.Business.Exceptions;
using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Business.IServices;
using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Marelli.Business.Services;

public class JenkinsService : IJenkinsService
{
    private readonly IJenkinsRepository _jenkinsRepository;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<BuildStateHub> _hubContext;
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly IBuildTableRowService _buildTableRowService;
    private readonly IBaselineService _baselineService;
    private readonly IFileService _fileService;
    private readonly IBuildLogService _buildLogService;
    private readonly HttpClient _httpClient;
    private bool error = false;


    public JenkinsService(IConfiguration configuration,
        IJenkinsRepository jenkinsRepository,
        IHubContext<BuildStateHub> hubContext,
        IUserService userService,
        IProjectService projectService,
        IBuildTableRowService buildTableRowService,
        IBaselineService baselineService,
        ICustomHttpClientFactory httpClientFactory,
        IFileService fileService,
        IBuildLogService buildLogService)
    {
        _jenkinsRepository = jenkinsRepository;
        _configuration = configuration;
        _hubContext = hubContext;
        _userService = userService;
        _projectService = projectService;
        _buildTableRowService = buildTableRowService;
        _baselineService = baselineService;
        _fileService = fileService;
        _buildLogService = buildLogService;
        _httpClient = httpClientFactory.GetHttpClient();
    }

    public async Task<BuildingState> Invoke(int userId, int projectId, int buildId, bool sendNotification, bool rebuild)
    {
        error = false;
        var buildTable = new BuildTableRow();
        var buildingState = new BuildingState();

        try
        {
            using (HttpClient client = _httpClient)
            {
                var baseUrl = _configuration["Jenkins:UrlHost"];
                var token = _configuration["Jenkins:Token"];

                buildTable = await _buildTableRowService.GetBuildTable(buildId);

                if (!new[] { "Build", "Failed", "Download" }.Contains(buildTable.Status))
                {
                    throw new InvalidOperationException("The build has already been started.");
                }

                if (buildTable.Status.Equals("In Queue"))
                {
                    throw new InvalidOperationException("The build is already queued.");
                }

                var baseline = await _baselineService.GetBaselineByProject(projectId);

                if (baseline == null)
                {
                    throw new InvalidOperationException($"There is no baseline defined for this project. Please set a baseline before proceeding.");
                }

                var byteArray = new System.Text.UTF8Encoding().GetBytes($"admin:{token}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var currentTimestamp = "";
                var oldTimestamp = buildTable.TagName;
                var newFileName = "";
                var inProgressBuilds = await _buildTableRowService.ListBuildTableInProgress(0);

                var currentDate = DateTime.UtcNow;
                currentTimestamp = currentDate.ToString("yyyyMMddHHmmssffff");

                var oldDirectory = $"{buildTable.ProjectId}/{buildTable.UserId}/{buildId}/staging";
                var oldFileName = $"{buildTable.ProjectId}_{buildTable.UserId}_{oldTimestamp}";

                var newDirectory = $"{projectId}/{userId}/{buildId}/staging";
                newFileName = $"{projectId}_{userId}_{currentTimestamp}";

                await _fileService.RenameZip(oldDirectory, oldFileName, newDirectory, newFileName);

                buildingState = new BuildingState()
                {
                    UserId = userId,
                    ProjectId = projectId,
                    BuildId = buildId,
                    JenkinsBuildLogFile = "",
                    Date = currentDate,
                    InQueue = inProgressBuilds.Count > 0,
                    Starting = inProgressBuilds.Count > 0 ? "Waiting" : "In Progress",
                    StartingDate = DateTime.UtcNow,
                    Integrating = "Waiting",
                    IntegratingDate = DateTime.MinValue,
                    ApplGen = "Waiting",
                    ApplGenDate = DateTime.MinValue,
                    NvmGen = "Waiting",
                    NvmGenDate = DateTime.MinValue,
                    ParametersGen = "Waiting",
                    ParametersGenDate = DateTime.MinValue,
                    DiagnoseGen = "Waiting",
                    DiagnoseGenDate = DateTime.MinValue,
                    NetworkGen = "Waiting",
                    NetworkGenDate = DateTime.MinValue,
                    RteGen = "Waiting",
                    RteGenDate = DateTime.MinValue,
                    UpdateIds = "Waiting",
                    UpdateIdsDate = DateTime.MinValue,
                    Compiling = "Waiting",
                    CompilingDate = DateTime.MinValue,
                    Finished = "Waiting",
                    FinishedDate = DateTime.MinValue,
                    Download = "Waiting"
                };

                buildTable.UserId = userId;
                buildTable.ProjectId = projectId;
                buildTable.Date = currentDate;
                buildTable.TagName = currentTimestamp;
                buildTable.SendNotification = sendNotification;
                buildTable.Status = inProgressBuilds.Count > 0 ? "In Queue" : "Starting";

                await _buildTableRowService.UpdateBuildTable(buildId, buildTable);

                if (rebuild)
                {
                    await _buildLogService.DeleteBuildLogsByBuildId(buildId);

                    var buildingStateByBuildId = await GetBuildingStateByBuildId(buildId);

                    if (buildingStateByBuildId != null)
                    {
                        buildingState = await UpdateBuildingState(buildingStateByBuildId.Id, buildingState);
                    }
                }
                else
                {
                    buildingState = await SaveBuildingState(buildingState);
                }

                await CreateJob(client, newFileName, baseUrl);
                await StartBuild(client, newFileName, baseUrl, userId, projectId, buildId);

                return buildingState;
            }
        }
        catch (InvalidOperationException ioex)
        {
            error = true;
            throw new InvalidOperationException("Invalid Operation: " + ioex.Message);
        }
        catch (Exception ex)
        {
            error = true;
            throw new Exception($"Internal server error: {ex.Message}");
        }
        finally
        {
            if (error)
            {
                buildTable.Status = "Failed";
                await _buildTableRowService.UpdateBuildTable(buildId, buildTable);

                if (buildingState.Id > 0)
                {
                    buildingState.Starting = "Failed";
                    buildingState = await UpdateBuildingState(buildingState.Id, buildingState);
                }
                else
                {
                    var buildingStateByBuildId = await GetBuildingStateByBuildId(buildId);

                    if (buildingStateByBuildId != null)
                    {
                        await DeleteBuildingState(buildingStateByBuildId.Id);
                    }
                }
            }
        }
    }


    private async Task CreateJob(HttpClient client, string fileName, string baseUrl)
    {

        var createJobContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("NEW_JOB_NAME", fileName)
        });

        HttpResponseMessage createJobResponse = await client.PostAsync($"{baseUrl}/job/create_jobs/buildWithParameters", createJobContent);

        if (!createJobResponse.IsSuccessStatusCode)
        {
            error = true;
            throw new Exception($"Error while invoking Jenkins: {createJobResponse.StatusCode}");
        }

        var isJobCreated = false;

        while (isJobCreated == false)
        {
            HttpResponseMessage jobJsonResponse = await client.GetAsync($"{baseUrl}/job/create_jobs/api/json");
            var jobJson = JObject.Parse(await jobJsonResponse.Content.ReadAsStringAsync());
            var inQueue = jobJson["inQueue"].ToObject<bool>();

            HttpResponseMessage lastBuildJobJsonResponse = await client.GetAsync($"{baseUrl}/job/create_jobs/lastBuild/api/json");
            var lastBuildJobJson = JObject.Parse(await lastBuildJobJsonResponse.Content.ReadAsStringAsync());
            var inProgress = lastBuildJobJson["inProgress"].ToObject<bool>();
            var result = lastBuildJobJson["result"].ToString();

            if (!inQueue && !inProgress)
            {
                if (result.Equals("FAILURE"))
                {
                    error = true;
                    throw new Exception($"Error while invoking Jenkins: {createJobResponse.StatusCode}");
                }

                isJobCreated = true;
            }

        }
    }

    private async Task StartBuild(HttpClient client, string fileName, string baseUrl, int userId, int projectId, int buildId)
    {
        var buildJobContent = new FormUrlEncodedContent(new[]
        {
                        new KeyValuePair<string, string>("FILE_NAME", $"{fileName}.zip"),
                        new KeyValuePair<string, string>("PROJECT_ID", projectId.ToString()),
                        new KeyValuePair<string, string>("USER_ID", userId.ToString()),
                        new KeyValuePair<string, string>("BUILD_ID", buildId.ToString())
                    });

        HttpResponseMessage buildJobResponse = await client.PostAsync($"{baseUrl}/job/{fileName}/buildWithParameters", buildJobContent);

        if (!buildJobResponse.IsSuccessStatusCode)
        {
            error = true;
            throw new Exception($"Error while starting build: {buildJobResponse.StatusCode}");
        }
    }

    public async Task StopBuild(int buildId)
    {
        using (HttpClient client = _httpClient)
        {
            var baseUrl = _configuration["Jenkins:UrlHost"];
            var token = _configuration["Jenkins:Token"];

            var buildTable = await _buildTableRowService.GetBuildTable(buildId);
            var buildJobName = $"{buildTable.ProjectId}_{buildTable.UserId}_{buildTable.TagName}";

            if (new[] { "Build", "Failed", "Download" }.Contains(buildTable.Status))
            {
                throw new InvalidOperationException("The build is not in progress.");
            }

            var byteArray = new System.Text.UTF8Encoding().GetBytes($"admin:{token}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            HttpResponseMessage jobResponse = await client.GetAsync($"{baseUrl}/job/{buildJobName}/api/json");

            if (!jobResponse.IsSuccessStatusCode)
            {
                throw new Exception("Error while deleting Build. Jenkins job not found.");
            }

            while (jobResponse.IsSuccessStatusCode)
            {
                await client.PostAsync($"{baseUrl}/job/{buildJobName}/doDelete", null);

                jobResponse = await client.GetAsync($"{baseUrl}/job/{buildJobName}/api/json");
            }

            buildTable.Status = "Build";

            await _buildTableRowService.UpdateBuildTable(buildId, buildTable);

            var buildingState = await GetBuildingStateByBuildId(buildId);

            if (buildingState != null)
            {
                buildingState = new BuildingState()
                {
                    Id = buildingState.Id,
                    UserId = buildTable.UserId,
                    ProjectId = buildTable.ProjectId,
                    BuildId = buildId,
                    JenkinsBuildLogFile = "",
                    Date = DateTime.MinValue,
                    InQueue = false,
                    Starting = "Waiting",
                    StartingDate = DateTime.UtcNow,
                    Integrating = "Waiting",
                    IntegratingDate = DateTime.MinValue,
                    ApplGen = "Waiting",
                    ApplGenDate = DateTime.MinValue,
                    NvmGen = "Waiting",
                    NvmGenDate = DateTime.MinValue,
                    ParametersGen = "Waiting",
                    ParametersGenDate = DateTime.MinValue,
                    DiagnoseGen = "Waiting",
                    DiagnoseGenDate = DateTime.MinValue,
                    NetworkGen = "Waiting",
                    NetworkGenDate = DateTime.MinValue,
                    RteGen = "Waiting",
                    RteGenDate = DateTime.MinValue,
                    UpdateIds = "Waiting",
                    UpdateIdsDate = DateTime.MinValue,
                    Compiling = "Waiting",
                    CompilingDate = DateTime.MinValue,
                    Finished = "Waiting",
                    FinishedDate = DateTime.MinValue,
                    Download = "Waiting"
                };

                await UpdateBuildingState(buildingState.Id, buildingState);

                await DeleteBuildingState(buildingState.Id);
            }

            var logAndArtifact = await GetLogAndArtifactByBuildId(buildId);

            if (logAndArtifact != null)
            {
                await DeleteLogAndArtifact(logAndArtifact.Id);
            }
        }
    }

    public async Task<JenkinsAllDataResponse> GetAllData(int userId, int projectId)
    {
        return await _jenkinsRepository.GetAllData(userId, projectId);
    }

    public async Task<BuildingState> SaveBuildingState(BuildingState request)
    {
        var buildingState = await _jenkinsRepository.SaveBuildingState(request);

        await _hubContext.Clients.All.SendAsync("ReceiveBuildState", buildingState);

        return buildingState;
    }

    public async Task<List<BuildingState>> ListBuildingState(int userId, int projectId)
    {
        return await _jenkinsRepository.ListBuildingState(userId, projectId);
    }

    public async Task<BuildingState> GetBuildingStateById(int id)
    {
        var buildingState = await _jenkinsRepository.GetBuildingStateById(id);

        if (buildingState == null)
        {
            throw new NotFoundException($"Building State not found with ID {id}.");
        }

        return buildingState;
    }

    public async Task<BuildingState> GetBuildingStateByBuildId(int buildId)
    {
        var buildingState = await _jenkinsRepository.GetBuildingStateByBuildId(buildId);

        //if (buildingState == null)
        //{
        //    throw new NotFoundException($"Building State not found with Build ID {buildId}.");
        //}

        return buildingState;
    }

    public async Task<BuildingState> UpdateBuildingState(int id, BuildingState request)
    {
        var savedBuildingState = await GetBuildingStateById(id);

        var buildingStateUpdated = await _jenkinsRepository.UpdateBuildingState(id, savedBuildingState, request);

        await _hubContext.Clients.All.SendAsync("ReceiveBuildState", buildingStateUpdated);

        return buildingStateUpdated;
    }

    public async Task<int> DeleteBuildingState(int id)
    {
        var buildingState = await GetBuildingStateById(id);

        return await _jenkinsRepository.DeleteBuildingState(buildingState);
    }


    public async Task<int> SaveLogAndArtifact(LogAndArtifact request)
    {
        var logAndArtifactByBuildId = await GetLogAndArtifactByBuildId(request.BuildId);

        if (logAndArtifactByBuildId != null)
        {
            return await UpdateLogAndArtifact(logAndArtifactByBuildId.Id, request);
        }

        return await _jenkinsRepository.SaveLogAndArtifact(request);
    }

    public async Task<List<LogAndArtifact>> ListLogAndArtifact(int userId, int projectId)
    {
        return await _jenkinsRepository.ListLogAndArtifact(userId, projectId);
    }

    public async Task<LogAndArtifact> GetLogAndArtifactById(int id)
    {
        var logAndArtifact = await _jenkinsRepository.GetLogAndArtifactById(id);

        if (logAndArtifact == null)
        {
            throw new NotFoundException($"Log and Artifact not found with ID: {id}");
        }

        return logAndArtifact;
    }

    public async Task<LogAndArtifact> GetLogAndArtifactByBuildId(int buildId)
    {
        var logAndArtifact = await _jenkinsRepository.GetLogAndArtifactByBuildId(buildId);

        return logAndArtifact;
    }

    public async Task<int> UpdateLogAndArtifact(int id, LogAndArtifact request)
    {
        var logAndArtifact = await GetLogAndArtifactById(id);

        return await _jenkinsRepository.UpdateLogAndArtifact(id, logAndArtifact, request);
    }

    public async Task<int> DeleteLogAndArtifact(int id)
    {
        var logAndArtifact = await GetLogAndArtifactById(id);

        return await _jenkinsRepository.DeleteLogAndArtifact(logAndArtifact);
    }

    public async Task<int> SaveFileVerify(FileVerify request)
    {
        return await _jenkinsRepository.SaveFileVerify(request);
    }

    public async Task<List<FileVerify>> ListFileVerify(int userId, int projectId)
    {
        return await _jenkinsRepository.ListFileVerify(userId, projectId);
    }

    public async Task<FileVerify> GetFileVerifyById(int id)
    {
        var fileVerify = await _jenkinsRepository.GetFileVerifyById(id);

        if (fileVerify == null)
        {
            throw new NotFoundException($"File Verify not found with ID: {id}");
        }

        return fileVerify;
    }

    public async Task<FileVerify> GetFileVerifyByBuildId(int buildId)
    {
        var fileVerify = await _jenkinsRepository.GetFileVerifyByBuildId(buildId);

        return fileVerify;
    }

    public async Task<int> UpdateFileVerify(int id, FileVerify request)
    {
        var fileVerify = await GetFileVerifyById(id);

        return await _jenkinsRepository.UpdateFileVerify(id, fileVerify, request);
    }

    public async Task<int> DeleteFileVerify(int id)
    {
        var fileVerify = await GetFileVerifyById(id);

        return await _jenkinsRepository.DeleteFileVerify(fileVerify);
    }

}

