using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace StartUp
{
    public partial class FormMain : Form
    {
        // ShowWindow from pinvoke.net
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // IsWindowVisible from pinvoke.net
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        // Data manager
        private DataMng m_DataManager = null;

        // Window styles
        public enum WindowShowStyle : int
        {
            Hide = 0,
            ShowNormal = 1,
            ShowMinimized = 2,
            ShowMaximized = 3,
            Maximize = 3,
            ShowNormalNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActivate = 7,
            ShowNoActivate = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimized = 11
        }
       
        // FormMain instance class
        public FormMain()
        {
            InitializeComponent();

            m_DataManager = new DataMng();       
        }

        // Closing form main ask to exit
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If timer is not running
            if (timerCheck.Enabled == false)
            {
                // Ask to exit
                if (MessageBox.Show(Properties.Resources.Msg_QuestionClose, Properties.Resources.Msg_Question, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    // If no, cancel exit
                    e.Cancel = true;
                }
            }
            else
            {
                // Cancel exit event, application is running
                e.Cancel = true;

                // Show messagebox of warning
                MessageBox.Show(Properties.Resources.Msg_CloseExit, Properties.Resources.Msg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Exit click
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Try to exit app
            Application.Exit();
        }

        // Settings for seconds click
        private void secondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem target = sender as ToolStripMenuItem;

            foreach (ToolStripMenuItem item in target.Owner.Items)
            {
                item.Checked = (item == target);
                
                if (item.Checked)
                {
                    timerCheck.Interval = int.Parse(item.Tag.ToString());
                }
            }

            m_DataManager.SetRegSetting("CheckDelay", timerCheck.Interval);
        }

        // Add applicaion click
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open file dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Restore last used path
                openFileDialog.RestoreDirectory = true;

                // Filter to executable files string
                openFileDialog.Filter = Properties.Resources.Msg_Executable_Files;

                // If show dialog and get result is OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Add new row to grid list, with run state unchecked
                    dataGridViewMain.Rows.Add(dataGridViewMain.Rows.Count, false, Properties.Resources.off, "2000", openFileDialog.FileName, "", "Show");
                }
            }

            // Save data to XML
            m_DataManager.SaveDataToXml(dataGridViewMain);

            // Update Controls in form
            FormMain_UpdateControls();
        }

        // Remove application click
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show dialog to remove row
            if (MessageBox.Show(Properties.Resources.Msg_QuestionRemoveApp, Properties.Resources.Msg_Question, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // If has row selected
                if (dataGridViewMain.CurrentRow != null)
                {
                    // If is not running the process of row (Checked by Tag value)
                    if (dataGridViewMain.CurrentRow.Tag == null)
                    {
                        // Remove row by index
                        dataGridViewMain.Rows.RemoveAt(dataGridViewMain.CurrentRow.Index);

                        // If has data manager instance
                        if (m_DataManager != null)
                        {
                            // Save to XML
                            m_DataManager.SaveDataToXml(dataGridViewMain);
                        }
                    }
                    else
                    {
                        // Show message to stop app before remove it
                        MessageBox.Show(Properties.Resources.Msg_StopSelectedApp, Properties.Resources.Msg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Start All Apps click
        private void startAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If is in notify icon
            if (notifyIconMain.Visible)
            {
                // Show ballon tip (10 seconds)
                notifyIconMain.ShowBalloonTip(10, Properties.Resources.Msg_Information, Properties.Resources.Msg_RunningAll, ToolTipIcon.Info);
            }

            // Check for not running apps immediatly
            FormMain_CheckApplications(sender, e);

            // Start check timer
            timerCheck.Start();

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Stop all Apps click
        private void stopAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If is in notify icon
            if (notifyIconMain.Visible)
            {
                // Show ballon tip (10 seconds)
                notifyIconMain.ShowBalloonTip(10, Properties.Resources.Msg_Information, Properties.Resources.Msg_StoppingAll, ToolTipIcon.Info);
            }

            // Stop timer check
            timerCheck.Stop();

            // Stop all applications
            FormMain_StopApplications();

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Show all running Apps click
        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If is in notify icon
            if (notifyIconMain.Visible)
            {
                // Show ballon tip (10 seconds)
                notifyIconMain.ShowBalloonTip(10, Properties.Resources.Msg_Information, Properties.Resources.Msg_ShowingAll, ToolTipIcon.Info);
            }

            // Update rows in main grid if thread is running
            foreach (DataGridViewRow row in dataGridViewMain.Rows)
            {
                // If has process data in tag (If is running)
                if (row.Tag != null)
                {
                    // Get temp process data
                    Process temp = row.Tag as Process;

                    // Refresh
                    temp.Refresh();

                    // Is is runnig in really
                    if (temp.HasExited == false)
                    {
                        // If has window handler in cell[0] tag
                        if (row.Cells[0].Tag != null)
                        {
                            // Show window (0)
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Show);
                        }
                    }
                }
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Hide all running Apps click
        private void hideAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If is in notify icon
            if (notifyIconMain.Visible)
            {
                // Show ballon tip (10 seconds)
                notifyIconMain.ShowBalloonTip(10, Properties.Resources.Msg_Information, Properties.Resources.Msg_HiddingAll, ToolTipIcon.Info);
            }

            // Update rows in main grid if thread is running
            foreach (DataGridViewRow row in dataGridViewMain.Rows)
            {
                // If has process data in tag (If is running)
                if (row.Tag != null)
                {
                    // Get temp process data
                    Process temp = row.Tag as Process;

                    // Refresh
                    temp.Refresh();

                    // Is is runnig in really
                    if (temp.HasExited == false)
                    {
                        // If has window handler
                        if (row.Cells[0].Tag != null)
                        {
                            // Hide window (5)
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Hide);
                        }
                    }
                }
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Delete all Apps running from grid click
        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If is not running
            if (!timerCheck.Enabled)
            {
                // Ask to user if he wants to remove all applications
                if (MessageBox.Show(Properties.Resources.Msg_ClearGridConfirm, Properties.Resources.Msg_Question, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Clear grid
                    dataGridViewMain.Rows.Clear();
                }
            }
            else
            {
                // Show error if StartUp is running
                MessageBox.Show(Properties.Resources.Msg_ClearGridError, Properties.Resources.Msg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Form Main Shown event
        private void FormMain_Shown(object sender, EventArgs e)
        {
            // Load data grid settings
            m_DataManager.LoadDataFromXml();

            // Load registry settings
            timerCheck.Interval = (int)m_DataManager.LoadRegSetting("CheckDelay", timerCheck.Interval);

            // Check time drop down
            foreach (ToolStripMenuItem item in checkTimeToolStripMenuItem.DropDownItems)
            {
                item.Checked = (timerCheck.Interval == int.Parse(item.Tag.ToString()));
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Save grid to xml file
        private void dataGridViewMain_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // If row index is valid
            if (e.RowIndex >= 0)
            {
                // Save data to xml
                m_DataManager.SaveDataToXml(dataGridViewMain);
            }
        }

        // Validate delay and path values in grid
        private void dataGridViewMain_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // If will validate a valid row index in grid
            if (e.RowIndex >= 0)
            {
                // Validate delay value
                if (e.ColumnIndex == 3)
                {
                    // Your variable
                    int delay = 0;
                    
                    // If has an invalid value
                    if ((e.FormattedValue != null) && int.TryParse(e.FormattedValue.ToString(), out delay) == false)
                    {
                        // Cancel change
                        e.Cancel = true;

                        // Show error
                        MessageBox.Show(Properties.Resources.Msg_InvalidDelay,Properties.Resources.Msg_Error,MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                }
                
                // Validate path value
                if (e.ColumnIndex == 4)
                {
                    // If is a valid path and file exists
                    if ((e.FormattedValue != null) && File.Exists(e.FormattedValue.ToString()) == false)
                    {
                        // Cancel change
                        e.Cancel = true;

                        // Show error
                        MessageBox.Show(Properties.Resources.Msg_InvalidPath, Properties.Resources.Msg_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Check running Apps, if crashed or exited restart then 
        private void FormMain_CheckApplications(object sender, EventArgs e)
        {
            // Create thread to row applicains
            Thread thread = new Thread(delegate()
            {
                // Stop edit or changes in datagridview (Important to get correct results)
                dataGridViewMain.EndEdit();

                // Update rows in main grid if thread is running
                foreach (DataGridViewRow row in dataGridViewMain.Rows)
                {
                    // Get checlbox of run column
                    DataGridViewCheckBoxCell CheckBoxRun = row.Cells[1] as DataGridViewCheckBoxCell;

                    // If is marked to run
                    if (Convert.ToBoolean(CheckBoxRun.Value) == true)
                    {
                        // If row have process data (If is running)
                        if (row.Tag != null)
                        {
                            // Get temp process data
                            Process temp = row.Tag as Process;

                            // Refresh
                            temp.Refresh();

                            // Is process closed
                            if (temp.HasExited)
                            {
                                // Set tag to null to restart process in next loop
                                row.Tag = null;
                            }
                        }
                        
                        // If is not running yet or crashed, start it
                        if(row.Tag == null)
                        {
                            // Get delay
                            int Delay = Convert.ToInt32(row.Cells[3].Value);

                            // Get file path
                            string FileName = Convert.ToString(row.Cells[4].Value);

                            // Get arguments
                            string Arguments = Convert.ToString(row.Cells[5].Value);

                            // Get window style
                            DataGridViewComboBoxCell WindowStyleBox = row.Cells[6] as DataGridViewComboBoxCell;

                            // Show (5)
                            WindowShowStyle WindowStyle = WindowShowStyle.Show;

                            // Loop item list and see what index is selected
                            for (int index = 0; index < WindowStyleBox.Items.Count; index++)
                            {
                                // If item is equal the value
                                if (WindowStyleBox.Items[index].ToString() == WindowStyleBox.Value.ToString())
                                {
                                    // Set window style
                                    WindowStyle = (WindowShowStyle)index;
                                }
                            }

                            // Create process instance
                            Process process = new Process();

                            // Set path
                            process.StartInfo.FileName = FileName;

                            // Set arguments
                            process.StartInfo.Arguments = Arguments;

                            // Set working directory
                            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(FileName);

                            // Set window style to normal before use ShowWindow function after open
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                            // Start process
                            process.Start();

                            // Wait the delay
                            Thread.Sleep(Delay);

                            // Store main window handle at index cell[0] tag, duo to show/hide command (Store before change window handle style)
                            row.Cells[0].Tag = process.MainWindowHandle;

                            // Update window display style
                            ShowWindow(process.MainWindowHandle, (int)WindowStyle);

                            // Refresh process
                            process.Refresh();

                            // Set tag to true (Running)
                            row.Tag = process;
                        }
                    }
                }
            });

            // Start thread
            thread.Start();

            // Wait until thread end
            thread.Join();

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Stop single App click
        private void FormMain_StopApplications()
        {
            // Create thread to row applicains
            Thread thread = new Thread(delegate()
            {
                // Update rows in main grid if thread is running
                foreach (DataGridViewRow row in dataGridViewMain.Rows)
                {
                    // If is set to rum
                    if (row.Tag != null)
                    {
                        // Get temp process data
                        Process temp = row.Tag as Process;

                        // Refresh
                        temp.Refresh();

                        // If is running
                        if (temp.HasExited == false)
                        {
                            // Kill Process
                            temp.Kill();
                        }

                        // Clear Tag
                        row.Tag = null;
                    }
                }
            });

            // Start thread
            thread.Start();

            // Wait until thread end
            thread.Join();

            // Update Controls in main form
            FormMain_UpdateControls();
        }

        // Update form controls while apps running or not and other updates
        private void FormMain_UpdateControls()
        {
            // If has rows (applications) in data grid view
            bool hasRows = (dataGridViewMain.Rows.Count > 0);

            // Running count
            int RunningCount = 0;

            // Mark to run count
            int MarkRunCount = 0;

            // Update rows in main grid if thread is running
            foreach (DataGridViewRow row in dataGridViewMain.Rows)
            {
                // If row has process set to run
                bool isRunning = (row.Tag != null) ? true : false;

                // Set ON/OFF icon
                row.Cells[2].Value = isRunning ? Properties.Resources.on : Properties.Resources.off;

                // If is running, mark as read only
                row.ReadOnly = isRunning;

                // Increment running count
                if (isRunning)
                {
                    // Increment unning count
                    RunningCount++;
                }

                // Get checlbox of run column
                DataGridViewCheckBoxCell CheckBoxRun = row.Cells[1] as DataGridViewCheckBoxCell;

                // If is marked to run
                if (Convert.ToBoolean(CheckBoxRun.Value) == true)
                {
                    // Increment your counter
                    MarkRunCount++;
                }
            }

            // Show delete menu button and toolbar delete button when has rows and thread is not running
            removeToolStripMenuItem.Visible = hasRows && (RunningCount < MarkRunCount);
            toolStripButtonRemove.Visible = hasRows && (RunningCount < MarkRunCount);

            // Show start all menu button and toolbar start all button when has rows and thread is not running
            startAllToolStripMenuItem.Visible = hasRows && (MarkRunCount > RunningCount);
            toolStripButtonStartAll.Visible = hasRows && (MarkRunCount > RunningCount);

            // Separator add/remove
            toolStripSeparatorMenuAddRemove.Visible = hasRows;
            toolStripSeparatorAddRemove.Visible = hasRows;

            // Show stop all menu button and toolbar stop all button when has rows and thread is not running
            stopAllToolStripMenuItem.Visible = (RunningCount > 0);
            toolStripButtonStopAll.Visible = (RunningCount > 0);

            // Enable show/hide buttons in main menu
            toolStripSeparatorMenuStartStopAll.Visible = (RunningCount > 0);
            showAllToolStripMenuItem.Visible = (RunningCount > 0);
            hideAllToolStripMenuItem.Visible = (RunningCount > 0);

            // Enable show/hide buttons in main toolbar
            toolStripSeparatorStartStopAll.Visible = (RunningCount > 0);
            toolStripButtonShowAll.Visible = (RunningCount > 0);
            toolStripButtonHideAll.Visible = (RunningCount > 0);

            // Delete all menu and toolbar option
            toolStripSeparatorDelAll.Visible = (hasRows && (RunningCount == 0));
            deleteAllToolStripMenuItem.Visible = (hasRows && (RunningCount == 0));

            // Hide separator of trash all
            toolStripSeparatorDellAll.Visible = (hasRows && (RunningCount == 0));
            toolStripButtonDelAll.Visible = (hasRows && (RunningCount == 0));

            // Update status
            toolStripStatusLabelStatus.Text = (RunningCount > 0) ? Properties.Resources.Msg_StartUpIsRunning : Properties.Resources.Msg_StartUpIsNotRunnig;

            // Update start/stop notify menu items
            toolStripMenuItemNotifyStartAll.Visible = hasRows && (MarkRunCount > RunningCount);
            toolStripMenuItemNotifyStopAll.Visible = (RunningCount > 0);

            // Update show/hide notify menu items
            toolStripMenuItemNotifyShowAll.Visible = (RunningCount > 0);
            toolStripMenuItemNotifyHideAll.Visible = (RunningCount > 0);

            // Update start/stop notify separator
            toolStripSeparatorNotifyStartStopAll.Visible = (RunningCount > 0);

            // If has applicatins in start mode
            if (RunningCount > 0)
            {
                // Update running count in status
                toolStripStatusLabelRunningCount.Text = RunningCount + Properties.Resources.Msg_ApplicationsRunning;
            }
            else
            {
                // Default tip
                toolStripStatusLabelRunningCount.Text = Properties.Resources.Msg_StatusTipText;
            }
        }

        // Double click to change path using open file dialog or show/hide running App
        private void dataGridViewMain_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Row
            DataGridViewRow row = dataGridViewMain.Rows[e.RowIndex];

            // If process is running
            if (row.Tag != null)
            {
                // Get temp process data
                Process temp = row.Tag as Process;

                // Refresh
                temp.Refresh();

                // Is is runnig in really
                if (temp.HasExited == false)
                {
                    // If has window handler
                    if (row.Cells[0].Tag != null)
                    {
                        // Store window handle
                        IntPtr WindowHandle = (IntPtr)row.Cells[0].Tag;

                        if (IsWindowVisible(WindowHandle))
                        {
                            // Show or hide main window
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Hide);
                        }
                        else
                        {
                            // Show or hide main window
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Show);
                        }
                    }
                }
            }
            else
            {
                // If is file path column
                if (e.ColumnIndex == 4)
                {
                    // Open file dialog
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        // Restore last used path
                        openFileDialog.RestoreDirectory = true;

                        // Filter to executable files string
                        openFileDialog.Filter = Properties.Resources.Msg_Executable_Files;

                        // If show dialog and get result is OK
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Add new row to grid list
                            row.Cells[e.ColumnIndex].Value = openFileDialog.FileName;
                        }
                    }
                }

                // If is in edit mode
                if (dataGridViewMain.IsCurrentCellDirty)
                {
                    // Commit changes
                    dataGridViewMain.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
        }

        // Open your link
        private void perfectZonecombrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open your link
            System.Diagnostics.Process.Start(Properties.Resources.Msg_AboutLink);
        }

        // Display grid view menu 
        private void dataGridViewMain_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            // If is right click
            if (e.Button == MouseButtons.Right)
            {
                // If is valid row index and cell index
                if (e.RowIndex != -1 && e.ColumnIndex != -1)
                {
                    // Update Controls in main form
                    FormMain_UpdateControls();

                    // Set as selected
                    dataGridViewMain.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;

                    // Store current row
                    DataGridViewRow row = dataGridViewMain.Rows[e.RowIndex];

                    // Update context menu according selected row
                    //toolStripMenuItemAddApp.Visible
                    //toolStripSeparatorMenuGridAddRemove.Visible
                    toolStripMenuItemDelApp.Visible = (row.Tag == null);
                    toolStripMenuItemStartApp.Visible = (row.Tag == null);
                    toolStripMenuItemStopApp.Visible = (row.Tag != null);
                    toolStripSeparatorMenuGridStartStop.Visible = (row.Tag != null);
                    showApplicationToolStripMenuItem.Visible = (row.Tag != null);
                    hideApplicationToolStripMenuItem.Visible = (row.Tag != null);

                    // Show the menu
                    contextMenuStripGrid.Show(Cursor.Position);
                }
            }
        }

        // Start single App from grid view menu
        private void toolStripMenuItemStartApp_Click(object sender, EventArgs e)
        {
            // If selected row/cell is not null
            if (dataGridViewMain.CurrentRow != null)
            {
                // Create thread to row applicains
                Thread thread = new Thread(delegate()
                {
                    // Stop edit or changes in datagridview (Important to get correct results)
                    dataGridViewMain.EndEdit();

                    // Get row
                    DataGridViewRow row = dataGridViewMain.CurrentRow;

                    // If is not running yet
                    if (row.Tag == null)
                    {
                        // Get checlbox of run column
                        DataGridViewCheckBoxCell CheckBoxRun = row.Cells[1] as DataGridViewCheckBoxCell;

                        // If is marked to run
                        if (Convert.ToBoolean(CheckBoxRun.Value) == true)
                        {
                            // Get delay
                            int Delay = Convert.ToInt32(row.Cells[3].Value);

                            // Get file path
                            string FileName = Convert.ToString(row.Cells[4].Value);

                            // Get arguments
                            string Arguments = Convert.ToString(row.Cells[5].Value);

                            // Get window style
                            DataGridViewComboBoxCell WindowStyleBox = row.Cells[6] as DataGridViewComboBoxCell;

                            // Show (5)
                            WindowShowStyle WindowStyle = WindowShowStyle.Show;

                            // Loop item list and see what index is selected
                            for (int index = 0; index < WindowStyleBox.Items.Count; index++)
                            {
                                // If item is equal the value
                                if (WindowStyleBox.Items[index].ToString() == WindowStyleBox.Value.ToString())
                                {
                                    // Set window style
                                    WindowStyle = (WindowShowStyle)index;
                                }
                            }

                            // Create process instance
                            Process process = new Process();

                            // Set path
                            process.StartInfo.FileName = FileName;

                            // Set arguments
                            process.StartInfo.Arguments = Arguments;

                            // Set working directory
                            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(FileName);

                            // Set window style to normal before use ShowWindow function after open
                            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

                            // Start process
                            process.Start();

                            // Wait the delay
                            Thread.Sleep(Delay);

                            // Store main window handle at index cell[0] tag, duo to show/hide command (Store before change window handle style)
                            row.Cells[0].Tag = process.MainWindowHandle;

                            // Update window display style
                            ShowWindow(process.MainWindowHandle, (int)WindowStyle);

                            // Refresh process
                            process.Refresh();

                            // Set tag to true (Running)
                            row.Tag = process;
                        }
                    }
                });

                // Start thread
                thread.Start();

                // Wait until thread end
                thread.Join();

                // Running count
                int RunningCount = 0;

                // Marked to run count
                int MarkRunCount = 0;

                // Loop datagrid view roos
                foreach (DataGridViewRow row in dataGridViewMain.Rows)
                {
                    // If is running
                    if (row.Tag != null)
                    {
                        RunningCount++;
                    }

                    // Get checlbox of run column
                    DataGridViewCheckBoxCell CheckBoxRun = row.Cells[1] as DataGridViewCheckBoxCell;

                    // If is marked to run
                    if (Convert.ToBoolean(CheckBoxRun.Value) == true)
                    {
                        MarkRunCount++;
                    }
                }

                // If need to stop timer
                if (RunningCount >= MarkRunCount)
                {
                    // Start timer checl
                    timerCheck.Start();
                }

                // Update Controls in main form
                FormMain_UpdateControls();
            }
        }

        // Stop single App from grid view menu
        private void toolStripMenuItemStopApp_Click(object sender, EventArgs e)
        {
            // If selected row/cell is not null
            if (dataGridViewMain.CurrentRow != null)
            {
                // Create thread to row applicains
                Thread thread = new Thread(delegate()
                {
                    // Stop edit or changes in datagridview (Important to get correct results)
                    dataGridViewMain.EndEdit();

                    // Get row
                    DataGridViewRow row = dataGridViewMain.CurrentRow;

                    // If is running
                    if (row.Tag != null)
                    {
                        // Get temp process data
                        Process temp = row.Tag as Process;

                        // Refresh
                        temp.Refresh();

                        // If is running
                        if (temp.HasExited == false)
                        {
                            // Kill Process
                            temp.Kill();
                        }

                        // Clear Tag
                        row.Tag = null;
                    }
                });

                // Start thread
                thread.Start();

                // Wait until thread end
                thread.Join();

                // We need to stop timer?
                bool stopTimerCheck = true;

                // Loop datagrid view roos
                foreach (DataGridViewRow row in dataGridViewMain.Rows)
                {
                    // If is running
                    if (row.Tag != null)
                    {
                        // We do not need to stop the timer
                        stopTimerCheck = false;
                    }
                }

                // If need to stop timer
                if (stopTimerCheck)
                {
                    // Stop it
                    timerCheck.Stop();
                }

                // Update Controls in main form
                FormMain_UpdateControls();
            }
        }

        // Show single App from grid view menu
        private void showApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If selected row/cell is not null
            if (dataGridViewMain.CurrentRow != null)
            {
                // Stop edit or changes in datagridview (Important to get correct results)
                dataGridViewMain.EndEdit();

                // Get row
                DataGridViewRow row = dataGridViewMain.CurrentRow;

                // If is running
                if (row.Tag != null)
                {
                    // Get temp process data
                    Process temp = row.Tag as Process;

                    // Refresh
                    temp.Refresh();

                    // If is running
                    if (temp.HasExited == false)
                    {
                        // If has window handle
                        if (row.Cells[0].Tag != null)
                        {
                            // Show window
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Show);
                        }
                    }
                }
            }
        }

        // Hide single App from grid view menu
        private void hideApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If selected row/cell is not null
            if (dataGridViewMain.CurrentRow != null)
            {
                // Stop edit or changes in datagridview (Important to get correct results)
                dataGridViewMain.EndEdit();

                // Get row
                DataGridViewRow row = dataGridViewMain.CurrentRow;

                // If is running
                if (row.Tag != null)
                {
                    // Get temp process data
                    Process temp = row.Tag as Process;

                    // Refresh
                    temp.Refresh();

                    // If is running
                    if (temp.HasExited == false)
                    {
                        // If has window handle
                        if (row.Cells[0].Tag != null)
                        {
                            // Hide window
                            ShowWindow((IntPtr)row.Cells[0].Tag, (int)WindowShowStyle.Hide);
                        }
                    }
                }
            }
        }

        // Detect form minimize to show notify icon on tray
        private void FormMain_Resize(object sender, EventArgs e)
        {
            // If is minimize action
            if (this.WindowState == FormWindowState.Minimized)
            {
                // If is not visible
                if (notifyIconMain.Visible == false)
                {
                    // Hide current window
                    Hide();

                    // Show notification 
                    notifyIconMain.Visible = true;

                    // Show ballon tip (10 seconds)
                    notifyIconMain.ShowBalloonTip(10, Properties.Resources.Msg_StartUpMinimized, Properties.Resources.Msg_StartUpMinimizedText, ToolTipIcon.Info);
                }

                // Update Controls in main form
                FormMain_UpdateControls();
            }
        }

        // Detect form minimize to hide notify icon on tray
        private void notifyIconMain_DoubleClick(object sender, EventArgs e)
        {
            // If has notification
            if (notifyIconMain.Visible)
            {
                // Show window again
                Show();

                // Show window normal
                this.WindowState = FormWindowState.Normal;  

                // Hide notification icon
                notifyIconMain.Visible = false;
            }

            // Update Controls in main form
            FormMain_UpdateControls();
        }
    }
}
