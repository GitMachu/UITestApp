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
    [Control("ComboBox")]
    public class UIComboBox : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;

        private string newUserStringPrefix = "Someone new: ";
        private string comboBoxListItemXPATH = "./ancestor::div[@class='combobox']//div[@id='ComboboxList-apm-name']//ul[not(contains(@class, 'u-hide'))]//li";

        public UIComboBox(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
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

        [UITestApp.Libraries.Action("Set")]
        public void Set(string TextToSet)
        {
            try
            {
                MainLibrary.TargetControl[threadIndex].Clear();
                MainLibrary.TargetControl[threadIndex].Click();
                if (TextToSet != String.Empty)
                {
                    MainLibrary.TargetControl[threadIndex].SendKeys(TextToSet);
                }
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Text " + TextToSet + " successfully set");
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Set failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("SetAndSelect")]
        public void SetAndSelect(string TextToSetAndSelect)
        {
            try
            {
                MainLibrary.TargetControl[threadIndex].Clear();
                MainLibrary.TargetControl[threadIndex].Click();
                if (TextToSetAndSelect != String.Empty)
                {
                    MainLibrary.TargetControl[threadIndex].SendKeys(TextToSetAndSelect);
                    List<IWebElement> comboBoxListItems = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(comboBoxListItemXPATH)).ToList();
                    if (comboBoxListItems.Any(x => x.Text == newUserStringPrefix + TextToSetAndSelect))
                    {
                        comboBoxListItems.FirstOrDefault(x => x.Text == newUserStringPrefix + TextToSetAndSelect).Click();
                    }
                }
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Text " + TextToSetAndSelect + " successfully set");
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "SetAndSelect failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("SetAndPressEnter")]
        public void SetAndPressEnter(string TextToSet)
        {
            try
            {
                MainLibrary.TargetControl[threadIndex].Clear();
                MainLibrary.TargetControl[threadIndex].Click();
                if (TextToSet != String.Empty)
                {
                    MainLibrary.TargetControl[threadIndex].SendKeys(TextToSet);
                    MainLibrary.TargetControl[threadIndex].SendKeys(Keys.Enter);
                }
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Text " + TextToSet + " successfully set");
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "SetAndPressEnter failed", e.Message);
            }
        }
    }
}
