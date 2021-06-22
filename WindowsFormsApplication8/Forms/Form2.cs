using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Subtitle_Synchronizer
{
    public partial class popUp_Fixing : Form
    {
        #region PROPERTIES

        int _previousStepNumber = 0;

        int _currentIteration = 0;

        tasks _previousTask = tasks.parsingSubs;
        tasks _currentTask = tasks.parsingSubs;

        public string Message
        {
            set
            {
                string[] arr = new string[2];
                arr[0] = value;
                arr[1] = "Done";
                ListViewItem itm = new ListViewItem(arr);
                listView1.Items.Add(itm);
            }
        }

        public int currentIteration
        {
            set
            {
                label_IterationNumber.Text = "Iteration n: " + value.ToString();
                _currentIteration = value;
            }
        }
        public string buttonMessage
        {
            set { button_Cancel.Text = value; }
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }

        public int currentStep
        {
            set
            {
                label_CurrentStep.Text = value.ToString();

              //  if (value < _previousStepNumber)
             //       _currentIteration++;

                currentIteration = _currentIteration;
                _previousStepNumber = value;
            }
        }

        public int totalSteps
        {
            set { label_TotalSteps.Text = value.ToString(); }
        }

        public enum tasks
        { parsingSubs, findingAnchors, findingWithAnchors, findingWithPermutations, postProcessing, correctTimingWithSpeechRec };

        public tasks currentTask
        {
            set
            {
                _previousTask = _currentTask;
                _currentTask = value;

                currentIteration = 0;

                int v = (int)value;
                int index = 0;
                if (v >= listView1.Items.Count)
                    index = listView1.Items.Count - 1;
                else if (v < 0)
                    index = 0;
                else
                    index = v;

                listView1.Items[index].BackColor = Color.LightSalmon;

                if (index > 0)
                {
                    listView1.Items[index - 1].SubItems[1].Text = "Done";
                    listView1.Items[index - 1].BackColor = Color.GreenYellow;
                }
                listView1.Items[index].SubItems[1].Text = "In Process";

            }
        }

        #endregion

        #region METHODS
        public popUp_Fixing()
        {
            InitializeComponent();
        }

        #endregion

        #region EVENTS

        public event EventHandler<EventArgs> Canceled;
        private void button_Cancel_Click(object sender, EventArgs e)
        {
            // Create a copy of the event to work with
            EventHandler<EventArgs> ea = Canceled;
            /* If there are no subscribers, ea will be null so we need to check
                * to avoid a NullReferenceException. */
            if (ea != null)
                ea(this, e);

            button_Cancel.Text = "Please wait, canceling...";
            button_Cancel.Enabled = false;
        }

        #endregion

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        public enum possibleStatus { pending, inProgress, done, };

        public string currentStatus(possibleStatus possStatus)
        {
            switch (possStatus)
            {
                case possibleStatus.pending:
                    return "Pending";
                case possibleStatus.inProgress:
                    return "In progress";
                case possibleStatus.done:
                    return "Done";

                default:
                    return String.Empty;
            }
        }

        private void popUp_Fixing_Load(object sender, EventArgs e)
        {
            //  TopMost = true;

            listView1.View = View.Details;
            listView1.GridLines = false;
            listView1.FullRowSelect = true;
            listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            //Add column header
            listView1.Columns.Add("Task", 300);
            listView1.Columns.Add("Status", 70);

            button_Cancel.Text = "Cancel";
            button_Cancel.Enabled = true;

            //Add items in the listview
            string[] arr = new string[4];
            ListViewItem itm;

            addProgressStatusToListView(listView1, "Parsing unfixed subtitles...", "Pending");
            addProgressStatusToListView(listView1, "Finding word Anchors...", "Pending");
            addProgressStatusToListView(listView1, "Finding provisory subtitles with anchors...", "Pending");
            addProgressStatusToListView(listView1, "Finding best matched subtitles by permutating...", "Pending");
            addProgressStatusToListView(listView1, "Post-processing subtitles...", "Pending");
            addProgressStatusToListView(listView1, "Correcting Time With Speech Recognition...", "Pending");
        }

        public void addProgressStatusToListView(ListView lView, string arr0, string arr1)
        {
            string[] arr = new string[4];
            arr[0] = arr0;
            arr[1] = arr1;
            ListViewItem itm = new ListViewItem(arr);
            lView.Items.Add(itm);
        }
    }
}
