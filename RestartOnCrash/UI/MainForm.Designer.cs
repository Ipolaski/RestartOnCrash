namespace RestartOnCrash.UI
{
    /// <summary>
    /// Main menu fo work with application.
    /// </summary>
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            startServiceButton = new System.Windows.Forms.Button();
            selectFileButton = new System.Windows.Forms.Button();
            listOfAddedPrograms = new System.Windows.Forms.ListBox();
            selectProgramToCheck = new System.Windows.Forms.OpenFileDialog();
            stopServiceButton = new System.Windows.Forms.Button();
            waitBeforeRestart = new System.Windows.Forms.CheckBox();
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            timeTextBox = new System.Windows.Forms.MaskedTextBox();
            restartPeriod = new System.Windows.Forms.Label();
            toolTipCheckbox = new System.Windows.Forms.ToolTip(components);
            removeFileButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // startServiceButton
            // 
            resources.ApplyResources(startServiceButton, "startServiceButton");
            startServiceButton.Name = "startServiceButton";
            startServiceButton.Tag = "StartButton";
            startServiceButton.UseVisualStyleBackColor = true;
            startServiceButton.Click += startServiceButton_Click;
            // 
            // selectFileButton
            // 
            resources.ApplyResources(selectFileButton, "selectFileButton");
            selectFileButton.Name = "selectFileButton";
            selectFileButton.UseVisualStyleBackColor = true;
            selectFileButton.Click += selectFileButton_Click;
            // 
            // listOfAddedPrograms
            // 
            listOfAddedPrograms.BackColor = System.Drawing.SystemColors.ActiveBorder;
            listOfAddedPrograms.BorderStyle = System.Windows.Forms.BorderStyle.None;
            listOfAddedPrograms.FormattingEnabled = true;
            resources.ApplyResources(listOfAddedPrograms, "listOfAddedPrograms");
            listOfAddedPrograms.Name = "listOfAddedPrograms";
            // 
            // selectProgramToCheck
            // 
            selectProgramToCheck.FileName = "openFileDialog1";
            // 
            // stopServiceButton
            // 
            resources.ApplyResources(stopServiceButton, "stopServiceButton");
            stopServiceButton.Name = "stopServiceButton";
            stopServiceButton.UseVisualStyleBackColor = true;
            stopServiceButton.Click += StopServiceThread_Click;
            // 
            // waitBeforeRestart
            // 
            resources.ApplyResources(waitBeforeRestart, "waitBeforeRestart");
            waitBeforeRestart.Name = "waitBeforeRestart";
            waitBeforeRestart.UseVisualStyleBackColor = true;
            waitBeforeRestart.CheckedChanged += waitBeforeRestart_CheckedChanged;
            // 
            // notifyIcon
            // 
            resources.ApplyResources(notifyIcon, "notifyIcon");
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            // 
            // timeTextBox
            // 
            timeTextBox.BackColor = System.Drawing.SystemColors.ControlDark;
            timeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(timeTextBox, "timeTextBox");
            timeTextBox.Name = "timeTextBox";
            timeTextBox.TextChanged += timeTextBox_TextChanged;
            // 
            // restartPeriod
            // 
            resources.ApplyResources(restartPeriod, "restartPeriod");
            restartPeriod.Name = "restartPeriod";
            // 
            // toolTipCheckbox
            // 
            toolTipCheckbox.BackColor = System.Drawing.SystemColors.ControlDark;
            // 
            // removeFileButton
            // 
            resources.ApplyResources(removeFileButton, "removeFileButton");
            removeFileButton.Name = "removeFileButton";
            removeFileButton.UseVisualStyleBackColor = true;
            removeFileButton.Click += removeFileButton_Click;
            // 
            // MainForm
            // 
            AcceptButton = startServiceButton;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.SystemColors.ControlDark;
            CancelButton = stopServiceButton;
            resources.ApplyResources(this, "$this");
            Controls.Add(removeFileButton);
            Controls.Add(restartPeriod);
            Controls.Add(timeTextBox);
            Controls.Add(waitBeforeRestart);
            Controls.Add(stopServiceButton);
            Controls.Add(listOfAddedPrograms);
            Controls.Add(selectFileButton);
            Controls.Add(startServiceButton);
            Name = "MainForm";
            FormClosed += MainForm_FormClosed;
            Resize += MainForm_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button startServiceButton;
        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.ListBox listOfAddedPrograms;
        private System.Windows.Forms.OpenFileDialog selectProgramToCheck;
        private System.Windows.Forms.Button stopServiceButton;
        private System.Windows.Forms.CheckBox waitBeforeRestart;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.MaskedTextBox timeTextBox;
        private System.Windows.Forms.Label restartPeriod;
        private System.Windows.Forms.ToolTip toolTipCheckbox;
        private System.Windows.Forms.Button removeFileButton;
    }
}