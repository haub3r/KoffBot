param global object
param naming object

resource eventFctStorage 'Microsoft.Storage/storageAccounts@2020-08-01-preview' = {
  name: naming.storage
  location: global.location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
  }
}

resource fct 'Microsoft.Web/sites@2020-06-01' = {
  name: naming.funcApp
  location: global.location
  kind: 'functionapp'
  properties: {
    serverFarmId: resourceId('Microsoft.Web/serverFarms', naming.funcAppPlan)
    httpsOnly: true
    siteConfig: {
      use32BitWorkerProcess: false
      http20Enabled: true
    }
    clientAffinityEnabled: false
  }
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    'hidden-related:diagnostics/changeAnalysisScanEnabled': 'true'
  }
}

output fctWebAppName string = fct.name
output fctWebAppObjectId string = fct.identity.principalId