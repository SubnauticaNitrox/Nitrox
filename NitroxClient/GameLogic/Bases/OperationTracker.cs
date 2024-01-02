using System;

namespace NitroxClient.GameLogic.Bases;

public class OperationTracker
{
    // TODO: Add the target's Id in here if future works requires it
    public int LastOperationId = -1;
    /// <summary>
    /// Accounts for locally-issued build actions
    /// </summary>
    public int LocalOperations;
    /// <summary>
    /// Calculated number of missed build actions
    /// </summary>
    public int MissedOperations;
    /// <summary>
    /// Number of detected issues when trying to apply actions remotely
    /// </summary>
    public int FailedOperations;

    public void RegisterOperation(int newOperationId)
    {
        // If the progress was never registered, we don't need to account for missed operations
        if (LastOperationId != -1)
        {
            MissedOperations += Math.Max(newOperationId - (LastOperationId + LocalOperations) - 1, 0);
        }
        LastOperationId = newOperationId;
        LocalOperations = 0;
    }

    public void ResetToId(int operationId = 0)
    {
        LastOperationId = operationId;
        LocalOperations = 0;
        MissedOperations = 0;
        FailedOperations = 0;
    }

    public bool IsDesynced()
    {
        return MissedOperations + FailedOperations > 0;
    }
}
