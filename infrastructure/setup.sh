# ====================================
# Parameters
# ====================================
project_name="abstract"
location="westeurope"
locationAbbreviation="we"

resource_group_name="rg-$project_name-$locationAbbreviation"
storage_account_name="sa"$project_name$locationAbbreviation
app_service_plan_name="asp-$project_name-$locationAbbreviation"
app_service_web_name="app-$project_name-$locationAbbreviation"
cosmosdb_name="cdb-$project_name-$locationAbbreviation"
functionapp_name="fnc-$project_name-$locationAbbreviation"
acr_registry_name="acr"$project_name$locationAbbreviation
aks_cluster_name="aks-$project_name-$locationAbbreviation"
cognitiveservices_account_name="cs-$project_name-$locationAbbreviation"
machinelearning_workspace_name="aml-$project_name-$locationAbbreviation"

# login / set default account
#az account set --subscription $subscriptionId


# ====================================
# Create Resources on Azure
# ====================================
# resource group
az group create \
    -n $resource_group_name \
    -l $location

# storage account
az storage account create \
    -n $storage_account_name \
    -g $resource_group_name \
    -l $location --enable-hierarchical-namespace \
    --https-only \
    --kind StorageV2 \
    --min-tls-version TLS1_2 \
    --sku Standard_LRS

# web app
az appservice plan create \
    -n $app_service_plan_name \
    -g $resource_group_name \
    --sku B1

az webapp create \
    -g $resource_group_name \
    -p $app_service_plan_name \
    -n $app_service_web_name

# cosmosdb
az cosmosdb create \
    -n $cosmosdb_name \
    -g $resource_group_name

az cosmosdb sql database create \
  -a $cosmosdb_name \
  -n "RequestsDb" \
  -g $resource_group_name \
  --throughput 400

az cosmosdb sql container create \
  -a $cosmosdb_name \
  -d "RequestsDb" \
  -n "RequestsContainer" \
  -g $resource_group_name \
  --partition-key-path "/userName"

az cosmosdb sql database create \
  -a $cosmosdb_name \
  -n "ResultsDb" \
  -g $resource_group_name \
  --throughput 400

az cosmosdb sql container create \
  -a $cosmosdb_name \
  -d "ResultsDb" \
  -n "ResultsContainer" \
  -g $resource_group_name \
  --partition-key-path "/userName"

# function app
az functionapp create \
    -n $functionapp_name \
    -g $resource_group_name \
    --storage-account $storage_account_name \
    --plan $app_service_plan_name \
    --functions-version 3 \
    --os-type Windows \
    --runtime dotnet

# kubernetes
az acr create \
  -n $acr_registry_name \
  -g $resource_group_name \
  -l $location \
  --sku Basic \
  --admin-enabled true \
  --zone-redundancy Disabled

az aks create \
  -n $aks_cluster_name \
  -g $resource_group_name \
  --load-balancer-sku Standard \
  --enable-managed-identity \
  --attach-acr $acr_registry_name

# Cognitive Services
az cognitiveservices account create \
  --kind "FormRecognizer" \
  -n $cognitiveservices_account_name \
  -g $resource_group_name \
  -l $location \
  --sku "S0" \
  --yes

# Azure ML Studio
storageAccountResourceId=$(az storage account show -n $storage_account_name -g $resource_group_name --query "id" --output tsv)
containerRegistryResourceId=$(az acr show -n $acr_registry_name -g $resource_group_name --query "id" --output tsv )
az ml workspace create \
  -w $machinelearning_workspace_name \
  -g $resource_group_name \
  -l $location \
  --storage-account $storageAccountResourceId \
  --container-registry $containerRegistryResourceId
  
# ====================================
# Configure resources
# ====================================
cosmosDbConnectionString=$(az cosmosdb keys list --type connection-strings -n $cosmosdb_name -g $resource_group_name --query "connectionStrings[?description=='Primary SQL Connection String'].connectionString" --output tsv)
storageAccountConnectionString=$(az storage account show-connection-string -n $storage_account_name -g $resource_group_name --output tsv)
cognitiveServicesUri=$(az cognitiveservices account show -n $cognitiveservices_account_name -g $resource_group_name --query "properties.endpoint" --output tsv)
cognitiveServicesKey=$(az cognitiveservices account keys list -n $cognitiveservices_account_name -g $resource_group_name --query key1 --output tsv)

# app service
az webapp config appsettings set \
  -g $resource_group_name \
  -n $app_service_web_name \
  --settings CosmosDB=$cosmosDbConnectionString storageaccountblob=$storageAccountConnectionString

# function
az functionapp config appsettings set \
  -g $resource_group_name \
  -n $functionapp_name \
  --settings CosmosDB=$cosmosDbConnectionString StorageAccount=$storageAccountConnectionString formrecognizerkey=$cognitiveServicesKey formrecognizeruri=$cognitiveServicesUri