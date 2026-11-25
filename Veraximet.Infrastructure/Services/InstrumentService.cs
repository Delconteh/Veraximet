using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Repos;
using Veraximet.Contracts.Services;
using Veraximet.Domain.Entitys;
using Veraximet.Domain.Enums;
using Veraximet.Infrastructure.Validation;

namespace Veraximet.Infrastructure.Services
{
        
    public sealed class InstrumentService : IInstrumentService
    {
        private readonly IInstrumentRepo _repository;

        public InstrumentService(IInstrumentRepo repository) =>
            _repository = repository;

        public async Task<Guid> CreateAsync( CreateInstrumentRequest request, CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var code = InstrumentInputValidator.NormalizeCode(request.Code);
            var details = InstrumentInputValidator.NormalizeDetails(request.Details);

            var instrument = new Instrument(code);
            ApplyDetails(instrument, details);
            return await _repository.CreateAsync(instrument, cancellationToken);
        }

        public async Task<InstrumentDto?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default)
        {
            var instrument = await _repository.GetByIdAsync(id, cancellationToken);
            return instrument is null ? null : ToDto(instrument);
        }

        public async Task<PagedResult<InstrumentDto>> SearchAsync( InstrumentSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            InstrumentInputValidator.ValidateSearch(criteria);
            var page = await _repository.SearchAsync(criteria, cancellationToken);

            return new PagedResult<InstrumentDto>( page.Items.Select(ToDto).ToArray(), page.TotalCount, page.Page, page.PageSize);
        }

        public async Task<bool> UpdateAsync(  Guid id, InstrumentDetailsDto details, CancellationToken cancellationToken = default)
        {
            var normalized = InstrumentInputValidator.NormalizeDetails(details);
            var instrument = await _repository.GetByIdAsync(id, cancellationToken);

            if (instrument is null)
                return false;

            ApplyDetails(instrument, normalized);

            return await _repository.UpdateDetailsAsync(instrument, cancellationToken);
        }

        public async Task<bool> ChangeOperationalStatusAsync( Guid id, OperationalStatus status, CancellationToken cancellationToken = default)
        {



            if (!Enum.IsDefined(status))
                throw new ArgumentOutOfRangeException(nameof(status));

            var instrument = await _repository.GetByIdAsync(id, cancellationToken);

            if (instrument is null)
                return false;

            instrument.ChangeOperationalStatus(status);

            return await _repository.UpdateOperationalStatusAsync(instrument, cancellationToken);
        }

        public Task<bool> ArchiveAsync(Guid id, CancellationToken cancellationToken = default) =>
            ChangeOperationalStatusAsync(id, OperationalStatus.Archived, cancellationToken);

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            _repository.DeleteAsync(id, cancellationToken);

        private static void ApplyDetails(Instrument instrument, InstrumentDetailsDto details)
        {
            instrument.Name = details.Name;
            instrument.Category = details.Category;
            instrument.VerificationIntervalMonths = details.VerificationIntervalMonths;
            instrument.Description = details.Description;
            instrument.Manufacturer = details.Manufacturer;
            instrument.Model = details.Model;
            instrument.SerialNumber = details.SerialNumber;
            instrument.Department = details.Department;
            instrument.Location = details.Location;
            instrument.Notes = details.Notes;
        }

        private static InstrumentDto ToDto(Instrument instrument) => new InstrumentDto(
            instrument.Id, instrument.Code, instrument.Name, instrument.Category, 
            instrument.VerificationIntervalMonths, instrument.Description,
            instrument.Manufacturer, instrument.Model, instrument.SerialNumber, instrument.Department, instrument.Location, instrument.Notes,
            instrument.LastVerificationDate, instrument.NextDueDate,instrument.OperationalStatus, instrument.ConformityStatus);
    }

}
