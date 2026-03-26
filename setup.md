# Setup instruction for Learning AI Agent Development 

This document lists the recommended tools and setup steps for completing the labs in this repository and building AI agents on Azure.

## 1. Required Accounts and Access

Prepare these accounts before installing local tools:

- An active Azure subscription
- Access to Azure AI Foundry or Azure OpenAI resources used by the labs
- A GitHub account for source control and optional GitHub Copilot usage

Azure account setup:

- Azure free account: https://azure.microsoft.com/free/
- Azure portal: https://portal.azure.com/
- Azure AI Foundry: https://ai.azure.com/

## 2. Core Developer Tools

Install these base tools on your machine.

### Visual Studio Code

Visual Studio Code is the recommended editor for the lab exercises.

- Download: https://code.visualstudio.com/
- Setup guide: https://code.visualstudio.com/docs/setup/setup-overview

### Git

Git is required to clone the repository and manage changes.

- Download: https://git-scm.com/downloads
- Setup guide: https://git-scm.com/book/en/v2/Getting-Started-Installing-Git

### Python 3.10+

Several labs include Python implementations. Use Python 3.10 or newer unless your course environment specifies a different version.

- Download: https://www.python.org/downloads/
- Setup guide: https://docs.python.org/3/using/index.html

After installation, verify:

```bash
python --version
pip --version
```

## 3. Azure Command-Line Tools

### Azure CLI

Azure CLI is required for signing in, selecting subscriptions, and validating Azure resources.

- Download and install: https://learn.microsoft.com/cli/azure/install-azure-cli
- Documentation: https://learn.microsoft.com/cli/azure/


### Azure Developer CLI (Optional but Recommended)

The Azure Developer CLI can help when working with Azure application templates and environment provisioning.

- Install: https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd
- Documentation: https://learn.microsoft.com/azure/developer/azure-developer-cli/

Verify installation:

```bash
azd version
```

## 5. Recommended VS Code Extensions

Install the following VS Code extensions for a smoother development workflow.


### Azure Tools Extension Pack

Provides a bundle of Azure development extensions.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-vscode.vscode-node-azure-pack

### Python

Required for running and debugging Python labs.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-python.python

### Pylance

Improves Python IntelliSense, type analysis, and navigation.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-python.vscode-pylance

### AI Toolkit for Visual Studio Code

Useful for building and experimenting with AI applications and agents in Microsoft Foundry.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-windows-ai-studio.windows-ai-studio

### Azure MCP Server

Provides Model Context Protocol integration and Azure tooling inside VS Code.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azure-mcp-server

### Azure Resources

Lets you browse and manage Azure resources from within VS Code.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureresourcegroups

### Microsoft Foundry

Adds Microsoft Foundry integration to VS Code.

- Marketplace: https://marketplace.visualstudio.com/items?itemName=teamsdevapp.vscode-ai-foundry
