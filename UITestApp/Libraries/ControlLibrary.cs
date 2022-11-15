using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using UITestApp.Controls;
using UITestApp.Functions;
using UITestApp.Utilities;

namespace UITestApp.Libraries
{
    /// <summary>
    /// Methods related to controls to be interacted with
    /// </summary>
    public class ControlLibrary
    {
        public static List<UIBaseControl> allControls;

        /// <summary>
        /// Adds all controls from XML file to global list
        /// </summary>
        public static void AddControlRecordToLibrary(string path)
        {
            XDocument controlXML = XMLHelper.LoadXML(path);
            var dataScreen = from doc in controlXML.Descendants("controlrecord")
                select new
                {
                    screen = doc.Attribute("screen").Value
                };
            string controlScreen = dataScreen.First().screen;
            var dataControls = from doc in controlXML.Descendants("control")
                select new
                {
                    name = doc.Attribute("name").Value,
                    type = doc.Element("type").Value,
                    searchtype = doc.Element("searchtype").Value,
                    searchparameter = doc.Element("searchparameter").Value
                };

            foreach (var value in dataControls)
            {
                allControls.Add(new UIBaseControl(value.name, controlScreen, value.type, value.searchtype, value.searchparameter));
            }
        }

        /// <summary>
        /// Returns list of all screens
        /// </summary>
        public static List<string> GetAllScreens()
        {
            List<string> allScreens = new List<string>();
            foreach (UIBaseControl baseControl in allControls)
            {
                if (!allScreens.Contains(baseControl.ControlScreen))
                {
                    allScreens.Add(baseControl.ControlScreen);
                }
            }
            if (allScreens == null)
            {
                allScreens = new List<string>();
            }
            allScreens.Sort();
            return allScreens;
        }

        /// <summary>
        /// Returns list of all controls located on the screen as enumerated by XML file
        /// </summary>
        public static List<string> GetControlsFromScreens(string Screen)
        {
            List<string> screenControls = new List<string>();
            foreach (UIBaseControl baseControl in allControls)
            {
                if (baseControl.ControlScreen == Screen)
                {
                    screenControls.Add(baseControl.ControlName);
                }
            }
            if (screenControls == null)
            {
                screenControls = new List<string>();
            }
            screenControls.Sort();
            return screenControls;
        }

        /// <summary>
        /// Returns all actions of control
        /// </summary>
        public static Dictionary<string, List<string>> GetAllControlActions()
        {
            Dictionary<string, List<string>> actions = new Dictionary<string, List<string>>();
            Assembly commonAssembly = Assembly.GetExecutingAssembly();
            foreach (Type aType in commonAssembly.GetTypes())
            {
                foreach (Attribute attributeControl in aType.GetCustomAttributes(typeof(Control), true))
                {
                    string control = ((Control)attributeControl).control;
                    List<string> actionList = new List<string>();
                    foreach (MethodInfo mi in aType.GetMethods())
                    {
                        foreach (Attribute attribute in mi.GetCustomAttributes(typeof(UITestApp.Libraries.Action), true))
                        {
                            string actionString = ((UITestApp.Libraries.Action)attribute).action;
                            if (actionList.Contains(actionString))
                            {
                                continue;
                            }
                            actionList.Add(actionString);
                        }
                    }
                    actions.Add(control, actionList);
                }
            }
            return actions;
        }

        /// <summary>
        /// Returns all actions based on type of control
        /// </summary>
        public static List<string> GetActionsFromControlType(string screenName, string controlName)
        {
            List<string> controlActions = GetAllControlActions().Where(x => x.Key == GetControlType(screenName, controlName)).FirstOrDefault().Value;
            if (controlActions == null)
            {
                controlActions = new List<string>();
            }
            controlActions.Sort();
            return controlActions;
        }

        /// <summary>
        /// Retrieves control type of control
        /// </summary>
        public static string GetControlType(string screenName, string controlName)
        {
            try
            {
                string controlType = allControls.FirstOrDefault(x => x.ControlScreen == screenName && x.ControlName == controlName).ControlType;
                return controlType;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns data related to locating the control on screen
        /// </summary>
        public static string GetSearchValues(string screenName, string controlName, out string searchParameters)
        {
            UIBaseControl targetControl = allControls.FirstOrDefault(x => x.ControlScreen == screenName && x.ControlName == controlName);
            searchParameters = targetControl.ControlSearchParameter;
            return targetControl.ControlSearchType;
        }
    }
}
