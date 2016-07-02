using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HomeSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            syncButton.Click += SyncButton_Click;

            StartTimer();
        }

        private async void StartTimer()
        {
            while (true)
            {
                Sync();
                await Task.Delay(5 * 60 * 1000);
            }
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            Sync();
        }

        private void Sync()
        {
            lastUpdate.Text = "Last sync: Working...";

            IEnumerable<string> remotedir = null;
            IEnumerable<string> localdir = null;

            try
            {
                remotedir = Directory.EnumerateFiles(remote.Text).Select(System.IO.Path.GetFileName);
                localdir = Directory.EnumerateFiles(local.Text).Select(System.IO.Path.GetFileName);
            }
            catch (Exception)
            {

                lastUpdate.Text = "Last sync: Directory not found";
                return;
            }
            

            var newRemote = remotedir.Except(localdir);
            var newLocal = localdir.Except(remotedir);
            var matching = remotedir.Intersect(localdir);

            foreach (var file in newRemote)
            {
                try
                {
                    File.Copy(remote.Text + file, local.Text + file, true);
                    Log($"New {file}");
                }
                catch (Exception)
                {

                    Log($"[FAILED] New {file}");
                }
            }

            foreach (var file in newLocal)
            {
                try
                {
                    File.Copy(local.Text + file, remote.Text + file, true);
                    Log($"Sending {file}");
                }
                catch (Exception)
                {

                    Log($"[FAILED] Sending {file}");
                }
            }

            foreach (var file in matching)
            {
                var localMinusRemote = File.GetLastWriteTimeUtc(local.Text + file) - File.GetLastWriteTimeUtc(remote.Text + file);

                if (localMinusRemote > TimeSpan.Zero)
                {
                    try
                    {
                        File.Copy(local.Text + file, remote.Text + file, true);
                        Log($"Update {file}");
                    }
                    catch (Exception ex)
                    {

                        Log($"[FAILED] Update {file}");
                    }
                }
                else if (localMinusRemote < TimeSpan.Zero)
                {
                    try
                    {
                        File.Copy(remote.Text + file, local.Text + file, true);
                        Log($"Outdated {file}");
                    }
                    catch (Exception ex)
                    {

                        Log($"[FAILED] Outdated {file}");
                    }
                }
            }

            lastUpdate.Text = "Last sync: " + DateTime.Now.ToShortTimeString();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("Program started.");
        }

        private void Log(string s)
        {
            this.eventLog.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " : " + s);
        }
    }
}
