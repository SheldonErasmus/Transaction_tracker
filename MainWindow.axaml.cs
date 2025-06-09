using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using transaction_tracker.models;
using Avalonia.Platform.Storage;

namespace transaction_tracker;

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
        var files = await this.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open CSV File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("CSV Files") { Patterns = new[] { "*.csv" } }
            ]
        });

        if (files == null || files.Count == 0) return;

        var file = files[0];
        using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);

        LoadTransactionsFromCsv(reader);
        UpdateOverview();
    }

    private async void LoadTransactionsFromCsv(StreamReader reader)
    {
        _transactions.Clear();
        bool isFirstLine = true;
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (isFirstLine) { isFirstLine = false; continue; }
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(',');
            if (parts.Length < 4) continue;
            _transactions.Add(new Transaction
            {
                Date = parts[0],
                Description = parts[1],
                Type = parts[2],
                Amount = double.TryParse(parts[3], out var amt) ? amt : 0
            });
        }
    }

    private void UpdateOverview()
    {
        var grouped = _transactions
            .GroupBy(t => t.Type)
            .Select(g => $"{g.Key}: {g.Sum(t => t.Amount):C}");
        OverviewList.ItemsSource = grouped.ToList();
    }
}