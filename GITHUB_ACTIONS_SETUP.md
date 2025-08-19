# GitHub Actions for Pull Request Testing

## Overview

This repository is configured with GitHub Actions to automatically run tests on every pull request to ensure code quality and prevent breaking changes from being merged.

## Workflow Files

### 1. `.github/workflows/pr-validation.yml`
- **Trigger**: Runs on pull request events (opened, updated, reopened) targeting the `main` branch
- **Jobs**:
  - **test**: Builds the solution and runs all unit tests
  - **code-quality**: Checks code formatting and performs static analysis

### 2. `.github/workflows/deploy-azure-function.yml` 
- **Trigger**: Runs on pushes to `main` branch
- **Purpose**: Deploys the Azure Function after tests pass

## Setting Up Branch Protection Rules

To enforce that tests must pass before merging, follow these steps:

### Step 1: Navigate to Branch Protection Settings
1. Go to your GitHub repository
2. Click on **Settings** tab
3. Select **Branches** from the left sidebar
4. Click **Add rule** or **Edit** if a rule already exists for `main`

### Step 2: Configure Protection Rules
Configure the following settings:

#### Required Settings:
- **Branch name pattern**: `main`
- ✅ **Require a pull request before merging**
  - ✅ **Require approvals**: 1 (or more if desired)
  - ✅ **Dismiss stale reviews when new commits are pushed**
- ✅ **Require status checks to pass before merging**
  - ✅ **Require branches to be up to date before merging**
  - **Required status checks**: Add these checks:
    - `test` (from pr-tests.yml)
    - `code-quality` (from pr-tests.yml)

#### Optional but Recommended:
- ✅ **Require conversation resolution before merging**
- ✅ **Include administrators** (applies rules to admins too)
- ✅ **Allow force pushes** → **Nobody** (recommended)
- ✅ **Allow deletions** → Unchecked (recommended)

### Step 3: Save Rules
Click **Create** or **Save changes** to apply the protection rules.

## What This Achieves

1. **Automated Testing**: Every PR automatically runs the full test suite
2. **Code Quality**: Formatting and static analysis checks ensure consistent code style
3. **Merge Protection**: PRs cannot be merged until all checks pass
4. **Visibility**: Test results are clearly displayed in the PR interface
5. **History**: All test runs are logged and can be reviewed

## Workflow Features

### Caching
- NuGet packages are cached to speed up builds
- Cache keys are based on project files to ensure accuracy

### Test Reporting
- Test results are published in a readable format
- Failed tests are clearly highlighted in the PR interface

### Multiple Checks
- Unit tests must pass
- Code formatting must be correct
- Build must succeed

## Local Development

To ensure your changes will pass the GitHub Actions:

```bash
# Run tests locally
dotnet test

# Check code formatting
dotnet format --verify-no-changes

# Build in release mode
dotnet build --configuration Release
```

## Troubleshooting

### Tests Failing in CI but Passing Locally
- Ensure you're testing with the same .NET version (8.0.x)
- Check for environment-specific dependencies
- Verify all required secrets are configured

### Formatting Issues
- Run `dotnet format` locally before pushing
- Consider adding an `.editorconfig` file for consistent formatting

### Status Checks Not Appearing
- Ensure the workflow file is on the `main` branch
- Check that the job names in the workflow match the required status checks
- Workflows need to run at least once before they appear as available status checks
