using System;

namespace Veraximet.Contracts.Exceptions
{
    public sealed class DuplicateInstrumentCodeException : Exception
    {
        public DuplicateInstrumentCodeException(string code, Exception? innerException = null) : base($"Instrument code '{code}' already exists.", innerException)
        {
        }
    }
}
