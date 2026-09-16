# API deployment

The API pipeline builds and smoke-tests every change that affects the API. A push to `main` deploys the verified artifact to Azure App Service. Pull requests never deploy.

## Azure App Service

Create a Linux Azure App Service using the .NET 10 runtime. The free F1 plan is suitable for the initial private version. The App Service must exist before the first deployment workflow runs.

Keep the Azure resource name for the `AZURE_WEBAPP_NAME` GitHub variable.

## Passwordless GitHub access

Create a Microsoft Entra application or user-assigned managed identity for GitHub Actions. Give it the `Website Contributor` role scoped only to the target App Service.

Add a federated credential that trusts this repository's `main` branch:

```text
repo:<github-owner>/<github-repository>:ref:refs/heads/main
```

No client secret or publish profile is required.

## GitHub repository variables

Add these under **Settings > Secrets and variables > Actions > Variables**:

| Variable | Value |
| --- | --- |
| `AZURE_CLIENT_ID` | Client ID of the deployment identity |
| `AZURE_TENANT_ID` | Microsoft Entra tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_WEBAPP_NAME` | Exact App Service resource name |

## First deployment

Push the repository to GitHub with `main` as its default branch. The workflow in `.github/workflows/api.yml` then:

1. Restores and builds the .NET 10 API.
2. Starts the compiled API and checks `/health`.
3. Publishes an App Service deployment package.
4. Authenticates to Azure using GitHub OIDC.
5. Deploys the package to the configured App Service.

The deployment job can also be rerun manually from the GitHub Actions page after the repository variables and Azure identity are configured.
