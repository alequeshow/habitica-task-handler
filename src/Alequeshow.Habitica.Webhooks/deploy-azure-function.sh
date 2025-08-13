#!/bin/bash

# Exit on error
set -e

# Set these variables before running the script
# RESOURCE_GROUP="<ResourceGroupName>"
# LOCATION="<AzureRegion>"
# STORAGE_ACCOUNT="<StorageAccountName>"
FUNCTION_APP="<FunctionAppName>"
SUBSCRIPTION_ID="<SubscriptionId>"

# Login to Azure
az login
# Log in with a service principal using client secret. Use --password=secret if the first
#     character of the password is '-'.
#         az login --service-principal --username APP_ID --password CLIENT_SECRET --tenant TENANT_ID

# Set subscription if needed
az account set --subscription "$SUBSCRIPTION_ID"

# Create resource group (First deploy only)
# az group create --name $RESOURCE_GROUP --location $LOCATION

# # Create storage account (First deploy only)
# az storage account create --name $STORAGE_ACCOUNT --location $LOCATION --resource-group $RESOURCE_GROUP --sku Standard_LRS

# Create a Service Principal (First deploy only)
# az ad sp create-for-rbac --name "$FUNCTION_APP" --role contributor --scopes /subscriptions/$SUBSCRIPTION_ID

# # Create function app (for .NET isolated process, adjust runtime if needed)
# az functionapp create \
#   --resource-group $RESOURCE_GROUP \
#   --consumption-plan-location $LOCATION \
#   --runtime dotnet-isolated \
#   --functions-version 4 \
#   --name $FUNCTION_APP \
#   --storage-account $STORAGE_ACCOUNT

# Publish the function app
func azure functionapp publish $FUNCTION_APP --dotnet-isolated

echo "Deployment complete!"

# Replace the placeholder variables at the top with your actual values.
# Make the script executable:
#   chmod +x deploy-azure-function.sh
# Run it from your project root:
#   ./deploy-azure-function.sh