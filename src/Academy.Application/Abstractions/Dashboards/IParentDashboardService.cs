using Academy.Application.Contracts.Dashboards;

namespace Academy.Application.Abstractions.Dashboards;

public interface IParentDashboardService
{
    Task<ParentDashboardDto> GetAsync(CancellationToken ct);
}
