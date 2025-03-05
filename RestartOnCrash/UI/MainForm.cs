using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ThreadState = System.Threading.ThreadState;

namespace RestartOnCrash.UI;

partial class MainForm : Form
{
    private const short NotSelectedElementIndex = -1;
    private const string ConfigurationFileName = "configuration.json";
    private Configuration _configuration = new Configuration();
    private bool _hasAlreadyStartedManuallyOneTime;
    private bool _checkerStarted = false;

    private ResourceManager _resourceManager;
    private CancellationTokenSource _cancelTokenSource = new();
    private readonly ConfigAttributes _programs = new();
    private Thread _restartOnCrashService;
    private string _fileName = string.Empty;
    private string _filePath = string.Empty;

    public class ConfigAttributes
    {
        public string[] PathToApplicationsToMonitor { get; set; }
        public List<string> FullPathToApplicationToMonitor { get; set; } = [];

        private string CheckInterval { get; set; } = "0:20:0";
        private string StartApplicationOnlyAfterFirstExecution { get; set; }

        #region [set <attribute>]
        public void SetUserReCheckTimer(TimeOnly time)
        {

            CheckInterval = time.ToString("HH:mm:ss");
        }

        public void SetUserReCheckTimer(TimeSpan time)
        {

            CheckInterval = TimeOnly.FromTimeSpan(time).ToString("HH:mm:ss"); ;
        }

        public void SetUserReCheckTimer(string time = "00:00:20")
        {
            CheckInterval = $"{time}";
        }

        public void SetUserSturtupOption(bool value)
        {
            StartApplicationOnlyAfterFirstExecution = $"{value}";
        }

        public void SetUserAplicationPathsFromMainForm()
        {
            if ( FullPathToApplicationToMonitor == null || FullPathToApplicationToMonitor.Count == 0 )
                return;

            for ( var i = 0; i < FullPathToApplicationToMonitor.Count; i++ )
                PathToApplicationsToMonitor[i] = FullPathToApplicationToMonitor[i];
        }
        #endregion

        #region [get <attribute>]
        public string GetUserReCheckTimer()
        {
            return $"\"CheckInterval\":\"{CheckInterval}\",";
        }

        public string GetUserSturtupOption()
        {
            string value = StartApplicationOnlyAfterFirstExecution.ToLower();
            return $"\"StartApplicationOnlyAfterFirstExecution\":{value}";
        }

        public string GetApplicationPathsForConfigString()
        {
            var returnValue = string.Empty;

            foreach ( var t in PathToApplicationsToMonitor )
                returnValue += $"{Environment.NewLine}\"{t.Replace("\\", "\\\\")}\",";

            returnValue = returnValue.TrimEnd(',');
            return $"\"PathToApplicationsToMonitor\":[{returnValue}\n],";
        }
        #endregion
    }

    public MainForm()
    {
        InitializeComponent();
        _resourceManager = new ResourceManager(typeof(MainForm));

        const byte correctionIndex = 1;

        #region [config "Add exe" buton]
        selectProgramToCheck.Filter = "|*.exe";
        selectProgramToCheck.DefaultExt = "exe";
        selectProgramToCheck.Title = "Select program to add in List";
        toolTipCheckbox.SetToolTip(
            this.waitBeforeRestart,
            "If true, the monitored application gets started only if it is already started a first time by its own.\n" +
            "It is useful when you have an application in \"startup\".");
        #endregion

        #region [Load config from file]
        var configurationProvider = new JsonFileConfigurationProvider(ConfigurationFileName);
        var tempConfiguration = configurationProvider.Get();
        _configuration = tempConfiguration;
        tempConfiguration = null;
        #endregion

        #region [Load list of programs]
        listOfAddedPrograms.Items.Clear();
        if ( _configuration.PathToApplicationsToMonitor != null )
            foreach ( var currentElement in _configuration.PathToApplicationsToMonitor )
            {
                _programs.FullPathToApplicationToMonitor.Add(currentElement);
                var lastIndex = currentElement.LastIndexOf('\\') + correctionIndex;
                listOfAddedPrograms.Items.Add(currentElement[lastIndex..]);
            }
        #endregion

        _programs.SetUserSturtupOption(waitBeforeRestart.Checked);

        if ( _configuration.CheckInterval != TimeSpan.Zero )
            timeTextBox.Text = TimeOnly.FromTimeSpan(_configuration.CheckInterval).ToString("HH:mm:ss");

        waitBeforeRestart.Checked = _configuration.StartApplicationOnlyAfterFirstExecution;

        StartCheckerThread();

        LoadFormTranslation();
    }

    /// <summary>
    /// Restarts the verification process with parameters taken from the form again
    /// </summary>
    private void startServiceButton_Click(object sender, EventArgs e)
    {
        using var logger = new EventViewerLogger();

        if ( ProcessUtilities.IsRestartOnCrashRunning() )
        {
            logger.LogWarning("RestartOnCrash is already running, cannot start");
            ToastService.Notify($"RestartOnCrash is already running, cannot start");
            return;
        }
        else
        {
            #region [Start Service]
            if ( _checkerStarted )
                StopServiceThread_Click(null, null);
            StartCheckerThread();
            #endregion
        }
    }

    private void StartCheckerThread()
    {
        _checkerStarted = true;
        _restartOnCrashService = new(() => StartRestartOnCrashService(_cancelTokenSource.Token));
        _restartOnCrashService.Start();
    }

    private void StartRestartOnCrashService(CancellationToken cancellationToken)
    {
        var logger = new EventViewerLogger();

        var configurationProvider = new JsonFileConfigurationProvider(ConfigurationFileName);
        SetCurrentConfigurationFromForm();

        if ( _configuration.PathToApplicationsToMonitor.Length != 0 )
        {
            logger.LogInformation(
                Environment.NewLine
                + $"Application to monitor: {_configuration.PathToApplicationsToMonitor}"
                + Environment.NewLine
                + $"Watching every: {Math.Round(_configuration.CheckInterval.TotalSeconds, 0)} seconds"
                + Environment.NewLine
                + $"{nameof(_configuration.StartApplicationOnlyAfterFirstExecution)}: {_configuration.StartApplicationOnlyAfterFirstExecution}");
            StartRestartOnCrashService(logger, cancellationToken: cancellationToken);

        }
        else
        {
            ToastService.Notify("You list of apps if empty");
        }

    }

    private void SetCurrentConfigurationFromForm()
    {
        _configuration = new()
        {
            CheckInterval = TimeSpan.Parse(timeTextBox.Text),
            StartApplicationOnlyAfterFirstExecution = waitBeforeRestart.Checked,
            PathToApplicationsToMonitor = _programs.FullPathToApplicationToMonitor!.ToArray()
        };
    }

    /// <summary>
    /// Starts a loop that checks whether each of the added exe is running until a CancellationToken arrives.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="cancellationToken">Cancellation token to end the cycle</param>
    private void StartRestartOnCrashService(EventViewerLogger logger, CancellationToken cancellationToken)
    {
        while ( !cancellationToken.IsCancellationRequested )
        {
            foreach ( var currentPath in _configuration.PathToApplicationsToMonitor )
            {
                if ( !ProcessUtilities.IsProcessRunning(currentPath) )
                {
                    if ( _configuration.StartApplicationOnlyAfterFirstExecution && !_hasAlreadyStartedManuallyOneTime )
                        continue;

                    logger.LogInformation("Process restarting...");
                    var processInfo = new ProcessStartInfo(currentPath)
                    {
                        // This is very important as if the restarted application searches for assets
                        // in relative folder, it couldn't find them
                        WorkingDirectory = Path.GetDirectoryName(currentPath)
                    };

                    var process = new Process
                    {
                        StartInfo = processInfo
                    };

                    if ( process.Start() )
                    {
                        logger.LogInformation(
                            $"Process \"{_configuration.PathToApplicationsToMonitor}\" restarted succesfully!");
                        ToastService.Notify(
                            $"\"{Path.GetFileNameWithoutExtension(currentPath)}\" is restarting...");
                    }
                    else
                    {
                        logger.LogError($"Cannot restart \"{_configuration.PathToApplicationsToMonitor}\"!");
                        ToastService.Notify(
                            $"Cannot restart \"{Path.GetFileNameWithoutExtension(currentPath)}\"!");
                    }
                }
                else
                {
                    _hasAlreadyStartedManuallyOneTime = true;
                }
            }

            Thread.Sleep(_configuration.CheckInterval);
        }
    }

    private async Task WriteConfigsToFileAsync()
    {
        string currentFolder = Directory.GetCurrentDirectory();
        string configFile = Directory.GetFiles(currentFolder).First(x => x.Contains(ConfigurationFileName));

        if ( string.IsNullOrWhiteSpace(configFile) )
        {
            File.Create(currentFolder + ConfigurationFileName);
            configFile = Directory.GetFiles(currentFolder).First(x => x.Contains(ConfigurationFileName));
        }

        #region [Get configs from form]
        _programs.PathToApplicationsToMonitor = new string[listOfAddedPrograms.Items.Count];
        _programs.SetUserAplicationPathsFromMainForm();
        _programs.SetUserSturtupOption(waitBeforeRestart.Checked);
        _programs.SetUserReCheckTimer(timeTextBox.Text);
        #endregion
        var temp = string.Empty;

        await using ( var sw = new StreamWriter(configFile, false) )
        {
            try
            {
                await sw.WriteAsync("");
                temp = '{' +
                       Environment.NewLine + _programs.GetApplicationPathsForConfigString() +
                       Environment.NewLine + _programs.GetUserReCheckTimer() +
                       Environment.NewLine + _programs.GetUserSturtupOption() +
                       Environment.NewLine + '}';
            }
            catch ( Exception ex )
            {
                ToastService.Notify($"Exception when try to build string to write config file\n{ex}");
            }

            await sw.WriteAsync(temp);
            sw.Close();
        }

        SetCurrentConfigurationFromForm();
    }

    private void StopServiceThread_Click(object sender, EventArgs e)
    {
        if ( _checkerStarted )
        {
            _cancelTokenSource.Cancel();
            ToastService.Notify("RestartOnCrush service stoped.");
            _checkerStarted = false;
            _cancelTokenSource = new();
        }
        else
            ToastService.Notify("There's nothing to stop.");
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
        if ( WindowState != FormWindowState.Minimized )
            return;

        Hide();
        notifyIcon.Visible = true;
    }

    private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
    {
        Show();
        WindowState = FormWindowState.Normal;
        notifyIcon.Visible = false;
    }

    private void selectFileButton_Click(object sender, EventArgs e)
    {
        if ( selectProgramToCheck.ShowDialog() != DialogResult.OK )
            return;

        _filePath = selectProgramToCheck.FileName;

        _fileName = selectProgramToCheck.SafeFileName;
        if ( listOfAddedPrograms.Items.Contains(_fileName) )
        {
            ToastService.Notify($"{_fileName} already added");
        }
        else
        {
            listOfAddedPrograms.Items.Add(_fileName);
            _programs.FullPathToApplicationToMonitor.Add(_filePath);
            ToastService.Notify($"{_fileName} added");
        }

        WriteConfigsToFileAsync().Wait();
    }

    private void removeFileButton_Click(object sender, EventArgs e)
    {
        if ( listOfAddedPrograms.Items.Count > 0 )
        {
            var removedElement = listOfAddedPrograms.SelectedIndex;
            if ( removedElement != NotSelectedElementIndex )
            {
                listOfAddedPrograms.Items.RemoveAt(removedElement);
                _programs.FullPathToApplicationToMonitor.RemoveAt(removedElement);
            }
        }

        WriteConfigsToFileAsync().Wait();
    }

    private void waitBeforeRestart_CheckedChanged(object sender, EventArgs e)
    {
        WriteConfigsToFileAsync().Wait();
    }

    private void timeTextBox_TextChanged(object sender, EventArgs e)
    {
        WriteConfigsToFileAsync().Wait();
    }

    private void waitBeforeRestart_Click(object sender, EventArgs e)
    {
        //WriteConfigsToFileAsync().Wait();
    }

    /// <summary>
    /// Loads localization for the application
    /// </summary>
    private void LoadFormTranslation()
    {
        startServiceButton.Text = _resourceManager.GetString("startServiceButton.Text");
        stopServiceButton.Text = _resourceManager.GetString("stopServiceButton.Text");
        waitBeforeRestart.Text = _resourceManager.GetString("waitBeforeRestart.Text");
        restartPeriod.Text = _resourceManager.GetString("restartPeriod.Text");
        removeFileButton.Text = _resourceManager.GetString("removeFileButton.Text");
        selectFileButton.Text = _resourceManager.GetString("selectFileButton.Text");
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        _restartOnCrashService.Interrupt();
    }
}