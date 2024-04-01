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

        private string _testCaseKey;
        private string _assetKey;
        private string _vansahResult = "passed";

        private static string _sprintName = "SM Sprint 1";
        private static string _releaseName = "Release 24";
        private static string _environmentName = "SYS";

        [When("the Test Case key is (.*) And the Asset is (.*)")]
        public void GetTestAssetDetails(string testCaseKey, string assetKey)
        {
            _testCaseKey = testCaseKey;
            _assetKey = assetKey;
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

        private void SendTestResultToVansah(string testCaseKey, string result)
        {
            VansahNode vansah = new VansahNode();

            // Required
            vansah.SetVansahToken = Environment.GetEnvironmentVariable("VANSAH_TOKEN") ?? "No Value found!";
            vansah.JiraIssueKey = _assetKey;

            // Optional
            vansah.SprintName = _sprintName;
            vansah.release_Name = _releaseName;
            vansah.environment_Name = _environmentName;

            vansah.AddQuickTestFromJiraIssue(testCaseKey, result);
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
