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
    [Control("List")]
    public class UIList : UIBaseControl
    {
        private string name = "";
        private string screen = "";
        private string action = "";
        private string searchType = "";
        private string searchParameter = "";
        private int threadIndex = 0;

        private string listHeadersXPATH = "./preceding-sibling::div//h3[@role='button']";

        public UIList(int ThreadIndex, string ControlName, string Screen, string Action, string Type, string SearchType, string SearchParameter)
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

        [UITestApp.Libraries.Action("ClickRowByName")]
        public void ClickRowByName(string NameOfRowToClick)
        {
            try
            {
                List<IWebElement> rows = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath("./li")).Where(x => x.Displayed).ToList();
                if (!rows.Any())
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickRowByName failed", "Target list item not found", "No list items found");
                    return;
                }
                List<IWebElement> rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[contains(@class, 'name')]")).ToList();
                if (!rowNames.Any())
                {
                    rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[@class='Avatar-title']//span[contains(@class, 'name')]")).ToList();
                    if (!rowNames.Any())
                    {
                        CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickRowByName failed", "Target list item not found", "List item not found in retrieved list items");
                        return;
                    }
                }
                if (rowNames.Any(x => x.Text == NameOfRowToClick))
                {
                    rowNames.FirstOrDefault(x => x.Text == NameOfRowToClick).Click();
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "List item " + NameOfRowToClick + " successfully clicked");
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickRowByName failed", "Target list item not found", "List item not found in retrieved list items");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickRowByName failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("ClickHeaderByName")]
        public void ClickHeaderByName(string NameOfHeaderToClick)
        {
            try
            {
                List<IWebElement> headers = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(listHeadersXPATH)).ToList();
                if (!headers.Any())
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickHeaderByName failed", "Target header not found", "No headers found");
                    return;
                }
                if (headers.Any(x => x.Text == NameOfHeaderToClick))
                {
                    headers.FirstOrDefault(x => x.Text == NameOfHeaderToClick).Click();
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Header " + NameOfHeaderToClick + " successfully clicked");
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickHeaderByName failed", "Target header not found", "Header not found in retrieved header list");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "ClickHeaderByName failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("VerifyRowExistsByName")]
        public void VerifyRowExistsByName(string NameOfRowToVerify)
        {
            try
            {
                List<IWebElement> rows = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath("./li")).ToList();
                if (!rows.Any())
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyRowExistsByName failed", "Target list item not found", "No list items found");
                    return;
                }
                List<IWebElement> rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[contains(@class, 'name')]")).ToList();
                if (!rowNames.Any())
                {
                    rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[@class='Avatar-title']//span[contains(@class, 'name')]")).ToList();
                    if (!rowNames.Any())
                    {
                        CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyRowExistsByName failed", "Target list item not found", "List item not found in retrieved list items");
                        return;
                    }
                }
                bool isPassingStep = rowNames.Any(x => x.Text == NameOfRowToVerify);
                if (isPassingStep)
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, NameOfRowToVerify + " found in retrieved list items");
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, NameOfRowToVerify + " not found in retrieved list items", "Verification mismatch", "Expected value not found in retrieved list items");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyRowExistsByName failed", e.Message);
            }
        }

        [UITestApp.Libraries.Action("VerifyListSortOrder")]
        public void VerifyListSortOrder(string AscendingOrDescending)
        {
            try
            {
                List<IWebElement> rows = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath("./li")).ToList();
                if (!rows.Any())
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyListSortOrder failed", "Target list item not found", "No list items found");
                    return;
                }
                List<IWebElement> rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[contains(@class, 'name')]")).ToList();
                if (!rowNames.Any())
                {
                    rowNames = MainLibrary.TargetControl[threadIndex].FindElements(By.XPath(".//p[@class='Avatar-title']//span[contains(@class, 'name')]")).ToList();
                    if (!rowNames.Any())
                    {
                        CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyListSortOrder failed", "Target list item not found", "List item not found in retrieved list items");
                        return;
                    }
                }
                string listActualOrder = "Unsorted";
                List<IWebElement> rowNamesToReorder = rowNames;
                List<string> actualList = rowNames.Select(x => x.Text).ToList();
                List<string> orderedList = rowNamesToReorder.OrderBy(x => x.Text).Select(x => x.Text).ToList();
                if (actualList.SequenceEqual(orderedList))
                {
                    listActualOrder = "Ascending";
                }
                else
                {
                    orderedList = rowNamesToReorder.OrderByDescending(x => x.Text).Select(x => x.Text).ToList();
                    if (actualList.SequenceEqual(orderedList))
                    {
                        listActualOrder = "Descending";
                    }
                }
                bool isPassingStep = AscendingOrDescending == listActualOrder;
                if (isPassingStep)
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Passed, "Expected result: " + AscendingOrDescending + ", actual result: " + listActualOrder);
                }
                else
                {
                    CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "Expected result: " + AscendingOrDescending + ", actual result: " + listActualOrder, "Expected order mismatch", "Expected sort order is not equal to actual sort order");
                }
            }
            catch (Exception e)
            {
                CommonFunctions.CreateResult(threadIndex, ResultLibrary.StepResult.Failed, "VerifyListSortOrder failed", e.Message);
            }
        }
    }
}
