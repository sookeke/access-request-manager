using k8s;
using k8s.Models;
using UserAccessManager.Services.Kafka.Models;
using UserAccessManager.Services.ServiceNow;
using Octokit;
using UserAccessManager.Services.Kafka;

namespace UserAccessManager.Services.Kafka.Consumer.ConsumerRetry;
public class RetryProvisioningHandler : IKafkaHandler<string, RetryAccessRequest>
{
    private readonly IServiceNowTableClient serviceNow;
    private readonly ILogger logger;
    private readonly AccessRequestConfiguration config;
    private readonly GitHubClient gitHubClient;
    private readonly IKafkaProducer<string, RetryAccessRequest> producer;
    public RetryProvisioningHandler(IServiceNowTableClient serviceNow, ILogger logger, AccessRequestConfiguration config, IKafkaProducer<string, RetryAccessRequest> producer, GitHubClient gitHubClient)
    {
        this.serviceNow = serviceNow;
        this.logger = logger;
        this.config = config;
        this.producer = producer;
        this.gitHubClient = gitHubClient;
    }
    public async Task<Task> HandleRetryAsync(string consumerName, string key, RetryAccessRequest value, int retryCount, string topicName)
    {
        try
        {
            var approvedUser = await serviceNow
                .GetServiceNowUserById(value.payload.product_owner);
            var requestedfor = await serviceNow
                .GetServiceNowUserById(value.payload.requested_for);

            var envList = value.payload.environment.ToLower().Split(',')
                .Select(n => $"{value.payload.openshift_namespace.ToLower()}-{n}")
                .ToList();

            foreach (var env in envList)
            {
                string accessRequestYmlObject = await UserK8sYmlAsync(value, env, approvedUser, serviceNow, logger, config);

                if (!string.IsNullOrEmpty(accessRequestYmlObject))
                {
                    var owner = config.GithubClient.RepositoryOwner;
                    var repoName = config.GithubClient.RepositoryName;
                    var filePath = $"{value.payload.openshift_cluster}/{value.payload.openshift_namespace}/{env}.yml";
                    var branch = config.GithubClient.Branch;

                    IReadOnlyList<RepositoryContent>? fileDetails;
                    try
                    {
                        fileDetails = await this.gitHubClient.Repository.Content.GetAllContentsByRef(owner, repoName, filePath, branch);
                    }
                    catch (Exception)
                    {

                        fileDetails = null;
                    }
                    if (fileDetails == null)
                    {
                        await this.gitHubClient.Repository.Content.CreateFile(owner, repoName, filePath, new CreateFileRequest($"Access request for {requestedfor?.Name} was approved by {approvedUser?.Name} ", accessRequestYmlObject.ToString(), branch));
                    }
                    else
                    {
                        await this.gitHubClient.Repository.Content.UpdateFile(owner, repoName, filePath, new UpdateFileRequest($"Access request for {requestedfor?.Name} was approved for update by {approvedUser?.Name}", accessRequestYmlObject.ToString(), fileDetails.First().Sha));

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
            if (retryCount == value.retryNumber && topicName == config.KafkaCluster.InitialRetryTopicName)
            {
                //commit the offset at last trial and allow next consumer to retry
                await ProduceToErrorTopic(value, config.KafkaCluster.MidRetryTopicName, config.RetryPolicy.MidRetryTopicName.RetryCount, config.RetryPolicy.MidRetryTopicName.WaitAfterInMins);
                logger.LogUserAccessPublishError(value.payload.request_last_name, value.payload.sys_id, config.KafkaCluster.MidRetryTopicName);
            }
            else if (retryCount == value.retryNumber && topicName == config.KafkaCluster.MidRetryTopicName)
            {
                //commit the offset at last trial and allow next consumer to retry
                await ProduceToErrorTopic(value, config.KafkaCluster.FinalRetryTopic, config.RetryPolicy.FinalRetryTopicName.RetryCount, config.RetryPolicy.FinalRetryTopicName.WaitAfterInMins);
                logger.LogUserAccessPublishError(value.payload.request_last_name, value.payload.sys_id, config.KafkaCluster.FinalRetryTopic);
            }
            else if (retryCount == value.retryNumber && topicName == config.KafkaCluster.FinalRetryTopic)
            {
                var createIssue = new NewIssue("Openshift User Provisioning Error");
                createIssue.Body = $"The system could not provisioned user {value.payload.sys_id} due to the following error {e.Message}.</br> <p> Retry topicName: {config.KafkaCluster.FinalRetryTopic}";
                createIssue.Labels.Add("rbac");
                createIssue.Labels.Add("error");
                createIssue.Labels.Add("openshift");
                createIssue.Labels.Add("user provisioning");
                await this.gitHubClient.Issue.Create(config.GithubClient.RepositoryOwner, config.GithubClient.RepositoryName, createIssue);
                var o = await serviceNow.UpdateServiceNowTable(config.ServiceNow.TableName, value.payload.sys_id, new AccessRequestDto() { state = State.Pending, sys_id = value.payload.sys_id, work_notes = $"We cannot process your request due to the following error {e.Message}. Please, contact the admin or try again" });
                logger.LogUserAccessRetryError(value.payload.sys_id);
                return Task.FromException(e);
            }
            else
                return Task.FromException(e);

        }
        return Task.CompletedTask;
    }

    private async Task ProduceToErrorTopic(RetryAccessRequest value, string topicName, int retryNumber, int timeInMin)
    {
        await producer.ProduceAsync(topicName, value.payload.sys_id, new RetryAccessRequest
        {
            payload = new Payload
            {
                request_last_name = value.payload.request_last_name,
                requested_role = value.payload.requested_role,
                product_owner = value.payload.product_owner,
                openshift_cluster = value.payload.openshift_cluster,
                sys_mod_count = value.payload.sys_mod_count,
                description = value.payload.description,
                requested_for = value.payload.requested_for,
                sys_updated_on = value.payload.sys_updated_on,
                sys_tags = value.payload.sys_tags,
                openshift_namespace = value.payload.openshift_namespace,
                approval_history = value.payload.approval_history,
                sys_id = value.payload.sys_id,
                approved_by = value.payload.approved_by,
                environment = value.payload.environment,
                requested_by = value.payload.requested_by,
                sys_updated_by = value.payload.request_last_name,
                sys_created_on = value.payload.sys_created_on,
                approved_b0 = value.payload.approved_b0,
                state = value.payload.state,
                work_notes = value.payload.work_notes,
                sys_created_by = value.payload.sys_created_by,
            },
            schemaId = value.schemaId,
            retryNumber = retryNumber,
            retryDuration = TimeSpan.FromMinutes(timeInMin)
        });
    }

    private static async Task<string> UserK8sYmlAsync(RetryAccessRequest value, string envNamespace, Result? approvedBy, IServiceNowTableClient serviceNow, ILogger logger, AccessRequestConfiguration configuration)
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
                    Name = value.payload.requested_role.Contains("admin") ? "admin" : "edit",//admin,edit,devops
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

    public Task<Task> HandleAsync(string consumerName, string key, RetryAccessRequest value)
    {
        throw new NotImplementedException();
    }
}

