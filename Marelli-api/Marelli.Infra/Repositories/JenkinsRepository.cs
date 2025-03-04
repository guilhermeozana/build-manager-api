using Marelli.Domain.Dtos;
using Marelli.Domain.Entities;
using Marelli.Infra.Context;
using Marelli.Infra.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Marelli.Infra.Repositories;

public class JenkinsRepository : IJenkinsRepository
{
    private readonly DemurrageContext _context;

    public JenkinsRepository(DemurrageContext context)
    {
        _context = context;
    }

    public async Task<JenkinsAllDataResponse> GetAllData(int userId, int projectId)
    {
        var buildingStates = await ListBuildingState(userId, projectId);
        var logAndArtifacts = await ListLogAndArtifact(userId, projectId);
        var fileVerifies = await ListFileVerify(userId, projectId);

        return new JenkinsAllDataResponse
        {
            BuildingStates = buildingStates,
            LogAndArtifacts = logAndArtifacts,
            FileVerifies = fileVerifies
        };
    }

    //Building State

    public async Task<BuildingState> SaveBuildingState(BuildingState entity)
    {
        _context.BuildingState.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<List<BuildingState>> ListBuildingState(int userId, int projectId)
    {
        return await _context.BuildingState
            .Where(bs => bs.UserId == userId && bs.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<BuildingState> GetBuildingStateById(int id)
    {
        return await _context.BuildingState
            .Where(bs => bs.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<BuildingState> GetBuildingStateByBuildId(int buildId)
    {
        return await _context.BuildingState
            .Where(bs => bs.BuildId == buildId)
            .FirstOrDefaultAsync();
    }

    public async Task<BuildingState> UpdateBuildingState(int id, BuildingState current, BuildingState updated)
    {

        current.UserId = updated.UserId;
        current.ProjectId = updated.ProjectId;
        current.BuildId = updated.BuildId;
        current.JenkinsBuildLogFile = updated.JenkinsBuildLogFile;
        current.Date = updated.Date;
        current.Integrating = updated.Integrating;
        current.IntegratingDate = updated.IntegratingDate;
        current.Starting = updated.Starting;
        current.StartingDate = updated.StartingDate;
        current.ApplGen = updated.ApplGen;
        current.ApplGenDate = updated.ApplGenDate;
        current.NvmGen = updated.NvmGen;
        current.NvmGenDate = updated.NvmGenDate;
        current.ParametersGen = updated.ParametersGen;
        current.ParametersGenDate = updated.ParametersGenDate;
        current.DiagnoseGen = updated.DiagnoseGen;
        current.DiagnoseGenDate = updated.DiagnoseGenDate;
        current.NetworkGen = updated.NetworkGen;
        current.NetworkGenDate = updated.NetworkGenDate;
        current.RteGen = updated.RteGen;
        current.RteGenDate = updated.RteGenDate;
        current.UpdateIds = updated.UpdateIds;
        current.UpdateIdsDate = updated.UpdateIdsDate;
        current.Compiling = updated.Compiling;
        current.CompilingDate = updated.CompilingDate;
        current.Finished = updated.Finished;
        current.FinishedDate = updated.FinishedDate;
        current.Download = updated.Download;

        _context.BuildingState.Entry(current).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return current;
    }

    public async Task<int> DeleteBuildingState(BuildingState buildingState)
    {

        _context.Remove(buildingState);

        return await _context.SaveChangesAsync();

    }

    //Log And Artifact

    public async Task<int> SaveLogAndArtifact(LogAndArtifact entity)
    {
        _context.LogAndArtifact.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<List<LogAndArtifact>> ListLogAndArtifact(int userId, int projectId)
    {
        return await _context.LogAndArtifact
            .Where(bs => bs.UserId == userId && bs.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<LogAndArtifact> GetLogAndArtifactById(int id)
    {
        return await _context.LogAndArtifact
            .Where(la => la.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<LogAndArtifact> GetLogAndArtifactByBuildId(int buildId)
    {
        return await _context.LogAndArtifact
            .Where(la => la.BuildId == buildId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateLogAndArtifact(int id, LogAndArtifact current, LogAndArtifact req)
    {

        current.UserId = req.UserId;
        current.ProjectId = req.ProjectId;
        current.BuildId = req.BuildId;
        current.FileLogS3BucketLocation = req.FileLogS3BucketLocation;
        current.FileOutS3BucketLocation = req.FileOutS3BucketLocation;

        _context.LogAndArtifact.Entry(current).State = EntityState.Modified;
        return await _context.SaveChangesAsync();

    }

    public async Task<int> DeleteLogAndArtifact(LogAndArtifact logAndArtifact)
    {

        _context.Remove(logAndArtifact);

        return await _context.SaveChangesAsync();

    }

    //File Verify

    public async Task<int> SaveFileVerify(FileVerify entity)
    {
        _context.FileVerify.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<List<FileVerify>> ListFileVerify(int userId, int projectId)
    {
        return await _context.FileVerify
            .Where(f => f.UserId == userId && f.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<FileVerify> GetFileVerifyById(int id)
    {
        return await _context.FileVerify
            .Where(la => la.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<FileVerify> GetFileVerifyByBuildId(int buildId)
    {
        return await _context.FileVerify
            .Where(la => la.BuildId == buildId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> UpdateFileVerify(int id, FileVerify current, FileVerify req)
    {

        current.UserId = req.UserId;
        current.ProjectId = req.ProjectId;
        current.BuildId = req.BuildId;
        current.FileZipS3BucketLocation = req.FileZipS3BucketLocation;
        current.IsFileOk = req.IsFileOk;
        current.Filename = req.Filename;

        _context.FileVerify.Entry(current).State = EntityState.Modified;
        return await _context.SaveChangesAsync();
    }

    public async Task<int> DeleteFileVerify(FileVerify fileVerify)
    {

        _context.Remove(fileVerify);

        return await _context.SaveChangesAsync();

    }

}
