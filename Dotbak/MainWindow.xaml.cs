using dotbak.Models;
using dotbak.Services;
using dotbak.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace Dotbak.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ServerInfoServices serverInfoService = new ServerInfoServices();
        private DispatcherTimer dispatcherTimer;
        private string path = string.Empty;
        int switcher = 0;
        public MainWindow()
        {
            InitializeComponent();
            GetServerInfo();
            GetDatabases();
        }
        private void CmbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedDbName = CmbDatabases.SelectedItem.ToString() ?? string.Empty;
            ServerDatabase databaseInfo = this.serverInfoService.GetDatabaseInfoByName(selectedDbName);
            TxtDatabaselocation.Text = databaseInfo.DatabaseLocation;
            TxtDatabaseCreatedDate.Text = $"Created In: {databaseInfo.CreatedDate}";
        }
        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var dialog = new WinForms.FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    if (result == WinForms.DialogResult.OK)
                    {
                        path = dialog.SelectedPath;
                        SelectedPathTxtBox.Text = $"Selected Path: {path.Trim()}";
                        if (CmbDatabases.SelectedIndex >= 0)
                            BackupBtn.Visibility = Visibility.Visible;
                        else
                            BackupBtn.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void BackupBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbDatabases.SelectedIndex < 0)
                {
                    System.Windows.MessageBox.Show("Please Select Database", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (string.IsNullOrEmpty(path))
                {
                    System.Windows.MessageBox.Show("Please Select Database Location", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    string databaseName = (string)CmbDatabases.SelectedItem;
                    string filePath = $"{path}\\{databaseName}_{DateTime.Now.ToString("ddMMyyyyhhmmss")}.bak";
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show($"BackUp Database {databaseName}", $"Are you sure?", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        StartLoader();
                        await Task<string>.Run(() => this.serverInfoService.CreateBackup(filePath, databaseName));
                        EndLoader();
                        SuccessDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                EndLoader();
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ResetForm();
            }
        }

        #region METHODS
        private void GetServerInfo()
        {
            try
            {
                Server server = this.serverInfoService.GetServers();
                TxtServerName.Text = server.ServerName;
                TxtProviderName.Text = server.ProviderName;
                TxtProviderType.Text = server.ProviderType;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void GetDatabases()
        {
            try
            {
                List<string> serverDatabase = this.serverInfoService.GetDatabases();
                serverDatabase.ForEach(e =>
                {
                    CmbDatabases.Items.Add(e);
                });
                CmbDatabases.SelectedIndex = 0;
                TxtDatabaseCreatedDate.Text = string.Empty;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void StartLoader()
        {
            BackupBtn.Visibility = Visibility.Collapsed;
            SetUpTimer(Loader);
        }
        private void EndLoader()
        {
            dispatcherTimer.Stop();
            BackupBtn.Visibility = Visibility.Visible;
            switcher = 0;
            ResetForm();
        }
        private void SuccessDialog()
        {
            System.Windows.MessageBox.Show("BackUp Process Completed Successfully ❤", "Successed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Loader(object sender, EventArgs e)
        {
            switch (switcher)
            {
                case 0:
                    Image_LoadImage.Source = new BitmapImage(new Uri(Constants.loading1dot, UriKind.Relative));
                    break;
                case 1:
                    Image_LoadImage.Source = new BitmapImage(new Uri(Constants.loading2dot, UriKind.Relative));
                    break;
                case 2:
                    Image_LoadImage.Source = new BitmapImage(new Uri(Constants.loading3dot, UriKind.Relative));
                    break;
                default:
                    break;
            }
            if (switcher == 2) switcher = 0; else switcher++;
        }
        private void SetUpTimer(EventHandler @event)
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(@event);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void ResetForm()
        {
            TxtDatabaselocation.Text = string.Empty;
            TxtDatabaseCreatedDate.Text = string.Empty;
            SelectedPathTxtBox.Text = string.Empty;
            Image_LoadImage.Source = null;
            CmbDatabases.SelectedIndex = 0;
        }
        #endregion METHODS
    }
}
