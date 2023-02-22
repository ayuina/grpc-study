param prefix string = 'grpc-study'
param region string = resourceGroup().location

var logAnalyticsName = '${prefix}-laws'
var appInsightsName = '${prefix}-ai' 
var uamiName = '${prefix}-uami' 
var acaenvName = '${prefix}-env'
var acrName = replace('${prefix}-acr', '-', '')

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: region
  properties:{
    sku:{
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource appinsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: region
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

resource acaenv 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: acaenvName
  location: region
  sku: {
    name: 'Consumption'
  }
  properties: {
     appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
     }
  }
}

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  name: acrName
  location: region
  sku: {
    name: 'Standard'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Disabled'
  }
}

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: uamiName
  location: region
}

resource acrPull 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d'
}

resource assign 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: acr
  name: guid(acr.id, acrPull.id, uami.id)
  properties: {
    roleDefinitionId: acrPull.id
    principalType: 'ServicePrincipal'
    principalId: uami.properties.principalId
  }
}
