using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UITestApp.Libraries
{
    public class BaseStep
    {
        public int StepNumber { get; set; }
        public string StepScreen { get; set; }
        public string StepControl { get; set; }
        public string StepAction { get; set; }
        public string StepParameter { get; set; }

        public BaseStep(int StepCount, string Screen, string Control, string Action, string Parameter)
        {
            StepNumber = StepCount;
            StepScreen = Screen;
            StepControl = Control;
            StepAction = Action;
            StepParameter = Parameter;
        }
    }
}
