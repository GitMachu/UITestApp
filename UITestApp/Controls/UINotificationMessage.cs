using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UITestApp.Functions;
using UITestApp.Libraries;
using System.Threading;

namespace UITestApp.Controls
{
    [Control("NotificationMessage")]
    public class UINotificationMessage : UIBaseControl
    {
        private const int NOTIFICATION_MESSAGE_WAIT_TIME = 40;

        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;
        public UINotificationMessage(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
            : base(ControlName, Screen, Type, SearchType, SearchParameter)
        {
            name = ControlName;
            screen = Screen;
            action = Action;
            searchType = SearchType;
            searchParameter = SearchParameter;
            threadIndex = ThreadIndex;
            Initialize();
        }

        private void Initialize()
        {
            if (action != "VerifyExists")
                MainLibrary.TargetControl[threadIndex] = CommonFunctions.SearchElement(threadIndex, searchType, searchParameter, 40);
        }

        [UITestApp.Libraries.Action("VerifyText")]
        public void VerifyText(string TextToVerify)
        {
            try
            {
                bool isPassingStep = MainLibrary.TargetControl[threadIndex].Text == TextToVerify;
                if (isPassingStep)
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Expected result: " + TextToVerify + ", actual result: " + MainLibrary.TargetControl[threadIndex].Text);
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Expected result: " + TextToVerify + ", actual result: " + MainLibrary.TargetControl[threadIndex].Text, "Comparison mismatch", "Expected value is not equal to actual value");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyText failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("VerifyNotificationDisplayed")]
        public void VerifyNotificationDisplayed(string TrueOrFalse)
        {
            try
            {
                bool isDisplayed = false;
                bool expectedResult = Convert.ToBoolean(TrueOrFalse);
                string logMessage = "";
                for (int count = 1; count <= NOTIFICATION_MESSAGE_WAIT_TIME; count++)
                {
                    string notificationState = MainLibrary.TargetControl[threadIndex].GetAttribute("class") != null ? MainLibrary.TargetControl[threadIndex].GetAttribute("class") : "";
                    switch(notificationState == null ? "" : notificationState)
                    {
                        case "inner js-notification":
                            logMessage = "Default notification status found. ";
                            break;
                        case "":
                            logMessage = "Notification status not found. ";
                            break;
                        case string pendingStatus when notificationState.Contains("js-notificationPending"):
                            logMessage = "Pending notification status found. ";
                            break;
                        case string shownStatus when notificationState.Contains("js-notificationShown"):
                            logMessage = "Shown notification status found. ";
                            isDisplayed = true;
                            break;
                    }
                    if (isDisplayed == expectedResult)
                    {
                        CommonFunctions.LogMessage(threadIndex, logMessage + "Expected result " + TrueOrFalse + " is equal to actual result " + isDisplayed.ToString(), true);
                        CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Expected result: " + TrueOrFalse + ", actual result: " + isDisplayed.ToString());
                        break;
                    }
                    else if (count == NOTIFICATION_MESSAGE_WAIT_TIME)
                    {
                        CommonFunctions.LogMessage(threadIndex, logMessage + NOTIFICATION_MESSAGE_WAIT_TIME + " seconds has elapsed, maximum wait limit reached", true, false, CommonFunctions.LogMessageType.Error);
                        CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Expected result: " + TrueOrFalse + ", actual result: " + isDisplayed.ToString(), "Comparison mismatch", "Expected value is not equal to actual value");
                    }
                    else
                    {
                        CommonFunctions.LogMessage(threadIndex, logMessage + count + " seconds has elapsed. Retrying...", true, false, CommonFunctions.LogMessageType.Warning);
                        Thread.Sleep(1000);
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyNotificationDisplayed failed", e.Message);
            }
        }
    }
}
