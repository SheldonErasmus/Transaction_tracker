using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using transaction_tracker.models;

namespace Transaction_tracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<Transaction> _transactions = [];

        public MainWindow()
        {
            InitializeComponent();
            TransactionsGrid.ItemsSource = _transactions;
        }

        private async void OnLoadCsvClicked(object? sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Open CSV File",
                Multiselect = false,
                Filter = "CSV Files (*.csv)|*.csv",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileStream = openFileDialog.OpenFile();
                using var reader = new StreamReader(fileStream);

                await LoadTransactionsFromCsv(reader);
                UpdateOverview();
            }
        }

        private async Task LoadTransactionsFromCsv(StreamReader reader)
        {
            _transactions.Clear();
            bool isFirstLine = true;
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (isFirstLine) { isFirstLine = false; continue; }
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 6) continue;
                _transactions.Add(new Transaction
                {
                    Date = parts[0],
                    Description = parts[3],
                    Type = parts[2],
                    Amount = double.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) ? amt : 0
                });
            }
        }

        private void UpdateOverview()
        {
            var grouped = _transactions
                .GroupBy(t => t.Type)
                .Select(g => $"{g.Key}: {g.Sum(t => t.Amount)}");

            OverviewList.ItemsSource = grouped.ToList();
        }

        private void AboutClick(object? sender, RoutedEventArgs e)
        {
            MessageBox.Show("A transaction traking app");
        }

        private void OnExitClick(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}