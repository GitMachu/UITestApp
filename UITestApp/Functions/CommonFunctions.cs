using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using UITestApp.Libraries;
using UITestApp.Controls;
using UITestApp.UtilityScreens;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace UITestApp.Functions
{
    /// <summary>
    /// Common methods used by other parts of the application
    /// </summary>
    public class CommonFunctions
    {
        #region ARRAY MEMBERS FOR PARALLEL EXECUTION
        public static bool[] ManualExecutionStop = new bool[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        public static bool[] HasPendingLogText = new bool[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        public static StatusLogForm[] StatusForm = new StatusLogForm[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        public static Libraries.BaseResult[] currentResult = new BaseResult[TOTAL_THREADS_FOR_PARALLEL_EXECUTION];
        #endregion

        #region CONSTANTS
        private const int TOTAL_THREADS_FOR_PARALLEL_EXECUTION = 6;
        private const int TOTAL_VERIFYELEMENTEXISTS_WAIT_TIME = 20;
        #endregion

        #region PRIVATE MEMBERS
        private static string logPath = new FileInfo(CommonFunctions.GetAssemblyPath()).FullName + "\\Logs\\";
        #endregion

        #region PUBLIC MEMBERS
        public enum LogMessageType
        {
            Info,
            Warning,
            Error,
            SuccessMessage
        }
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Finds target element based on search type and parameters in XML files,
        /// and retries looking for the control depending on elementWaitTime
        /// </summary>
        public static IWebElement SearchElement(int threadIndex, string searchType, string searchParameter, int elementWaitTime)
        {
            try
            {
                List<IWebElement> possibleElements = null;
                for (int count = 1; count <= elementWaitTime; count++)
                {
                    switch (searchType)
                    {
                        case "XPATH":
                            possibleElements = MainLibrary.AutoDriver[threadIndex].FindElements(By.XPath(searchParameter)).ToList();
                            break;
                        case "XPATH_DISPLAYED":
                            possibleElements = MainLibrary.AutoDriver[threadIndex].FindElements(By.XPath(searchParameter)).Where(x => x.Displayed).ToList();
                            break;
                        case "ID":
                            possibleElements = MainLibrary.AutoDriver[threadIndex].FindElements(By.Id(searchParameter)).ToList();
                            break;
                    }
                    if (possibleElements.Count == 0)
                    {
                        LogMessage(threadIndex, "Target control cannot be found. Time elapsed: " + count + " second/s. " + (count < elementWaitTime ? "Retrying..." : ""), true, false, LogMessageType.Warning);
                        Thread.Sleep(1000);
                    }
                    else if (possibleElements.Count > 1)
                    {
                        LogMessage(threadIndex, "Warning: More than 1 control found - target control might not be the correct control", true, false, LogMessageType.Warning);
                    }
                }
                if (possibleElements.Count == 0)
                {
                    LogMessage(threadIndex, "Target control cannot be found after " + elementWaitTime + " seconds.", true, false, LogMessageType.Error);
                    return null;
                }
                else
                {
                    return possibleElements.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Search element error", ex.Message, "");
                return null;
            }
        }

        /// <summary>
        /// Flag that signifies whether or not the execution has been interrupted,
        /// which can be triggered by script failure or manual cancellation
        /// </summary>
        public static bool StopExecution(int threadIndex)
        {
            return currentResult[threadIndex].StepExecutionResult != ResultLibrary.StepResult.Passed || ManualExecutionStop[threadIndex];
        }

        /// <summary>
        /// Returns executable location
        /// </summary>
        public static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Main method for logging messages into status log window
        /// </summary>
        public static void LogMessage(int threadIndex, string message, bool showTimeStamp = false, bool omitEscape = false, LogMessageType messageType = LogMessageType.Info)
        {
            string cancellationMessageNewLine = "";
            if (HasPendingLogText[threadIndex] && message.StartsWith("Cancelling execution"))
            {
                cancellationMessageNewLine = Environment.NewLine;
            }
            HasPendingLogText[threadIndex] = omitEscape;
            StatusLogForm.LogMessageType = messageType;
            if (StatusForm != null)
            {
                StatusForm[threadIndex].LogText = cancellationMessageNewLine + (showTimeStamp ? "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " : "") + message + (omitEscape ? "" : Environment.NewLine);
            }
        }

        /// <summary>
        /// Interprets step, then determines control type methods to call
        /// </summary>
        public static void InterpretStep(int threadIndex, BaseStep step)
        {
            UIButton baseButton;
            UINavigationMenu baseNavigationMenu;
            UIForm baseForm;
            UIComboBox baseComboBox;
            UIErrorHeader baseErrorHeader;
            UIList baseList;
            UIPanelItem basePanelItem;
            UITab baseTab;
            UIToolTip baseToolTip;
            UINotificationMessage baseNotificationMessage;
            MethodInfo methodInfo;
            string controlSearchParameters = "";
            string controlSearchType = ControlLibrary.GetSearchValues(step.StepScreen, step.StepControl, out controlSearchParameters);
            string controlType = ControlLibrary.GetControlType(step.StepScreen, step.StepControl);
            switch (controlType)
            {
                case "Button":
                    baseButton = new UIButton(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseButton.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseButton, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "NavigationMenu":
                    baseNavigationMenu = new UINavigationMenu(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseNavigationMenu.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseNavigationMenu, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "Form":
                    baseForm = new UIForm(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseForm.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseForm, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "ComboBox":
                    baseComboBox = new UIComboBox(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseComboBox.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseComboBox, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "ErrorHeader":
                    baseErrorHeader = new UIErrorHeader(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseErrorHeader.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseErrorHeader, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "List":
                    baseList = new UIList(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseList.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseList, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "PanelItem":
                    basePanelItem = new UIPanelItem(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = basePanelItem.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(basePanelItem, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "Tab":
                    baseTab = new UITab(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseTab.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseTab, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "ToolTip":
                    baseToolTip = new UIToolTip(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseToolTip.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseToolTip, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
                case "NotificationMessage":
                    baseNotificationMessage = new UINotificationMessage(threadIndex, step.StepControl, step.StepScreen, step.StepAction, controlType, controlSearchType, controlSearchParameters);
                    methodInfo = baseNotificationMessage.GetType().GetMethod(step.StepAction, IsActionParameterless(step.StepAction) ? new Type[] { } : new Type[] { typeof(string) });
                    methodInfo.Invoke(baseNotificationMessage, IsActionParameterless(step.StepAction) ? null : new[] { step.StepParameter });
                    break;
            }
            SaveResult(threadIndex, step);
        }

        /// <summary>
        /// Creates result to be interpreted by Results window later
        /// </summary>
        public static void CreateResult(int threadIndex, ResultLibrary.StepResult result, string executionDetails, string error = "", string errorDetails = "")
        {
            currentResult[threadIndex] = new BaseResult(result, executionDetails, error, errorDetails);
        }

        /// <summary>
        /// Saves result into global result list, then displays messages based on the result
        /// </summary>
        public static void SaveResult(int threadIndex, BaseStep step)
        {
            ResultLibrary.allResults[threadIndex].Add(new BaseResult(currentResult[threadIndex], step));
            if (step.StepNumber == 0)
            {
                LogMessage(threadIndex, "Setup step " + (currentResult[threadIndex].StepExecutionResult == ResultLibrary.StepResult.Passed ? "successfully executed" : "failed"));
                LogMessage(threadIndex, "Execution details", false);
                LogMessage(threadIndex, "Execution status: ", false, true);
                LogMessage(threadIndex, currentResult[threadIndex].StepExecutionResult == ResultLibrary.StepResult.Passed ? "PASSED" : "FAILED", false, false, LogMessageType.SuccessMessage);
                LogMessage(threadIndex, step.StepParameter);
                return;
            }
            if (currentResult[threadIndex].StepExecutionResult == ResultLibrary.StepResult.Passed)
            {
                LogMessage(threadIndex, "Step " + step.StepNumber + ": " + step.StepScreen + " " + step.StepControl + " " + step.StepAction + (step.StepParameter != "" ? " with parameter \"" + step.StepParameter + "\"" : "") + " successfully executed");
                LogMessage(threadIndex, "Execution details", false);
                LogMessage(threadIndex, "Execution status: ", false, true);
                LogMessage(threadIndex, "PASSED", false, false, LogMessageType.SuccessMessage);
                LogMessage(threadIndex, "Action message received: " + currentResult[threadIndex].StepExecutionDetails, false);
            }
            else if (currentResult[threadIndex].StepExecutionResult == ResultLibrary.StepResult.Failed)
            {
                LogMessage(threadIndex, "Step " + step.StepNumber + ": " + step.StepScreen + " screen " + step.StepControl + " control " + step.StepAction + (step.StepParameter != "" ? " with parameter \"" + step.StepParameter + "\"" : "") + " failed to execute");
                LogMessage(threadIndex, "Execution details", false);
                LogMessage(threadIndex, "Execution status: ", false, true);
                LogMessage(threadIndex, "FAILED", false, false, LogMessageType.Error);
                LogMessage(threadIndex, "Action message received: " + currentResult[threadIndex].StepExecutionDetails, false);
                LogMessage(threadIndex, "Error message received: " + currentResult[threadIndex].StepError, false);
                LogMessage(threadIndex, "Error details received: " + currentResult[threadIndex].StepErrorDetails, false);
            }
            else
            {
                LogMessage(threadIndex, "Step " + step.StepNumber + ": " + step.StepScreen + " " + step.StepControl + " " + step.StepAction + " has been skipped");
                LogMessage(threadIndex, "Execution details", false);
                LogMessage(threadIndex, "Execution status: ", false, true);
                LogMessage(threadIndex, "SKIPPED", false, false);
            }
        }

        /// <summary>
        /// Checks whether or not an action doesn't use parameters
        /// </summary>
        public static bool IsActionParameterless(string action)
        {
            return action == "Click";
        }

        /// <summary>
        /// Base method for VerifyExists action used by some controls,
        /// verifies whether or not target element exists
        /// </summary>
        public static void VerifyElementExists(int threadIndex, string searchType, string searchParameter, string TrueOrFalse)
        {
            try
            {
                MainLibrary.TargetControl[threadIndex] = CommonFunctions.SearchElement(threadIndex, searchType, searchParameter, TOTAL_VERIFYELEMENTEXISTS_WAIT_TIME);
                bool controlExists = MainLibrary.TargetControl[threadIndex] == null ? false : MainLibrary.TargetControl[threadIndex].Displayed;
                bool isPassingStep = controlExists == Convert.ToBoolean(TrueOrFalse);
                if (isPassingStep)
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Expected result: " + TrueOrFalse + ", actual result: " + controlExists.ToString());
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Expected result: " + TrueOrFalse + ", actual result: " + controlExists.ToString(), "Comparison mismatch", "Expected value is not equal to actual value");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyExists failed", e.Message);
            }
        }

        /// <summary>
        /// Method to save encountered exception into a text file located inside the Logs folder
        /// </summary>
        public static void SaveErrorToFile(string errorMessage, Exception exception, bool showMessageBox = false)
        {
            try
            {
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                logPath = logPath + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt";
                if (exception != null)
                {
                    File.AppendAllLines(logPath, new string[]
                    {
                  "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + "Error encountered in " + exception.Source + Environment.NewLine,
                  "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + exception.Message + Environment.NewLine,
                  "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + exception.StackTrace + Environment.NewLine
                    });
                }
                else
                {
                    File.AppendAllLines(logPath, new string[] { "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + errorMessage + Environment.NewLine });
                }
                if (showMessageBox)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                
            }
        }
        #endregion
    }
}
