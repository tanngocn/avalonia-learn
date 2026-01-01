// ============================================
// CODE HOÀN CHỈNH CHO EXTERNAL API SERVICE
// ============================================

// ============================================
// 1. SERVICES/IExternalApiService.cs
// ============================================
/*
namespace camera.Services;

public interface IExternalApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
}
*/

// ============================================
// 2. SERVICES/ExternalApiService.cs
// ============================================
/*
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace camera.Services;

public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Request timeout: {ex.Message}");
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            return false;
        }
    }
}
*/

// ============================================
// 3. APP.AXAML.CS - Cấu Hình DI
// ============================================
/*
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using camera.Services;

public override void OnFrameworkInitializationCompleted()
{
    var collection = new ServiceCollection();

    // ... các service khác ...

    // ========== CẤU HÌNH HTTP CLIENT ==========
    
    // Cấu hình HttpClient với Base URL và Headers
    collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
    {
        // Base URL - endpoint sẽ được append vào đây
        client.BaseAddress = new Uri("https://api.example.com/v1/");
        
        // Timeout (mặc định: 100 giây)
        client.Timeout = TimeSpan.FromSeconds(30);
        
        // Default Headers
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
        
        // API Key (nếu cần)
        // client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key-here");
        
        // Bearer Token (nếu cần)
        // client.DefaultRequestHeaders.Authorization = 
        //     new AuthenticationHeaderValue("Bearer", "your-token-here");
    });

    // Đăng ký Service
    collection.AddScoped<IExternalApiService, ExternalApiService>();

    // ==========================================

    var services = collection.BuildServiceProvider();
    // ... phần còn lại ...
}
*/

// ============================================
// 4. SỬ DỤNG TRONG VIEWMODEL
// ============================================
/*
using camera.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class ApiViewModel : ObservableObject
{
    private readonly IExternalApiService _apiService;

    [ObservableProperty]
    private string? data;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    public ApiViewModel(IExternalApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // GET request
            var result = await _apiService.GetAsync<YourModel>("endpoint/path");
            Data = result?.ToString();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateDataAsync()
    {
        try
        {
            var newData = new { Name = "Test", Value = 123 };
            
            // POST request
            var result = await _apiService.PostAsync<YourModel>("endpoint/path", newData);
            Console.WriteLine($"Created: {result}");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }
}
*/

// ============================================
// 5. VÍ DỤ: TODO API SERVICE
// ============================================
/*
// ITodoApiService.cs
namespace camera.Services;

public interface ITodoApiService
{
    Task<List<TodoItem>> GetAllTodosAsync();
    Task<TodoItem?> GetTodoByIdAsync(int id);
    Task<TodoItem> CreateTodoAsync(TodoItem todo);
    Task<bool> DeleteTodoAsync(int id);
}

// TodoApiService.cs
using System.Text;
using System.Text.Json;
using camera.Models;

namespace camera.Services;

public class TodoApiService : ITodoApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TodoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<TodoItem>> GetAllTodosAsync()
    {
        var response = await _httpClient.GetAsync("todos");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<TodoItem>>(content, _jsonOptions) 
            ?? new List<TodoItem>();
    }

    public async Task<TodoItem?> GetTodoByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"todos/{id}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItem>(content, _jsonOptions);
    }

    public async Task<TodoItem> CreateTodoAsync(TodoItem todo)
    {
        var json = JsonSerializer.Serialize(todo, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("todos", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TodoItem>(responseContent, _jsonOptions)!;
    }

    public async Task<bool> DeleteTodoAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"todos/{id}");
        return response.IsSuccessStatusCode;
    }
}

// Cấu hình trong App.axaml.cs
collection.AddHttpClient<ITodoApiService, TodoApiService>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

collection.AddScoped<ITodoApiService, TodoApiService>();
*/

// ============================================
// 6. CẤU HÌNH VỚI appsettings.json
// ============================================
/*
// appsettings.json
{
  "ExternalApi": {
    "BaseUrl": "https://api.example.com/v1/",
    "Timeout": 30,
    "ApiKey": "your-api-key-here"
  }
}

// App.axaml.cs
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var apiConfig = configuration.GetSection("ExternalApi");

collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri(apiConfig["BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(int.Parse(apiConfig["Timeout"] ?? "30"));
    client.DefaultRequestHeaders.Add("X-API-Key", apiConfig["ApiKey"]!);
});
*/

// ============================================
// 7. AUTHENTICATION EXAMPLES
// ============================================
/*
// Bearer Token
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", "your-token");
});

// API Key
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
});

// Basic Auth
var credentials = Convert.ToBase64String(
    Encoding.ASCII.GetBytes("username:password"));
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Basic", credentials);
});
*/

// ============================================
// 8. ERROR HANDLING NÂNG CAO
// ============================================
/*
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService>? _logger;

    public ExternalApiService(HttpClient httpClient, ILogger<ExternalApiService>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            var response = await _httpClient.GetAsync(endpoint);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger?.LogWarning($"Resource not found: {endpoint}");
                return default(T);
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, $"HTTP Error calling {endpoint}");
            throw new ApiException($"API call failed: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger?.LogError(ex, $"Request timeout: {endpoint}");
            throw new ApiException("Request timeout", ex);
        }
    }
}

// Custom Exception
public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) 
        : base(message, innerException) { }
}
*/

