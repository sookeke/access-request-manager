# access-request-manager

##Actors
- Requestor
- Approver


<img align="center" width="1110" src="doc/AARForm.PNG">

###Workflow
- User submit openshift access request by filling the aar form
- User set the request state to in progress
- Once the request state changes to in progress, an approval request will be sent to the product owner and admin
- The approver approves or deny the request
- if the approver approves the request
	- The request state is set to approved
- If the approver deny the request, the approves state is set to rejected


###Event Sourcing
- All the aboove work flows generate an individual state events into the kafka topic (topic name = prefix.aar_table_name)
- The user manager service subscribes to this topic and consumes all generated state event published by the servicenow kafka connector
- Un approved generated state events are ignored and offset committed by the consumer by a dedicated consumer group id
- Approved stated events are processed and corresponding RBAC manifest are generated based on the request
- The generated RBAC manifest is commited to the Git RBAC manifest repository to be handles by argo cd for deployment
- The result of this process will be published to the access-request-result topic for analysis
- The result will be used to notify the user the state of their request wheather it was succefully or not. The is acchived by updating the aar form via serviceNow tableAPI service request with the result from the access-request-result topic 


<img align="center" width="1110" src="doc/user access manager.drawio.png">