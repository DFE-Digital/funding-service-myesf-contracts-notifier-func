# Manage your education and skills funding - Contracts notifications function

The Manage Your Education and Skills Funding (MYESF) Contracts Notifications Function is used by the MYESF web application to notify interested applications and consumers e.g. learning providers of:

- Significant contract changes
- Funding claim changes
- Subcontractor declaration notifications

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is an ASP.NET Core 8 serverless function app utilising Azure App Service for deployment.

The function app runs on an Azure App service on Azure and is triggered by an azure service bus message.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `local.settings.json` file will need to be created in the `Pds.Contracts.Notifications.Func` project. Below, and included in the repo, there is `local.settings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

## Local Settings (`local.settings.json`)

```json
{
  "IsEncrypted": false,
  "Values": {
    "Environment": "local",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "PdsApplicationInsights:InstrumentationKey": "",
    "PdsApplicationInsights:Environment": "local",
    "Pds.Contracts.Notifications.Topic": "",
    "ContractsDataApiConfiguration:ApiBaseAddress": "",
    "ContractsDataApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "ContractsDataApiConfiguration:TenantId": "",
    "ContractsDataApiConfiguration:ClientId": "",
    "ContractsDataApiConfiguration:ClientSecret": "",
    "ContractsDataApiConfiguration:AppUri": "",
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:reminderInterval": 14,
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:page": 1,
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:count": 25,
    "AuditApiConfiguration:ApiBaseAddress": "",
    "AuditApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "AuditApiConfiguration:TenantId": "",
    "AuditApiConfiguration:ClientId": "",
    "AuditApiConfiguration:ClientSecret": "",
    "AuditApiConfiguration:AppUri": "",
    "HttpPolicyOptions:HttpRetryCount": 3,
    "HttpPolicyOptions:HttpRetryBackoffPower": 2,
    "HttpPolicyOptions:CircuitBreakerToleranceCount": 5,
    "HttpPolicyOptions:CircuitBreakerDurationOfBreak": "0.00:00:15",
    "NotificationServiceBusOptions:ConnectionString": "",
    "NotificationServiceBusOptions:ServiceBusQueueName": "",
    "Pds.Contracts.ReadyToSign.Subscription": "",
    "Pds.Contracts.Approved.Subscription": "",
    "Pds.Contracts.ReadyToReview.Subscription": "",
    "Pds.Contracts.Withdrawn.Subscription": "",
    "AzureMessagingServiceBusOptions:ConnectionString": "",
    "CdsUserExceptionEmail": "",
    "ServiceNowEmailAddress": "",
    "FundingClaimsDataApiConfiguration:ApiBaseAddress": "",
    "FundingClaimsDataApiConfiguration:AppUri": "",
    "FundingClaimsDataApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "FundingClaimsDataApiConfiguration:ClientId": "",
    "FundingClaimsDataApiConfiguration:ClientSecret": "",
    "FundingClaimsDataApiConfiguration:TenantId": "",
    "SubcontractorDeclarationDataApiConfiguration:ApiBaseAddress": "",
    "SubcontractorDeclarationDataApiConfiguration:AppUri": "",
    "SubcontractorDeclarationDataApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "SubcontractorDeclarationDataApiConfiguration:ClientId": "",
    "SubcontractorDeclarationDataApiConfiguration:ClientSecret": "",
    "SubcontractorDeclarationDataApiConfiguration:TenantId": "",
    "DfESignin:PublicApi:Url": "",
    "DfESignin:PublicApi:ClientID": "",
    "DfESignin:PublicApi:ClientSecret": ""
  }
}
```

### Setting Details

- **`AzureWebJobsStorage`**
  The core application setting used by the Azure Functions and Azure WebJobs runtime to establish a connection to an Azure Storage account.

- **`FUNCTIONS_EXTENSION_VERSION`**      
  The functions extension version number.

- **`FUNCTIONS_WORKER_RUNTIME`**
  The functions runtime mode.

- **`PdsApplicationInsights:InstrumentationKey`**  
  The key value for Application Insights resource for logging purposes.
   
- **`PdsApplicationInsights:Environment`**  
  The environment which the app is running on for Application Insights for logging purposes.

- **`Pds.Contracts.Notifications.Topic`**  
  The Azure Service Bus topic that handles several subscriptions for different kinds of contract change notifications
 
- **`ContractsDataApiConfiguration:ApiBaseAddress`** 
  The base URL endpoint used by a client application to route network requests to the Contracts Data API backend.

- **`ContractsDataApiConfiguration:Authority`** 
  The base URL of the Identity Provider responsible for authenticating and issuing tokens for the Contracts Data API client.

- **`ContractsDataApiConfiguration:TenantId`** 
  The unique identifier for your azure ad tenant.

- **`ContractsDataApiConfiguration:ClientId`** 
  The Contracts Data API application (client) ID registered in azure ad.

- **`ContractsDataApiConfiguration:ClientSecret`** 
  The confidential credential used by the Contracts Data API application to securely prove its identity to the Identity Provider.

- **`ContractsDataApiConfiguration:AppUri`** 
  The unique Application ID URI used as the identifier for the protected Contracts Data API resource within the Identity Provider.

- **`AuditApiConfiguration:ApiBaseAddress`** 
  The base URL endpoint used by a client application to route network requests to the Audit API backend.

- **`AuditApiConfiguration:Authority`** 
  The base URL of the Identity Provider responsible for authenticating and issuing tokens for the Audit API client.

- **`AuditApiConfiguration:TenantId`** 
  The unique identifier for your azure ad tenant.

- **`AuditApiConfiguration:ClientId`** 
  The Audit API application (client) ID registered in azure ad.

- **`AuditApiConfiguration:ClientSecret`** 
  The confidential credential used by the Audit API application to securely prove its identity to the Identity Provider.

- **`AuditApiConfiguration:AppUri`** 
  The unique Application ID URI used as the identifier for the protected Audit API resource within the Identity Provider.

- **`HttpPolicyOptions:HttpRetryCount`** 
  The number of times that the Http client would automatically resend failed Http requests.

- **`HttpPolicyOptions:HttpRetryBackoffPower`** 
  The handler lifetime in minutes to limit exponential backoff for retrying Http requests.

- **`HttpPolicyOptions:CircuitBreakerToleranceCount`** 
  The limited number of failed requests the circuit breaker will tolerate before beginning a time-out timer.

- **`HttpPolicyOptions:CircuitBreakerDurationOfBreak`** 
  The duration in seconds the circuit breaker remains open before transitioning to a half-open state for re-evaluation.

- **`NotificationServiceBusOptions:ConnectionString`** 
  The standard application setting used by Azure Functions and Azure WebJobs to securely connect to the Notification Service Bus.

- **`NotificationServiceBusOptions:ServiceBusQueueName`** 
  The Notification Service Bus's queue name.

- **`Pds.Contracts.ReadyToSign.Subscription`** 
  The service bus subscription that filters for Contract Ready To Sign messages.

- **`Pds.Contracts.Approved.Subscription`** 
  The service bus subscription that filters for Contract Approved messages.

- **`Pds.Contracts.ReadyToReview.Subscription`** 
  The service bus subscription that filters for Contract Ready To Review messages.

- **`Pds.Contracts.Withdrawn.Subscription`** 
  The service bus subscription that filters for Contract Withdrawn messages.

- **`AzureMessagingServiceBusOptions:ConnectionString`** 
  The standard application setting used by Azure Functions and Azure WebJobs to securely connect to the Azure Messaging Service Bus.

- **`CdsUserExceptionEmail`** 
  If a feed triggers an exception, an email is sent to this email address.

- **`ServiceNowEmailAddress`** 
  If a user sends a contract query message, it is sent to this email address.

- **`FundingClaimsDataApiConfiguration:ApiBaseAddress`** 
  The base URL endpoint used by a client application to route network requests to the Funding Claims Data API backend.

- **`FundingClaimsDataApiConfiguration:AppUri`** 
  The unique Application ID URI used as the identifier for the protected Funding Claims Data API resource within the Identity Provider.

- **`FundingClaimsDataApiConfiguration:Authority`** 
  The base URL of the Identity Provider responsible for authenticating and issuing tokens for the Funding Claims Data API client.

- **`FundingClaimsDataApiConfiguration:ClientId`** 
  The Funding Claims Data API application (client) ID registered in azure ad.

- **`FundingClaimsDataApiConfiguration:ClientSecret`** 
  The confidential credential used by the Funding Claims Data API application to securely prove its identity to the Identity Provider.

- **`FundingClaimsDataApiConfiguration:TenantId`** 
  The unique identifier for your azure ad tenant.

- **`SubcontractorDeclarationDataApiConfiguration:ApiBaseAddress`** 
  The base URL endpoint used by a client application to route network requests to the Subcontractor Declaration Data API backend.

- **`SubcontractorDeclarationDataApiConfiguration:AppUri`** 
  The unique Application ID URI used as the identifier for the protected Subcontractor Declaration Data API resource within the Identity Provider.

- **`SubcontractorDeclarationDataApiConfiguration:Authority`** 
  The base URL of the Identity Provider responsible for authenticating and issuing tokens for the Subcontractor Declaration Data API client.

- **`SubcontractorDeclarationDataApiConfiguration:ClientId`** 
  The Subcontractor Declaration Data API application (client) ID registered in azure ad.

- **`SubcontractorDeclarationDataApiConfiguration:ClientSecret`** 
  The confidential credential used by the Subcontractor Declaration Data API application to securely prove its identity to the Identity Provider.

- **`SubcontractorDeclarationDataApiConfiguration:TenantId`** 
  The unique identifier for your azure ad tenant.

- **`DfESignin:PublicApi:Url`** 
  The public Url for DfE Sign-In API.

- **`DfESignin:PublicApi:ClientID`** 
  The DfE Sign-In API application (client) ID registered in azure ad.

- **`DfESignin:PublicApi:ClientSecret`** 
  The confidential credential used by the DfE Sign-In API application to securely prove its identity to the Identity Provider.

## Build and Test

To build and test locally, you can either use Visual Studio, Visual Studio Code or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

- If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
- If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.