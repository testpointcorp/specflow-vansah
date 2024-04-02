using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;

namespace Vansah
{
    public class VansahNode
    {

        //--------------------------- ENDPOINTS -------------------------------------------------------------------------------

        // The API version to be used for requests. This ensures compatibility with the specific version of the Vansah API.
        private static string api_Version = "v1";

        /// <summary>
        /// The default URL for the Vansah API. This URL is used unless another URL is specified via the SetVansahURL property.
        /// </summary>
        private static string default_Vansah_URL = "https://prod.vansahnode.app";

        /// <summary>
        /// The actual URL used for the Vansah API requests. It defaults to the default_Vansah_URL but can be overridden using the SetVansahURL property.
        /// </summary>
        private static string vansah_URL = default_Vansah_URL;

        /// <summary>
        /// Sets a custom URL for the Vansah API. If a null value is provided, it defaults back to the predefined URL ("https://prod.vansahnode.app").
        /// </summary>
        public string SetVansahURL
        {
            set
            {
                vansah_URL = value ?? default_Vansah_URL;
            }
        }

        // Endpoint for adding a test run. Constructs the URL dynamically based on the Vansah URL and API version.
        private static string add_Test_Run => $"{vansah_URL}/api/{api_Version}/run";

        // Endpoint for adding a test log. Constructs the URL dynamically, allowing for the addition of logs to a test run.
        private static string add_Test_Log => $"{vansah_URL}/api/{api_Version}/logs";

        // Endpoint for updating a test log. The specific log ID will be appended during the request to target a specific log.
        private static string update_Test_Log => $"{vansah_URL}/api/{api_Version}/logs/";

        // Endpoint for removing a test log. Similar to update, the log ID is appended to this base URL in the actual request.
        private static string remove_Test_Log => $"{vansah_URL}/api/{api_Version}/logs/";

        // Endpoint for removing a test run. The run ID will be appended to this URL to specify which run to remove.
        private static string remove_Test_Run => $"{vansah_URL}/api/{api_Version}/run/";

        // Endpoint to retrieve test scripts based on the test case. This is used to list scripts associated with a case.
        private static string test_Script => $"{vansah_URL}/api/{api_Version}/testCase/list/testScripts";

        //--------------------------- INFORM YOUR UNIQUE VANSAH TOKEN HERE ---------------------------------------------------

        /// <summary>
        /// The Vansah API token used for authenticating requests. This should be set to your unique token provided by Vansah.
        /// </summary>
        private string vansahToken = "Your Token Here";

        /// <summary>
        /// Sets the Vansah API token for use in authenticating requests. If a null value is provided, the token is set to a default message indicating that the token is not properly set.
        /// </summary>
        public string SetVansahToken
        {
            set
            {
                vansahToken = value ?? "Vansah Connect Token is not properly set";
            }
        }

        //--------------------------- INFORM IF YOU WANT TO UPDATE VANSAH HERE -----------------------------------------------

        // Controls whether results are sent to Vansah. "0" means no results will be sent; "1" means results will be sent.
        private static readonly string updateVansah = "1";

        //--------------------------------------------------------------------------------------------------------------------	


        // Public and private properties and fields used for configuring test runs and logs:
        /// <summary>
        /// Gets or sets the unique identifier for the test folder in Vansah. This is mandatory unless a JiraIssueKey is provided.
        /// </summary>
        public string TestFolderID { get; set; }

        /// <summary>
        /// Gets or sets the JIRA issue key associated with the test. This is mandatory unless TestFolderID is provided.
        /// </summary>
        public string JiraIssueKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the sprint associated with the test. This field is mandatory.
        /// </summary>
        public string SprintName { get; set; }

        //The internal caseKey ID (e.g., "TEST-C1") used for identifying the test case. This is a mandatory field.
        private string? caseKey;

        /// <summary>
        /// Gets or sets the release or version key from JIRA associated with the test. This field is mandatory.
        /// </summary>
        public string release_Name { get; set; }

        /// <summary>
        /// Gets or sets the environment ID from Vansah for the JIRA app (e.g., "SYS" or "UAT"). This field is mandatory.
        /// </summary>
        public string environment_Name { get; set; }


        // The result of the test expressed as an integer (e.g., 0 = N/A, 1 = FAIL, 2 = PASS, 3 = Not tested). Mandatory.
        private int resultKey;

        // Boolean indicating whether a screenshot of the webpage to be tested should be uploaded. Default is false.
        private bool uploadScreenshot = false;

        // Textual comment about the actual result of the test.
        private string comment;

        // The order of the test step within the test case. This is used to identify the sequence of test steps.
        private int step_Order;

        // A unique identifier for the test run, generated by an API request.
        private string test_Run_Identifier;

        // A unique identifier for the test log, generated by an API request.
        private string test_Log_Identifier;

        // Path to the file to be used for screenshot upload. This is internally managed.
        private string file;

        // The base64-encoded string of the file specified for upload. This is used when attaching screenshots to logs.
        private string base64FilefromUser;

        // The number of test rows. This could be used for iterating over multiple test cases or steps.
        private int testRows;

        // The HttpClient used for making API requests to Vansah. It is configured with necessary headers and authorization.
        private HttpClient httpClient;

        // A mapping from string representations of test results to their corresponding integer codes.
        private Dictionary<string, int> resultAsName = new Dictionary<string, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VansahNode"/> class with specific test folder and JIRA issue identifiers.
        /// </summary>
        /// <param name="testFolders">The test folder identifier. Used to categorize tests within Vansah.</param>
        /// <param name="jiraIssue">The JIRA issue key. Links the tests to a specific JIRA issue.</param>
        public VansahNode(string testFolders, string jiraIssue)
        {
            TestFolderID = testFolders;
            JiraIssueKey = jiraIssue;
            // Initialize test result mapping
            resultAsName.Add("NA", 0);
            resultAsName.Add("FAILED", 1);
            resultAsName.Add("PASSED", 2);
            resultAsName.Add("UNTESTED", 3);
        }

        /// <summary>
        /// Default constructor. Initializes a new instance of the <see cref="VansahNode"/> class without initial test folder or JIRA issue identifiers.
        /// </summary>
        public VansahNode()
        {
            // Initialize test result mapping
            resultAsName.Add("NA", 0);
            resultAsName.Add("FAILED", 1);
            resultAsName.Add("PASSED", 2);
            resultAsName.Add("UNTESTED", 3);
        }

        /// <summary>
        /// Creates a new test run identifier for a specified JIRA issue. This identifier is used for subsequent testing actions related to the JIRA issue.
        /// </summary>
        /// <param name="testCase">The test case identifier associated with the JIRA issue.</param>
        public void AddTestRunFromJiraIssue(string testCase)
        {
            caseKey = testCase;
            ConnectToVansahRest("AddTestRunFromJiraIssue");
        }

        /// <summary>
        /// Creates a new test run identifier for a specified test folder. This identifier is used for subsequent testing actions related to the test folder.
        /// </summary>
        /// <param name="testCase">The test case identifier associated with the test folder.</param>
        public void AddTestRunFromTestFolder(string testCase)
        {
            caseKey = testCase;
            ConnectToVansahRest("AddTestRunFromTestFolder");
        }
        /// <summary>
        /// Adds a new test log for the specified test case. This method does not include a screenshot.
        /// </summary>
        /// <param name="result">The result of the test step. It uses predefined integer values (e.g., 0 = N/A, 1 = Fail, 2 = Pass, 3 = Not tested).</param>
        /// <param name="Comment">A comment or description of the test result.</param>
        /// <param name="testStepRow">The order or index of the test step within the test case.</param>
        public void AddTestLog(int result, string Comment, int testStepRow)
        {
            resultKey = result;
            comment = Comment;
            step_Order = testStepRow;
            uploadScreenshot = false;
            ConnectToVansahRest("AddTestLog");
        }

        /// <summary>
        /// Adds a new test log for the specified test case, including a path to a screenshot file.
        /// </summary>
        /// <param name="result">The result of the test step. It uses predefined integer values (e.g., 0 = N/A, 1 = Fail, 2 = Pass, 3 = Not tested).</param>
        /// <param name="Comment">A comment or description of the test result.</param>
        /// <param name="testStepRow">The order or index of the test step within the test case.</param>
        /// <param name="screenshotPath">The file path to the screenshot to be uploaded. The screenshot should illustrate the test result.</param>
        public void AddTestLog(int result, string Comment, int testStepRow, string screenshotPath)
        {
            resultKey = result;
            comment = Comment;
            step_Order = testStepRow;
            Console.WriteLine(Validatefile(screenshotPath));
            ConnectToVansahRest("AddTestLog");
        }

        /// <summary>
        /// Adds a new test log for the specified test case. This overload allows specifying the result as a string.
        /// </summary>
        /// <param name="result">The result of the test step as a string (e.g., "PASS", "FAIL"). The string is case-insensitive.</param>
        /// <param name="Comment">A comment or description of the test result.</param>
        /// <param name="testStepRow">The order or index of the test step within the test case.</param>
        public void AddTestLog(string result, string Comment, int testStepRow)
        {
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            comment = Comment;
            step_Order = testStepRow;
            uploadScreenshot = false;
            ConnectToVansahRest("AddTestLog");
        }

        /// <summary>
        /// Adds a new test log for the specified test case, including a path to a screenshot file. This overload allows specifying the result as a string.
        /// </summary>
        /// <param name="result">The result of the test step as a string (e.g., "PASS", "FAIL"). The string is case-insensitive.</param>
        /// <param name="Comment">A comment or description of the test result.</param>
        /// <param name="testStepRow">The order or index of the test step within the test case.</param>
        /// <param name="screenshotPath">The file path to the screenshot to be uploaded. The screenshot should illustrate the test result.</param>
        public void AddTestLog(string result, string Comment, int testStepRow, string screenshotPath)
        {
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            comment = Comment;
            step_Order = testStepRow;
            Console.WriteLine(Validatefile(screenshotPath));
            ConnectToVansahRest("AddTestLog");
        }
        /// <summary>
        /// Creates a new test run and log for a specified test case linked to a JIRA issue. This is useful for test cases without steps, where only the overall result matters.
        /// </summary>
        /// <param name="testCase">The test case identifier.</param>
        /// <param name="result">The overall test result as a string (e.g., "PASS", "FAIL"). The string is case-insensitive.</param>
        public void AddQuickTestFromJiraIssue(string testCase, string result)
        {
            // Converts the string result to its corresponding integer value.
            caseKey = testCase;
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            ConnectToVansahRest("AddQuickTestFromJiraIssue");
        }

        /// <summary>
        /// Creates a new test run and log for a specified test case associated with a test folder. Useful for cases without steps, focusing on the overall result.
        /// </summary>
        /// <param name="testCase">The test case identifier.</param>
        /// <param name="result">The overall test result as a string (e.g., "PASS", "FAIL"). The string is case-insensitive.</param>
        public void AddQuickTestFromTestFolders(string testCase, string result)
        {
            // Converts the string result to its corresponding integer value.
            caseKey = testCase;
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            ConnectToVansahRest("AddQuickTestFromTestFolders");
        }

        /// <summary>
        /// Creates a new test run and log for a specified test case linked to a JIRA issue. This is useful for test cases without steps, where only the overall result matters.
        /// </summary>
        /// <param name="testCase">The test case identifier.</param>
        /// <param name="result">The overall test result as an integer (0 = N/A, 1 = Fail, 2 = Pass, 3 = Not tested).</param>
        public void AddQuickTestFromJiraIssue(string testCase, int result)
        {
            // Directly uses the integer result value.
            caseKey = testCase;
            resultKey = result;
            ConnectToVansahRest("AddQuickTestFromJiraIssue");
        }

        /// <summary>
        /// Creates a new test run and log for a specified test case associated with a test folder. Useful for cases without steps, focusing on the overall result.
        /// </summary>
        /// <param name="testCase">The test case identifier.</param>
        /// <param name="result">The overall test result as an integer (0 = N/A, 1 = Fail, 2 = Pass, 3 = Not tested).</param>
        public void AddQuickTestFromTestFolders(string testCase, int result)
        {
            // Directly uses the integer result value.
            caseKey = testCase;
            resultKey = result;
            ConnectToVansahRest("AddQuickTestFromTestFolders");
        }
        /// <summary>
        /// Deletes the test run created by the AddTestRunFromJiraIssue or AddTestRunFromTestFolder methods.
        /// </summary>
        public void RemoveTestRun()
        {
            ConnectToVansahRest("RemoveTestRun");
        }

        /// <summary>
        /// Deletes a test log identifier created by any AddTestLog method variant.
        /// </summary>
        public void RemoveTestLog()
        {
            ConnectToVansahRest("RemoveTestLog");
        }

        /// <summary>
        /// Updates a test log with a new result and comment. This variant does not include a screenshot.
        /// </summary>
        /// <param name="result">The updated result of the test as an integer (e.g., 0 = N/A, 1 = Fail, 2 = Pass, 3 = Not tested).</param>
        /// <param name="Comment">The updated comment or description of the test result.</param>
        public void UpdateTestLog(int result, string Comment)
        {
            resultKey = result;
            comment = Comment;
            uploadScreenshot = false;
            ConnectToVansahRest("UpdateTestLog");
        }

        /// <summary>
        /// Updates a test log with a new result and comment, and includes a path to a screenshot file.
        /// </summary>
        /// <param name="result">The updated result of the test as an integer.</param>
        /// <param name="Comment">The updated comment or description of the test result.</param>
        /// <param name="screenshotPath">The file path to the screenshot to be uploaded, illustrating the test result.</param>
        public void UpdateTestLog(int result, string Comment, string screenshotPath)
        {
            resultKey = result;
            comment = Comment;
            Console.WriteLine(Validatefile(screenshotPath));
            ConnectToVansahRest("UpdateTestLog");
        }

        /// <summary>
        /// Updates a test log with a new result and comment. This variant allows specifying the result as a string.
        /// </summary>
        /// <param name="result">The result of the test step as a string (e.g., "PASS", "FAIL").</param>
        /// <param name="Comment">The updated comment or description of the test result.</param>
        public void UpdateTestLog(string result, string Comment)
        {
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            comment = Comment;
            uploadScreenshot = false;
            ConnectToVansahRest("UpdateTestLog");
        }

        /// <summary>
        /// Updates a test log with a new result and comment, including a path to a screenshot file. This variant allows specifying the result as a string.
        /// </summary>
        /// <param name="result">The result of the test step as a string.</param>
        /// <param name="Comment">The updated comment or description of the test result.</param>
        /// <param name="screenshotPath">The file path to the screenshot to be uploaded.</param>
        public void UpdateTestLog(string result, string Comment, string screenshotPath)
        {
            resultKey = resultAsName.GetValueOrDefault(result.ToUpper(), 0);
            comment = Comment;
            Console.WriteLine(Validatefile(screenshotPath));
            ConnectToVansahRest("UpdateTestLog");
        }
        /// <summary>
        /// Connects to the Vansah REST API to perform various operations such as adding, removing, and updating test runs and logs.
        /// This method dynamically constructs the request based on the specified type and sends it to the Vansah API.
        /// </summary>
        /// <param name="type">The type of operation to perform, which determines the endpoint to be called and the request body to be sent.</param>
        /// <remarks>
        /// This method handles the construction of HTTP requests, including setting headers, building the request body, and handling responses.
        /// Supported types include "AddTestRunFromJiraIssue", "AddTestRunFromTestFolder", "AddTestLog", "AddQuickTestFromJiraIssue",
        /// "AddQuickTestFromTestFolders", "RemoveTestRun", "RemoveTestLog", and "UpdateTestLog". Depending on the operation,
        /// additional properties such as test run and log identifiers, result codes, comments, and screenshot paths might be utilized.
        /// </remarks>

        private void ConnectToVansahRest(string type)
        {

            if (updateVansah == "1")
            {
                httpClient = new HttpClient();
                HttpResponseMessage response = null;
                JsonObject requestBody;
                HttpContent Content;

                //Adding headers
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("Authorization", vansahToken);
                if (uploadScreenshot)
                {
                    base64FilefromUser = ConvertImageToBase64(file);
                }
                if (type == "AddTestRunFromJiraIssue")
                {

                    requestBody = new();
                    requestBody.Add("case", TestCase());
                    requestBody.Add("asset", JiraIssueAsset());
                    if (Properties().Count != 0) { requestBody.Add("properties", Properties()); }

                    httpClient.BaseAddress = new Uri(add_Test_Run);

                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);
                    response = httpClient.PostAsync("", Content).Result;

                }
                if (type == "AddTestRunFromTestFolder")
                {
                    requestBody = new();
                    requestBody.Add("case", TestCase());
                    requestBody.Add("asset", TestFolderAsset());
                    if (Properties().Count != 0) { requestBody.Add("properties", Properties()); }

                    //Console.WriteLine(requestBody);
                    httpClient.BaseAddress = new Uri(add_Test_Run);

                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);
                    response = httpClient.PostAsync("", Content).Result;


                }
                if (type == "AddTestLog")
                {
                    requestBody = AddTestLogProp();
                    if (uploadScreenshot)
                    {
                        JsonArray array = new();
                        array.Add(AddAttachment(FileName()));

                        requestBody.Add("attachments", array);
                    }
                    Console.WriteLine(requestBody.ToJsonString());
                    httpClient.BaseAddress = new Uri(add_Test_Log);

                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);
                    response = httpClient.PostAsync("", Content).Result;

                }
                if (type == "AddQuickTestFromJiraIssue")
                {

                    requestBody = new();
                    requestBody.Add("case", TestCase());
                    requestBody.Add("asset", JiraIssueAsset());
                    if (Properties().Count != 0)
                    {
                        requestBody.Add("properties", Properties());
                    }
                    requestBody.Add("result", resultObj(resultKey));
                    if (uploadScreenshot)
                    {
                        JsonArray array = new();
                        array.Add(AddAttachment(FileName()));

                        requestBody.Add("attachments", array);
                    }

                    httpClient.BaseAddress = new Uri(add_Test_Run);

                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);

                    response = httpClient.PostAsync("", Content).Result;

                }
                if (type == "AddQuickTestFromTestFolders")
                {
                    requestBody = new();
                    requestBody.Add("case", TestCase());
                    requestBody.Add("asset", TestFolderAsset());
                    if (Properties().Count != 0)
                    {
                        requestBody.Add("properties", Properties());
                    }
                    requestBody.Add("result", resultObj(resultKey));
                    if (uploadScreenshot)
                    {
                        JsonArray array = new();
                        array.Add(AddAttachment(FileName()));

                        requestBody.Add("attachments", array);
                    }

                    httpClient.BaseAddress = new Uri(add_Test_Run);

                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);
                    response = httpClient.PostAsync("", Content).Result;

                }
                if (type == "RemoveTestRun")
                {

                    httpClient.BaseAddress = new Uri(remove_Test_Run + test_Run_Identifier);
                    response = httpClient.DeleteAsync("").Result;
                }
                if (type == "RemoveTestLog")
                {

                    httpClient.BaseAddress = new Uri(remove_Test_Log + test_Log_Identifier);
                    response = httpClient.DeleteAsync("").Result;
                }
                if (type == "UpdateTestLog")
                {
                    requestBody = new();

                    requestBody.Add("result", resultObj(resultKey));
                    requestBody.Add("actualResult", comment);
                    if (uploadScreenshot)
                    {
                        JsonArray array = new();
                        array.Add(AddAttachment(FileName()));

                        requestBody.Add("attachments", array);
                    }
                    httpClient.BaseAddress = new Uri(update_Test_Log + test_Log_Identifier);
                    Content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json" /* or "application/json" in older versions */);
                    response = httpClient.PutAsync("", Content).Result;
                }
                if (response.IsSuccessStatusCode)
                {

                    var responseMessage = response.Content.ReadAsStringAsync().Result;
                    var obj = JObject.Parse(responseMessage);

                    if (type == "AddTestRunFromJiraIssue")
                    {

                        test_Run_Identifier = obj.SelectToken("data.run.identifier").ToString();
                        Console.WriteLine($"Test Run has been created Successfully RUN ID : {test_Run_Identifier}");

                    }
                    if (type == "AddTestRunFromTestFolder")
                    {
                        test_Run_Identifier = obj.SelectToken("data.run.identifier").ToString();
                        Console.WriteLine($"Test Run has been created Successfully RUN ID : {test_Run_Identifier}");
                    }
                    if (type == "AddTestLog")
                    {
                        test_Log_Identifier = obj.SelectToken("data.log.identifier").ToString();
                        Console.WriteLine($"Test Log has been Added to a test Step Successfully LOG ID : {test_Log_Identifier}");

                    }
                    if (type == "AddQuickTestFromJiraIssue")
                    {
                        test_Run_Identifier = obj.SelectToken("data.run.identifier").ToString();
                        string message = obj.SelectToken("message").ToString();
                        Console.WriteLine($"Quick Test : {message}");

                    }
                    if (type == "AddQuickTestFromTestFolders")
                    {
                        test_Run_Identifier = obj.SelectToken("data.run.identifier").ToString();
                        string message = obj.SelectToken("message").ToString();
                        Console.WriteLine($"Quick Test : {message}");

                    }
                    if (type == "RemoveTestLog")
                    {
                        Console.WriteLine($"Test Log has been removed from a test Step Successfully LOG ID : {test_Log_Identifier}");
                    }
                    if (type == "RemoveTestRun")
                    {
                        Console.WriteLine($"Test Run has been removed Successfully for the testCase : {caseKey} RUN ID : {test_Run_Identifier}");

                    }
                    if (type == "UpdateTestLog")
                    {
                        Console.WriteLine($"Test Log has been updated Successfully LOG ID : {test_Log_Identifier}");
                    }
                    response.Dispose();

                }
                else
                {
                    var responseMessage = response.Content.ReadAsStringAsync().Result;
                    var obj = JObject.Parse(responseMessage);
                    Console.WriteLine(obj.SelectToken("message").ToString());
                    response.Dispose();
                }

            }
            else
            {
                Console.WriteLine("Sending Test Results to Vansah TM for JIRA is Disabled");
            }
        }

        //JsonObject - Test Run Properties 
        private JsonObject Properties()
        {
            JsonObject environment = new();
            environment.Add("name", environment_Name);

            JsonObject release = new();
            release.Add("name", release_Name);

            JsonObject sprint = new();
            sprint.Add("name", SprintName);

            JsonObject Properties = new();
            if (SprintName != null)
            {
                if (SprintName.Length >= 2)
                {
                    Properties.Add("sprint", sprint);
                }
            }
            if (release_Name != null)
            {
                if (release_Name.Length >= 2)
                {
                    Properties.Add("release", release);
                }
            }
            if (environment_Name != null)
            {
                if (environment_Name.Length >= 2)
                {
                    Properties.Add("environment", environment);
                }
            }

            return Properties;
        }


        //JsonObject - To Add TestCase Key
        private JsonObject TestCase()
        {

            JsonObject testCase = new();
            if (caseKey != null)
            {
                if (caseKey.Length >= 2)
                {
                    testCase.Add("key", caseKey);
                }
            }
            else
            {
                Console.WriteLine("Please Provide Valid TestCase Key");
            }

            return testCase;
        }
        //JsonObject - To Add Result ID
        private JsonObject resultObj(int result)
        {

            JsonObject resultID = new();

            resultID.Add("id", result);


            return resultID;
        }
        //JsonObject - To Add JIRA Issue name
        private JsonObject JiraIssueAsset()
        {

            JsonObject asset = new();
            if (JiraIssueKey != null)
            {
                if (JiraIssueKey.Length >= 2)
                {
                    asset.Add("type", "issue");
                    asset.Add("key", JiraIssueKey);
                }
            }
            else
            {
                Console.WriteLine("Please Provide Valid JIRA Issue Key");
            }


            return asset;
        }
        //JsonObject - To Add TestFolder ID 
        private JsonObject TestFolderAsset()
        {

            JsonObject asset = new();
            if (TestFolderID != null)
            {
                if (TestFolderID.Length >= 2)
                {
                    asset.Add("type", "folder");
                    asset.Add("identifier", TestFolderID);
                }
            }
            else
            {
                Console.WriteLine("Please Provide Valid TestFolder ID");
            }


            return asset;
        }

        //JsonObject - To AddTestLog
        private JsonObject AddTestLogProp()
        {

            JsonObject testRun = new();
            testRun.Add("identifier", test_Run_Identifier);

            JsonObject stepNumber = new();
            stepNumber.Add("number", step_Order);

            JsonObject testResult = new();
            testResult.Add("id", resultKey);

            JsonObject testLogProp = new();

            testLogProp.Add("run", testRun);

            testLogProp.Add("step", stepNumber);

            testLogProp.Add("result", testResult);

            testLogProp.Add("actualResult", comment);


            return testLogProp;
        }
        //JsonObject - To Add Add Attachments to a Test Log
        private JsonObject AddAttachment(string[] file)
        {

            JsonObject attachmentsInfo = new();
            attachmentsInfo.Add("name", file[0]);
            attachmentsInfo.Add("extension", file[1]);
            attachmentsInfo.Add("file", base64FilefromUser);

            return attachmentsInfo;

        }

        private string[] FileName()
        {   
            string fileName = Path.GetFileNameWithoutExtension(file);

            string fileExtension = Path.GetExtension(file);
            string[] file_Name = { fileName, fileExtension };
            return file_Name;
        }

        private string Validatefile(String filePath)
        {
            bool ispresent = File.Exists(filePath);

            if (ispresent)
            {

                uploadScreenshot = true;
                file = filePath;
                return "Screenshot file is getting uploaded";

            }

            return "Provided Screenshot File cannot be located \nPlease provide correct filePath";
        }

        private string ConvertImageToBase64(string imagePath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return null;
            }
        }


    }
}
