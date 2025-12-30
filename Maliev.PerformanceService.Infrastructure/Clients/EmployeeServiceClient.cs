using System.Net.Http.Json;
using Maliev.PerformanceService.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Maliev.PerformanceService.Infrastructure.Clients;

/// <summary>
/// Implementation of the employee service client using HttpClient and distributed cache.
/// </summary>
public class EmployeeServiceClient : IEmployeeServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IDistributedCache _cache;
    private const string CacheKeyPrefix = "employee:";

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeServiceClient"/> class.
    /// </summary>
    public EmployeeServiceClient(HttpClient httpClient, IDistributedCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{employeeId}";
        var cachedEmployee = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedEmployee))
        {
            return JsonSerializer.Deserialize<EmployeeDto>(cachedEmployee);
        }

        try
        {
            var employee = await _httpClient.GetFromJsonAsync<EmployeeDto>($"/api/v1/employees/{employeeId}", cancellationToken);
            
            if (employee != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(employee), cacheOptions, cancellationToken);
            }

            return employee;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateEmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeByIdAsync(employeeId, cancellationToken);
        return employee != null;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateManagerEmployeeRelationshipAsync(Guid managerId, Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeByIdAsync(employeeId, cancellationToken);
        return employee?.ManagerId == managerId;
    }
}