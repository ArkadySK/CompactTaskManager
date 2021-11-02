using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;

namespace CompactTaskManager
{
    /// <summary>
    /// Provides UI for launching of a new process
    /// </summary>
    public partial class NewProcess : Window
    {
        public NewProcess()
        {
            InitializeComponent();
        }

        private void locateExeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog().GetValueOrDefault())
                pathTextBox.Text = fileDialog.FileName;
        }

        /// <summary>
        /// Confirm to run a new process
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessViewModel.StartProcess(pathTextBox.Text, argumentsTextBox.Text);
            Close();
        }
    }
}
