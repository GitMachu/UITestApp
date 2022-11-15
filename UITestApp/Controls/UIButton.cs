using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UITestApp.Functions;
using UITestApp.Libraries;

namespace UITestApp.Controls
{
    [Control("Button")]
    public class UIButton : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;
        public UIButton(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
            : base (ControlName, Screen, Type, SearchType, SearchParameter)
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
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Button " + name + " successfully clicked");
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Click failed", e.Message);
            }
        }
    }
}
