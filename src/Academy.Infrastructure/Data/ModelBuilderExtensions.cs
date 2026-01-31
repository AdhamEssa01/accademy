using System.Linq.Expressions;
using Academy.Domain;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Data;

public static class ModelBuilderExtensions
{
    public static void ApplyAcademyScopedQueryFilters(
        this ModelBuilder builder,
        Expression<Func<Guid?>> academyIdExpression)
    {
        var entityTypes = builder.Model.GetEntityTypes()
            .Where(entityType => typeof(IAcademyScoped).IsAssignableFrom(entityType.ClrType));

        var academyId = academyIdExpression.Body;
        var nullValue = Expression.Constant(null, typeof(Guid?));
        var isNull = Expression.Equal(academyId, nullValue);

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var academyIdProperty = Expression.Property(parameter, nameof(IAcademyScoped.AcademyId));
            var academyIdNullable = Expression.Convert(academyIdProperty, typeof(Guid?));
            var equalExpression = Expression.Equal(academyIdNullable, academyId);
            var body = Expression.OrElse(isNull, equalExpression);
            var lambda = Expression.Lambda(body, parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
