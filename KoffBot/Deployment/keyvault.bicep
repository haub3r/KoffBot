param global object
param naming object
param developerObjectId string

resource keyVault 'Microsoft.KeyVault/vaults@2019-09-01' = {
  name: naming.keyVault
  location: global.location
  properties: {
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: false
    enableSoftDelete: true
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: reference(resourceId('Microsoft.Web/sites', naming.funcApp), '2020-06-01', 'Full').identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: developerObjectId
        permissions: {
          secrets: [
            'get'
            'list'
            'set'
            'delete'
            'recover'
            'backup'
            'restore'
            'purge'
          ]
        }
      }
    ]
  }
}

output keyVaultName string = naming.keyVault
output keyVaultURL string = keyVault.properties.vaultUri