using Marelli.Business.Exceptions;
using Marelli.Business.Factories;
using Marelli.Business.Hubs;
using Marelli.Business.IServices;
using Marelli.Domain.Entities;
using Marelli.Infra.IRepositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;

namespace Marelli.Business.Services
{

    public class BuildTableRowService : IBuildTableRowService
    {
        private readonly IBuildTableRowRepository _buildTableRowsRepository;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IProjectService _projectService;
        private readonly IHubContext<BuildStateHub> _hubContext;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IJenkinsRepository _jenkinsRepository;
        private readonly IFileService _fileService;


        public BuildTableRowService(IBuildTableRowRepository buildTableRowsRepository, IEmailService emailService, IUserService userService, IProjectService projectService, IHubContext<BuildStateHub> hubContext, IConfiguration configuration, ICustomHttpClientFactory httpClientFactory, IJenkinsRepository jenkinsRepository, IFileService fileService)
        {
            _buildTableRowsRepository = buildTableRowsRepository;
            _emailService = emailService;
            _userService = userService;
            _projectService = projectService;
            _hubContext = hubContext;
            _configuration = configuration;
            _httpClient = httpClientFactory.GetHttpClient();
            _jenkinsRepository = jenkinsRepository;
            _fileService = fileService;
        }

        public async Task<BuildTableRow> SaveBuildTable(BuildTableRow request)
        {
            var buildTable = await _buildTableRowsRepository.SaveBuildTableAsync(request);

            var buildTableRows = await ListBuildTable(request.UserId);

            await _hubContext.Clients.All.SendAsync("ReceiveBuildTableRows", buildTableRows);

            return buildTable;
        }

        public async Task<List<BuildTableRow>> ListBuildTable(int userId)
        {
            return await _buildTableRowsRepository.ListBuildTableAsync(userId);
        }

        public async Task<List<BuildTableRow>> ListBuildTableInProgress(int userId)
        {
            return await _buildTableRowsRepository.ListBuildTableInProgressAsync(userId);
        }

        public async Task<List<BuildTableRow>> ListBuildTableInQueue()
        {
            return await _buildTableRowsRepository.ListBuildTableInQueueAsync();
        }

        public async Task<List<BuildTableRow>> ListBuildTableByProject(int projectId)
        {
            return await _buildTableRowsRepository.ListBuildTableByProjectAsync(projectId);
        }

        public async Task<List<BuildTableRow>> ListAllInProgressBuildsOlderThan(int hours, CancellationToken cancellationToken)
        {
            return await _buildTableRowsRepository.ListAllInProgressBuildsOlderThanAsync(hours, cancellationToken);
        }

        public async Task<BuildTableRow> GetBuildTable(int id)
        {
            var buildTable = await _buildTableRowsRepository.GetBuildTableRowAsync(id);

            if (buildTable == null)
            {
                throw new NotFoundException($"Build Table Row not found with ID: {id}.");
            }

            return buildTable;
        }

        public async Task<BuildTableRow> GetLastUploadedByUser(int userId)
        {
            var buildTable = await _buildTableRowsRepository.GetLastUploadedByUserAsync(userId);

            return buildTable;
        }

        public async Task<BuildTableRow> GetLastUploadedByUserProjects(int userId)
        {
            var buildTable = await _buildTableRowsRepository.GetLastUploadedByUserProjectsAsync(userId);

            return buildTable;
        }

        public async Task<BuildTableRow> GetFirstInQueue(int userId)
        {
            var buildTable = await _buildTableRowsRepository.GetFirstInQueueAsync(userId);

            return buildTable;
        }

        public async Task<int> UpdateBuildTable(int id, BuildTableRow request)
        {
            var current = await GetBuildTable(id);

            var updated = await _buildTableRowsRepository.UpdateBuildTableAsync(id, current, request);

            if (new[] { "Finished", "Failed" }.Contains(request.Status) && request.SendNotification)
            {
                await SendEmailBuildFinished(current);
            }

            var buildTableRows = await ListBuildTable(request.UserId);

            await _hubContext.Clients.All.SendAsync("ReceiveBuildTableRows", buildTableRows);

            return updated;
        }

        public async Task<int> DeleteBuildTable(int id)
        {
            var buildTable = await GetBuildTable(id);

            var result = await _buildTableRowsRepository.DeleteBuildTableAsync(buildTable);

            using (HttpClient client = _httpClient)
            {
                var baseUrl = _configuration["Jenkins:UrlHost"];
                var token = _configuration["Jenkins:Token"];
                var buildJobName = $"{buildTable.ProjectId}_{buildTable.UserId}_{buildTable.TagName}";

                var byteArray = new UTF8Encoding().GetBytes($"admin:{token}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                await client.PostAsync($"{baseUrl}/job/{buildJobName}/doDelete", null);
            }

            return result;
        }

        public async Task<int> DeleteBuildTable(BuildTableRow buildTable)
        {
            return await _buildTableRowsRepository.DeleteBuildTableAsync(buildTable);
        }

        private async Task SendEmailBuildFinished(BuildTableRow buildTableRow)
        {
            var user = await _userService.GetUserById(buildTableRow.UserId);
            var project = await _projectService.GetProjectById(buildTableRow.ProjectId);

            var bodyMessage = $@"
            Hi, {user.Name},

            The build with Tag: #{buildTableRow.TagName} from Project '{project.Name}' has been finished {(buildTableRow.Status.Equals("Failed") ? "with errors" : "successfully")}.
            ";

            await _emailService.SendEmail(user.Email, $"Build from {project.Name} has been finished.", bodyMessage);
        }

    }
}