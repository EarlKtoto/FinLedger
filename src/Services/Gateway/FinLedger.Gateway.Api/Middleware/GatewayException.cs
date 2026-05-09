namespace FinLedger.Gateway.Api.Middleware;

public abstract class GatewayException : Exception
{
    protected GatewayException(int statusCode, string title, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Title = title;
    }

    public int StatusCode { get; }

    public string Title { get; }
}

public sealed class GatewayValidationException : GatewayException
{
    public GatewayValidationException(string message)
        : base(StatusCodes.Status400BadRequest, "Bad Request", message)
    {
    }
}

public sealed class GatewayNotFoundException : GatewayException
{
    public GatewayNotFoundException(string message)
        : base(StatusCodes.Status404NotFound, "Not Found", message)
    {
    }
}

public sealed class GatewayConflictException : GatewayException
{
    public GatewayConflictException(string message)
        : base(StatusCodes.Status409Conflict, "Conflict", message)
    {
    }
}

public sealed class GatewayBadGatewayException : GatewayException
{
    public GatewayBadGatewayException(string message, Exception? innerException = null)
        : base(StatusCodes.Status502BadGateway, "Bad Gateway", message, innerException)
    {
    }
}

public sealed class GatewayTimeoutException : GatewayException
{
    public GatewayTimeoutException(string message, Exception? innerException = null)
        : base(StatusCodes.Status504GatewayTimeout, "Gateway Timeout", message, innerException)
    {
    }
}
