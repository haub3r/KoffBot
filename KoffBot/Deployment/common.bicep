param global object
param naming object
param webAppSku object

resource appInsights 'Microsoft.Insights/components@2020-02-02-preview' = {
  name: naming.appInsights
  location: global.location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
  }
}

resource funcAppPlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: naming.funcAppPlan
  location: global.location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
}

output commonAiName string = appInsights.name
output funcAppPlanName string = funcAppPlan.name