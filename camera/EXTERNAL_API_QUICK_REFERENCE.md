# External API - Quick Reference

## ⚙️ Cấu Hình HttpClient

### Cơ Bản
```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

### Với API Key
```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("X-API-Key", "your-api-key");
});
```

### Với Bearer Token
```csharp
collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", "your-token");
});
```

## 📝 Service Interface

```csharp
public interface IExternalApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object data);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
}
```

## 🔧 Service Implementation

```csharp
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExternalApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
    }
}
```

## 📖 Sử Dụng

### GET Request
```csharp
var result = await _apiService.GetAsync<YourModel>("endpoint/path");
```

### POST Request
```csharp
var data = new { Name = "Test", Value = 123 };
var result = await _apiService.PostAsync<YourModel>("endpoint/path", data);
```

### PUT Request
```csharp
var data = new { Name = "Updated" };
var result = await _apiService.PutAsync<YourModel>("endpoint/path", data);
```

### DELETE Request
```csharp
var success = await _apiService.DeleteAsync("endpoint/path");
```

## 🔐 Authentication Types

| Type | Code |
|------|------|
| **API Key** | `client.DefaultRequestHeaders.Add("X-API-Key", "key")` |
| **Bearer Token** | `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token")` |
| **Basic Auth** | `client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials)` |

## ⚠️ Error Handling

```csharp
try
{
    var result = await _apiService.GetAsync<Model>("endpoint");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"HTTP Error: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    Console.WriteLine($"Timeout: {ex.Message}");
}
```

## 📋 Checklist

- [ ] Đăng ký HttpClient trong DI
- [ ] Cấu hình Base URL
- [ ] Set Timeout
- [ ] Thêm Headers (Accept, User-Agent)
- [ ] Cấu hình Authentication (nếu cần)
- [ ] Implement Service với HttpClient
- [ ] Xử lý Error
- [ ] Sử dụng async/await

## 🎯 Best Practices

✅ **Nên:**
- Sử dụng Typed HttpClient
- Set timeout phù hợp
- Xử lý exception đầy đủ
- Set Content-Type cho POST/PUT
- Sử dụng async/await

❌ **Không nên:**
- Tạo HttpClient mới mỗi lần
- Quên set Content-Type
- Bỏ qua error handling
- Block UI thread với sync methods

## 🔗 Common Endpoints Pattern

```csharp
// Base URL: https://api.example.com/v1/
// Endpoint: "users" → Full URL: https://api.example.com/v1/users
// Endpoint: "users/123" → Full URL: https://api.example.com/v1/users/123

var users = await _apiService.GetAsync<List<User>>("users");
var user = await _apiService.GetAsync<User>("users/123");
```

## 📚 Files Cần Tạo

1. `Services/IExternalApiService.cs` - Interface
2. `Services/ExternalApiService.cs` - Implementation
3. Cấu hình trong `App.axaml.cs` - DI setup

## 🚀 Quick Start

1. Tạo Interface và Service
2. Đăng ký trong `App.axaml.cs`:
   ```csharp
   collection.AddHttpClient<IExternalApiService, ExternalApiService>(client =>
   {
       client.BaseAddress = new Uri("https://api.example.com/");
   });
   collection.AddScoped<IExternalApiService, ExternalApiService>();
   ```
3. Inject vào ViewModel và sử dụng!


