using Academy.Application.Contracts.Dashboards;

namespace Academy.Application.Abstractions.Dashboards;

public interface IInstructorDashboardService
{
    Task<InstructorDashboardDto> GetAsync(CancellationToken ct);
}
