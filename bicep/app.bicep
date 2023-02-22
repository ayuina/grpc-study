param prefix string = 'grpc-study'
param region string = resourceGroup().location

param repository string = 'streaming-server'
param tag string = '0.1.0'
param containerPort int = 5000

var acaenvName = '${prefix}-env'
var uamiName = '${prefix}-uami' 
var acrName = replace('${prefix}-acr', '-', '')

resource env 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
  name: acaenvName
}

resource acr 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: acrName
}

resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' existing = {
  name: uamiName
}

//output acr string = acr.listCredentials().passwords[0].value

resource containerApp 'Microsoft.App/containerApps@2022-10-01' = {
  name: repository
  location: region
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities:{
      '${uami.id}': { }
    }
  }
  properties: {
    managedEnvironmentId: env.id
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [
        {
          server: acr.properties.loginServer
          identity: uami.id
        }
      ]
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'http2'
      }
      
    }
    template: {
      containers: [
        {
          name: repository
          image: '${acr.properties.loginServer}/${repository}:${tag}'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Development'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 10
      }
    }
  }
}

output appfqdn string = containerApp.properties.latestRevisionFqdn

