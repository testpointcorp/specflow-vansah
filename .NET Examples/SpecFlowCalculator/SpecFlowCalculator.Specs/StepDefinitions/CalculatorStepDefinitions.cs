using FluentAssertions;
using TechTalk.SpecFlow;
using Vansah;
namespace SpecFlowCalculator.Specs.Steps
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        private readonly Calculator _calculator;
        private int _result;
        
        private ScenarioContext _scenarioContext;

        private String _testCaseKey;
        private String _assetKey;
        private Boolean isIssueKey;
        private String _vansahResult = "passed";

        private static String _sprintName = "SM Sprint 1";
        private static String _releaseName = "Release 24";
        private static String _environmentName = "SYS";

        [When("the Test Case key is (.*) And the Issue Key is (.*)")]
        public void GetTestIssueKeyDetails(String testCaseKey, String issueKey)
        {
            _testCaseKey = testCaseKey;
            _assetKey = issueKey;
            isIssueKey = true;
        }
        [When("the Test Case key is (.*) And the Test Folder ID is (.*)")]
        public void GetTestFolderIDDetails(String testCaseKey, String testFolderID)
        {
            _testCaseKey = testCaseKey;
            _assetKey = testFolderID;
            isIssueKey = false;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            if (_scenarioContext.TestError != null)
            {
                _vansahResult = "failed";
                SendTestResultToVansah(_testCaseKey, _vansahResult);
            }
            else
            {
                SendTestResultToVansah(_testCaseKey, _vansahResult);
            }
        }

        private void SendTestResultToVansah(String testCaseKey, String result)
        {
            VansahNode vansah = new VansahNode();

            // Required
            vansah.SetVansahToken = Environment.GetEnvironmentVariable("VANSAH_TOKEN") ?? "No Value found!";
            

            // Optional
            vansah.SprintName = _sprintName;
            vansah.release_Name = _releaseName;
            vansah.environment_Name = _environmentName;
            if (isIssueKey)
            {
                vansah.JiraIssueKey = _assetKey;
                vansah.AddQuickTestFromJiraIssue(testCaseKey, result);
            }
            else {
                vansah.TestFolderID = _assetKey;
                vansah.AddQuickTestFromTestFolders(testCaseKey, result);
            }
           
        }

        public CalculatorStepDefinitions(Calculator calculator, ScenarioContext scenarioContext)
        {
            _calculator = calculator;
            _scenarioContext = scenarioContext;
        }
        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(int number)
        {
            _calculator.FirstNumber = number;
        }

        [Given("the second number is (.*)")]
        public void GivenTheSecondNumberIs(int number)
        {
            _calculator.SecondNumber = number;
        }

        [When("the two numbers are added")]
        public void WhenTheTwoNumbersAreAdded()
        {
            _result = _calculator.Add();
        }

        [When("the two numbers are subtracted")]
        public void WhenTheTwoNumbersAreSubtracted()
        {
            _result = _calculator.Subtract();
        }
        [When("the two numbers are divided")]
        public void WhenTheTwoNumbersAreDivided()
        {
            _result = _calculator.Divide();
        }

        [When("the two numbers are multiplied")]
        public void WhenTheTwoNumbersAreMultiplied()
        {
            _result = _calculator.Multiply();
        }

        [Then("the result should be (.*)")]
        public void ThenTheResultShouldBe(int result)
        {
            _result.Should().Be(result);
        }

    }
}
