using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Veraximet.Contracts.Dto;
using Veraximet.Contracts.Exceptions;
using Veraximet.Contracts.Repos;
using Veraximet.Domain.Entitys;
using Veraximet.Domain.Enums;
using Veraximet.Infrastructure.Persistence;

namespace Veraximet.Infrastructure.Repos
{
    public sealed class InstrumentRepo : IInstrumentRepo
    {
        private readonly IDbContextFactory<VeraximetDbContext> _dbFactory;

        public InstrumentRepo(IDbContextFactory<VeraximetDbContext> dbFactory) =>
            _dbFactory = dbFactory;

        public async Task<Instrument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Instruments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<PagedResult<Instrument>> SearchAsync( InstrumentSearchCriteria criteria, CancellationToken cancellationToken = default)
        {
            if (criteria is null)
                throw new ArgumentNullException(nameof(criteria));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            IQueryable<Instrument> query = db.Instruments.AsNoTracking();

            if (!criteria.IncludeArchived && criteria.OperationalStatus != OperationalStatus.Archived)
                query = query.Where(x => x.OperationalStatus != OperationalStatus.Archived);

            if (criteria.OperationalStatus is { } operationalStatus)
                query = query.Where(x => x.OperationalStatus == operationalStatus);

            if (criteria.ConformityStatus is { } conformityStatus)
                query = query.Where(x => x.ConformityStatus == conformityStatus);

            if (!string.IsNullOrWhiteSpace(criteria.SearchText))
            {
                var escaped = criteria.SearchText.Trim()
                    .Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("%", "\\%", StringComparison.Ordinal)
                    .Replace("_", "\\_", StringComparison.Ordinal);
                var pattern = $"%{escaped}%";


                query = query.Where(x =>
                    EF.Functions.Like(x.Code, pattern, "\\") ||
                    EF.Functions.Like(x.Name, pattern, "\\") ||
                    (x.SerialNumber != null && EF.Functions.Like(x.SerialNumber, pattern, "\\")));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Category))
            {
                var category = criteria.Category.Trim();
                query = query.Where(x => x.Category == category);
            }

            if (!string.IsNullOrWhiteSpace(criteria.Department))
            {
                var department = criteria.Department.Trim();
                query = query.Where(x => x.Department == department);
            }



            if (criteria.DueStatus is { } dueStatus)
            {
                var today = criteria.Today ?? DateOnly.FromDateTime(DateTime.Today);
                var dueSoonLimit = today.AddDays(criteria.DueSoonDays);
                query = dueStatus switch
                {
                    DueStatus.NotScheduled => query.Where(x => x.NextDueDate == null),
                    DueStatus.Overdue => query.Where(x => x.NextDueDate != null && x.NextDueDate < today),
                    DueStatus.DueSoon => query.Where(x => x.NextDueDate != null && x.NextDueDate >= today && x.NextDueDate <= dueSoonLimit),
                    DueStatus.Valid => query.Where(x => x.NextDueDate != null && x.NextDueDate > dueSoonLimit),
                    _ => throw new ArgumentOutOfRangeException(nameof(criteria.DueStatus))
                };
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var offset = checked((criteria.Page - 1) * criteria.PageSize);
            var rows = await query.OrderBy(x => x.Code).Skip(offset).Take(criteria.PageSize).ToListAsync(cancellationToken);

            return new PagedResult<Instrument>(rows, totalCount, criteria.Page, criteria.PageSize);
        }

        public async Task<Guid> CreateAsync( Instrument instrument, CancellationToken cancellationToken = default)
        {
            if (instrument is null)
                throw new ArgumentNullException(nameof(instrument));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            db.Instruments.Add(instrument);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new DuplicateInstrumentCodeException(instrument.Code, ex);
            }

            return instrument.Id;
        }

        public async Task<bool> UpdateDetailsAsync( Instrument instrument, CancellationToken cancellationToken = default)
        {
            if (instrument is null) 
                throw new ArgumentNullException(nameof(instrument));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);




            var nextDue = instrument.ConformityStatus == ConformityStatus.Conforming && instrument.LastVerificationDate is { } last
                ? last.AddMonths(instrument.VerificationIntervalMonths)
                : (DateOnly?)null;

            var affected = await db.Instruments.Where(x => x.Id == instrument.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Name, instrument.Name)
                    .SetProperty(x => x.Category, instrument.Category)
                    .SetProperty(x => x.VerificationIntervalMonths, instrument.VerificationIntervalMonths)
                    .SetProperty(x => x.NextDueDate, nextDue)
                    .SetProperty(x => x.Description, instrument.Description)
                    .SetProperty(x => x.Manufacturer, instrument.Manufacturer)
                    .SetProperty(x => x.Model, instrument.Model)
                    .SetProperty(x => x.SerialNumber, instrument.SerialNumber)
                    .SetProperty(x => x.Department, instrument.Department)
                    .SetProperty(x => x.Location, instrument.Location)
                    .SetProperty(x => x.Notes, instrument.Notes), cancellationToken);

            return affected > 0;


        }

        public async Task<bool> UpdateOperationalStatusAsync( Instrument instrument, CancellationToken cancellationToken = default)
        {

            if (instrument == null) 
                throw new ArgumentNullException(nameof(instrument));

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);


            var affected = await db.Instruments.Where(x => x.Id == instrument.Id)
                .ExecuteUpdateAsync(setters => setters .SetProperty(x => x.OperationalStatus, instrument.OperationalStatus), cancellationToken);



            return affected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var instrument = await db.Instruments.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);


            if (instrument is null)
                return false;

            db.Instruments.Remove(instrument);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
            {
                throw new InstrumentInUseException(id, ex);
            }

            return true;
        }

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException
            {
                SqliteErrorCode: 19,
                SqliteExtendedErrorCode: 2067
            };

        private static bool IsForeignKeyViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException
            {
                SqliteErrorCode: 19,
                SqliteExtendedErrorCode: 787
            };
    }
}
