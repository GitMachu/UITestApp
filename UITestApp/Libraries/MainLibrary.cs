using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace UITestApp.Libraries
{
    public class MainLibrary
    {
        public static IWebDriver[] AutoDriver = new IWebDriver[6];

        public static IWebElement[] TargetControl = new IWebElement[6];
    }
}
