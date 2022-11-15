using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UITestApp.Controls
{
    public class UIBaseControl
    {
        public string ControlName { get; set; }
        public string ControlScreen { get; set; }
        public string ControlType { get; set; }
        public string ControlSearchType { get; set; }
        public string ControlSearchParameter { get; set; }
        public UIBaseControl(string Name, string Screen, string Type, string SearchType, string SearchParameter)
        {
            ControlName = Name;
            ControlScreen = Screen;
            ControlType = Type;
            ControlSearchType = SearchType;
            ControlSearchParameter = SearchParameter;
        }
    }
}
