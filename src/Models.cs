using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace F1GridEditorApp;

public class NationalityItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Flag { get; set; } = "";
    public string DisplayText => $"{Flag} {Name} ({Id})";

    public override string ToString() => DisplayText;
}

public class DriverConfig : INotifyPropertyChanged
{
    private string _targetFirstName = "";
    private string _targetLastName = "";
    private string _targetFullName = "";
    private string _targetShortName = "";
    private string _targetInitials = "";
    private string _targetNationality = "";

    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("driver_index")]
    public int DriverIndex { get; set; }

    [JsonPropertyName("metadata_id")]
    public string MetadataId { get; set; } = "";

    [JsonPropertyName("target_first_name")]
    public string TargetFirstName
    {
        get => _targetFirstName;
        set { SetField(ref _targetFirstName, value); UpdateDerivedNames(); }
    }

    [JsonPropertyName("target_last_name")]
    public string TargetLastName
    {
        get => _targetLastName;
        set { SetField(ref _targetLastName, value); UpdateDerivedNames(); }
    }

    [JsonPropertyName("target_full_name")]
    public string TargetFullName
    {
        get => _targetFullName;
        set => SetField(ref _targetFullName, value);
    }

    [JsonPropertyName("target_short_name")]
    public string TargetShortName
    {
        get => _targetShortName;
        set => SetField(ref _targetShortName, value);
    }

    [JsonPropertyName("target_initials")]
    public string TargetInitials
    {
        get => _targetInitials;
        set => SetField(ref _targetInitials, value?.ToUpperInvariant() ?? "");
    }

    [JsonPropertyName("target_nationality")]
    public string TargetNationality
    {
        get => _targetNationality;
        set
        {
            SetField(ref _targetNationality, value);
            OnPropertyChanged(nameof(SelectedNationalityItem));
        }
    }

    [JsonPropertyName("first_name_key")]
    public string FirstNameKey { get; set; } = "";

    [JsonPropertyName("last_name_key")]
    public string LastNameKey { get; set; } = "";

    [JsonPropertyName("full_name_key")]
    public string FullNameKey { get; set; } = "";

    [JsonPropertyName("short_name_key")]
    public string ShortNameKey { get; set; } = "";

    [JsonPropertyName("original_name")]
    public string OriginalName { get; set; } = "";

    [JsonPropertyName("car_number")]
    public int CarNumber { get; set; }

    [JsonPropertyName("slot_name")]
    public string SlotName { get; set; } = "";

    [JsonIgnore]
    public NationalityItem? SelectedNationalityItem
    {
        get => NationalitiesData.FindById(TargetNationality);
        set
        {
            if (value != null && TargetNationality != value.Id)
            {
                TargetNationality = value.Id;
            }
        }
    }

    private void UpdateDerivedNames()
    {
        if (string.IsNullOrWhiteSpace(TargetFirstName) && string.IsNullOrWhiteSpace(TargetLastName))
            return;

        string first = TargetFirstName?.Trim() ?? "";
        string last = TargetLastName?.Trim() ?? "";
        TargetFullName = $"{first} {last}".Trim();

        string initial = first.Length > 0 ? $"{first[0]}." : "";
        TargetShortName = $"{initial} {last}".Trim();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

public class TeamConfig : INotifyPropertyChanged
{
    private string _teamName = "";
    private string _teamTitleVal = "";
    private string _teamShortVal = "";
    private string _teamColor = "#FFFFFF";

    [JsonPropertyName("team_id")]
    public string TeamId { get; set; } = "";

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = "";

    [JsonPropertyName("team_name")]
    public string TeamName
    {
        get => _teamName;
        set => SetField(ref _teamName, value);
    }

    [JsonPropertyName("showroom_key")]
    public string ShowroomKey { get; set; } = "";

    [JsonPropertyName("showroom_lineup")]
    public string ShowroomLineup { get; set; } = "";

    [JsonPropertyName("team_title_key")]
    public string TeamTitleKey { get; set; } = "";

    [JsonPropertyName("team_title_val")]
    public string TeamTitleVal
    {
        get => _teamTitleVal;
        set => SetField(ref _teamTitleVal, value);
    }

    [JsonPropertyName("team_short_key")]
    public string TeamShortKey { get; set; } = "";

    [JsonPropertyName("team_short_val")]
    public string TeamShortVal
    {
        get => _teamShortVal;
        set => SetField(ref _teamShortVal, value);
    }

    [JsonPropertyName("base_slot")]
    public string BaseSlot { get; set; } = "";

    [JsonPropertyName("team_color")]
    public string TeamColor
    {
        get => _teamColor;
        set
        {
            if (SetField(ref _teamColor, value))
            {
                OnPropertyChanged(nameof(ColorBrush));
            }
        }
    }

    [JsonPropertyName("drivers")]
    public ObservableCollection<DriverConfig> Drivers { get; set; } = new();

    [JsonIgnore]
    public System.Windows.Media.Brush ColorBrush
    {
        get
        {
            try
            {
                return (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString(TeamColor);
            }
            catch
            {
                return System.Windows.Media.Brushes.Gray;
            }
        }
    }

    [JsonIgnore]
    public DriverConfig? Driver1 => Drivers.Count > 0 ? Drivers[0] : null;

    [JsonIgnore]
    public DriverConfig? Driver2 => Drivers.Count > 1 ? Drivers[1] : null;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

public class GridConfig
{
    [JsonPropertyName("season")]
    public string Season { get; set; } = "F1 2020 Grid Configuration";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("teams")]
    public ObservableCollection<TeamConfig> Teams { get; set; } = new();
}

public class AuditResult
{
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    [JsonPropertyName("gameDir")]
    public string GameDir { get; set; } = "";

    [JsonPropertyName("isPatched")]
    public bool IsPatched { get; set; }

    [JsonPropertyName("dx11Patched")]
    public bool Dx11Patched { get; set; }

    [JsonPropertyName("dx12Patched")]
    public bool Dx12Patched { get; set; }

    [JsonPropertyName("dbPatched")]
    public bool DbPatched { get; set; }

    [JsonPropertyName("hasLocalisatioa")]
    public bool HasLocalisatioa { get; set; }

    [JsonPropertyName("hasModularMods")]
    public bool HasModularMods { get; set; }

    [JsonPropertyName("dx11Hash")]
    public string? Dx11Hash { get; set; }

    [JsonPropertyName("dx12Hash")]
    public string? Dx12Hash { get; set; }

    [JsonPropertyName("dbHash")]
    public string? DbHash { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public static class NationalitiesData
{
    public static readonly List<NationalityItem> All = new()
    {
        new() { Id = "argentina", Name = "Argentina", Flag = "🇦🇷" },
        new() { Id = "australia", Name = "Australia", Flag = "🇦🇺" },
        new() { Id = "austria", Name = "Austria", Flag = "🇦🇹" },
        new() { Id = "azerbaijan", Name = "Azerbaijan", Flag = "🇦🇿" },
        new() { Id = "bahrain", Name = "Bahrain", Flag = "🇧🇭" },
        new() { Id = "barbados", Name = "Barbados", Flag = "🇧🇧" },
        new() { Id = "belgium", Name = "Belgium", Flag = "🇧🇪" },
        new() { Id = "bolivia", Name = "Bolivia", Flag = "🇧🇴" },
        new() { Id = "brazil", Name = "Brazil", Flag = "🇧🇷" },
        new() { Id = "britain", Name = "Britain", Flag = "🇬🇧" },
        new() { Id = "bulgaria", Name = "Bulgaria", Flag = "🇧🇬" },
        new() { Id = "cameroon", Name = "Cameroon", Flag = "🇨🇲" },
        new() { Id = "canada", Name = "Canada", Flag = "🇨🇦" },
        new() { Id = "chile", Name = "Chile", Flag = "🇨🇱" },
        new() { Id = "china", Name = "China", Flag = "🇨🇳" },
        new() { Id = "columbia", Name = "Colombia", Flag = "🇨🇴" },
        new() { Id = "costa_rica", Name = "Costa Rica", Flag = "🇨🇷" },
        new() { Id = "croatia", Name = "Croatia", Flag = "🇭🇷" },
        new() { Id = "cyprus", Name = "Cyprus", Flag = "🇨🇾" },
        new() { Id = "czech_republic", Name = "Czech Republic", Flag = "🇨🇿" },
        new() { Id = "denmark", Name = "Denmark", Flag = "🇩🇰" },
        new() { Id = "netherlands", Name = "Netherlands", Flag = "🇳🇱" },
        new() { Id = "ecuador", Name = "Ecuador", Flag = "🇪🇨" },
        new() { Id = "england", Name = "England", Flag = "🏴󠁧󠁢󠁥󠁮󠁧󠁿" },
        new() { Id = "uae", Name = "United Arab Emirates", Flag = "🇦🇪" },
        new() { Id = "estonia", Name = "Estonia", Flag = "🇪🇪" },
        new() { Id = "finland", Name = "Finland", Flag = "🇫🇮" },
        new() { Id = "france", Name = "France", Flag = "🇫🇷" },
        new() { Id = "germany", Name = "Germany", Flag = "🇩🇪" },
        new() { Id = "ghana", Name = "Ghana", Flag = "🇬🇭" },
        new() { Id = "greece", Name = "Greece", Flag = "🇬🇷" },
        new() { Id = "guatemala", Name = "Guatemala", Flag = "🇬🇹" },
        new() { Id = "honduras", Name = "Honduras", Flag = "🇭🇳" },
        new() { Id = "hong_kong", Name = "Hong Kong", Flag = "🇭🇰" },
        new() { Id = "hungary", Name = "Hungary", Flag = "🇭🇺" },
        new() { Id = "iceland", Name = "Iceland", Flag = "🇮🇸" },
        new() { Id = "india", Name = "India", Flag = "🇮🇳" },
        new() { Id = "indonesia", Name = "Indonesia", Flag = "🇮🇩" },
        new() { Id = "ireland", Name = "Ireland", Flag = "🇮🇪" },
        new() { Id = "israel", Name = "Israel", Flag = "🇮🇱" },
        new() { Id = "italy", Name = "Italy", Flag = "🇮🇹" },
        new() { Id = "jamaica", Name = "Jamaica", Flag = "🇯🇲" },
        new() { Id = "japan", Name = "Japan", Flag = "🇯🇵" },
        new() { Id = "jordan", Name = "Jordan", Flag = "🇯🇴" },
        new() { Id = "kuwait", Name = "Kuwait", Flag = "🇰🇼" },
        new() { Id = "latvia", Name = "Latvia", Flag = "🇱🇻" },
        new() { Id = "lebanon", Name = "Lebanon", Flag = "🇱🇧" },
        new() { Id = "lithuania", Name = "Lithuania", Flag = "🇱🇹" },
        new() { Id = "luxembourg", Name = "Luxembourg", Flag = "🇱🇺" },
        new() { Id = "malaysia", Name = "Malaysia", Flag = "🇲🇾" },
        new() { Id = "malta", Name = "Malta", Flag = "🇲🇹" },
        new() { Id = "mexico", Name = "Mexico", Flag = "🇲🇽" },
        new() { Id = "monaco", Name = "Monaco", Flag = "🇲🇨" },
        new() { Id = "new_zealand", Name = "New Zealand", Flag = "🇳🇿" },
        new() { Id = "nicaragua", Name = "Nicaragua", Flag = "🇳🇮" },
        new() { Id = "north_korea", Name = "North Korea", Flag = "🇰🇵" },
        new() { Id = "northern_ireland", Name = "Northern Ireland", Flag = "🇬🇧" },
        new() { Id = "norway", Name = "Norway", Flag = "🇳🇴" },
        new() { Id = "oman", Name = "Oman", Flag = "🇴🇲" },
        new() { Id = "pakistan", Name = "Pakistan", Flag = "🇵🇰" },
        new() { Id = "panama", Name = "Panama", Flag = "🇵🇦" },
        new() { Id = "paraguay", Name = "Paraguay", Flag = "🇵🇾" },
        new() { Id = "peru", Name = "Peru", Flag = "🇵🇪" },
        new() { Id = "poland", Name = "Poland", Flag = "🇵🇱" },
        new() { Id = "portugal", Name = "Portugal", Flag = "🇵🇹" },
        new() { Id = "qatar", Name = "Qatar", Flag = "🇶🇦" },
        new() { Id = "romania", Name = "Romania", Flag = "🇷🇴" },
        new() { Id = "russia", Name = "Russia", Flag = "🇷🇺" },
        new() { Id = "el_salvador", Name = "El Salvador", Flag = "🇸🇻" },
        new() { Id = "saudi_arabia", Name = "Saudi Arabia", Flag = "🇸🇦" },
        new() { Id = "scotland", Name = "Scotland", Flag = "🏴󠁧󠁢󠁥󠁮󠁧󠁿" },
        new() { Id = "serbia", Name = "Serbia", Flag = "🇷🇸" },
        new() { Id = "singapore", Name = "Singapore", Flag = "🇸🇬" },
        new() { Id = "slovakia", Name = "Slovakia", Flag = "🇸🇰" },
        new() { Id = "slovenia", Name = "Slovenia", Flag = "🇸🇮" },
        new() { Id = "south_korea", Name = "South Korea", Flag = "🇰🇷" },
        new() { Id = "south_africa", Name = "South Africa", Flag = "🇿🇦" },
        new() { Id = "spain", Name = "Spain", Flag = "🇪🇸" },
        new() { Id = "sweden", Name = "Sweden", Flag = "🇸🇪" },
        new() { Id = "switzerland", Name = "Switzerland", Flag = "🇨🇭" },
        new() { Id = "taiwan", Name = "Taiwan", Flag = "🇹🇼" },
        new() { Id = "thailand", Name = "Thailand", Flag = "🇹🇭" },
        new() { Id = "turkey", Name = "Turkey", Flag = "🇹🇷" },
        new() { Id = "uruguay", Name = "Uruguay", Flag = "🇺🇾" },
        new() { Id = "ukraine", Name = "Ukraine", Flag = "🇺🇦" },
        new() { Id = "usa", Name = "United States", Flag = "🇺🇸" },
        new() { Id = "venezuela", Name = "Venezuela", Flag = "🇻🇪" },
        new() { Id = "vietnam", Name = "Vietnam", Flag = "🇻🇳" },
        new() { Id = "wales", Name = "Wales", Flag = "🏴󠁧󠁢󠁷󠁬󠁳󠁿" }
    };

    public static NationalityItem? FindById(string? id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return All.Find(n => string.Equals(n.Id, id, StringComparison.OrdinalIgnoreCase));
    }
}
