using FirstFloor.ModernUI.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WindowsSwitcher.ViewModel
{
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        private ObservableCollection<Process> _allProcessesList;
        private ObservableCollection<Process> _selectedProcessesList;
        private bool _showAllProcesses;
        private string _displayMemberPath;
        private double _interval;
        private string _buttonStartStopContent;
        private bool _listBoxIsEnabled;
        private bool _buttonRefreshIsEnabled;
        private bool _checkBoxShowAllProcessesIsEnabled;
        Timer timer;
        private int NumberOfApplication { get; set; }
        private bool isStart;

        public ObservableCollection<Process> AllProcessList
        {
            get { return _allProcessesList; }
            set
            {
                Set(() => AllProcessList, ref _allProcessesList, value);
            }
        }

        public ObservableCollection<Process> SelectedProcessesList
        {
            get { return _selectedProcessesList; }
            set
            {
                Set(() => SelectedProcessesList, ref _selectedProcessesList, value);
            }
        }

        public bool ShowAllProcesses
        {
            get { return _showAllProcesses; }
            set
            {
                Set(() => ShowAllProcesses, ref _showAllProcesses, value);
            }
        }

        public string DisplayMemberPath
        {
            get { return _displayMemberPath; }
            set
            {
                Set(() => DisplayMemberPath, ref _displayMemberPath, value);
            }
        }

        public double Interval
        {
            get { return _interval; }
            set
            {
                Set(() => Interval, ref _interval, value);
            }
        }

        public string ButtonStartStopContent
        {
            get { return _buttonStartStopContent; }
            set
            {
                Set(() => ButtonStartStopContent, ref _buttonStartStopContent, value);
            }
        }

        public bool ListBoxIsEnabled
        {
            get { return _listBoxIsEnabled; }
            set
            {
                Set(() => ListBoxIsEnabled, ref _listBoxIsEnabled, value);
            }
        }

        public bool ButtonRefreshIsEnabled
        {
            get { return _buttonRefreshIsEnabled; }
            set
            {
                Set(() => ButtonRefreshIsEnabled, ref _buttonRefreshIsEnabled, value);
            }
        }

        public bool CheckBoxShowAllProcessesIsEnabled
        {
            get { return _checkBoxShowAllProcessesIsEnabled; }
            set
            {
                Set(() => CheckBoxShowAllProcessesIsEnabled, ref _checkBoxShowAllProcessesIsEnabled, value);
            }
        }

        public MainViewModel()
        {
            AllProcessList = new ObservableCollection<Process>();
            SelectedProcessesList = new ObservableCollection<Process>();
            Interval = 1;
            isStart = true;
            ButtonStartStopContent = "Start";
            ListBoxIsEnabled = true;
            ButtonRefreshIsEnabled = true;
            CheckBoxShowAllProcessesIsEnabled = true;

            StartCommand = new RelayCommand(() => StartSwitching());
            RefreshCommand = new RelayCommand(() => GetApplicationList());

            GetApplicationList();
        }

        public ICommand StartCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        private void GetApplicationList()
        {
            Process[] procs = Process.GetProcesses();
            var listRezult = new List<Process>();
            if (ShowAllProcesses == true)
            {
                DisplayMemberPath = "ProcessName";
                foreach (var item in procs)
                    listRezult.Add(item);
                listRezult.Sort((x, y) => x.ProcessName.CompareTo(y.ProcessName));
            }
            else
            {
                DisplayMemberPath = "MainWindowTitle";
                foreach (var app in procs)
                {
                    if (app.MainWindowTitle.Length > 0)
                        listRezult.Add(app);
                }
            }
            AllProcessList.Clear();
            foreach (var item in listRezult)
                AllProcessList.Add(item);
        }

        private void StartSwitching()
        {
            if (isStart == true)
            {
                NumberOfApplication = 0;
                try
                {
                    if (SelectedProcessesList.Count == 0)
                        throw new Exception("Select any windows");

                    double seconds = Interval * 1000;
                    timer = new Timer();
                    timer.Interval = seconds;
                    timer.Elapsed += new ElapsedEventHandler(OnTimerEvent);
                    timer.Start();
                    ChangeButtonToStop();
                }
                catch (Exception ex)
                {
                    var btn = MessageBoxButton.OK;
                    ModernDialog.ShowMessage(ex.ToString(), "Помилка", btn);
                }
            }
            else
            {
                timer.Stop();
                ChangeButtonToStart();
            }
        }

        private void ChangeButtonToStart()
        {
            ButtonStartStopContent = "Start";
            isStart = true;
            ListBoxIsEnabled = true;
            ButtonRefreshIsEnabled = true;
            CheckBoxShowAllProcessesIsEnabled = true;
        }

        private void ChangeButtonToStop()
        {
            ButtonStartStopContent = "Stop";
            isStart = false;
            ListBoxIsEnabled = false;
            ButtonRefreshIsEnabled = false;
            CheckBoxShowAllProcessesIsEnabled = false;
        }

        private void OnTimerEvent(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            new Action(() => {
                try
                {
                    var app = SelectedProcessesList[NumberOfApplication];
                    IntPtr hwnd = (app as Process).MainWindowHandle;
                    SetForegroundWindow(hwnd);

                    NumberOfApplication++;
                    if (NumberOfApplication == SelectedProcessesList.Count)
                        NumberOfApplication = 0;

                    return;
                }
                catch (Exception ex)
                {
                    var btn = MessageBoxButton.OK;
                    ModernDialog.ShowMessage(ex.ToString(), "Помилка", btn);
                }
            }));
        }

        #region IDataErrorInfo implementation
        public string Error
        {
            get { return null; }
        }
        public string this[string columnName]
        {
            get { return null; }
        }

        #endregion
    }
}