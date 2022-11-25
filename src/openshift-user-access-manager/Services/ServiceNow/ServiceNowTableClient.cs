using UserAccessManager.Services.HttpClients;
using UserAccessManager.Services.Kafka.Models;

namespace UserAccessManager.Services.ServiceNow;
public class ServiceNowTableClient : BaseClient, IServiceNowTableClient
{
    public ServiceNowTableClient(HttpClient client, ILogger<ServiceNowTableClient> logger) : base(client, logger)
    {
    }

    public async Task<Result?> GetServiceNowUserById(string sysId)
    {
        var result = await this.GetAsync<User>($"/api/now/v1/table/sys_user?sysparm_query=sys_id&sys_id={sysId}&sysparm_fields=name,email,first_name,last_name");
        if (!result.IsSuccess)
        {
            return null;
        }
        return result.Value.result.Single();
    }

    public async Task<bool> UpdateServiceNowTable(string tableName, string sysId, AccessRequestDto accessRequestDto)
    {
        var result = await this.PutAsync($"/api/now/v1/table/{tableName}/{sysId}", accessRequestDto);
        if (!result.IsSuccess)
        {
            Logger.ServiceNowNon();
        }
        return result.IsSuccess;
    }
}
public static partial class ServiceNowClientLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Error, "Error updating service user record")]
    public static partial void ServiceNowNon(this ILogger logger);
}


