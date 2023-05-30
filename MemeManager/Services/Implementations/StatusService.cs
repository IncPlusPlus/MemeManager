using System;
using System.Collections.Concurrent;
using System.Linq;
using MemeManager.Extensions;
using MemeManager.Models;
using MemeManager.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace MemeManager.Services.Implementations;

public class StatusService : IStatusService
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs;
    private readonly ILogger _log;

    public StatusService(ILogger logger)
    {
        _log = logger;
        _jobs = new ConcurrentDictionary<Guid, Job>();
    }

    public event EventHandler<StatusChangedArgs>? StatusChanged;
    // public event EventHandler<AddJobEventArgs>? AddJobEvent;
    // public event EventHandler<UpdateJobEventArgs>? UpdateJobEvent;
    // public event EventHandler<RemoveJobEventArgs>? RemoveJobEvent;

    public void AddStatus(string statusText)
    {
        // TODO: Add a way for these "status messages" to expire
        var newGuid = Guid.NewGuid();
        if (!_jobs.TryAdd(newGuid, new Job() { JobText = statusText, HasProgress = false }))
        {
            _log.LogError("Tried to add a new job with text '{Text}' but failed to add it to the ConcurrentDictionary",
                statusText);
        }

        StatusChanged.Raise(this, ComputeStatusChangedEventArgs());
    }

    public Guid AddJob(string jobText, int currentProgress, int maxProgress)
    {
        var newGuid = Guid.NewGuid();
        if (!_jobs.TryAdd(newGuid,
                new Job()
                {
                    JobText = jobText,
                    HasProgress = true,
                    CurrentProgress = currentProgress,
                    MaxProgress = maxProgress
                }))
        {
            _log.LogError("Tried to add a new job with text '{Text}' but failed to add it to the ConcurrentDictionary",
                jobText);
        }

        StatusChanged.Raise(this, ComputeStatusChangedEventArgs());
        return newGuid;
    }

    public Guid AddJob(string jobText, int progressPercent)
    {
        return AddJob(jobText, progressPercent, 100);
    }

    public void UpdateJob(Guid jobNumber, int currentProgress, int maxProgress)
    {
        if (!_jobs.TryGetValue(jobNumber, out var original))
        {
            _log.LogError("Tried to update job {JobNumber} but couldn't find a job with that ID", jobNumber);
            return;
        }

        if (!_jobs.TryUpdate(jobNumber,
                new Job()
                {
                    JobText = original.JobText,
                    HasProgress = true,
                    CurrentProgress = currentProgress,
                    MaxProgress = maxProgress
                }, original))
        {
            _log.LogError("Tried to update job {JobNumber} but the update operation failed", jobNumber);
        }

        StatusChanged.Raise(this, ComputeStatusChangedEventArgs());
    }

    public void UpdateJob(Guid jobNumber, int progressPercent)
    {
        UpdateJob(jobNumber, progressPercent, 100);
    }

    public void RemoveJob(Guid jobNumber)
    {
        if (!_jobs.TryRemove(jobNumber, out _))
        {
            _log.LogError("Failed to remove job {JobNumber}", jobNumber);
        }

        StatusChanged.Raise(this, ComputeStatusChangedEventArgs());
    }

    private StatusChangedArgs ComputeStatusChangedEventArgs()
    {
        var jobs = _jobs.ToArray().Select(pair => pair.Value);
        var statusText = "One active job";
        var numJobs = 0;
        var hasProgress = false;
        var totalCurrentProgress = 0;
        var totalMaxProgress = 0;
        jobs.ForEach(job =>
        {
            numJobs++;
            // Only set the status text to a non-progress job's text if there are no other jobs with progress.
            // If there are multiple non-progress jobs, only one of those jobs will have their text displayed.
            if (!job.HasProgress)
            {
                if (totalMaxProgress == 0)
                {
                    statusText = job.JobText;
                }
            }
            else
            {
                hasProgress = true;
                // Overwrite the status text with the text from this job. If there are multiple and we're doing a
                // bunch of overwriting, it doesn't matter.
                // That'll be handled in the return statement of ComputeStatusChangedEventArgs() 
                statusText = job.JobText;
                totalCurrentProgress += job.CurrentProgress;
                totalMaxProgress += job.MaxProgress;
            }
        });


        return new StatusChangedArgs(numJobs == 0 ? "No active jobs" : (numJobs > 1 ? $"{numJobs} jobs: " : statusText),
            numJobs, hasProgress, totalCurrentProgress, totalMaxProgress);
    }
}
