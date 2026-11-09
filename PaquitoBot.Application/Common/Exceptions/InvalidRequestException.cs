namespace PaquitoBot.Application.Common.Exceptions;

public class InvalidRequestException : Exception
{
    public InvalidRequestException(string message) : base(message)
    {
    }
}
