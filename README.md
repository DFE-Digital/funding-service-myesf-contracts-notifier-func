# Manage your education and skills funding - Contracts notifications function

## Introduction

Contracts notification function is a serverless azure function that handles significant contract changes, funding claim changes, and subcontractor declaration notifications, and notifies interested applications and consumers e.g. learning providers. This function is triggered by an azure service bus message.

## Provider

[The Department for Education](https://www.gov.uk/government/organisations/department-for-education)

## About this project

This project is an ASP.NET Core 8 function app utilising Azure App Service for deployment.

The function app runs on an Azure App service on Azure.

**Note:** The project is currently being updated to be containerised via Docker where the deployment method and target will change, this document will be updated when these changes have been finalised.

# Local Configuration Guide

In order to run the application locally a valid `appsettings.json` file will need to be created in the `Pds.Contracts.Notifications.Func` project. Below, and included in the repo, there is `appsettings.example.json` which can be used as a base and populated with the required values, which can be retrieved from the Azure Portal.

### Getting Started

This product is a Visual Studio 2022 solution containing several projects (Azure function application, service, and repository layers, with associated unit test and integration test projects).
To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

### Installing

Clone the project and open the solution in Visual Studio 2022.

#### List of dependencies

|Item |Purpose|
|-------|-------|
|Azure Storage Emulator| The Microsoft Azure Storage Emulator is a tool that emulates the Azure Blob, Queue, and Table services for local development purposes. This is required for webjob storage used by azure functions.|
|Azure function development tools | To run and test azure functions locally. |
|Azure service bus | To trigger this function, it cannot be set up locally, you will need an azure subscription to set-up azure service bus. |
|Contracts API | API for managing contracts. |
|Audit API | Audit API provides a single shared service to audit events in "Manage your education and skills funding". |

#### Azure Storage Emulator

The Storage Emulator is available as part of the Microsoft Azure SDK. Azure functions require it for local development.

#### Azure function development tools

You can use your favourite code editor and development tools to create and test functions on your local computer.
We used visual studio and Azure core tools CLI for development and testing. You can find more information for your favourite code editor at <https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-local>.

* Using Visual Studio - To develop functions using visual studio, include the Azure development workload in your Visual Studio installation. More detailed information can be found at <https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs>.
* Azure Functions Core Tools - These tools provide CLI with core runtime and templates for creating functions, which can be used to develop and run functions without visual studio. This can be installed using package managers like `npm` or `chocolately` more detailed information can be found at <https://www.npmjs.com/package/azure-functions-core-tools>.

#### Azure service bus

Microsoft Azure Service Bus is a fully managed enterprise message broker.
Publish-subscribe topics are used by this application to decouple approval processing.
There are no emulators available for azure service bus, hence you will need an azure subscription and set-up a service bus namesapce with a topic created to run this application.
Once you have set-up an azure service bus namespace, you will need to create a shared access policy to set in local configuration settings.

#### Contracts API

Contract API can be found at <https://github.com/SkillsFundingAgency/pds-contracts-data-api>.

#### Audit API

Audit API can be found at <https://github.com/SkillsFundingAgency/pds-shared-audit-api>.

#### Funding Claim API

(Funding Claim API to be added.)

#### Subcontractor Declaration API

(Subcontractor Declaration API to be added.)

### Local Config Files

Once you have cloned the public repo you need the following configuration files listed below.

| Location | config file |
|-------|-------|
| Pds.Contracts.Notifications.Func | local.settings.json |

The following is a sample configuration file

```json
{
  "IsEncrypted": false,
  "Version": "2.0",
  "Values": {
    "Environment": "",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_EXTENSION_VERSION": "~4",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "WEBSITE_TIME_ZONE": "GMT Standard Time",
    "APPINSIGHTS_INSTRUMENTATIONKEY": "",
    "PdsApplicationInsights:InstrumentationKey": "",
    "PdsApplicationInsights:Environment": "local",
    "Pds.Contracts.Notifications.Topic": "replace_contracts_notification_topic",
    "ContractsDataApiConfiguration:ApiBaseAddress": "replace_local_contract_api_or_stub",
    "ContractsDataApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "ContractsDataApiConfiguration:TenantId": "",
    "ContractsDataApiConfiguration:ClientId": "",
    "ContractsDataApiConfiguration:ClientSecret": "",
    "ContractsDataApiConfiguration:AppUri": "",
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:reminderInterval": 14,
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:page": 1,
    "ContractsDataApiConfiguration:ContractReminderQuerystring:QueryParameters:count": 25,
    "AuditApiConfiguration:ApiBaseAddress": "replace_local_audit_api_or_stub",
    "AuditApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "AuditApiConfiguration:TenantId": "",
    "AuditApiConfiguration:ClientId": "",
    "AuditApiConfiguration:ClientSecret": "",
    "AuditApiConfiguration:AppUri": "",
    "HttpPolicyOptions:HttpRetryCount": 3,
    "HttpPolicyOptions:HttpRetryBackoffPower": 2,
    "HttpPolicyOptions:CircuitBreakerToleranceCount": 5,
    "HttpPolicyOptions:CircuitBreakerDurationOfBreak": "0.00:00:15",
    "NotificationServiceBusOptions:ConnectionString": "replace_ServiceBusConnectionString",
    "NotificationServiceBusOptions:ServiceBusQueueName": "",
    "Pds.Contracts.ReadyToSign.Subscription": "replace_contracts_readytosign_subscription",
    "Pds.Contracts.Approved.Subscription": "replace_contracts_approved_subscription",
    "Pds.Contracts.ReadyToReview.Subscription": "replace_contracts_readytoreview_subscription",
    "Pds.Contracts.Withdrawn.Subscription": "replace_contracts_withdrawn_subscription",
    "AzureMessagingServiceBusOptions:ConnectionString": "",
    "CdsUserExceptionEmail": "replace_CdsUserExceptionEmail",
    "ServiceNowEmailAddress": "replace_ServiceNowEmailAddress",
    "FundingClaimsDataApiConfiguration:ApiBaseAddress": "replace_local_fundingclaim_api_or_stub",
    "FundingClaimsDataApiConfiguration:AppUri": "",
    "FundingClaimsDataApiConfiguration:Authority": "https://login.microsoftonline.com/",
    "FundingClaimsDataApiConfiguration:ClientId": "",
    "FundingClaimsDataApiConfiguration:ClientSecret": "",
    "FundingClaimsDataApiConfiguration:TenantId": "",
    "SubcontractorDeclarationDataApiConfiguration:ApiBaseAddress": "replace_local_contract_api_or_stub",
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

- **`APPINSIGHTS_INSTRUMENTATIONKEY`**
  The key value for Application Insights resource for logging purposes.

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

The following configurations need to be replaced with your values.
|Key|Token|Example|
|-|-|-|
|ContractsDataApiConfiguration.ApiBaseAddress|replace_local_contract_api_or_stub|<http://localhost:5001>|
|SubcontractorDeclarationDataApiConfiguration.ApiBaseAddress|replace_local_contract_api_or_stub|<http://localhost:5001>|
|FundingClaimDataApiConfiguration.ApiBaseAddress|replace_local_fundingclaim_api_or_stub|
|AuditApiConfiguration.ApiBaseAddress|replace_local_audit_api_or_stub|<http://localhost:5002/>|
|NotificationServiceBusOptions.ConnectionString|replace_ServiceBusConnectionString|A valid azure service bus connection string|
|Pds.Contracts.Notifications.Topic|replace_contracts_notification_topic|notification-topic|
|Pds.Contracts.ReadyToSign.Subscription|replace_contracts_readytosign_subscription|readytosign-subscription|
|Pds.Contracts.Approved.Subscription|replace_contracts_approved_subscription|approved-subscription|
|Pds.Contracts.ReadyToReview.Subscription|replace_contracts_readytoreview_subscription|readytoreview-subscription|
|Pds.Contracts.Withdrawn.Subscription|replace_contracts_withdrawn_subscription|withdrawn-subscription|
|CdsUserExceptionEmail|replace_CdsUserExceptionEmail|
|ServiceNowEmailAddress|replace_ServiceNowEmailAddress|

## Build and Test

This API is built using

* Microsoft Visual Studio 2022
* .Net Core 6.0

To build and test locally, you can either use visual studio 2022 or VSCode or simply use dotnet CLI `dotnet build` and `dotnet test` more information in dotnet CLI can be found at <https://docs.microsoft.com/en-us/dotnet/core/tools/>.

## Contribute

To contribute,

* If you are part of the team then create a branch for changes and then submit your changes for review by creating a pull request.
* If you are external to the organisation then fork this repository and make necessary changes and then submit your changes for review by creating a pull request.