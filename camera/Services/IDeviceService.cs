using System.Collections.Generic;
using System.Threading.Tasks;
using camera.ViewModels;

namespace camera.Services;

/// <summary>
/// Service để quản lý devices
/// </summary>
public interface IDeviceService
{
    Task<List<DeviceNode>> GetDevicesAsync();
    Task<DeviceNode?> GetDeviceByIdAsync(string deviceId);
    Task<bool> ConnectDeviceAsync(string deviceId);
    Task<bool> DisconnectDeviceAsync(string deviceId);
}

