using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Edge;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using UITestApp.Controls;
using UITestApp.Functions;
using UITestApp.Libraries;
using UITestApp.Utilities;

namespace UITestApp
{
    /// <summary>
    /// Main window of application
    /// </summary>
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Initialize();
        }
        #region PRIVATE MEMBERS
        
        private DataTable dtSteps = new DataTable();
        private List<string> allActions = new List<string>();
        private string scriptPath = "";
        
        private bool isClicked = false;
        
        
        #endregion

        #region ARRAY MEMBERS FOR PARALLEL EXECUTION
        //Arrays created to separate variables by thread
        private Stopwatch[] executionWatch = new Stopwatch[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        private string[] currentBrowser = new string[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        private string[] currentScriptName = new string[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        private bool[] retainBrowser = new bool[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        private UtilityScreens.StatusLogForm[] statusLogForm = new UtilityScreens.StatusLogForm[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        private BackgroundWorker[] parallelWorkers = new BackgroundWorker[TOTAL_THREADS_FOR_PARALLEL_EXECUTION]; // reserve first thread for dedicated runs
        private DataTable[] stepTables = new DataTable[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        #endregion

        #region CONSTANTS
        private const string URL = "https://www.demo.bnz.co.nz/client/";
        private const string defaultItemString = "--SELECT--";
        private const int TOTAL_THREADS_FOR_PARALLEL_EXECUTION = 6;
        #endregion

        #region PRIVATE METHODS

        /// <summary>
        /// Runs initial code needed by the tool
        /// </summary>
        private void Initialize()
        {
            InitializeControls();
            InitializeStepGrid();
            InitializeScreens();
            scriptPath = new FileInfo(CommonFunctions.GetAssemblyPath()).FullName + "\\Scripts\\";
            PopulateScriptList(scriptPath);
            PrepareArrayVariablesForParallelExecution();
        }

        /// <summary>
        /// Instantiates all parallel variables to be used by threads
        /// </summary>
        private void PrepareArrayVariablesForParallelExecution()
        {
            for (int count = 0; count < TOTAL_THREADS_FOR_PARALLEL_EXECUTION; count++)
            {
                parallelWorkers[count] = new BackgroundWorker();
                ResultLibrary.allResults[count] = new List<BaseResult>();
                executionWatch[count] = new Stopwatch();
                currentBrowser[count] = "";
                currentScriptName[count] = "";
                retainBrowser[count] = false;
                stepTables[count] = new DataTable();
            }

        }

        /// <summary>
        /// Creates datagridview in code for better customization
        /// </summary>
        private void InitializeStepGrid()
        {
            cmbBrowser.SelectedIndex = 0;
            DataGridViewComboBoxColumn cmbScreen = new DataGridViewComboBoxColumn();
            cmbScreen.HeaderText = "Screen";
            cmbScreen.Name = "cmbScreen";
            dgvSteps.Columns.Add(cmbScreen);
            DataGridViewComboBoxColumn cmbControl = new DataGridViewComboBoxColumn();
            cmbControl.HeaderText = "Control";
            cmbControl.Name = "cmbControl";
            dgvSteps.Columns.Add(cmbControl);
            DataGridViewComboBoxColumn cmbAction = new DataGridViewComboBoxColumn();
            cmbAction.HeaderText = "Action";
            cmbAction.Name = "cmbAction";
            dgvSteps.Columns.Add(cmbAction);
            DataGridViewTextBoxColumn txtParameters = new DataGridViewTextBoxColumn();
            txtParameters.HeaderText = "Parameters";
            txtParameters.Name = "txtParameters";
            txtParameters.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvSteps.Columns.Add(txtParameters);
            dtSteps.Clear();
            dgvSteps.DataSource = dtSteps;
        }

        /// <summary>
        /// Compiles all control records from XML files to be used by scripts.
        /// The XMLs contain info on locating controls (XPATH strings, IDs, control types, etc.)
        /// </summary>
        private void InitializeControls()
        {
            ControlLibrary.allControls = new List<UIBaseControl>();
            string controlRecordsPath = new FileInfo(CommonFunctions.GetAssemblyPath()).FullName + "\\ControlRecords";
            DirectoryInfo dirInfo = new DirectoryInfo(controlRecordsPath);
            FileInfo[] fileInfos = dirInfo.GetFiles("*.xml", SearchOption.AllDirectories);
            foreach (FileInfo file in fileInfos)
            {
                try
                {
                    ControlLibrary.AddControlRecordToLibrary(file.FullName);
                }
                catch (Exception ex)
                {
                    CommonFunctions.SaveErrorToFile("", ex.InnerException);
                    continue;
                }
            }
        }

        /// <summary>
        /// Adds all possible screens to the Screen dropdowns in the step grid
        /// </summary>
        private void InitializeScreens()
        {
            DataGridViewComboBoxColumn cmb = dgvSteps.Columns[0] as DataGridViewComboBoxColumn;
            cmb.Items.Clear();
            cmb.Items.Add(defaultItemString);
            foreach (string name in ControlLibrary.GetAllScreens())
            {
                if (!cmb.Items.Contains(name))
                {
                    cmb.Items.Add(name);
                }
            }
        }

        /// <summary>
        /// Adds all possible controls to the Control dropdown based on selected screen
        /// </summary>
        private void PopulateControls(string screen, int rowIndex)
        {
            DataGridViewComboBoxCell cmbCell = dgvSteps.Rows[rowIndex].Cells[1] as DataGridViewComboBoxCell;
            cmbCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cmbCell.ReadOnly = false;
            cmbCell.Items.Clear();
            cmbCell.Items.Add(defaultItemString);
            foreach (string name in ControlLibrary.GetControlsFromScreens(screen))
            {
                if (!cmbCell.Items.Contains(name))
                {
                    cmbCell.Items.Add(name);
                }
            }
            cmbCell.Value = cmbCell.Items[0];
            ResizeDropdownWidth(cmbCell);
        }

        /// <summary>
        /// Adds all possible actions to the Action dropdown based on selected control
        /// </summary>
        private void PopulateActions(string screenName, string controlName, int rowIndex)
        {
            DataGridViewComboBoxCell cmbCell = dgvSteps.Rows[rowIndex].Cells[2] as DataGridViewComboBoxCell;
            cmbCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cmbCell.ReadOnly = false;
            cmbCell.Items.Clear();
            cmbCell.Items.Add(defaultItemString);
            foreach (string name in ControlLibrary.GetActionsFromControlType(screenName, controlName))
            {
                if (!cmbCell.Items.Contains(name))
                {
                    cmbCell.Items.Add(name);
                }
            }
            cmbCell.Value = cmbCell.Items[0];
            ResizeDropdownWidth(cmbCell);
        }

        /// <summary>
        /// Checks if any thread for parallel execution is running.
        /// First thread is reserved for dedicated runs
        /// </summary>
        private bool IsAnyParallelTestRunning(bool includeFirstThread = false)
        {
            return includeFirstThread ? parallelWorkers.Any(x => x.IsBusy) : parallelWorkers.Skip(1).Any(x => x.IsBusy);
        }

        /// <summary>
        /// Checks if all threads have running tests
        /// </summary>
        private bool AreAllParallelThreadsInUse(bool includeFirstThread = false)
        {
            return includeFirstThread ? parallelWorkers.All(x => x.IsBusy) : parallelWorkers.Skip(1).All(x => x.IsBusy);
        }

        private int GetAvailableThreadIndex()
        {
            for (var threadCount = 1; threadCount < TOTAL_THREADS_FOR_PARALLEL_EXECUTION; threadCount++)
            {
                if (!parallelWorkers[threadCount].IsBusy)
                {
                    return threadCount;
                }
            }
            return -1;
        }

        /// <summary>
        /// Initializes WebDriver using chosen browser, then navigates to the URL
        /// </summary>
        private void InitializeEnvironment(int threadIndex)
        {
            try
            {
                CommonFunctions.LogMessage(threadIndex, "Initializing " + currentBrowser[threadIndex] + " automation environment: ", true, true);
                switch (currentBrowser[threadIndex])
                {
                    case "Chrome":
                        CreateChromeEnvironment(threadIndex);
                        break;
                    case "Firefox":
                        CreateFirefoxEnvironment(threadIndex);
                        break;
                    case "Internet Explorer":
                        CreateIEEnvironment(threadIndex);
                        break;
                    case "Edge":
                        CreateEdgeEnvironment(threadIndex);
                        break;
                    default:
                        break;
                }
                CommonFunctions.LogMessage(threadIndex, "Automation environment established");
                CommonFunctions.LogMessage(threadIndex, "Generating " + currentBrowser[threadIndex] + " browser and loading page URL...", true, true);
                MainLibrary.AutoDriver[threadIndex].Navigate().GoToUrl(URL);
                CommonFunctions.LogMessage(threadIndex, currentBrowser[threadIndex] + " browser loaded");
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Setup step completed");
            }
            catch (Exception ex)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Setup failed", ex.Message, "");
            }
            finally
            {
                CommonFunctions.SaveResult(threadIndex, new BaseStep(0, "SETUP", "", "", "Browser for execution: " + currentBrowser[threadIndex]));
            }
        }

        /// <summary>
        /// Creates Chrome WebDriver using downloaded driver in folder
        /// </summary>
        private void CreateChromeEnvironment(int threadIndex)
        {
            string driverPath = CommonFunctions.GetAssemblyPath();
            ChromeOptions ChromeSettings = new ChromeOptions();
            ChromeSettings.AddArgument("--test-type");
            ChromeSettings.AddArgument("--start-maximized=true");
            ChromeSettings.AcceptInsecureCertificates = true;
            MainLibrary.AutoDriver[threadIndex] = new ChromeDriver(driverPath, ChromeSettings);
        }

        /// <summary>
        /// Creates Internet Explorer WebDriver.
        /// Not in use because of indefinite loading with the URL
        /// </summary>
        private void CreateIEEnvironment(int threadIndex)
        {
            string driverPath = CommonFunctions.GetAssemblyPath();
            InternetExplorerOptions IESettings = new InternetExplorerOptions();
            IESettings.UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore;
            IESettings.EnablePersistentHover = false;
            InternetExplorerDriverService service = InternetExplorerDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            MainLibrary.AutoDriver[threadIndex] = new InternetExplorerDriver(IESettings);
        }

        /// <summary>
        /// Creates Edge WebDriver using NUGet driver
        /// </summary>
        private void CreateEdgeEnvironment(int threadIndex)
        {
            string driverPath = CommonFunctions.GetAssemblyPath();
            EdgeOptions EdgeSettings = new EdgeOptions();
            EdgeSettings.UnhandledPromptBehavior = UnhandledPromptBehavior.Ignore;
            EdgeSettings.AcceptInsecureCertificates = true;
            EdgeDriverService service = EdgeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            MainLibrary.AutoDriver[threadIndex] = new EdgeDriver(service, EdgeSettings);
            MainLibrary.AutoDriver[threadIndex].Manage().Window.Maximize();
        }

        /// <summary>
        /// Creates Firefox WebDriver using downloaded driver in folder
        /// </summary>
        private void CreateFirefoxEnvironment(int threadIndex)
        {
            string driverPath = CommonFunctions.GetAssemblyPath();
            FirefoxOptions FirefoxSettings = new FirefoxOptions();
            FirefoxSettings.BrowserExecutableLocation = GetFirefoxPathFromRegistry();
            FirefoxSettings.AcceptInsecureCertificates = true;
            FirefoxDriverService FirefoxService = FirefoxDriverService.CreateDefaultService(driverPath, "geckodriver.exe");
            FirefoxService.HideCommandPromptWindow = true;
            MainLibrary.AutoDriver[threadIndex] = new FirefoxDriver(FirefoxService, FirefoxSettings);
        }

        /// <summary>
        /// Gets Firefox browser path to be used by WebDriver
        /// </summary>
        private string GetFirefoxPathFromRegistry()
        {
            string firefoxPath = "";
            string regPath = @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet";
            RegistryKey installedBrowsers = Registry.LocalMachine.OpenSubKey(regPath);
            string[] browserStrings = installedBrowsers.GetSubKeyNames();
            var selectedBrowser = browserStrings.FirstOrDefault(x => installedBrowsers.OpenSubKey(x).GetValue(null).ToString().ToLower().Contains("firefox"));
            if (!string.IsNullOrEmpty(selectedBrowser))
            {
                RegistryKey browserKey = installedBrowsers.OpenSubKey(selectedBrowser);
                RegistryKey browserPath = browserKey.OpenSubKey(@"shell\open\command");
                firefoxPath = (string)browserPath.GetValue(null).ToString();
                firefoxPath = firefoxPath.Substring(1, firefoxPath.Length - 2);
            }
            return firefoxPath;
        }

        /// <summary>
        /// Populates treeview with scripts
        /// </summary>
        private void PopulateScriptList(string path)
        {
            tvwScripts.Nodes.Clear();
            var rootDirectory = new DirectoryInfo(path);
            if (!Directory.Exists(path))
            {
                MessageBox.Show("The Scripts folder is missing. An empty folder will be created.", "Script folder missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Directory.CreateDirectory(path);
            }
            foreach (var directory in rootDirectory.GetDirectories())
            {
                var stack = new Stack<TreeNode>();
                var node = new TreeNode(directory.Name) { Tag = directory };
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();
                    var directoryInfo = (DirectoryInfo)currentNode.Tag;
                    foreach (var dir in directoryInfo.GetDirectories())
                    {
                        var childDirectoryNode = new TreeNode(dir.Name) { Tag = dir };
                        currentNode.Nodes.Add(childDirectoryNode);
                        stack.Push(childDirectoryNode);
                    }
                    foreach (var file in directoryInfo.GetFiles())
                    {
                        if (file.Extension.ToLower() == ".xml")
                        {
                            currentNode.Nodes.Add(new TreeNode(file.Name));
                        }
                    }
                }
                tvwScripts.Nodes.Add(node);
            }
            foreach (var file in rootDirectory.GetFiles())
                if (file.Extension.ToLower() == ".xml")
                {
                    tvwScripts.Nodes.Add(new TreeNode(file.Name));
                }
        }

        /// <summary>
        /// Clears all step grid rows
        /// </summary>
        private void ClearStepGrid()
        {
            DataTable stepTable = (DataTable)dgvSteps.DataSource;
            if (stepTable != null)
            {
                stepTable.Clear();
            }
        }

        /// <summary>
        /// Checks validity of all data in grid - all must be valid steps in order to be used for execution
        /// </summary>
        private bool IsAnyStepInvalid()
        {
            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                if (row.Cells[0].Value.ToString() == defaultItemString || row.Cells[1].Value.ToString() == defaultItemString || row.Cells[2].Value.ToString() == defaultItemString)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Resizes dropdown list to fit all possible items
        /// </summary>
        private void ResizeDropdownWidth(DataGridViewComboBoxCell comboBox)
        {
            int maxWidth = 0, temporaryWidth = 0;
            foreach (var obj in comboBox.Items)
            {
                temporaryWidth = TextRenderer.MeasureText(obj.ToString(), comboBox.Style.Font).Width;
                if (temporaryWidth > maxWidth)
                {
                    maxWidth = temporaryWidth;
                }
            }
            comboBox.DropDownWidth = maxWidth;
        }

        /// <summary>
        /// Initializes main thread code, including test execution
        /// </summary>
        private void ExecuteSteps(int threadIndex)
        {
            parallelWorkers[threadIndex] = new BackgroundWorker();
            int stepCount = 0;
            int totalStepCount = dgvSteps.Rows.Count;
            statusLogForm[threadIndex].StatusText = "Step " + stepCount + "/" + totalStepCount;
            parallelWorkers[threadIndex].WorkerReportsProgress = true;
            parallelWorkers[threadIndex].WorkerSupportsCancellation = true;

            parallelWorkers[threadIndex].DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;
                CommonFunctions.LogMessage(threadIndex, "Execution running on thread " + threadIndex+1);
                InitializeEnvironment(threadIndex);
                b.ReportProgress(0);
                foreach (DataRow row in stepTables[threadIndex].Rows)
                {
                    string screen = row[0].ToString();
                    string control = row[1].ToString();
                    string action = row[2].ToString();
                    string parameters = row[3].ToString();
                    stepCount++;
                    BaseStep step = new BaseStep(stepCount, screen, control, action, parameters);
                    try
                    {
                        if (CommonFunctions.StopExecution(threadIndex))
                        {
                            CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Skipped, "Action skipped because of " + (CommonFunctions.ManualExecutionStop[threadIndex] ? " manual cancellation" : " a previous step's failure"));
                            CommonFunctions.SaveResult(threadIndex, step);
                            continue;
                        }
                        CommonFunctions.LogMessage(threadIndex, "Executing action " + stepCount + ":", true);
                        CommonFunctions.LogMessage(threadIndex, "Action to be executed in screen " + screen);
                        CommonFunctions.LogMessage(threadIndex, "Action to be performed is " + action);
                        CommonFunctions.LogMessage(threadIndex, "Action to target " + control + " control with " + (CommonFunctions.IsActionParameterless(action) ? "no parameters" : "parameters \"" + parameters + "\""));
                        if (CommonFunctions.StopExecution(threadIndex))
                        {

                            CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Skipped, "Action skipped because of " + (CommonFunctions.ManualExecutionStop[threadIndex] ? " manual cancellation" : " a previous step's failure"));
                            CommonFunctions.SaveResult(threadIndex, step);
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (ResultLibrary.allResults[threadIndex].Count == 1)
                        {
                            BaseResult result = ResultLibrary.allResults[threadIndex].First();
                            result.StepExecutionResult = ResultLibrary.StepResult.Failed;
                            result.StepError = e.Message;
                            result.StepErrorDetails = "";
                        }
                        else if (ResultLibrary.allResults[threadIndex].Any(x => x.Step.StepNumber == stepCount))
                        {
                            BaseResult result = ResultLibrary.allResults[threadIndex].FirstOrDefault(x => x.Step.StepNumber == stepCount);
                            result.StepExecutionResult = ResultLibrary.StepResult.Failed;
                            result.StepError = e.Message;
                            result.StepErrorDetails = "";
                        }
                    }
                    CommonFunctions.InterpretStep(threadIndex, step);
                    b.ReportProgress(stepCount / totalStepCount * 100);
                }
                if (ResultLibrary.allResults[threadIndex].Count - 1 < stepTables[threadIndex].Rows.Count)
                {
                    try
                    {
                        for (int count = ResultLibrary.allResults[threadIndex].Count - 1; count < dgvSteps.Rows.Count; count++)
                        {
                            DataRow row = stepTables[threadIndex].Rows[count];
                            string screen = row[0].ToString();
                            string control = row[1].ToString();
                            string action = row[2].ToString();
                            string parameters = row[3].ToString();
                            BaseStep step = new BaseStep(count, screen, control, action, parameters);
                            CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Skipped, "Action skipped because of " + (CommonFunctions.ManualExecutionStop[threadIndex] ? " manual cancellation" : " a previous step's failure"));
                            CommonFunctions.SaveResult(threadIndex, step);
                        }
                    }
                    catch (Exception e)
                    {
                        CommonFunctions.LogMessage(threadIndex, "An error has been encountered while compiling skipped rows. Error message follows", true);
                        CommonFunctions.LogMessage(threadIndex, e.Message);
                    }
                }
                if (!retainBrowser[threadIndex])
                {
                    CommonFunctions.LogMessage(threadIndex, "Closing all automation browser instances...", true, true);
                    MainLibrary.AutoDriver[threadIndex].Quit();
                    CommonFunctions.LogMessage(threadIndex, "All automation browsers closed");
                }
                executionWatch[threadIndex].Stop();
                if (CommonFunctions.ManualExecutionStop[threadIndex])
                {
                    CommonFunctions.LogMessage(threadIndex, "Execution manually cancelled", true, false, CommonFunctions.LogMessageType.Warning);
                }
                int totalStepsExecuted = ResultLibrary.allResults[threadIndex].FindAll(x => x.Step.StepNumber != 0 && x.StepExecutionResult == ResultLibrary.StepResult.Passed).Count();
                CommonFunctions.LogMessage(threadIndex, "Execution " + (CommonFunctions.StopExecution(threadIndex) ? "interrupted " : "completed ") + ": " + totalStepsExecuted + "/" + totalStepCount + " steps executed", true);
                CommonFunctions.LogMessage(threadIndex, "Script execution stopped after " + executionWatch[threadIndex].Elapsed.ToString(@"m\:ss\.fff"));
                b.ReportProgress(100);
            });

            parallelWorkers[threadIndex].ProgressChanged += new ProgressChangedEventHandler(
            delegate (object o, ProgressChangedEventArgs args)
            {
                statusLogForm[threadIndex].StatusText = "Step " + stepCount + "/" + totalStepCount;
            });

            parallelWorkers[threadIndex].RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if (args.Error != null)
                {
                    CommonFunctions.SaveErrorToFile(args.Error.StackTrace, args.Error);
                    CommonFunctions.LogMessage(threadIndex, "An unhandled exception has been encountered in the execution thread. Please send the following message to your administrator for diagnosis.", true, false, CommonFunctions.LogMessageType.Error);
                    CommonFunctions.LogMessage(threadIndex, args.Error.Message, false, false, CommonFunctions.LogMessageType.Error);
                }
                statusLogForm[threadIndex].ChangeStatusFormButtonStates(true);
                UtilityScreens.ResultsForm resultsForm = new UtilityScreens.ResultsForm(threadIndex);
                resultsForm.ChangeScriptNameLabel(currentScriptName[threadIndex]);
                resultsForm.ChangeBrowserLabel(currentBrowser[threadIndex]);
                resultsForm.Show();
                CommonFunctions.ManualExecutionStop[threadIndex] = false;
                CommonFunctions.HasPendingLogText[threadIndex] = false;
                if (threadIndex == 0)
                {
                    btnRun.Enabled = true;
                }
                parallelWorkers[threadIndex].Dispose();
            });
            parallelWorkers[threadIndex].RunWorkerAsync();
        }

        /// <summary>
        /// Kills all possible WebDriver instances - skips if any parallel test is running
        /// </summary>
        private void KillDriverProcesses(String[] DriversToKill, int threadIndex)
        {
            if (IsAnyParallelTestRunning(true))
            {
                CommonFunctions.LogMessage(threadIndex, "At least 1 thread is busy - aborting driver process termination", false);
                return;
            }
            int killCount = 0;
            foreach (string ProcName in DriversToKill)
            {
                try
                {
                    List<Process> processes = new List<Process>(Process.GetProcesses());
                    foreach (Process process in processes.FindAll(x => x.ProcessName == ProcName))
                    {
                        process.Kill();
                        CommonFunctions.LogMessage(threadIndex, "Killed " + ProcName + " driver instance", true);
                        killCount++;
                    }
                }
                catch
                {
                    CommonFunctions.LogMessage(threadIndex, "Couldn't kill " + ProcName + " driver instance", true, false, CommonFunctions.LogMessageType.Error);
                }

            }
            CommonFunctions.LogMessage(threadIndex, "A total of " + killCount + " driver processes terminated", true);
        }

        /// <summary>
        /// Starts parallel execution
        /// </summary>
        private void StartParallelRun(bool isDedicated)
        {
            if (cmbBrowser.SelectedIndex == 0)
            {
                MessageBox.Show("Please select a browser environment.", "Invalid browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (dgvSteps.Rows.Count == 0)
            {
                MessageBox.Show("The step grid is empty. Please add at least 1 step for it to be a valid script.", "Step grid empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (IsAnyStepInvalid())
            {
                MessageBox.Show("One of the steps has an invalid value. Please remove all instances of \"--SELECT--\" in all step dropdowns.", "Invalid step value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int threadIndex = isDedicated ? 0 : GetAvailableThreadIndex();
            if (threadIndex < 0)
            {
                MessageBox.Show("All parallel threads are currently in use. You can run on the dedicated thread if available, or wait until one of the tests are finished", "All threads in use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (isDedicated)
            {
                btnRun.Enabled = false;
            }
            stepTables[threadIndex] = new DataTable();
            foreach (DataGridViewColumn col in dgvSteps.Columns)
            {
                stepTables[threadIndex].Columns.Add(col.Name);
            }

            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                DataRow dataRow = stepTables[threadIndex].NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    dataRow[cell.ColumnIndex] = cell.Value;
                }
                stepTables[threadIndex].Rows.Add(dataRow);
            }
            retainBrowser[threadIndex] = chkRetainBrowser.Checked;
            ResultLibrary.allResults[threadIndex].Clear();
            CommonFunctions.currentResult[threadIndex] = null;
            currentBrowser[threadIndex] = cmbBrowser.Text;
            currentScriptName[threadIndex] = txtScriptName.Text;
            statusLogForm[threadIndex] = new UtilityScreens.StatusLogForm(threadIndex);
            statusLogForm[threadIndex].ChangeScriptNameLabel(currentScriptName[threadIndex]);
            statusLogForm[threadIndex].ChangeStatusFormButtonStates(false);
            CommonFunctions.StatusForm[threadIndex] = statusLogForm[threadIndex];
            statusLogForm[threadIndex].Show();
            executionWatch[threadIndex].Restart();
            KillDriverProcesses(new string[] { "IEDriverServer", "chromedriver", "geckodriver", "msedgedriver" }, threadIndex);
            ExecuteSteps(threadIndex);
        }
        #endregion

        #region EVENTS
        private void btnRun_Click(object sender, EventArgs e)
        {
            StartParallelRun(true);
        }

        private void dgvSteps_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            bool validClick = (e.RowIndex != -1 && e.ColumnIndex != -1 && isClicked);
            var datagridview = sender as DataGridView;
            bool isCellReadOnly = dgvSteps.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly;
            if (datagridview.Columns[e.ColumnIndex] is DataGridViewComboBoxColumn && validClick && !isCellReadOnly)
            {
                datagridview.BeginEdit(true);
                ((ComboBox)datagridview.EditingControl).DroppedDown = true;
            }
            isClicked = false;
        }


        private void dgvSteps_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvSteps.IsCurrentCellDirty)
            {
                dgvSteps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvSteps_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Skip logic if edited column is a parameter column
            if (e.ColumnIndex == 3)
            {
                return;
            }
            DataGridViewComboBoxCell cbEditedCell = (DataGridViewComboBoxCell)dgvSteps.Rows[e.RowIndex].Cells[e.ColumnIndex];
            DataGridViewComboBoxCell cbActionCell = (DataGridViewComboBoxCell)dgvSteps.Rows[e.RowIndex].Cells[2];
            DataGridViewTextBoxCell txtParamCell = (DataGridViewTextBoxCell)dgvSteps.Rows[e.RowIndex].Cells[3];
            if (cbEditedCell.Value != null)
            {
                string newValue = cbEditedCell.Value.ToString();
                if (newValue == defaultItemString)
                {
                    return;
                }
                string selectedScreen = dgvSteps.Rows[e.RowIndex].Cells[0].Value.ToString();
                switch (e.ColumnIndex)
                {
                    case 0:
                        cbActionCell.Value = cbActionCell.Items.Count > 0 ? cbActionCell.Items[0] : null;
                        cbActionCell.ReadOnly = true;
                        PopulateControls(selectedScreen, e.RowIndex);
                        break;
                    case 1:
                        string selectedControl = dgvSteps.Rows[e.RowIndex].Cells[1].Value.ToString();
                        PopulateActions(selectedScreen, selectedControl, e.RowIndex);
                        break;
                    case 2:
                        btnRun.Enabled = true;
                        if (CommonFunctions.IsActionParameterless(newValue))
                        {
                            txtParamCell.Value = "";
                            txtParamCell.ReadOnly = true;
                        }
                        else
                        {
                            txtParamCell.ReadOnly = false;
                        }
                        break;
                }
                dgvSteps.Invalidate();
            }
        }

        private void btnAddStep_Click(object sender, EventArgs e)
        {
            DataRow newRow = dtSteps.NewRow();
            dtSteps.Rows.Add(newRow);
            DataGridViewComboBoxCell cmbScreenCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[0] as DataGridViewComboBoxCell;
            DataGridViewComboBoxCell cmbControlCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[1] as DataGridViewComboBoxCell;
            DataGridViewComboBoxCell cmbActionCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[2] as DataGridViewComboBoxCell;
            DataGridViewTextBoxCell txtParamCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[3] as DataGridViewTextBoxCell;
            cmbControlCell.ReadOnly = true;
            cmbActionCell.ReadOnly = true;
            txtParamCell.ReadOnly = true;
            cmbControlCell.Items.Add(defaultItemString);
            cmbActionCell.Items.Add(defaultItemString);
            cmbScreenCell.Value = cmbScreenCell.Items[0];
            cmbControlCell.Value = cmbScreenCell.Items[0];
            cmbActionCell.Value = cmbScreenCell.Items[0];
        }

        private void dgvSteps_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            isClicked = true;
        }

        private void dgvSteps_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            isClicked = true;
        }

        private void dgvSteps_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dgvSteps.Rows.Count == 0)
            {
                MessageBox.Show("The step grid is empty. Please add at least 1 step for it to be a valid script.", "Step grid empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (IsAnyStepInvalid())
            {
                MessageBox.Show("One of the steps has an invalid value. Please remove all instances of \"--SELECT--\" in all step dropdowns.", "Invalid step value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string file = Directory.GetFiles(scriptPath, txtScriptName.Text + ".xml", SearchOption.AllDirectories).FirstOrDefault();
            if (file != null)
            {
                DialogResult saveMessageResult = MessageBox.Show("File already exists. Overwrite?", "Overwrite existing file?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (saveMessageResult == DialogResult.No)
                {
                    return;
                }
            }
            file = scriptPath + txtScriptName.Text + ".xml";
            List<XElement> steps = new List<XElement>();
            int stepNumber = 1;
            foreach (DataGridViewRow row in dgvSteps.Rows)
            {
                steps.Add(new XElement("step",
                    new XAttribute("stepnumber", stepNumber),
                    new XElement("screen", row.Cells[0].Value.ToString()),
                    new XElement("control", row.Cells[1].Value.ToString()),
                    new XElement("action", row.Cells[2].Value.ToString()),
                    new XElement("parameters", row.Cells[3].Value != null ? row.Cells[3].Value.ToString() : "")
                    )
                    );
                stepNumber++;
            }

            XElement scriptRecord = new XElement("script",
                    new XAttribute("scriptname", txtScriptName.Text),
                    steps
                    );
            XDocument xmlDocument = new XDocument(scriptRecord);
            XMLHelper.SaveXML(xmlDocument, file);
            PopulateScriptList(scriptPath);
            MessageBox.Show("Save successful", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (tvwScripts.SelectedNode != null)
            {
                DialogResult deleteDialogResult = MessageBox.Show($"Are you sure you want to delete {tvwScripts.SelectedNode.FullPath} ?", "Delete File", MessageBoxButtons.YesNo);
                if (deleteDialogResult == DialogResult.Yes)
                {
                    string pathToDelete = scriptPath + tvwScripts.SelectedNode.FullPath;
                    File.Delete(pathToDelete);
                    MessageBox.Show("Delete successful.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PopulateScriptList(scriptPath);
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (tvwScripts.SelectedNode != null)
            {
                ClearStepGrid();
                XDocument stepXML = XMLHelper.LoadXML(scriptPath + tvwScripts.SelectedNode.FullPath);
                var dataScript = from doc in stepXML.Descendants("script")
                                 select new
                                 {
                                     scriptname = doc.Attribute("scriptname").Value
                                 };
                txtScriptName.Text = dataScript.First().scriptname;
                var dataSteps = from doc in stepXML.Descendants("step")
                                select new
                                {
                                    stepnumber = doc.Attribute("stepnumber").Value,
                                    screen = doc.Element("screen").Value,
                                    control = doc.Element("control").Value,
                                    action = doc.Element("action").Value,
                                    parameters = doc.Element("parameters").Value
                                };

                foreach (var stepValue in dataSteps)
                {
                    int stepIndex = Convert.ToInt32(stepValue.stepnumber);
                    DataRow newRow = dtSteps.NewRow();
                    dtSteps.Rows.Add(newRow);
                    DataGridViewComboBoxCell cmbScreenCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[0] as DataGridViewComboBoxCell;
                    DataGridViewComboBoxCell cmbControlCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[1] as DataGridViewComboBoxCell;
                    DataGridViewComboBoxCell cmbActionCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[2] as DataGridViewComboBoxCell;
                    DataGridViewTextBoxCell txtParamCell = dgvSteps.Rows[dgvSteps.Rows.Count - 1].Cells[3] as DataGridViewTextBoxCell;
                    cmbControlCell.Items.Add(defaultItemString);
                    cmbActionCell.Items.Add(defaultItemString);
                    cmbScreenCell.Value = cmbScreenCell.Items[cmbScreenCell.Items.IndexOf(stepValue.screen) < 0 ? 0 : cmbScreenCell.Items.IndexOf(stepValue.screen)];
                    PopulateControls(stepValue.screen, stepIndex - 1);
                    cmbControlCell.Value = cmbControlCell.Items[cmbControlCell.Items.IndexOf(stepValue.control) < 0 ? 0 : cmbControlCell.Items.IndexOf(stepValue.control)];
                    PopulateActions(stepValue.screen, stepValue.control, stepIndex - 1);
                    cmbActionCell.Value = cmbActionCell.Items[cmbActionCell.Items.IndexOf(stepValue.action) < 0 ? 0 : cmbActionCell.Items.IndexOf(stepValue.action)];
                    txtParamCell.Value = stepValue.parameters;
                    ResizeDropdownWidth(cmbScreenCell);
                    ResizeDropdownWidth(cmbControlCell);
                    ResizeDropdownWidth(cmbActionCell);
                }
                dgvSteps.Refresh();
            }
            tvwScripts.SelectedNode = null;
            MessageBox.Show("Script load successful", "Load Script", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            txtScriptName.Clear();
            ClearStepGrid();
        }

        private void btnDeleteStep_Click(object sender, EventArgs e)
        {
            if (dgvSteps.SelectedCells.Count > 0)
            {
                dgvSteps.Rows.RemoveAt(dgvSteps.CurrentCell.RowIndex);
            }
        }

        private void btn_EnabledChanged(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.BackColor = button.Enabled == false ? Color.LightGray : Color.FromArgb(0, 85, 255);
        }

        private void btnRunParallel_Click(object sender, EventArgs e)
        {
            StartParallelRun(false);
        }
        #endregion
    }
}
