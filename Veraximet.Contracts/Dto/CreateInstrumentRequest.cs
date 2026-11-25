using System;

namespace Veraximet.Contracts.Dto
{
    public sealed record CreateInstrumentRequest(string Code, InstrumentDetailsDto Details);

}
