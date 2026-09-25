namespace CollectA.Application.Common.Interfaces;

public interface IUserLookupService
{
    Task<string?> GetUserFullNameAsync(Guid userId, CancellationToken cancellationToken = default);
}
