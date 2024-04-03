# Specflow Integration with Vansah Test Management for Jira

This tutorial guides you through the process of integrating Specflow tests with Vansah Test Management for Jira to automatically send test case results. 

By following this setup, you can streamline your testing workflow, ensuring that test outcomes are recorded directly in Vansah.

## Prerequisites
- SpecFlow project setup in Visual Studio.
- Make sure that [`Vansah`](https://marketplace.atlassian.com/apps/1224250/vansah-test-management-for-jira?tab=overview&hosting=cloud) is installed in your Jira workspace
- You need to Generate Vansah [`connect`](https://docs.vansah.com/docs-base/generate-a-vansah-api-token-from-jira-cloud/) token to authenticate with Vansah APIs.

## Setup
- Download VansahBinding: [Clone/download](https://github.com/testpointcorp/specflow-vansah/tree/prod/VansahBinding) from GitHub.

## Configuration
- Create/Update [specflow.json](/.NET%20Examples/SpecFlowCalculator/SpecFlowCalculator.Specs/specflow.json) into your Testing Project: Configure to recognize VansahBinding assembly.
```json
{
  "stepAssemblies": [
    {
      "assembly": "VansahBinding"
    }
  ]
}
```
- Add VansahBinding as Project Reference into your Project [*.csproj](/.NET%20Examples/SpecFlowCalculator/SpecFlowCalculator.Specs/SpecFlowCalculator.Specs.csproj) file
```xml
<ItemGroup>
    <ProjectReference Include="..\..\..\VansahBinding\VansahBinding.csproj" />
</ItemGroup>
```
- Setting Environment Variables: Store your Vansah API token as an environment variable for security. 

For Windows (use cmd)
```cmd
setx VANSAH_TOKEN "your_vansah_api_token_here"

```
For macOS
```bash
echo 'export VANSAH_TOKEN="your_vansah_api_token_here"' >> ~/.bash_profile

source ~/.bash_profile

```
For Linux (Ubuntu, Debian, etc.)
```bash
echo 'export VANSAH_TOKEN="your_vansah_api_token_here"' >> ~/.bashrc

source ~/.bashrc

``` 
- Use of Test Run Properties
  
 Go to your downloaded [VansahSpecflow.cs](/VansahBinding/VansahSpecflow.cs) and uncomment the Sprint, Release and Environment details as per your requirement
```csharp
       //Update these Values with your Project Test Run properties
        private static String _sprintName = "SM Sprint 1";
        private static String _releaseName = "Release 24";
        private static String _environmentName = "SYS";
        /**
          Other functions
        */       
        private void SendTestResultToVansah(String testCaseKey, String result)
        {
            //Process 1

            // Uncomment the below values if required; note: do not uncomment if not used.
            // vansah.SprintName = _sprintName;
            // vansah.release_Name = _releaseName;
            // vansah.environment_Name = _environmentName;

            //Next process

        }
```
## Writing Features
Use BDD statements to link scenarios with Vansah:
```gherkin
When the Test Case key is Test-C8 And the Issue Key is Test-2
When the Test Case key is Test-C9 And the Test Folder ID is b97fe80b-0b6a-11ee-8e52-5658ef8eadd5
```
## Usage Examples
View our Sample feature [file](/.NET%20Examples/SpecFlowCalculator/SpecFlowCalculator.Specs/Features/Calculator.feature)
```gherkin
Feature: Calculator

@Add
Scenario: Add two number
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	When the Test Case key is Test-C8 And the Issue Key is Test-2
	Then the result should be 120

@Subtract
Scenario: Subtract two numbers
	Given the first number is 50
	And the second number is 25
	When the two numbers are subtracted
	When the Test Case key is Test-C9 And the Test Folder ID is b97fe80b-0b6a-11ee-8e52-5658ef8eadd5
	Then the result should be 25
```
## Execution
Run your tests as usual. Results automatically sync with Vansah.

## Conclusion
By following these steps, you can efficiently manage your test cases and results in Vansah Test Management for Jira, improving the visibility and traceability of your testing efforts.

For more details on Specflow, visit the [Specflow documentation](https://docs.specflow.org/projects/specflow/en/latest/Bindings/Use-Bindings-from-External-Assemblies.html). 

For Vansah specific configurations and API details, please refer to the [Vansah API documentation](https://apidoc.vansah.com/).
