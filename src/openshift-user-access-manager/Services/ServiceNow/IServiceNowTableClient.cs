using UserAccessManager.Services.Kafka.Models;

namespace UserAccessManager.Services.ServiceNow;
public interface IServiceNowTableClient
{
    Task<bool> UpdateServiceNowTable(string tableName, string sysId, AccessRequestDto accessRequestDto);
    Task<Result?> GetServiceNowUserById(string sysId);
}

