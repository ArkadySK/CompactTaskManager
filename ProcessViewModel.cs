using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace CompactTaskManager
{
    public class ProcessViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged; // interface property

        public ProcessModel SelectedProcess { get; set; } //selected process
        public bool IsAdmin { get; set; } = true; //are admin rights allowed?
        public bool MinimalMode { get; set; } = true; //show only programs that have main thread (result of it is the same as taskbar)
        public ObservableCollection<ProcessModel> AllProcesses { get; private set; } = new ObservableCollection<ProcessModel>();


        public static void StartProcess(string name, string args)
        {
            try
            {
                Process.Start(new ProcessStartInfo(name, args));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        public void ClearProcesses() => AllProcesses.Clear();

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => Application.Current.Dispatcher.Invoke(() =>
        {
            PropertyChangedEventHandler propertyChanged = PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        });

        private void AddTask(Process process)
        {
            if (Application.Current.Dispatcher == null || string.IsNullOrEmpty(process.ProcessName) || MinimalMode && string.IsNullOrEmpty(process.MainWindowTitle))
            {
                return;
            }

            ProcessModel processModel = new ProcessModel(process, IsAdmin);
            if (AllProcesses.Contains(processModel))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() => AllProcesses.Add(processModel));
        }

        public async Task UpdateProcesses()
        {
            List<Task> processTasks = new List<Task>();
            Process[] processes = Process.GetProcesses();
            List<ProcessModel> processesToRemove = AllProcesses.ToList();
            Process[] processArray = processes;
            for (int index = 0; index < processArray.Length; ++index)
            {
                Process p = processArray[index];
                ProcessModel x = processesToRemove.FirstOrDefault(op => op.Id == p.Id);
                if (x == null && p != null)
                {
                    processTasks.Add(Task.Run((() => AddTask(p))));
                }

                ProcessModel pm = processesToRemove.FirstOrDefault(op => op.Id == p.Id);
                processesToRemove.Remove(pm);
                x = null;
                pm = null;
            }
            processArray = null;
            foreach (ProcessModel pm in processesToRemove)
            {
                AllProcesses.Remove(pm);
            }

            await Task.WhenAll(processTasks);
            processTasks = null;
            processes = null;
            processesToRemove = null;
        }

        public async Task UpdateMinimalisticProcesses()
        {
            ObservableCollection<ProcessModel> observableCollection = await Task.Run(() => new ObservableCollection<ProcessModel>(AllProcesses.Where(proc => proc != null).Where(proc => proc.Title != string.Empty).OrderBy(proc => proc.Id)));
            AllProcesses = observableCollection;
            observableCollection = null;
            await Task.Delay(100);
            NotifyPropertyChanged(nameof(UpdateMinimalisticProcesses));
        }

        public async Task SortProcesses(string propertyToOrderBy, bool ascending)
        {
            IOrderedEnumerable<ProcessModel> sortProcessesQuery = null;
            switch(propertyToOrderBy)
            {
                case "Path":
                    sortProcessesQuery = await Task.Run(() => from proc in AllProcesses
                                                              orderby proc.FileName ascending
                                                              select proc);
                    break;
                case "Name":
                    sortProcessesQuery = await Task.Run(() => from proc in AllProcesses
                                                              orderby proc.ProcessName ascending
                                                              select proc);
                    break;
                default:
                    sortProcessesQuery = await Task.Run(() => from proc in AllProcesses
                                                              orderby proc.Id ascending 
                                                              select proc);
                    break;
            }
                

            if (!ascending) //if ascending, reverse query
            {
                await Task.Run(() => sortProcessesQuery.Reverse());
            }

            ObservableCollection<ProcessModel> observableCollection = await Task.Run(() => new ObservableCollection<ProcessModel>(sortProcessesQuery));
            AllProcesses = observableCollection;
            observableCollection = null;
            await Task.Delay(10);
            NotifyPropertyChanged(nameof(SortProcesses));
        }

        public void EndProcess() => SelectedProcess.Kill();
    }
}
