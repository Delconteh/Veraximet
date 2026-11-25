using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veraximet.Contracts.Dto.Verification;
using Veraximet.Contracts.Services;
using Veraximet.Domain.Entitys;
using Veraximet.Domain.Enums;
using Veraximet.Infrastructure.Persistence;
using Veraximet.Metrology;

namespace Veraximet.Infrastructure.Services
{
    public sealed class VerificationService : IVerificationService
    {
        private readonly IDbContextFactory<VeraximetDbContext> _dbFactory;
        public VerificationService(IDbContextFactory<VeraximetDbContext> dbFactory) =>
            _dbFactory = dbFactory;

        public async Task<Guid> CreateAsync(CreateVerificationRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Date == default || request.Date > DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("A valid, non-future verification date is required.");


            if (string.IsNullOrWhiteSpace(request.PerformedBy) || request.PerformedBy.Trim().Length > 120)
                throw new ArgumentException("Operator is required (max. 120 characters).");

            if (string.IsNullOrWhiteSpace(request.Unit) || request.Unit.Trim().Length > 24)
                throw new ArgumentException("Unit is required (max. 24 characters).");

            if (request.Notes?.Length > 2000) throw new ArgumentException("Notes are too long.");

            if (request.Points is null || request.Points.Count == 0)
                throw new ArgumentException("At least one verification point is required.");


            var points = request.Points.Select(p => p with { Measurements = p.Measurements.ToArray() }).ToArray();
            bool passed = true;

            foreach (var point in points)
            {
                if (point.Description?.Length > 300) 
                    throw new ArgumentException("Point description is too long.");

                var result = VerificationCalculator.Calculate(point.ReferenceValue,
                    point.MaximumPermissibleError, point.Measurements);

                passed &= result.IsWithinTolerance;
            }
            var status = passed ? ConformityStatus.Conforming : ConformityStatus.Nonconforming;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var instrument = await db.Instruments.SingleOrDefaultAsync(x => x.Id == request.InstrumentId, cancellationToken) ?? throw new InvalidOperationException("Instrument no longer exists.");

            if (instrument.OperationalStatus == OperationalStatus.Archived)
                throw new InvalidOperationException("Archived instruments cannot be verified.");

            var verification = new Verification(instrument.Id, request.Date, request.PerformedBy, request.Unit, request.Notes, VerificationCalculator.DecisionRule, status);


            foreach (var point in points)
                verification.AddPoint(point.ReferenceValue, point.MaximumPermissibleError,point.Description, point.Measurements);

            instrument.RegisterVerification(request.Date, status);
            db.Verifications.Add(verification);

            await db.SaveChangesAsync(cancellationToken);
            return verification.Id;
        }

        public async Task<IReadOnlyList<VerificationDto>> GetHistoryAsync(Guid? instrumentId = null,CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

            var query = db.Verifications.AsNoTracking();

            if (instrumentId is { } id) 
                query = query.Where(x => x.InstrumentId == id);

            var records = await query.Include(x => x.Points).ThenInclude(x => x.Measurements)
                .OrderByDescending(x => x.Date).ThenBy(x => x.Id).ToListAsync(cancellationToken);


            var instruments = await db.Instruments.AsNoTracking()
                .Select(x => new { x.Id, x.Code, x.Name }).ToDictionaryAsync(x => x.Id, cancellationToken);

            return records.Select(v => new VerificationDto(v.Id, v.InstrumentId,
                instruments[v.InstrumentId].Code, instruments[v.InstrumentId].Name, v.Date, v.PerformedBy, v.Unit, v.Notes, v.DecisionRule, v.Result,
                v.Points.OrderBy(p => p.Order).Select(p => new VerificationPointDto( p.ReferenceValue, p.MaximumPermissibleError, p.Description, p.Measurements.OrderBy(m => m.Order).Select(m => m.Value).ToArray())).ToArray())).ToArray();
        }
    }
}
