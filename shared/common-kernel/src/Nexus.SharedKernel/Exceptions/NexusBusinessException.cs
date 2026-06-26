namespace Nexus.SharedKernel.Exceptions;

public sealed class NexusBusinessException : Exception
{
    public NexusBusinessException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
