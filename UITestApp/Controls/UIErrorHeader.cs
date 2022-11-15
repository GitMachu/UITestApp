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
    [Control("ErrorHeader")]
    public class UIErrorHeader : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;
        public UIErrorHeader(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
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

        [UITestApp.Libraries.Action("VerifyExists")]
        public void VerifyExists(string TrueOrFalse)
        {
            CommonFunctions.VerifyElementExists(threadIndex, searchType, searchParameter, TrueOrFalse);
        }
    }
}
