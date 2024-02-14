@description('Resource location. Default value is resource group\'s location.')
param location string = resourceGroup().location

@description('Key Vault base URL to avoid GitHub warnings.')
param keyVaultBaseUrl string

resource functionStorage 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: 'storageaccountkoffb843f'
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
  name: 'ASP-KoffBotDev-b443'
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
    value: 'c7c3076f-a1be-43f8-9b5b-dad40fc150f0'
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: 'InstrumentationKey=c7c3076f-a1be-43f8-9b5b-dad40fc150f0;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/'
  }
  {
    name: 'AZURE_FUNCTIONS_ENVIRONMENT'
    value: 'dev'
  }
  {
    name: 'AzureWebJobsStorage'
    value: storageConnectionString
  }
  {
    name: 'DbConnectionString'
    value: '@Microsoft.KeyVault(SecretUri=${keyVaultBaseUrl}/secrets/DbConnectionString/58f26df4602d4dce980d9c12378b4290)'
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
    value: '@Microsoft.KeyVault(SecretUri=${keyVaultBaseUrl}/secrets/OpenAiApiKey/3bd5534c5d9a4a1fa5dbcead2aa77363)'
  }
  {
    name: 'SlackSigningSecret'
    value: '@Microsoft.KeyVault(SecretUri=${keyVaultBaseUrl}/secrets/SlackSigningSecret/83a746126ad44c62ab042e40c3f8aa38)'
  }
  {
    name: 'SlackWebHook'
    value: '@Microsoft.KeyVault(SecretUri=${keyVaultBaseUrl}/secrets/SlackWebHook/9172ccff377348afae1862d6d4130f04)'
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
  name: 'koffbot-dev'
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
