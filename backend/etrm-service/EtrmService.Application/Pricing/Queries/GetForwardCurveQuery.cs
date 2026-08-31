using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Pricing.DTOs;
using MediatR;

namespace EtrmService.Application.Pricing.Queries;

public class GetForwardCurveQuery : IRequest<List<ForwardCurvePointDto>>
{
}

public class GetForwardCurveQueryHandler : IRequestHandler<GetForwardCurveQuery, List<ForwardCurvePointDto>>
{
    public async Task<List<ForwardCurvePointDto>> Handle(GetForwardCurveQuery request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return new List<ForwardCurvePointDto>
        {
            new ForwardCurvePointDto { Month = "2026-09", PldSE = 384.50m, PldS = 362.10m, PldNE = 402.30m, PldN = 377.80m },
            new ForwardCurvePointDto { Month = "2026-10", PldSE = 391.20m, PldS = 370.40m, PldNE = 410.60m, PldN = 388.90m },
            new ForwardCurvePointDto { Month = "2026-11", PldSE = 398.80m, PldS = 376.90m, PldNE = 418.20m, PldN = 395.10m },
            new ForwardCurvePointDto { Month = "2026-12", PldSE = 412.40m, PldS = 388.70m, PldNE = 432.50m, PldN = 408.60m },
            new ForwardCurvePointDto { Month = "2027-01", PldSE = 426.90m, PldS = 401.30m, PldNE = 448.10m, PldN = 422.40m },
            new ForwardCurvePointDto { Month = "2027-02", PldSE = 419.60m, PldS = 395.80m, PldNE = 440.70m, PldN = 415.90m },
            new ForwardCurvePointDto { Month = "2027-03", PldSE = 405.30m, PldS = 383.50m, PldNE = 425.90m, PldN = 402.60m }
        };
    }
}
