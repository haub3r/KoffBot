@description('Resource location. Default value is resource group\'s location.')
param location string = resourceGroup().location

resource functionStorage 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'storageaccountkoffbab19'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
  }
}

resource consumptionPlan 'Microsoft.Web/serverfarms@2021-01-01' = {
  name: 'ASP-KoffBot-8245'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${functionStorage.name};AccountKey=${functionStorage.listKeys().keys[0].value}'
var appSettings = [
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: 'c7f13bc2-e202-4150-91c7-c30987a2af2b'
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: 'InstrumentationKey=c7f13bc2-e202-4150-91c7-c30987a2af2b;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/'
  }
  {
    name: 'AZURE_FUNCTIONS_ENVIRONMENT'
    value: 'prod'
  }
  {
    name: 'AzureWebJobsStorage'
    value: storageConnectionString
  }
  {
    name: 'DbConnectionString'
    value: '@Microsoft.KeyVault(SecretUri=https://koffbot-kv.vault.azure.net/secrets/DbConnectionString/e96d10b1562c440cae2f85ed87b6fd03)'
  }
  {
    name: 'FUNCTIONS_EXTENSION_VERSION'
    value: '~4'
  }
  {
    name: 'FUNCTIONS_WORKER_RUNTIME'
    value: 'dotnet-isolated'
  }
  {
    name: 'OpenAiApiKey'
    value: '@Microsoft.KeyVault(SecretUri=https://koffbot-kv.vault.azure.net/secrets/OpenAiApiKey/6ec78a313d5847c9be18b50bd3ba0f30)'
  }
  {
    name: 'SlackSigningSecret'
    value: '@Microsoft.KeyVault(SecretUri=https://koffbot-kv.vault.azure.net/secrets/SlackSigningSecret/49579046017048a8a91486abc1cd4d99)'
  }
  {
    name: 'SlackWebHook'
    value: '@Microsoft.KeyVault(SecretUri=https://koffbot-kv.vault.azure.net/secrets/SlackWebHook/fa564c9e7bc343d885e0de7219c020ca)'
  }
  {
    name: 'TimerTriggerScheduleFridayFunction'
    value: '0 1 0 * * 5'
  }
  {
    name: 'TimerTriggerScheduleHolidayFunction'
    value: '0 0 0 * * *'
  }
  {
    name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
    value: storageConnectionString
  }
  {
    name: 'WEBSITE_RUN_FROM_PACKAGE'
    value: '1'
  }
  {
    name: 'WEBSITE_TIME_ZONE'
    value: 'FLE Standard Time'
  }
]

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: 'koffbot'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: consumptionPlan.id
    httpsOnly: true
    siteConfig: {
      defaultDocuments: []
      use32BitWorkerProcess: false
      http20Enabled: true
      appSettings: appSettings
      ftpsState: 'Disabled'
      netFrameworkVersion: 'v8.0'
    }
    clientAffinityEnabled: false
  }
  identity: {
    type: 'SystemAssigned'
  }
}
