using System;
using Veraximet.Contracts.Dto;

namespace Veraximet.Contracts.Services
{
    public interface IDemoInstrumentSeeder
    {
        Task<DemoSeedResultDto> SeedAsync(CancellationToken cancellationToken = default);
    }

}
