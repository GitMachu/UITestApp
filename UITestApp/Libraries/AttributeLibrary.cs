using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UITestApp.Libraries
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class Action : System.Attribute
    {
        public String action;

        public Action(String action)
        {
            this.action = action;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class Control : System.Attribute
    {
        public String control;

        public Control(String control)
        {
            this.control = control;
        }
    }
}
