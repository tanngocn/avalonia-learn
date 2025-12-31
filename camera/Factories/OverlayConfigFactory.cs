using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using camera.Data;

namespace camera.Factories;

/// <summary>
/// Factory for creating and managing OverlayConfig instances from services
/// </summary>
public class OverlayConfigFactory
{
    // TODO: Inject your service here that returns N OverlayConfig objects
    // Example: private readonly IOverlayConfigService _overlayConfigService;
    
    /// <summary>
    /// Get all overlay configs from service
    /// </summary>
    public async Task<IEnumerable<OverlayConfig>> GetAllOverlayConfigsAsync()
    {
        // TODO: Replace with actual service call
        // Example: return await _overlayConfigService.GetOverlayConfigsAsync();
        
        // Placeholder: return empty list until service is implemented
        return Enumerable.Empty<OverlayConfig>();
    }
    
    /// <summary>
    /// Get overlay config by index or identifier
    /// </summary>
    public async Task<OverlayConfig?> GetOverlayConfigAsync(int index)
    {
        var configs = await GetAllOverlayConfigsAsync();
        return configs.ElementAtOrDefault(index);
    }
    
    /// <summary>
    /// Get default overlay config (fallback when service returns nothing)
    /// </summary>
    public OverlayConfig GetDefaultOverlayConfig()
    {
        return new OverlayConfig
        {
            BoxSizeNorm = (0.15, 0.12),
            LabelText = "person",
            Topmost = false,
            CenterBox = true,
            InitialPositionNorm = (0.4, 0.3),
            ShowPolygon = true,
            PolygonSizeNorm = (0.15, 0.20),
            SafeMargin = 30,
            AutoCreate = true
        };
    }
    
    /// <summary>
    /// Get overlay config from service, or return default if service returns nothing
    /// </summary>
    public async Task<OverlayConfig> GetOverlayConfigOrDefaultAsync(int index = 0)
    {
        var config = await GetOverlayConfigAsync(index);
        return config ?? GetDefaultOverlayConfig();
    }
}






