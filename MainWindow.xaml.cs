using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CompactTaskManager
{
    public partial class MainWindow : Window
    {
        private string columnHeader;
        private DispatcherTimer refreshUITimer;
        private DispatcherTimer cleanCacheTimer;
        private bool sortAscending;

        public ProcessViewModel ProcessViewModel { get; set; } = new ProcessViewModel();

        #region Startup and Load
        public MainWindow()
        {
            InitializeComponent();
            //create a new timer for refreshing of UI
            refreshUITimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1.0)
            };
            refreshUITimer.Tick += new EventHandler(DispatcherTimer_Tick);
            refreshUITimer.Start();

            cleanCacheTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(2)
            };
            cleanCacheTimer.Tick += ResetCacheTimer_Tick;
            cleanCacheTimer.Start();
        }

        private async void ResetCacheTimer_Tick(object sender, EventArgs e)
        {
            await Task.Run(() => ImageManager.CleanCache());
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = ProcessViewModel;
            ProcessViewModel.PropertyChanged += new PropertyChangedEventHandler(ProcessViewModel_PropertyChanged);
        }
        #endregion

        #region Update UI
        private async void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (ProcessViewModel.IsProcessing) return;
            await ProcessViewModel.UpdateProcesses();
            await ProcessViewModel.SortProcesses(columnHeader, sortAscending);
        }

        /// <summary>
        /// After the notify event, the processesListView is getting updated
        /// </summary>
        private void ProcessViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            processesListView.Items.Refresh();
            processesCountLabel.Content = "Processes count: " + ProcessViewModel.AllProcesses.Count.ToString();
        }
        #endregion

        #region Adjusting UI

        /// <summary>
        /// Change organization of tabs, change ascending / descending
        /// </summary>
        private async void processesListView_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumn clickedColumn = (e.OriginalSource as GridViewColumnHeader).Column;
            if (clickedColumn == null)
                return;

            //if you click on the same header multiple times, it will change ascending / descending type
            if (clickedColumn.Header.ToString() == columnHeader)
                sortAscending = !sortAscending;
            columnHeader = clickedColumn.Header.ToString();

            try
            {
                await ProcessViewModel.SortProcesses(columnHeader, sortAscending);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error when sorting processes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Switch between the modes
        /// </summary>

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            ProcessViewModel.MinimalMode = !expander.IsExpanded;
            if (ProcessViewModel.MinimalMode)
            {
                expander.Header = "Show more";
            }
            else
            {
                expander.Header = "Show less";
            }

            ProcessViewModel.ClearProcesses();
            DispatcherTimer_Tick(this, e);
        }

        /// <summary>
        /// Switch between showing all processes
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ProcessViewModel.ClearProcesses();
            DispatcherTimer_Tick(this, e);
        }

        #endregion

        #region Managing a process
        /// <summary>
        /// Select a process and save it to ProcessViewModel
        /// </summary>
        private void processesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (processesListView.SelectedItem == null)
            {
                return;
            }
            ProcessViewModel.SelectedProcess = (ProcessModel)processesListView.SelectedItem;
        }

        /// <summary>
        /// Open context menu and pick an option
        /// </summary>
        private void ContextMenu_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ProcessModel selectedProcess = ProcessViewModel.SelectedProcess;
            if (selectedProcess == null || !(e.Source is MenuItem))
            {
                return;
            }

            if ((e.Source as MenuItem).Header is string header)
            {

                switch (header) 
                {
                    case "Kill":
                        selectedProcess.Kill();
                        break;
                    case "Kill All Processes This Name":
                        selectedProcess.KillAllWithThisName();
                        break;
                    case "Focus":
                        selectedProcess.Focus();
                        break;
                    case "Maximize":
                        selectedProcess.Maximize();
                        break;
                    case "Minimize":
                        selectedProcess.Minimize();
                        break;
                    case "Restore":
                        selectedProcess.Restore();
                        break;
                    case "Open File Location":
                        {
                            if (selectedProcess.FileName == null)
                            {
                                return;
                            }
                            ProcessViewModel.StartProcess("explorer", selectedProcess.FileName.Replace("\\"+selectedProcess.ShortName, ""));
                            break;
                        }
                    //TO DO
                    /*case "Start New Instance": 
                        {
                            if (selectedProcess.FileName == null)
                            {
                                return;
                            }
                            ProcessViewModel.StartProcess(selectedProcess.FileName, selectedProcess.Arguments);
                            break;
                        }*/
                }
            }
        }

        private void endTaskButton_Click(object sender, RoutedEventArgs e) => ProcessViewModel.SelectedProcess?.Kill();

        private void newTaskButton_Click(object sender, RoutedEventArgs e) => new NewProcess().ShowDialog();

        private void MenuItem_Click(object sender, RoutedEventArgs e) => Topmost = !Topmost;

        private void MenuItem_Click_1(object sender, RoutedEventArgs e) => refreshUITimer.Interval = TimeSpan.FromSeconds(1.0);

        private void MenuItem_Click_2(object sender, RoutedEventArgs e) => refreshUITimer.Interval = TimeSpan.FromSeconds(2.0);

        private void MenuItem_Click_3(object sender, RoutedEventArgs e) => refreshUITimer.Interval = TimeSpan.FromSeconds(4.0);

        private void MenuItem_Click_4(object sender, RoutedEventArgs e) => refreshUITimer.Interval = TimeSpan.FromSeconds(10.0);
        #endregion 
    }
}
