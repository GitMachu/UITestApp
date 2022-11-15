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
    [Control("Tab")]
    public class UITab : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;
        public UITab(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
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

        [UITestApp.Libraries.Action("Select")]
        public void Select(string ItemToSelect)
        {
            try
            {
                List<IWebElement> tabOptions = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath("./li")).ToList();
                if (!tabOptions.Any())
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Select failed", "Target tab item not found", "No tab items found");
                }
                if (tabOptions.Any(x => x.Text == ItemToSelect))
                {
                    tabOptions.First(x => x.Text == ItemToSelect).Click();
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Tab item " + ItemToSelect + " successfully clicked");
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Select failed", "Target tab item not found", "Tab item not found in retrieved tab items");
                }

            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Select failed", e.Message);
            }
        }
    }
}
