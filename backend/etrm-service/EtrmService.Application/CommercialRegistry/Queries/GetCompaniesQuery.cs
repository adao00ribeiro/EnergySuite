using System.Collections.Generic;
using MediatR;
using EtrmService.Application.CommercialRegistry.DTOs;

namespace EtrmService.Application.CommercialRegistry.Queries;

public class GetCompaniesQuery : IRequest<List<CompanyDto>>
{
}
