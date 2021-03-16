targetScope = 'resourceGroup'
param global object
param webAppSku object
param baseNames object
param developerObjectId string

param buildtag string = utcNow()

var naming = {
  appInsights: '${baseNames.appInsights}'
  storage: '${baseNames.storage}'
  funcApp: '${baseNames.funcApp}'
  funcAppPlan: '${baseNames.funcAppPlan}'
  keyVault: '${baseNames.keyVault}'
}

output resourceGroupName string = resourceGroup().name
module common './common.bicep' = {
  name: 'common-${buildtag}'
  params: {
    global: global
    naming: naming
    webAppSku: webAppSku
  }
}
output commonAiName string = common.outputs.commonAiName
output funcAppPlanName string = common.outputs.funcAppPlanName

module api './api.bicep' = {
  name: 'api-${buildtag}'
  params: {
    global: global
    naming: naming
  }
  dependsOn: [
    common
  ]
}
output fctWebAppName string = api.outputs.fctWebAppName
output fctWebAppObjectId string = api.outputs.fctWebAppObjectId

module keyvault './keyvault.bicep' = {
  name: 'keyvault-${buildtag}'
  params: {
    global: global
    naming: naming
    developerObjectId: developerObjectId
  }
  dependsOn: [
    common
    api
  ]
}
output keyVaultName string = keyvault.outputs.keyVaultName
output keyVaultURL string = keyvault.outputs.keyVaultURL