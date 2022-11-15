using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UITestApp.Functions;
using UITestApp.Libraries;
using OpenQA.Selenium;

namespace UITestApp.Controls
{
    [Control("PanelItem")]
    public class UIPanelItem : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;
        public UIPanelItem(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
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

        [UITestApp.Libraries.Action("Click")]
        public void Click()
        {
            try
            {
                MainLibrary.TargetControl[threadIndex].Click();
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "PanelItem " + name + " successfully clicked");
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Click failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("VerifyAmount")]
        public void VerifyAmount(string AmountToVerify)
        {
            try
            {
                IWebElement amountLabel = MainLibrary.TargetControl[threadIndex].FindElement(By.XPath(".//span[@class='account-balance']"));
                bool isPassingStep = amountLabel.Text == AmountToVerify;
                if (isPassingStep)
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Expected result: " + AmountToVerify + ", actual result: " + amountLabel.Text);
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex,ResultLibrary.StepResult.Failed, "Expected result: " + AmountToVerify + ", actual result: " + amountLabel.Text, "Comparison mismatch", "Expected value is not equal to actual value");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyAmount failed", e.Message);
            }
        }
    }
}
