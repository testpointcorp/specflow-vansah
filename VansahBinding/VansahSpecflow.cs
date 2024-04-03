using System.Configuration;
using TechTalk.SpecFlow;
using Vansah;

namespace VansahBinding
{
    [Binding]
    public class VansahSpecflow
    {
        private String _testCaseKey;
        private String _assetKey;
        private Boolean isIssueKey;
        private String _vansahResult = "passed";
        private ScenarioContext _scenarioContext;

        //Provide your Test Run Properties here
        private static String _sprintName = "SM Sprint 1";
        private static String _releaseName = "Release 24";
        private static String _environmentName = "SYS";

        public VansahSpecflow(ScenarioContext scenarioContext) {

            _scenarioContext = scenarioContext;
        }

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

            // Required - Use Environment Variables
            vansah.SetVansahToken = Environment.GetEnvironmentVariable("VANSAH_TOKEN") ?? "Provide your Vansah Connect Token here";

            // Optional - Uncomment the below proeprties if required else leave them as comment
            //vansah.SprintName = _sprintName;
            //vansah.release_Name = _releaseName;
            //vansah.environment_Name = _environmentName;

            if (isIssueKey)
            {
                vansah.JiraIssueKey = _assetKey;
                vansah.AddQuickTestFromJiraIssue(testCaseKey, result);
            }
            else
            {
                vansah.TestFolderID = _assetKey;
                vansah.AddQuickTestFromTestFolders(testCaseKey, result);
            }

        }

    }
}