#!/bin/bash

# Exit on error
set -e

# Install Azure CLI on Linux
set -ecurl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4 --unsafe-perm true