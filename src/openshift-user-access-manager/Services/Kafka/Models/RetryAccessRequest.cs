namespace UserAccessManager.Services.Kafka.Models;
public class RetryAccessRequest
{
    public int schemaId { get; set; }
    public Payload payload { get; set; }
    public int retryNumber { get; set; }
    public TimeSpan retryDuration { get; set; }
}
public class Payload
{
    public string request_last_name { get; set; }
    public string requested_role { get; set; }
    public string product_owner { get; set; }
    public string openshift_cluster { get; set; }
    public string sys_mod_count { get; set; }
    public string description { get; set; }
    public string requested_for { get; set; }
    public string sys_updated_on { get; set; }
    public string sys_tags { get; set; }
    public string openshift_namespace { get; set; }
    public string approval_history { get; set; }
    public string sys_id { get; set; }
    public string approved_by { get; set; }
    public string environment { get; set; }
    public string requested_by { get; set; }
    public string sys_updated_by { get; set; }
    public string sys_created_on { get; set; }
    public string approved_b0 { get; set; }
    public string state { get; set; }
    public string work_notes { get; set; }
    public string sys_created_by { get; set; }
}
