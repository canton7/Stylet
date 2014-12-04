using System;

namespace StyletIoC
{
    /// <summary>
    /// Base class for all exceptions describing StyletIoC-specific problems?
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public abstract class StyletIoCException : Exception
    {
        internal StyletIoCException(string message) : base(message) { }
        internal StyletIoCException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// A problem occured with a registration process (failed to register, failed to find a registration, etc)
    /// </summary>
    public class StyletIoCRegistrationException : StyletIoCException
    {
        internal StyletIoCRegistrationException(string message) : base(message) { }
        internal StyletIoCRegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// StyletIoC was unable to find a callable constructor for a type
    /// </summary>
    public class StyletIoCFindConstructorException : StyletIoCException
    {
        internal StyletIoCFindConstructorException(string message) : base(message) { }
    }

    /// <summary>
    /// StyletIoC was unable to create an abstract factory
    /// </summary>
    public class StyletIoCCreateFactoryException : StyletIoCException
    {
        internal StyletIoCCreateFactoryException(string message) : base(message) { }
        internal StyletIoCCreateFactoryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
