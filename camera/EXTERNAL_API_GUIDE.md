****# Hướng Dẫn Call External API - HttpClient Configuration

Guide này hướng dẫn cách cấu hình và sử dụng HttpClient để call API bên ngoài.

## 📋 Mục Lục
1. [Cấu Hình HttpClient Cơ Bản](#1-cấu-hình-httpclient-cơ-bản)
2. [Tạo Service Call API](#2-tạo-service-call-api)
3. [Cấu Hình Base URL và Headers](#3-cấu-hình-base-url-và-headers)
4. [Authentication](#4-authentication)
5. [Error Handling](#5-error-handling)
6. [Best Practices](#6-best-practices)
7. [Ví Dụ Hoàn Chỉnh](#7-ví-dụ-hoàn-chỉnh)

---

## 1. Cấu Hình HttpClient Cơ Bản

### 1.1. Thêm Package (nếu cần)

HttpClient đã có sẵn trong .NET, nhưng nếu cần JSON serialization:

```xml
<PackageReference Include="System.Text.Json" Version="8.0.0" />
<!-- Hoặc Newtonsoft.Json -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### 1.2. Cấu Hình trong DI Container

Cập nhật `App.axaml.cs`:

```csharp
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;

public override void OnFrameworkInitializationCompleted()
{
    var collection = new ServiceCollection();

    // ========== CẤU HÌNH HTTP CLIENT ==========
    
    // Option 1: Đăng ký HttpClient đơn giản
    collection.AddHttpClient();

    // Option 2: Đăng ký HttpClient với tên (Named Client) - KHUYẾN NGHỊ
    collection.AddHttpClient("ExternalAPI", client =>
    {
        client.BaseAddress = new Uri("https://api.example.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    });

    // Option 3: Đăng ký HttpClient với Typed Client - KHUYẾN NGHỊ NHẤT
    collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
    {
        client.BaseAddress = new Uri("https://api.example.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    // Đăng ký Service
    collection.AddScoped<IExternalApiService, ExternalApiService>();

    // ===========================================

    var services = collection.BuildServiceProvider();
    // ... phần còn lại ...
}
```

---

## 2. Tạo Service Call API

### 2.1. Tạo Interface

Tạo file `Services/IExternalApiService.cs`:

```csharp
namespace camera.Services;

public interface IExternalApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
}
```

### 2.2. Implement Service

Tạo file `Services/ExternalApiService.cs`:

```csharp
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

    // ========== GET REQUEST ==========

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

    // ========== POST REQUEST ==========

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

    // ========== PUT REQUEST ==========

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

    // ========== DELETE REQUEST ==========

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
```

---

## 3. Cấu Hình Base URL và Headers

### 3.1. Cấu Hình trong App.axaml.cs

```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    // Base URL của API
    client.BaseAddress = new Uri("https://api.example.com/v1/");
    
    // Timeout
    client.Timeout = TimeSpan.FromSeconds(30);
    
    // Default Headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");
    
    // API Key (nếu cần)
    // client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
});
```

### 3.2. Sử Dụng appsettings.json (Khuyến nghị)

Tạo file `appsettings.json`:

```json
{
  "ExternalApi": {
    "BaseUrl": "https://api.example.com/v1/",
    "Timeout": 30,
    "ApiKey": "your-api-key-here"
  }
}
```

Đọc config:

```csharp
using Microsoft.Extensions.Configuration;

// Trong App.axaml.cs
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
```

---

## 4. Authentication

### 4.1. API Key Authentication

```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
});
```

### 4.2. Bearer Token Authentication

```csharp
// Trong Service
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;

    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SetAccessTokenAsync(string token)
    {
        _accessToken = token;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    // Hoặc set token cho từng request
    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (!string.IsNullOrEmpty(_accessToken))
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }
        return request;
    }
}
```

### 4.3. Basic Authentication

```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    
    var credentials = Convert.ToBase64String(
        Encoding.ASCII.GetBytes("username:password"));
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Basic", credentials);
});
```

---

## 5. Error Handling

### 5.1. Service với Error Handling Nâng Cao

```csharp
using System.Net;

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

            if (response.StatusCode == HttpStatusCode.NotFound)
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
        catch (JsonException ex)
        {
            _logger?.LogError(ex, $"JSON deserialization error: {endpoint}");
            throw new ApiException("Invalid response format", ex);
        }
    }

    // Retry logic với Polly (nếu cần)
    public async Task<T?> GetWithRetryAsync<T>(string endpoint, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await GetAsync<T>(endpoint);
            }
            catch (HttpRequestException) when (i < maxRetries - 1)
            {
                await Task.Delay(1000 * (i + 1)); // Exponential backoff
            }
        }
        throw new ApiException("Max retries exceeded");
    }
}

// Custom Exception
public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
    public ApiException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

### 5.2. Sử Dụng Polly cho Retry (Optional)

Thêm package:

```xml
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
```

Cấu hình:

```csharp
using Polly;
using Polly.Extensions.Http;

collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
})
.AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan}");
            });
}
```

---

## 6. Best Practices

### 6.1. Sử Dụng Typed HttpClient

✅ **Nên:**
```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
});
```

❌ **Không nên:**
```csharp
// Tạo HttpClient mới mỗi lần
using var client = new HttpClient();
```

### 6.2. Dispose và Lifetime

- HttpClient được quản lý bởi DI container
- Không cần dispose thủ công
- Sử dụng Scoped hoặc Transient lifetime

### 6.3. Timeout Configuration

```csharp
client.Timeout = TimeSpan.FromSeconds(30); // Default: 100 seconds
```

### 6.4. Content Type Headers

```csharp
// Luôn set Content-Type cho POST/PUT
var content = new StringContent(json, Encoding.UTF8, "application/json");
```

---

## 7. Ví Dụ Hoàn Chỉnh

### 7.1. Ví Dụ: Todo API Service

Tạo file `Services/ITodoApiService.cs`:

```csharp
using camera.Models;

namespace camera.Services;

public interface ITodoApiService
{
    Task<List<TodoItem>> GetAllTodosAsync();
    Task<TodoItem?> GetTodoByIdAsync(int id);
    Task<TodoItem> CreateTodoAsync(TodoItem todo);
    Task<TodoItem> UpdateTodoAsync(int id, TodoItem todo);
    Task<bool> DeleteTodoAsync(int id);
}
```

Tạo file `Services/TodoApiService.cs`:

```csharp
using System.Net.Http.Headers;
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

    public async Task<TodoItem> UpdateTodoAsync(int id, TodoItem todo)
    {
        var json = JsonSerializer.Serialize(todo, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"todos/{id}", content);
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
```

### 7.2. Cấu Hình trong App.axaml.cs

```csharp
// Cấu hình Todo API Service
collection.AddHttpClient<ITodoApiService, TodoApiService>(client =>
{
    // Base URL - endpoint sẽ được append vào đây
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
    
    // Timeout
    client.Timeout = TimeSpan.FromSeconds(30);
    
    // Headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "TodoApp/1.0");
});

// Đăng ký service
collection.AddScoped<ITodoApiService, TodoApiService>();
```

### 7.3. Sử Dụng trong ViewModel

```csharp
using camera.Services;
using camera.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class TodoApiViewModel : ObservableObject
{
    private readonly ITodoApiService _todoApiService;

    [ObservableProperty]
    private List<TodoItem> todos = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    public TodoApiViewModel(ITodoApiService todoApiService)
    {
        _todoApiService = todoApiService;
        LoadTodosAsync();
    }

    [RelayCommand]
    private async Task LoadTodosAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Todos = await _todoApiService.GetAllTodosAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading todos: {ex.Message}";
            Console.WriteLine(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateTodoAsync()
    {
        try
        {
            var newTodo = new TodoItem
            {
                Title = "New Todo",
                Description = "From API"
            };

            var created = await _todoApiService.CreateTodoAsync(newTodo);
            Todos.Add(created);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating todo: {ex.Message}";
        }
    }
}
```

---

## 8. Cấu Hình Nâng Cao

### 8.1. Multiple API Endpoints

```csharp
// API 1
collection.AddHttpClient<ITodoApiService, TodoApiService>("TodoAPI", client =>
{
    client.BaseAddress = new Uri("https://api1.example.com/");
});

// API 2
collection.AddHttpClient<IUserApiService, UserApiService>("UserAPI", client =>
{
    client.BaseAddress = new Uri("https://api2.example.com/");
    client.DefaultRequestHeaders.Add("X-API-Key", "user-api-key");
});
```

### 8.2. Request/Response Logging

```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
})
.AddHttpMessageHandler(() => new LoggingHandler());

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Request: {request.Method} {request.RequestUri}");
        
        var response = await base.SendAsync(request, cancellationToken);
        
        Console.WriteLine($"Response: {response.StatusCode}");
        
        return response;
    }
}
```

---

## Tóm Tắt

### Các bước cơ bản:

1. ✅ **Đăng ký HttpClient** trong DI container
2. ✅ **Cấu hình Base URL** và Headers
3. ✅ **Tạo Service** với HttpClient injection
4. ✅ **Implement methods** (GET, POST, PUT, DELETE)
5. ✅ **Error handling** và logging
6. ✅ **Sử dụng** trong ViewModel

### Lưu ý quan trọng:

- ✅ Sử dụng Typed HttpClient (không tạo mới mỗi lần)
- ✅ Cấu hình timeout phù hợp
- ✅ Xử lý exception đầy đủ
- ✅ Set Content-Type cho POST/PUT
- ✅ Sử dụng async/await

---

## Tài Liệu Tham Khảo

- [HttpClient Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [IHttpClientFactory](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Polly for Retry](https://github.com/App-vNext/Polly)


