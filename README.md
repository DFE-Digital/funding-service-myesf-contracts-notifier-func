# Manage your education and skills funding - Contracts notifications function

## Introduction

Contracts notification function is a serverless azure function that handles significant contract changes and notifies interested applications and consumers e.g. learning providers. This function is triggered by an azure service bus message.

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

  "ContractsDataApiConfiguration": {
    "ApiBaseAddress": "replace_local_contract_api_or_stub",
    "ShouldSkipAuthentication": "true",
    "ContractReminderEndpoint": {
      "QueryParameters": {
        "reminderInterval": "14",
        "page": "1",
        "count": "25"
      }
    }
  },
  "FundingClaimsDataApiConfiguration": {
    "ApiBaseAddress": "replace_local_contract_api_or_stub",
    "ShouldSkipAuthentication": "true"
  },
  "SubcontractorDeclarationDataApiConfiguration": {
    "ApiBaseAddress": "replace_local_contract_api_or_stub",
    "ShouldSkipAuthentication": "true"
  },

  "AuditApiConfiguration": {
    "ApiBaseAddress": "replace_local_audit_api_or_stub",
    "ShouldSkipAuthentication": "true"
  },

  "HttpPolicyOptions": {
    "HttpRetryCount": 3,
    "HttpRetryBackoffPower": 2,
    "CircuitBreakerToleranceCount": 5,
    "CircuitBreakerDurationOfBreak": "0.00:00:15"
  },


  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "WEBSITE_TIME_ZONE": "GMT Standard Time",
    "NotifierServiceBusConnectionString": "replace_ServiceBusConnectionString",
    "Pds.Contracts.Notifications.Topic": "replace_contracts_notification_topic",
    "Pds.Contracts.ReadyToSign.Subscription": "replace_contracts_readytosign_subscription",
    "Pds.Contracts.Approved.Subscription": "replace_contracts_approved_subscription",
    "Pds.Contracts.ReadyToReview.Subscription": "replace_contracts_readytoreview_subscription",
    "Pds.Contracts.Withdrawn.Subscription": "replace_contracts_withdrawn_subscription",
    "CdsUserExceptionEmail": "replace_cdsUserExceptionEmail",
    "ServiceNowEmailAddress": "replace_ServiceNowEmailAddress"
  }
}
```

The following configurations need to be replaced with your values.
|Key|Token|Example|
|-|-|-|
|ContractsDataApiConfiguration.ApiBaseAddress|replace_local_contract_api_or_stub|<http://localhost:5001>|
|SubcontractorDeclarationDataApiConfiguration.ApiBaseAddress|replace_local_contract_api_or_stub|<http://localhost:5001>|
|FundingClaimDataApiConfiguration.ApiBaseAddress|replace_local_fundingclaim_api_or_stub|
|AuditApiConfiguration.ApiBaseAddress|replace_local_audit_api_or_stub|<http://localhost:5002/>|
|NotifierServiceBusConnectionString|replace_ServiceBusConnectionString|A valid azure service bus connection string|
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