using System;

namespace Veraximet.Contracts.Exceptions
{
    public sealed class InstrumentInUseException : Exception
    {
        public InstrumentInUseException(Guid instrumentId, Exception? innerException = null) : base($"Instrument '{instrumentId}' has dependent records and cannot be deleted.", innerException)
        {
        }
    }

}
