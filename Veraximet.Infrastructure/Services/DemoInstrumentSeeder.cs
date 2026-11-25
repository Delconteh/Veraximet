using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Exceptions;
using Veraximet.Contracts.Services;
using Veraximet.Domain.Enums;

namespace Veraximet.Infrastructure.Services
{
    public sealed class DemoInstrumentSeeder : IDemoInstrumentSeeder
    {
        private readonly IInstrumentService _instruments;
        public DemoInstrumentSeeder(IInstrumentService instruments) =>
            _instruments = instruments;

        public async Task<DemoSeedResultDto> SeedAsync(CancellationToken cancellationToken = default)
        {
            var created = 0;
            var alreadyPresent = 0;

            foreach (var sample in Samples())
            {

                cancellationToken.ThrowIfCancellationRequested();
                Guid id;

                try
                {
                    id = await _instruments.CreateAsync(new CreateInstrumentRequest(sample.Code,
                        new InstrumentDetailsDto(
                            Name: sample.Name,
                            Category: sample.Category,
                            VerificationIntervalMonths: sample.IntervalMonths,
                            Manufacturer: sample.Manufacturer,
                            Model: sample.Model,
                            SerialNumber: sample.SerialNumber,
                            Department: sample.Department,
                            Location: sample.Location,
                            Description: "Demonstration record")),
                        cancellationToken);
                }
                catch (DuplicateInstrumentCodeException)
                {
                    alreadyPresent++;
                    continue;
                }


                if (sample.OperationalStatus != OperationalStatus.InService && !await _instruments.ChangeOperationalStatusAsync(id, sample.OperationalStatus, cancellationToken))
                    throw new InvalidOperationException($"Could not set operational status for {sample.Code}.");

                created++;
            }

            return new DemoSeedResultDto(created, alreadyPresent);
        }

        private static IEnumerable<Sample> Samples()
        {
            var departments = new List<string> 
            {
                "Quality",
                "Machining",
                "Assembly",
                "Maintenance"
            };
            var locations = new List<string> 
            {
                "Metrology Lab",
                "CNC Line A",
                "Assembly Bench 2",
                "Tool Crib"
            };

            for (var i = 1; i <= 12; i++)
            {
                yield return new Sample(
                    $"CAL-{i:000}", i % 3 == 0 ? "Digital caliper 0–200 mm" : "Digital caliper 0–150 mm",
                    "Calipers", i % 2 == 0 ? "Mitutoyo" : "Sylvac",
                    i % 3 == 0 ? "200 Series" : "150 Series",
                    $"DC-26-{i:0000}", departments[(i - 1) % 4], locations[(i - 1) % 4], 12,
                    i == 9 ? OperationalStatus.Suspended : OperationalStatus.InService);
            }

            for (var i = 1; i <= 8; i++)
            {
                yield return new Sample(
                    $"MIC-{i:000}", i % 2 == 0 ? "Outside micrometer 25–50 mm" : "Outside micrometer 0–25 mm",
                    "Micrometers", "Mitutoyo", "Series 293", $"OM-26-{i:0000}",
                    departments[i % 4], locations[i % 4], 12,
                    i == 5 ? OperationalStatus.Suspended : OperationalStatus.InService);
            }

            for (var i = 1; i <= 6; i++)
            {
                yield return new Sample(
                    $"DIA-{i:000}", "Dial indicator 0–10 mm", "Dial indicators",
                    "Mahr", "Millimess", $"DI-26-{i:0000}",
                    departments[(i + 1) % 4], locations[(i + 1) % 4], 6,
                    OperationalStatus.InService);
            }

            for (var i = 1; i <= 4; i++)
            {
                yield return new Sample(
                    $"PRS-{i:000}", i % 2 == 0 ? "Pressure gauge 0–10 bar" : "Pressure gauge 0–16 bar",
                    "Pressure gauges", "WIKA", "232.50", $"PG-26-{i:0000}",
                    "Maintenance", "Utilities Room", 12,
                    OperationalStatus.InService);
            }

            for (var i = 1; i <= 4; i++)
            {
                yield return new Sample(
                    $"TMP-{i:000}", "Digital contact thermometer", "Thermometers",
                    "Testo", "925", $"TH-26-{i:0000}",
                    "Quality", "Metrology Lab", 12,
                    OperationalStatus.InService);
            }

            for (var i = 1; i <= 4; i++)
            {
                yield return new Sample(
                    $"BOR-{i:000}", "Three-point bore gauge 20–25 mm", "Bore gauges",
                    "Bowers", "XT3", $"BG-26-{i:0000}",
                    "Machining", "CNC Line A", 6,
                    i == 4 ? OperationalStatus.Archived : OperationalStatus.InService);
            }
        }

      
    }

}
