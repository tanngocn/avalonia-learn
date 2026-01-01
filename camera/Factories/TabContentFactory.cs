using System;
using System.Collections.Generic;
using Avalonia.Controls;
using camera.Data;
using camera.Views;

namespace camera.Factories;

/// <summary>
/// Factory để tạo tab content động dựa trên tab header hoặc index
/// </summary>
public static class TabContentFactory
{
    /// <summary>
    /// Dictionary mapping tab header với View type và LiveTabNames
    /// </summary>
    private static readonly Dictionary<string, TabInfo> TabMapping = new()
    {
        { "Print", new TabInfo(typeof(LivePrintView), LiveTabNames.Print) },
        { "Log", new TabInfo(typeof(LiveLogView), LiveTabNames.Log) },
        // Thêm các tab khác ở đây khi cần
        // { "Settings", new TabInfo(typeof(SettingsView), LiveTabNames.Settings) },
        // { "Reports", new TabInfo(typeof(ReportsView), LiveTabNames.Reports) },
    };

    /// <summary>
    /// Tạo content cho tab dựa trên header
    /// </summary>
    public static Control? CreateTabContent(string? header)
    {
        if (string.IsNullOrEmpty(header))
            return null;

        if (!TabMapping.TryGetValue(header, out var tabInfo))
        {
            System.Diagnostics.Debug.WriteLine($"Unknown tab header: {header}");
            return null;
        }

        try
        {
            return Activator.CreateInstance(tabInfo.ViewType) as Control;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating view for tab '{header}': {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Tạo content cho tab dựa trên index (sử dụng header từ TabControl)
    /// </summary>
    public static Control? CreateTabContent(TabControl tabControl, int index)
    {
        if (tabControl == null || index < 0 || index >= tabControl.Items.Count)
            return null;

        var tabItem = tabControl.Items[index] as TabItem;
        var header = tabItem?.Header?.ToString();
        return CreateTabContent(header);
    }

    /// <summary>
    /// Lấy LiveTabNames từ header
    /// </summary>
    public static LiveTabNames GetTabName(string? header)
    {
        if (string.IsNullOrEmpty(header))
            return LiveTabNames.Unknown;

        if (TabMapping.TryGetValue(header, out var tabInfo))
            return tabInfo.TabName;

        return LiveTabNames.Unknown;
    }

    /// <summary>
    /// Lấy LiveTabNames từ index
    /// </summary>
    public static LiveTabNames GetTabName(TabControl tabControl, int index)
    {
        if (tabControl == null || index < 0 || index >= tabControl.Items.Count)
            return LiveTabNames.Unknown;

        var tabItem = tabControl.Items[index] as TabItem;
        var header = tabItem?.Header?.ToString();
        return GetTabName(header);
    }

    /// <summary>
    /// Đăng ký tab mới động (có thể gọi từ ViewModel hoặc service)
    /// </summary>
    public static void RegisterTab(string header, Type viewType, LiveTabNames tabName)
    {
        if (string.IsNullOrEmpty(header))
            throw new ArgumentException("Header cannot be null or empty", nameof(header));

        if (viewType == null)
            throw new ArgumentNullException(nameof(viewType));

        if (!typeof(Control).IsAssignableFrom(viewType))
            throw new ArgumentException($"View type must inherit from Control", nameof(viewType));

        TabMapping[header] = new TabInfo(viewType, tabName);
    }

    /// <summary>
    /// Kiểm tra tab có tồn tại không
    /// </summary>
    public static bool HasTab(string header)
    {
        return !string.IsNullOrEmpty(header) && TabMapping.ContainsKey(header);
    }
}

/// <summary>
/// Thông tin về một tab
/// </summary>
internal class TabInfo
{
    public Type ViewType { get; }
    public LiveTabNames TabName { get; }

    public TabInfo(Type viewType, LiveTabNames tabName)
    {
        ViewType = viewType;
        TabName = tabName;
    }
}

