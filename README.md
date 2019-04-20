# serverless-core-api
Azure Functions Evaluation


Semi-instructions:
- install stable .net core sdk
- install stable node.js

npm install -g azure-functions-core-tools
func settings add FUNCTIONS_WORKER_RUNTIME dotnet
func host start -build
Login-AzAccount
Connect-AzureRmAccount
Get-AzureRmSubscription
Select-AzureRmSubscription -TenantId ''
func azure functionapp publish nm-test-func