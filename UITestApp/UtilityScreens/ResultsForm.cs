using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UITestApp.Libraries;

namespace UITestApp.UtilityScreens
{
    public partial class ResultsForm : Form
    {
        public ResultsForm(int ThreadIndex)
        {
            InitializeComponent();
            threadIndex = ThreadIndex;
            Initialize();
        }

        #region PRIVATE MEMBERS
        private int threadIndex = 0;
        private bool isPassedScript = false;
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Initializes window
        /// </summary>
        private void Initialize()
        {
            isPassedScript = ResultLibrary.allResults[threadIndex].All(x => x.StepExecutionResult == ResultLibrary.StepResult.Passed);
            EditScriptStatus();
            ConstructResultsGrid();
        }

        /// <summary>
        /// Constructs results grid content
        /// </summary>
        private void ConstructResultsGrid()
        {
            foreach (BaseResult result in ResultLibrary.allResults[threadIndex])
            {
                dgvResults.Rows.Add(result.Step.StepNumber, result.Step.StepScreen, result.Step.StepControl, result.Step.StepAction, result.Step.StepParameter, result.StepExecutionResult.ToString());
                DataGridViewRow row = dgvResults.Rows[dgvResults.Rows.Count - 1];
                DataGridViewTextBoxCell txtResultCell = row.Cells[5] as DataGridViewTextBoxCell;
                if (txtResultCell.Value.ToString() == "Passed")
                {
                    row.DefaultCellStyle.BackColor = Color.LimeGreen;
                }
                else if (txtResultCell.Value.ToString() == "Failed")
                {
                    row.DefaultCellStyle.BackColor = Color.Red;
                }
            }
        }

        /// <summary>
        /// Changes row color based on whether a step passed or failed
        /// </summary>
        private void EditScriptStatus()
        {
            lblStatus.Text = isPassedScript ? "PASSED" : "FAILED";
            lblStatus.ForeColor = isPassedScript ? Color.DarkGreen : Color.Red;
        }
        #endregion

        #region PUBLIC METHODS
        /// <summary>
        /// Handles changing of script name
        /// </summary>
        public void ChangeScriptNameLabel(string ScriptName)
        {
            lblScriptName.Text = ScriptName;
        }

        /// <summary>
        /// Handles changing of browser label
        /// </summary>
        public void ChangeBrowserLabel(string BrowserName)
        {
            lblBrowser.Text = BrowserName;
        }
        #endregion

        #region EVENTS
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
