using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace F1GridEditorApp;

public enum AppLanguage
{
    English,
    Spanish,
    Russian
}

public class Strings : INotifyPropertyChanged
{
    private AppLanguage _currentLanguage = AppLanguage.English;

    public AppLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnPropertyChanged(string.Empty); // notify all properties changed
            }
        }
    }

    public string Title => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Personalizador de Parrilla y Pilotos F1 2020",
        AppLanguage.Russian => "Редактор пилотов и команд F1 2020",
        _ => "F1 2020 Grid & Driver Customizer"
    };

    public string Subtitle => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Mod persistente de pilotos, abreviaciones, nacionalidades y colores de escudería.",
        AppLanguage.Russian => "Постоянный редактор пилотов, инициалов, национальностей с флагами и цветов команд.",
        _ => "Persistent editor for drivers, abbreviations, nationalities with flags & team colors."
    };

    public string BadgeMod => CurrentLanguage switch
    {
        AppLanguage.Spanish => "SIN PROCESOS EN SEGUNDO PLANO",
        AppLanguage.Russian => "БЕЗ ФОНОВЫХ ПРОЦЕССОВ",
        _ => "NO BACKGROUND PROCESSES"
    };

    public string FolderLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "CARPETA F1 2020:",
        AppLanguage.Russian => "ПАПКА С ИГРОЙ:",
        _ => "GAME FOLDER:"
    };

    public string BrowseBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "📁 Examinar...",
        AppLanguage.Russian => "📁 Обзор...",
        _ => "📁 Browse..."
    };

    public string DownloadsBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "📂 Descargas (Limpia)",
        AppLanguage.Russian => "📂 Загрузки (Чистая)",
        _ => "📂 Downloads (Clean)"
    };

    public string GamesBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "📂 Games (Principal)",
        AppLanguage.Russian => "📂 Games (Основная)",
        _ => "📂 Games (Main)"
    };

    public string PresetsLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "PRESETS:",
        AppLanguage.Russian => "СЕТКИ:",
        _ => "PRESETS:"
    };

    public string Load2026Btn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "⭐ Parrilla 2026 (Colapinto, Bortoleto...)",
        AppLanguage.Russian => "⭐ Сетка 2026 (Колапинто, Бортолето...)",
        _ => "⭐ 2026 Grid (Colapinto, Bortoleto...)"
    };

    public string LoadStockBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "🔄 Stock 2020",
        AppLanguage.Russian => "🔄 2020 Оригинал",
        _ => "🔄 Stock 2020"
    };

    public string ExportBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "💾 Exportar...",
        AppLanguage.Russian => "💾 Экспорт...",
        _ => "💾 Export..."
    };

    public string ImportBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "📂 Importar...",
        AppLanguage.Russian => "📂 Импорт...",
        _ => "📂 Import..."
    };

    public string TeamsHeader => CurrentLanguage switch
    {
        AppLanguage.Spanish => "ESCUDERÍAS (10)",
        AppLanguage.Russian => "КОМАНДЫ (10)",
        _ => "TEAMS (10)"
    };

    public string TeamConfigHeader => CurrentLanguage switch
    {
        AppLanguage.Spanish => "CONFIGURACIÓN DE ESCUDERÍA",
        AppLanguage.Russian => "НАСТРОЙКА КОМАНДЫ",
        _ => "CONSTRUCTOR SETTINGS"
    };

    public string TeamNameLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Nombre:",
        AppLanguage.Russian => "Название:",
        _ => "Team Name:"
    };

    public string TeamShortLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Nombre Corto:",
        AppLanguage.Russian => "Короткое имя:",
        _ => "Short Name:"
    };

    public string TeamTitleLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Título Oficial:",
        AppLanguage.Russian => "Официальное имя:",
        _ => "Official Title:"
    };

    public string TeamColorLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Color del Equipo (Timing Tower / Menús):",
        AppLanguage.Russian => "Цвет команды (Тайминг / Меню):",
        _ => "Team UI Color (Tower / Menus):"
    };

    public string PickColorBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "🎨 Elegir...",
        AppLanguage.Russian => "🎨 Цвет...",
        _ => "🎨 Pick..."
    };

    public string Driver1Header => CurrentLanguage switch
    {
        AppLanguage.Spanish => "PILOTO 1 - ",
        AppLanguage.Russian => "ПИЛОТ 1 - ",
        _ => "DRIVER 1 - "
    };

    public string Driver2Header => CurrentLanguage switch
    {
        AppLanguage.Spanish => "PILOTO 2 - ",
        AppLanguage.Russian => "ПИЛОТ 2 - ",
        _ => "DRIVER 2 - "
    };

    public string FirstNameLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Nombre:",
        AppLanguage.Russian => "Имя:",
        _ => "First Name:"
    };

    public string LastNameLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Apellido:",
        AppLanguage.Russian => "Фамилия:",
        _ => "Last Name:"
    };

    public string InitialsLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Iniciales (3):",
        AppLanguage.Russian => "Инициалы (3):",
        _ => "Initials (3):"
    };

    public string NationalityLabel => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Nacionalidad & Bandera:",
        AppLanguage.Russian => "Национальность и флаг:",
        _ => "Nationality & Flag:"
    };

    public string ConsoleHeader => CurrentLanguage switch
    {
        AppLanguage.Spanish => "CONSOLA DE ACTIVIDAD / LOGS",
        AppLanguage.Russian => "ЖУРНАЛ ДЕЙСТВИЙ / ЛОГИ",
        _ => "ACTIVITY CONSOLE / LOGS"
    };

    public string ClearBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Limpiar",
        AppLanguage.Russian => "Очистить",
        _ => "Clear"
    };

    public string ReAuditBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "🔍 Re-Auditar",
        AppLanguage.Russian => "🔍 Проверить",
        _ => "🔍 Re-Audit"
    };

    public string RollbackBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "↩️ RESTAURAR ORIGINAL",
        AppLanguage.Russian => "↩️ ВОССТАНОВИТЬ ОРИГИНАЛ",
        _ => "↩️ RESTORE STOCK"
    };

    public string ApplyBtn => CurrentLanguage switch
    {
        AppLanguage.Spanish => "🚀 APLICAR AL JUEGO",
        AppLanguage.Russian => "🚀 ПРИМЕНИТЬ К ИГРЕ",
        _ => "🚀 APPLY TO GAME"
    };

    public string StatusVerifying => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Verificando instalación de F1 2020...",
        AppLanguage.Russian => "Проверка файлов F1 2020...",
        _ => "Checking F1 2020 installation..."
    };

    public string StatusPathNotFound => CurrentLanguage switch
    {
        AppLanguage.Spanish => "❌ La ruta especificada no existe.",
        AppLanguage.Russian => "❌ Указанный путь не существует.",
        _ => "❌ The specified folder does not exist."
    };

    public string StatusInvalidFolder => CurrentLanguage switch
    {
        AppLanguage.Spanish => "❌ No es una carpeta válida de F1 2020 (faltan ejecutables o base de datos).",
        AppLanguage.Russian => "❌ Неверная папка F1 2020 (отсутствуют исполняемые файлы или база данных).",
        _ => "❌ Invalid F1 2020 folder (missing executables or game database)."
    };

    public string StatusCleanStock => CurrentLanguage switch
    {
        AppLanguage.Spanish => "✅ F1 2020 ORIGINAL LIMPIO (Ejecutables y base de datos stock detectados)",
        AppLanguage.Russian => "✅ ОРИГИНАЛЬНАЯ ЧИСТАЯ F1 2020 (Файлы игры чистые, готовы к моду)",
        _ => "✅ CLEAN STOCK F1 2020 (Stock executables & database detected)"
    };

    public string StatusPatched => CurrentLanguage switch
    {
        AppLanguage.Spanish => "🔵 F1 2020 PARCHEADO (Mod de pilotos y colores activo en esta carpeta)",
        AppLanguage.Russian => "🔵 F1 2020 МОДИФИЦИРОВАНА (Мод пилотов и цветов активен)",
        _ => "🔵 PATCHED F1 2020 (Custom drivers & colors mod active)"
    };

    public string StatusDetected => CurrentLanguage switch
    {
        AppLanguage.Spanish => "✅ Instalación de F1 2020 detectada",
        AppLanguage.Russian => "✅ Папка F1 2020 обнаружена",
        _ => "✅ F1 2020 installation detected"
    };

    public string ConfirmApplyTitle => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Confirmar Modificación",
        AppLanguage.Russian => "Подтверждение",
        _ => "Confirm Modifications"
    };

    public string ConfirmApplyMsg => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Se aplicarán los cambios a la siguiente carpeta:\n\n{0}\n\nSe creará un respaldo automático antes de continuar. ¿Deseas proceder?",
        AppLanguage.Russian => "Модификация будет применена к папке:\n\n{0}\n\nПеред внесением изменений будет создана резервная копия. Продолжить?",
        _ => "Apply custom grid, drivers, and team colors to:\n\n{0}\n\nAn automatic backup will be created before proceeding. Continue?"
    };

    public string ConfirmRollbackTitle => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Confirmar Rollback",
        AppLanguage.Russian => "Восстановление",
        _ => "Confirm Rollback"
    };

    public string ConfirmRollbackMsg => CurrentLanguage switch
    {
        AppLanguage.Spanish => "¿Deseas restaurar el juego a su estado original limpio?\n\n{0}\n\nSe restaurarán los ejecutables y la base de datos desde los respaldos.",
        AppLanguage.Russian => "Восстановить оригинальные файлы игры?\n\n{0}\n\nИсполняемые файлы и база данных будут восстановлены из резервной копии.",
        _ => "Restore this game installation back to clean original files?\n\n{0}\n\nExecutables and database will be restored from backup."
    };

    public string SuccessApplyTitle => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Modificación Exitosa",
        AppLanguage.Russian => "Успешно применено",
        _ => "Mod Successfully Applied"
    };

    public string SuccessApplyMsg => CurrentLanguage switch
    {
        AppLanguage.Spanish => "¡Modificación aplicada con éxito a F1 2020!\nPuedes iniciar el juego con normalidad.",
        AppLanguage.Russian => "Модификация успешно установлена в F1 2020!\nМожно запускать игру.",
        _ => "Grid and drivers successfully modified!\nYou can now start the game normally."
    };

    public string SuccessRollbackTitle => CurrentLanguage switch
    {
        AppLanguage.Spanish => "Restauración Completada",
        AppLanguage.Russian => "Восстановлено",
        _ => "Rollback Complete"
    };

    public string SuccessRollbackMsg => CurrentLanguage switch
    {
        AppLanguage.Spanish => "El juego ha sido restaurado exitosamente a su estado original limpio.",
        AppLanguage.Russian => "Игра успешно возвращена к чистому оригинальному состоянию.",
        _ => "Game installation has been successfully restored to clean original state."
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
