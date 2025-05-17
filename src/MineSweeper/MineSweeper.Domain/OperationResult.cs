namespace MineSweeper.Domain;

public sealed class OperationResult
{
    public bool IsSuccess { get; }

    public string? Message { get; }

    private OperationResult(bool isSuccess, string? message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public static OperationResult CreateFailure(string message) => new(false, message);

    public static OperationResult CreateSuccess(string message) => new(true, message);
    public static OperationResult Success { get; } = new(true, null);
}

public static class OperationResults
{
    public static OperationResult GameOver { get; } = OperationResult.CreateFailure("Game Over");

    public static OperationResult GameWon { get; } = OperationResult.CreateSuccess("Game Won");
}