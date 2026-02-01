using Academy.Application.Contracts.Dashboards;

namespace Academy.Application.Abstractions.Dashboards;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetAsync(CancellationToken ct);
}
