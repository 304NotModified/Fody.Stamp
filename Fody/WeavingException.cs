using System;

public class WeavingException : Exception
{
    public WeavingException(string message)
        : base(message)
    {

    }

    /// <inheritdoc />
    public WeavingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}