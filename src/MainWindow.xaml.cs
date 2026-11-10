using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// Disambiguation aliases
using WpfMessageBox = System.Windows.MessageBox;
using WpfMessageBoxButton = System.Windows.MessageBoxButton;
using WpfMessageBoxImage = System.Windows.MessageBoxImage;
using WpfMessageBoxResult = System.Windows.MessageBoxResult;
using WpfColor = System.Windows.Media.Color;
using WpfBrush = System.Windows.Media.Brush;

namespace F1GridEditorApp;

public partial class MainWindow : Window
{
    public Strings Loc { get; } = new();
    public GridConfig CurrentGrid { get; set; } = new();
    private TeamConfig? _currentEditingTeam;
    private bool _isUpdatingUi = false;
    private bool _logsCollapsed = false;

    private readonly string _appDir;
    private readonly string _presetsDir;
    private readonly string _rawLngDir;
    private readonly string _cliScriptPath;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _appDir = AppDomain.CurrentDomain.BaseDirectory;
        _presetsDir = Path.Combine(_appDir, "presets");
        _rawLngDir = Path.Combine(_appDir, "raw_languages");
        _cliScriptPath = Path.Combine(_appDir, "patch_cli.js");

        // Populate nationalities in combo boxes
        cmbNatDriver1.ItemsSource = NationalitiesData.All;
        cmbNatDriver2.ItemsSource = NationalitiesData.All;

        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateLangButtonStyles();
        Log("===================================================================");
        Log("       F1 2020 GRID & DRIVER CUSTOMIZER - v2.0 DESKTOP SUITE       ");
        Log("===================================================================");
        Log($"App directory: {_appDir}");

        // Load default preset (2026)
        LoadPresetFile(Path.Combine(_presetsDir, "grid_2026.json"), "2026 Default Grid");

        // Determine default game folder
        string downloadsGame = @"C:\Users\ciroj\Downloads\F1 2020";
        string gamesGame = @"C:\Games\F1 2020";

        if (Directory.Exists(downloadsGame))
        {
            txtGamePath.Text = downloadsGame;
            Log($"Detected clean F1 2020 installation in Downloads: {downloadsGame}");
        }
        else if (Directory.Exists(gamesGame))
        {
            txtGamePath.Text = gamesGame;
            Log($"Detected F1 2020 installation in Games: {gamesGame}");
        }
        else
        {
            txtGamePath.Text = Directory.GetCurrentDirectory();
        }

        AuditSelectedGameDir();
    }

    private void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{time}] {message}\r\n");
            txtLog.ScrollToEnd();
        });
    }

    private void BtnClearLog_Click(object sender, RoutedEventArgs e)
    {
        txtLog.Clear();
    }

    private void BtnToggleLogs_Click(object sender, RoutedEventArgs e)
    {
        _logsCollapsed = !_logsCollapsed;
        rowLogs.Height = _logsCollapsed ? new GridLength(28) : new GridLength(85);
    }

    #region Language Selection

    private void BtnLangEn_Click(object sender, RoutedEventArgs e)
    {
        Loc.CurrentLanguage = AppLanguage.English;
        UpdateLangButtonStyles();
        AuditSelectedGameDir();
    }

    private void BtnLangEs_Click(object sender, RoutedEventArgs e)
    {
        Loc.CurrentLanguage = AppLanguage.Spanish;
        UpdateLangButtonStyles();
        AuditSelectedGameDir();
    }

    private void BtnLangRu_Click(object sender, RoutedEventArgs e)
    {
        Loc.CurrentLanguage = AppLanguage.Russian;
        UpdateLangButtonStyles();
        AuditSelectedGameDir();
    }

    private void UpdateLangButtonStyles()
    {
        btnLangEn.Background = Loc.CurrentLanguage == AppLanguage.English ? new SolidColorBrush(WpfColor.FromRgb(0x35, 0x50, 0x70)) : new SolidColorBrush(WpfColor.FromRgb(0x22, 0x25, 0x2E));
        btnLangEs.Background = Loc.CurrentLanguage == AppLanguage.Spanish ? new SolidColorBrush(WpfColor.FromRgb(0x35, 0x50, 0x70)) : new SolidColorBrush(WpfColor.FromRgb(0x22, 0x25, 0x2E));
        btnLangRu.Background = Loc.CurrentLanguage == AppLanguage.Russian ? new SolidColorBrush(WpfColor.FromRgb(0x35, 0x50, 0x70)) : new SolidColorBrush(WpfColor.FromRgb(0x22, 0x25, 0x2E));
    }

    #endregion

    #region Folder Selection & Auditing

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = Loc.CurrentLanguage == AppLanguage.Spanish ? "Selecciona la carpeta de F1 2020" :
                    Loc.CurrentLanguage == AppLanguage.Russian ? "Выберите папку с F1 2020" :
                    "Select F1 2020 installation folder",
            InitialDirectory = Directory.Exists(txtGamePath.Text) ? txtGamePath.Text : @"C:\"
        };

        if (dialog.ShowDialog() == true)
        {
            txtGamePath.Text = dialog.FolderName;
        }
    }

    private void BtnSelectDownloads_Click(object sender, RoutedEventArgs e)
    {
        txtGamePath.Text = @"C:\Users\ciroj\Downloads\F1 2020";
    }

    private void BtnSelectGames_Click(object sender, RoutedEventArgs e)
    {
        txtGamePath.Text = @"C:\Games\F1 2020";
    }

    private void TxtGamePath_TextChanged(object sender, TextChangedEventArgs e)
    {
        AuditSelectedGameDir();
    }

    private void BtnReAudit_Click(object sender, RoutedEventArgs e)
    {
        AuditSelectedGameDir();
    }

    private void AuditSelectedGameDir()
    {
        string dir = txtGamePath.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
        {
            SetStatusBadge(false, false, Loc.StatusPathNotFound);
            return;
        }

        string dx11 = Path.Combine(dir, "F1_2020.exe");
        string dx12 = Path.Combine(dir, "F1_2020_dx12.exe");
        string db = Path.Combine(dir, "game_package", "gamedatabases", "gamedatabases.bin");
        string loc = Path.Combine(dir, "localisatioa");

        bool hasExe = File.Exists(dx11) || File.Exists(dx12);
        bool hasDb = File.Exists(db);

        if (!hasExe || !hasDb)
        {
            SetStatusBadge(false, false, Loc.StatusInvalidFolder);
            return;
        }

        bool hasLocalisatioa = Directory.Exists(loc);

        Task.Run(() =>
        {
            try
            {
                var audit = RunCliAudit(dir);
                Dispatcher.Invoke(() =>
                {
                    if (audit != null && audit.Valid)
                    {
                        if (audit.IsPatched)
                        {
                            SetStatusBadge(true, true, Loc.StatusPatched);
                        }
                        else
                        {
                            SetStatusBadge(true, false, Loc.StatusCleanStock);
                        }
                    }
                    else
                    {
                        SetStatusBadge(true, hasLocalisatioa, Loc.StatusDetected);
                    }
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    SetStatusBadge(true, hasLocalisatioa, Loc.StatusDetected);
                });
            }
        });
    }

    private void SetStatusBadge(bool valid, bool isPatched, string message)
    {
        txtStatus.Text = message;
        if (!valid)
        {
            borderStatus.Background = new SolidColorBrush(WpfColor.FromRgb(0x28, 0x12, 0x14));
            borderStatus.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x88, 0x22, 0x22));
            txtStatus.Foreground = new SolidColorBrush(WpfColor.FromRgb(0xFF, 0x88, 0x88));
        }
        else if (isPatched)
        {
            borderStatus.Background = new SolidColorBrush(WpfColor.FromRgb(0x10, 0x20, 0x2C));
            borderStatus.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x00, 0x88, 0xB8));
            txtStatus.Foreground = new SolidColorBrush(WpfColor.FromRgb(0x55, 0xDD, 0xFF));
        }
        else
        {
            borderStatus.Background = new SolidColorBrush(WpfColor.FromRgb(0x10, 0x26, 0x1A));
            borderStatus.BorderBrush = new SolidColorBrush(WpfColor.FromRgb(0x22, 0x99, 0x55));
            txtStatus.Foreground = new SolidColorBrush(WpfColor.FromRgb(0x77, 0xEE, 0xAA));
        }
    }

    #endregion

    #region Preset Management

    private void BtnLoad2026_Click(object sender, RoutedEventArgs e)
    {
        string p = Path.Combine(_presetsDir, "grid_2026.json");
        LoadPresetFile(p, "2026 Grid Preset");
    }

    private void BtnLoadStock_Click(object sender, RoutedEventArgs e)
    {
        string p = Path.Combine(_presetsDir, "grid_stock_2020.json");
        LoadPresetFile(p, "Stock 2020 Grid");
    }

    private void LoadPresetFile(string filePath, string label)
    {
        if (!File.Exists(filePath))
        {
            Log($"[WARN] Preset file not found: {filePath}");
            return;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            var grid = JsonSerializer.Deserialize<GridConfig>(json);
            if (grid != null && grid.Teams.Count > 0)
            {
                CurrentGrid = grid;
                lstTeams.ItemsSource = CurrentGrid.Teams;
                lstTeams.SelectedIndex = 0;
                Log($"[PRESET] Loaded: {label}");
            }
        }
        catch (Exception ex)
        {
            Log($"[ERROR] Failed to load preset: {ex.Message}");
        }
    }

    private void BtnExportJson_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentEditingTeam();
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            FileName = "my_f1_2020_grid.json",
            InitialDirectory = _presetsDir
        };

        if (dlg.ShowDialog() == true)
        {
            try
            {
                string json = JsonSerializer.Serialize(CurrentGrid, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dlg.FileName, json);
                Log($"[EXPORT] Preset saved to: {dlg.FileName}");
                WpfMessageBox.Show("Preset exported successfully.", "F1 2020 Grid Customizer", WpfMessageBoxButton.OK, WpfMessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Log($"[ERROR] Export failed: {ex.Message}");
            }
        }
    }

    private void BtnImportJson_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            InitialDirectory = _presetsDir
        };

        if (dlg.ShowDialog() == true)
        {
            LoadPresetFile(dlg.FileName, Path.GetFileName(dlg.FileName));
        }
    }

    #endregion

    #region Team & Driver Editing

    private void LstTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SaveCurrentEditingTeam();

        var team = lstTeams.SelectedItem as TeamConfig;
        if (team == null) return;

        _currentEditingTeam = team;
        _isUpdatingUi = true;

        txtTeamName.Text = team.TeamName;
        txtTeamShort.Text = team.TeamShortVal;
        txtTeamTitle.Text = team.TeamTitleVal;
        txtTeamColor.Text = team.TeamColor;
        UpdateColorPreview(team.TeamColor);

        // Driver 1
        if (team.Driver1 != null)
        {
            lblDriver1Slot.Text = team.Driver1.SlotName;
            lblDriver1Orig.Text = $"Original: {team.Driver1.OriginalName}";
            txtD1First.Text = team.Driver1.TargetFirstName;
            txtD1Last.Text = team.Driver1.TargetLastName;
            txtD1Init.Text = team.Driver1.TargetInitials;
            cmbNatDriver1.SelectedItem = team.Driver1.SelectedNationalityItem;
        }

        // Driver 2
        if (team.Driver2 != null)
        {
            lblDriver2Slot.Text = team.Driver2.SlotName;
            lblDriver2Orig.Text = $"Original: {team.Driver2.OriginalName}";
            txtD2First.Text = team.Driver2.TargetFirstName;
            txtD2Last.Text = team.Driver2.TargetLastName;
            txtD2Init.Text = team.Driver2.TargetInitials;
            cmbNatDriver2.SelectedItem = team.Driver2.SelectedNationalityItem;
        }

        _isUpdatingUi = false;
    }

    private void SaveCurrentEditingTeam()
    {
        if (_currentEditingTeam == null || _isUpdatingUi) return;

        _currentEditingTeam.TeamName = txtTeamName.Text?.Trim() ?? "";
        _currentEditingTeam.TeamShortVal = txtTeamShort.Text?.Trim() ?? "";
        _currentEditingTeam.TeamTitleVal = txtTeamTitle.Text?.Trim() ?? "";
        _currentEditingTeam.TeamColor = txtTeamColor.Text?.Trim() ?? "#FFFFFF";

        // Driver 1
        if (_currentEditingTeam.Driver1 != null)
        {
            _currentEditingTeam.Driver1.TargetFirstName = txtD1First.Text?.Trim() ?? "";
            _currentEditingTeam.Driver1.TargetLastName = txtD1Last.Text?.Trim() ?? "";
            _currentEditingTeam.Driver1.TargetInitials = txtD1Init.Text?.Trim()?.ToUpperInvariant() ?? "";
            var nat1 = cmbNatDriver1.SelectedItem as NationalityItem;
            if (nat1 != null) _currentEditingTeam.Driver1.TargetNationality = nat1.Id;
        }

        // Driver 2
        if (_currentEditingTeam.Driver2 != null)
        {
            _currentEditingTeam.Driver2.TargetFirstName = txtD2First.Text?.Trim() ?? "";
            _currentEditingTeam.Driver2.TargetLastName = txtD2Last.Text?.Trim() ?? "";
            _currentEditingTeam.Driver2.TargetInitials = txtD2Init.Text?.Trim()?.ToUpperInvariant() ?? "";
            var nat2 = cmbNatDriver2.SelectedItem as NationalityItem;
            if (nat2 != null) _currentEditingTeam.Driver2.TargetNationality = nat2.Id;
        }

        // Recompute showroom lineup
        if (_currentEditingTeam.Driver1 != null && _currentEditingTeam.Driver2 != null)
        {
            _currentEditingTeam.ShowroomLineup = $"{_currentEditingTeam.Driver1.TargetFullName}\n{_currentEditingTeam.Driver2.TargetFullName}";
        }
    }

    private void TxtTeamColor_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi) return;
        UpdateColorPreview(txtTeamColor.Text);
    }

    private void UpdateColorPreview(string hex)
    {
        try
        {
            var brush = (WpfBrush)new BrushConverter().ConvertFromString(hex);
            rectColorPreview.Background = brush;
            borderTeamColor.Background = brush;
            if (_currentEditingTeam != null)
            {
                _currentEditingTeam.TeamColor = hex;
            }
        }
        catch
        {
            // ignore partial hex typing
        }
    }

    private void BtnPickColor_Click(object sender, RoutedEventArgs e)
    {
        using var colorDlg = new System.Windows.Forms.ColorDialog
        {
            FullOpen = true,
            AnyColor = true
        };

        if (colorDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string hex = $"#{colorDlg.Color.R:X2}{colorDlg.Color.G:X2}{colorDlg.Color.B:X2}";
            txtTeamColor.Text = hex;
            UpdateColorPreview(hex);
            if (_currentEditingTeam != null)
            {
                _currentEditingTeam.TeamColor = hex;
            }
            lstTeams.Items.Refresh();
        }
    }

    private void TxtD1Names_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi || _currentEditingTeam?.Driver1 == null) return;
        _currentEditingTeam.Driver1.TargetFirstName = txtD1First.Text;
        _currentEditingTeam.Driver1.TargetLastName = txtD1Last.Text;
    }

    private void TxtD2Names_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdatingUi || _currentEditingTeam?.Driver2 == null) return;
        _currentEditingTeam.Driver2.TargetFirstName = txtD2First.Text;
        _currentEditingTeam.Driver2.TargetLastName = txtD2Last.Text;
    }

    #endregion

    #region Apply and Rollback Execution

    private async void BtnApply_Click(object sender, RoutedEventArgs e)
    {
        SaveCurrentEditingTeam();
        string targetGameDir = txtGamePath.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(targetGameDir) || !Directory.Exists(targetGameDir))
        {
            WpfMessageBox.Show(Loc.StatusInvalidFolder, "Error", WpfMessageBoxButton.OK, WpfMessageBoxImage.Warning);
            return;
        }

        string confirmMsg = string.Format(Loc.ConfirmApplyMsg, targetGameDir);
        if (WpfMessageBox.Show(confirmMsg, Loc.ConfirmApplyTitle, WpfMessageBoxButton.YesNo, WpfMessageBoxImage.Question) != WpfMessageBoxResult.Yes)
        {
            return;
        }

        btnApply.IsEnabled = false;
        btnRollback.IsEnabled = false;

        Log("-------------------------------------------------------------------");
        Log($"APPLYING GRID MOD TO: {targetGameDir}");

        try
        {
            string tempConfig = Path.Combine(_appDir, "active_patch_config.json");
            string json = JsonSerializer.Serialize(CurrentGrid, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tempConfig, json);

            string args = $"\"{_cliScriptPath}\" apply --game-dir \"{targetGameDir}\" --config \"{tempConfig}\" --raw-lng \"{_rawLngDir}\"";
            await RunProcessAsync("node", args);

            Log("PROCESS COMPLETE!");
            AuditSelectedGameDir();

            WpfMessageBox.Show(Loc.SuccessApplyMsg, Loc.SuccessApplyTitle, WpfMessageBoxButton.OK, WpfMessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Log($"[CRITICAL ERROR] Failed to apply mod: {ex.Message}");
            WpfMessageBox.Show($"Error applying mod:\n{ex.Message}", "Error", WpfMessageBoxButton.OK, WpfMessageBoxImage.Error);
        }
        finally
        {
            btnApply.IsEnabled = true;
            btnRollback.IsEnabled = true;
        }
    }

    private async void BtnRollback_Click(object sender, RoutedEventArgs e)
    {
        string targetGameDir = txtGamePath.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(targetGameDir) || !Directory.Exists(targetGameDir))
        {
            WpfMessageBox.Show(Loc.StatusInvalidFolder, "Error", WpfMessageBoxButton.OK, WpfMessageBoxImage.Warning);
            return;
        }

        string confirmMsg = string.Format(Loc.ConfirmRollbackMsg, targetGameDir);
        if (WpfMessageBox.Show(confirmMsg, Loc.ConfirmRollbackTitle, WpfMessageBoxButton.YesNo, WpfMessageBoxImage.Warning) != WpfMessageBoxResult.Yes)
        {
            return;
        }

        btnApply.IsEnabled = false;
        btnRollback.IsEnabled = false;

        Log("-------------------------------------------------------------------");
        Log($"ROLLING BACK INSTALLATION IN: {targetGameDir}");

        try
        {
            string args = $"\"{_cliScriptPath}\" rollback --game-dir \"{targetGameDir}\"";
            await RunProcessAsync("node", args);

            Log("STOCK INSTALLATION RESTORED!");
            AuditSelectedGameDir();

            WpfMessageBox.Show(Loc.SuccessRollbackMsg, Loc.SuccessRollbackTitle, WpfMessageBoxButton.OK, WpfMessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Log($"[ERROR] Rollback failed: {ex.Message}");
            WpfMessageBox.Show($"Error restoring game:\n{ex.Message}", "Error", WpfMessageBoxButton.OK, WpfMessageBoxImage.Error);
        }
        finally
        {
            btnApply.IsEnabled = true;
            btnRollback.IsEnabled = true;
        }
    }

    private Task<int> RunProcessAsync(string fileName, string arguments)
    {
        var tcs = new TaskCompletionSource<int>();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _appDir
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data)) Log(e.Data);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data)) Log($"[STDERR] {e.Data}");
        };

        process.Exited += (s, e) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return tcs.Task;
    }

    private AuditResult? RunCliAudit(string gameDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{_cliScriptPath}\" audit --game-dir \"{gameDir}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = _appDir
        };

        using var proc = Process.Start(psi);
        if (proc == null) return null;
        string output = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();

        if (proc.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
        {
            return JsonSerializer.Deserialize<AuditResult>(output);
        }
        return null;
    }

    #endregion
}
