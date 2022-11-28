using k8s;
using k8s.Models;
using UserAccessManager.Models;
using UserAccessManager.Services.Kafka.Models;
using UserAccessManager.Services.ServiceNow;
using Octokit;
using UserAccessManager.Services.Kafka.Consumer;

namespace UserAccessManager.Services.Kafka;
public class UserProvisioningHandler : IKafkaHandler<string, AccessRequest>
{
    private readonly IServiceNowTableClient serviceNow;
    private readonly ILogger logger;
    private readonly AccessRequestConfiguration config;
    private readonly IKafkaProducer<string, RetryAccessRequest> producer;
    public UserProvisioningHandler(IServiceNowTableClient serviceNow, ILogger logger, AccessRequestConfiguration config, IKafkaProducer<string, RetryAccessRequest> producer)
    {
        this.serviceNow = serviceNow;
        this.logger = logger;
        this.config = config;
        this.producer = producer;
    }
    public async Task<Task> HandleAsync(string consumerName, string key, AccessRequest value)
    {
        if (value.payload.state != State.Approved.ToLower())
        {
            logger.LogNotApproved(value.payload.requested_for, value.payload.state, consumerName);
            return Task.CompletedTask; //process and do nothing
        }
        try
        {
            var approvedUser = await serviceNow
                .GetServiceNowUserById(value.payload.product_owner);
            var requestedfor = await serviceNow
                .GetServiceNowUserById(value.payload.requested_for);

            var envList = value.payload.environment.ToLower().Split(',')
                .Select(n=>$"{value.payload.openshift_namespace.ToLower()}-{n}")
                .ToList();
            var github = new GitHubClient(new ProductHeaderValue("ocp-accessrequest"))
            {
                Credentials = new Credentials(config.GithubClient.PersonalAccessToken)
            };

            foreach (var env in envList)
            {
                string accessRequestYmlObject = await UserK8sYmlAsync(value, env, approvedUser, this.serviceNow, this.logger, this.config);

                if (!string.IsNullOrEmpty(accessRequestYmlObject))
                {
                    var owner = this.config.GithubClient.RepositoryOwner;
                    var repoName = this.config.GithubClient.RepositoryName;
                    var filePath = $"{value.payload.openshift_cluster}/{value.payload.openshift_namespace}/{env}.yml";
                    var branch = this.config.GithubClient.Branch;

                    IReadOnlyList<RepositoryContent>? fileDetails;
                    try
                    {
                        fileDetails = await github.Repository.Content.GetAllContentsByRef(owner, repoName, filePath, branch);
                    }
                    catch (Exception)
                    {

                        fileDetails = null;
                    }
                    if (fileDetails == null)
                    {
                        await github.Repository.Content.CreateFile(owner, repoName, filePath, new CreateFileRequest($"Access request for {requestedfor?.Name} was approved by {approvedUser?.Name} ", accessRequestYmlObject.ToString(), branch));
                    }
                    else
                    {
                        await github.Repository.Content.UpdateFile(owner, repoName, filePath, new UpdateFileRequest($"Access request for {requestedfor?.Name} was approved for update by {approvedUser?.Name}", accessRequestYmlObject.ToString(), fileDetails.First().Sha));

                    }
                    if (envList.IndexOf(env) == envList.Count() - 1)
                    {
                        var tableUpdate = await serviceNow.UpdateServiceNowTable(config.ServiceNow.TableName, value.payload.sys_id, new AccessRequestDto() { state = State.Close, sys_id = value.payload.sys_id, work_notes = $"Access Request Provisioned as approved by {approvedUser?.Name}" });
                        if (!tableUpdate)
                        {
                            return Task.FromException(new Exception());
                        }
                        logger.LogUserProvisioned(requestedfor?.Name, consumerName);
                    }

                    
                }

            }

        }
        catch (Exception e)
        {
            await this.producer.ProduceAsync(config.KafkaCluster.InitialRetryTopicName, value.payload.sys_id, new RetryAccessRequest
            {
                payload = new Models.Payload
                {
                    request_last_name = value.payload.request_last_name,
                    requested_role =value.payload.requested_role,
                     product_owner =value.payload.product_owner,
                     openshift_cluster =value.payload.openshift_cluster,
                     sys_mod_count =value.payload.sys_mod_count,
                     description =value.payload.description,
                     requested_for =value.payload.requested_for,
                     sys_updated_on =value.payload.sys_updated_on,
                    sys_tags =value.payload.sys_tags,
                     openshift_namespace =value.payload.openshift_namespace,
                     approval_history =value.payload.approval_history,
                    sys_id =value.payload.sys_id,
                     approved_by =value.payload.approved_by,
                    environment =value.payload.environment,
                    requested_by =value.payload.requested_by,
                     sys_updated_by =value.payload.request_last_name,
                     sys_created_on =value.payload.sys_created_on,
                    approved_b0 =value.payload.approved_b0,
                    state =value.payload.state,
                     work_notes =value.payload.work_notes,
                   sys_created_by =value.payload.sys_created_by,
                },
                schemaId = value.schemaId,
                retryNumber = 5,
                retryDuration = TimeSpan.FromSeconds(5)
            });
            var o = await serviceNow.UpdateServiceNowTable(config.ServiceNow.TableName, value.payload.sys_id, new AccessRequestDto() { state = State.Queued, sys_id = value.payload.sys_id, work_notes = $"Something went wrong during access provoisioning. We will try again later and let you know" });
            logger.LogUserAccessError(e);
        }

       
        return Task.CompletedTask;
    }

    private static async Task<string> UserK8sYmlAsync(AccessRequest value, string envNamespace, Result? approvedBy, IServiceNowTableClient serviceNow, ILogger logger, AccessRequestConfiguration configuration)
    {

        try
        {
            var config = KubernetesClientConfiguration
                .BuildConfigFromConfigFile(configuration.KubernateConfig.KubeConfigLocation);

            config.SkipTlsVerify = true;
            var client = new Kubernetes(config);
            var roleBindingList = client.ListNamespacedRoleBinding(envNamespace);

            var userRolebinding = roleBindingList.Items.Where(n => n.Subjects.Any(n => n.Kind.Equals("User", StringComparison.CurrentCultureIgnoreCase))).ToList();

            var roleref = value.payload.requested_role.Contains("admin") ? "admin" : "edit";
            var userExist = userRolebinding
                .SelectMany(
                x => x.Subjects
                .Where(
                    n => n.Name == value.payload.request_last_name 
                    && x.RoleRef.Name == roleref))
                .ToList()
                .FirstOrDefault();


            if (userExist != null)
            {
                //user already exist or provisoned
               await serviceNow
                    .UpdateServiceNowTable(configuration.ServiceNow.TableName, value.payload.sys_id, new AccessRequestDto()
                    {
                        state = State.Close,
                        sys_id = value.payload.sys_id,
                        work_notes = $"The user upn {value.payload.request_last_name} already has  {value.payload.requested_role} access to {envNamespace}/n"
                    });
                logger.LogUserAlreadyProvisioned(value.payload.requested_for, value.payload.requested_role, envNamespace);
                return string.Empty;
            }

            foreach (var item in userRolebinding)
            {
                for (int i = 0; i < item.Subjects.Count; i++)
                {

                    if (item.Subjects[i].Kind == "ServiceAccount")
                    {
                        item.Subjects.Remove(item.Subjects[i]);
                    }
                }

            }

            var sub = new V1Subject
            {
                Name = $"{value.payload.request_last_name}",
                ApiGroup = "rbac.authorization.k8s.io",
                Kind = "User",
                NamespaceProperty = envNamespace

            };
            userRolebinding.Add(new V1RoleBinding
            {
                RoleRef = new V1RoleRef
                {
                    ApiGroup = "rbac.authorization.k8s.io",
                    Name = value.payload.requested_role.Contains("admin") ? "admin" : "edit" ,//admin,edit,devops
                    Kind = "ClusterRole",

                },
                Metadata = new V1ObjectMeta
                {
                    Name = $"{value.payload.requested_role}",
                    NamespaceProperty = envNamespace,
                    Labels = new Dictionary<string, string>()
                    {
                        { "dev141657.service-now.com", $"jag-rbac-{envNamespace}" },
                        { $"approved by {approvedBy?.Name}", $"jag-rbac-{envNamespace}" }
                    }
                },
                Subjects = new List<V1Subject>() { sub }
            });

            var rb = new V1RoleBindingList()
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",

            };
            rb.Items = userRolebinding;

            rb.Validate();
            var userYaml = KubernetesYaml.Serialize(rb);
            return userYaml;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public Task<Task> HandleRetryAsync(string consumerName, string key, AccessRequest value, int retryCount, string topicName)
    {
        throw new NotImplementedException();
    }
}
public static partial class UserProvisioningHandlerLoggingExtensions
{
    [LoggerMessage(1, LogLevel.Error, "Error provisioning user access")]
    public static partial void LogUserAccessError(this ILogger logger, Exception e);
    [LoggerMessage(2, LogLevel.Warning, "The access request for userId {user} and state {state} has not been approved, offset committed by {msgId} but do nothing")]
    public static partial void LogNotApproved(this ILogger logger, string? user, string state, string msgId);
    [LoggerMessage(3, LogLevel.Information, "Access request for user {user} has been provisioned, offset committed by {msgId}")]
    public static partial void LogUserProvisioned(this ILogger logger, string? user, string msgId);

    [LoggerMessage(4, LogLevel.Information, "User {user} already has {role} to {env}")]
    public static partial void LogUserAlreadyProvisioned(this ILogger logger, string? user, string role, string env);
    [LoggerMessage(5, LogLevel.Warning, "Cannot provisioned {user} account. Published {sysId} cloned record for to {topic} topic for retrial")]
    public static partial void LogUserAccessPublishError(this ILogger logger, string? user, string sysId, string topic);
    [LoggerMessage(1, LogLevel.Error, "Error provisioning user {sysId} after final retry")]
    public static partial void LogUserAccessRetryError(this ILogger logger, string sysId);

}
