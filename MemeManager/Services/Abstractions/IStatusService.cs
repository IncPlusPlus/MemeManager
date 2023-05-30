using System;

namespace MemeManager.Services.Abstractions;

public interface IStatusService
{
    /// <summary>
    /// Fired when the status has changed and a class observing this should look at the new value
    /// </summary>
    event EventHandler<StatusChangedArgs> StatusChanged;
    // event EventHandler<AddJobEventArgs> AddJobEvent;
    // event EventHandler<UpdateJobEventArgs> UpdateJobEvent;
    // event EventHandler<RemoveJobEventArgs> RemoveJobEvent;

    void AddStatus(string statusText);
    Guid AddJob(string jobText, int currentProgress, int maxProgress);
    Guid AddJob(string jobText, int progressPercent);
    void UpdateJob(Guid jobNumber, int currentProgress, int maxProgress);
    void UpdateJob(Guid jobNumber, int progressPercent);
    void RemoveJob(Guid jobNumber);
}

public class Job
{
    public string JobText { get; set; } = null!;

    /// <summary>
    /// Will be false if this is just a basic status message that should last for ~5 minutes
    /// </summary>
    public bool HasProgress { get; set; }

    public int CurrentProgress { get; set; }
    public int MaxProgress { get; set; }
}

public class StatusChangedArgs : EventArgs
{
    public StatusChangedArgs(string statusText, int numJobs, bool hasProgress, int totalCurrentProgress,
        int totalMaxProgress)
    {
        StatusText = statusText;
        NumJobs = numJobs;
        HasProgress = hasProgress;
        TotalCurrentProgress = totalCurrentProgress;
        TotalMaxProgress = totalMaxProgress;
    }

    public string StatusText { get; }
    public int NumJobs { get; }
    public bool HasProgress { get; }
    public int TotalCurrentProgress { get; }
    public int TotalMaxProgress { get; }
}

// public class RemoveJobEventArgs : EventArgs
// {
//     public RemoveJobEventArgs(Guid jobNumber)
//     {
//         JobNumber = jobNumber;
//     }
//
//     public Guid JobNumber { get; }
// }
//
// public class UpdateJobEventArgs : EventArgs
// {
//     public UpdateJobEventArgs(int currentProgress, int maxProgress)
//     {
//         CurrentProgress = currentProgress;
//         MaxProgress = maxProgress;
//     }
//
//     public int CurrentProgress { get; }
//     public int MaxProgress { get; }
// }
//
// public class AddJobEventArgs : UpdateJobEventArgs
// {
//     public AddJobEventArgs(string jobText, int currentProgress, int maxProgress) : base(currentProgress:currentProgress, maxProgress:maxProgress)
//     {
//         JobText = jobText;
//     }
//
//     public string? JobText { get; }
// }
