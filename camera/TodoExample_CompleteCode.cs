// ============================================
// FILE NÀY CHỨA CODE HOÀN CHỈNH ĐỂ COPY-PASTE
// ============================================

// ============================================
// 1. MODELS/TodoItem.cs
// ============================================
/*
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace camera.Models;

public class TodoItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public int Priority { get; set; } = 2; // 1: Thấp, 2: Trung bình, 3: Cao

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? CompletedAt { get; set; }
}
*/

// ============================================
// 2. DATA/ApplicationDbContext.cs (Cập nhật)
// ============================================
/*
using Microsoft.EntityFrameworkCore;
using camera.Models;

namespace camera.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("TodoItems");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
*/

// ============================================
// 3. SERVICES/ITodoService.cs
// ============================================
/*
using camera.Models;

namespace camera.Services;

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<List<TodoItem>> GetPendingAsync();
    Task<List<TodoItem>> GetCompletedAsync();
    Task<List<TodoItem>> SearchAsync(string keyword);
    
    Task<TodoItem> CreateAsync(string title, string? description = null, int priority = 2);
    Task<TodoItem> CreateAsync(TodoItem todoItem);
    
    Task<TodoItem> UpdateAsync(TodoItem todoItem);
    Task<bool> MarkAsCompletedAsync(int id);
    Task<bool> MarkAsPendingAsync(int id);
    Task<bool> UpdatePriorityAsync(int id, int priority);
    
    Task<bool> DeleteAsync(int id);
    Task<int> DeleteCompletedAsync();
}
*/

// ============================================
// 4. SERVICES/TodoService.cs
// ============================================
/*
using Microsoft.EntityFrameworkCore;
using camera.Data;
using camera.Models;

namespace camera.Services;

public class TodoService : ITodoService
{
    private readonly ApplicationDbContext _context;

    public TodoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // READ
    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _context.TodoItems
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<TodoItem>> GetPendingAsync()
    {
        return await _context.TodoItems
            .Where(x => !x.IsCompleted)
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<TodoItem>> GetCompletedAsync()
    {
        return await _context.TodoItems
            .Where(x => x.IsCompleted)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<TodoItem>> SearchAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllAsync();

        var lowerKeyword = keyword.ToLower();
        return await _context.TodoItems
            .Where(x => x.Title.ToLower().Contains(lowerKeyword) ||
                       (x.Description != null && x.Description.ToLower().Contains(lowerKeyword)))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    // CREATE
    public async Task<TodoItem> CreateAsync(string title, string? description = null, int priority = 2)
    {
        var todoItem = new TodoItem
        {
            Title = title,
            Description = description,
            Priority = priority,
            IsCompleted = false,
            CreatedAt = DateTime.Now
        };

        return await CreateAsync(todoItem);
    }

    public async Task<TodoItem> CreateAsync(TodoItem todoItem)
    {
        todoItem.CreatedAt = DateTime.Now;
        todoItem.IsCompleted = false;
        todoItem.CompletedAt = null;

        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();

        return todoItem;
    }

    // UPDATE
    public async Task<TodoItem> UpdateAsync(TodoItem todoItem)
    {
        var existing = await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == todoItem.Id);

        if (existing == null)
            throw new ArgumentException($"TodoItem with ID {todoItem.Id} not found");

        existing.Title = todoItem.Title;
        existing.Description = todoItem.Description;
        existing.Priority = todoItem.Priority;
        existing.IsCompleted = todoItem.IsCompleted;

        if (todoItem.IsCompleted && !existing.IsCompleted)
        {
            existing.CompletedAt = DateTime.Now;
        }
        else if (!todoItem.IsCompleted && existing.IsCompleted)
        {
            existing.CompletedAt = null;
        }

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> MarkAsCompletedAsync(int id)
    {
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        if (todoItem == null) return false;

        todoItem.IsCompleted = true;
        todoItem.CompletedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkAsPendingAsync(int id)
    {
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        if (todoItem == null) return false;

        todoItem.IsCompleted = false;
        todoItem.CompletedAt = null;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePriorityAsync(int id, int priority)
    {
        if (priority < 1 || priority > 3)
            throw new ArgumentException("Priority must be between 1 and 3");

        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        if (todoItem == null) return false;

        todoItem.Priority = priority;
        await _context.SaveChangesAsync();
        return true;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var todoItem = await _context.TodoItems.FirstOrDefaultAsync(x => x.Id == id);
        if (todoItem == null) return false;

        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteCompletedAsync()
    {
        var completedItems = await _context.TodoItems
            .Where(x => x.IsCompleted)
            .ToListAsync();

        _context.TodoItems.RemoveRange(completedItems);
        return await _context.SaveChangesAsync();
    }
}
*/

// ============================================
// 5. VIEWMODELS/TodoViewModel.cs
// ============================================
/*
using camera.Models;
using camera.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace camera.ViewModels;

public partial class TodoViewModel : ObservableObject
{
    private readonly ITodoService _todoService;

    [ObservableProperty]
    private List<TodoItem> allTodos = new();

    [ObservableProperty]
    private List<TodoItem> filteredTodos = new();

    [ObservableProperty]
    private TodoItem? selectedTodo;

    [ObservableProperty]
    private string searchKeyword = string.Empty;

    [ObservableProperty]
    private string filterType = "All"; // All, Pending, Completed

    [ObservableProperty]
    private string newTodoTitle = string.Empty;

    [ObservableProperty]
    private string newTodoDescription = string.Empty;

    [ObservableProperty]
    private int newTodoPriority = 2;

    public TodoViewModel(ITodoService todoService)
    {
        _todoService = todoService;
        LoadTodosAsync();
    }

    [RelayCommand]
    private async Task LoadTodosAsync()
    {
        try
        {
            AllTodos = await _todoService.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading todos: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        var todos = AllTodos.AsEnumerable();

        if (FilterType == "Pending")
            todos = todos.Where(x => !x.IsCompleted);
        else if (FilterType == "Completed")
            todos = todos.Where(x => x.IsCompleted);

        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            todos = todos.Where(x =>
                x.Title.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)));
        }

        FilteredTodos = todos
            .OrderByDescending(x => x.Priority)
            .ThenByDescending(x => x.CreatedAt)
            .ToList();
    }

    partial void OnSearchKeywordChanged(string value) => ApplyFilter();
    partial void OnFilterTypeChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task CreateTodoAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTodoTitle))
            return;

        try
        {
            await _todoService.CreateAsync(NewTodoTitle, NewTodoDescription, NewTodoPriority);
            NewTodoTitle = string.Empty;
            NewTodoDescription = string.Empty;
            NewTodoPriority = 2;
            await LoadTodosAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating todo: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ToggleCompleteAsync(TodoItem todo)
    {
        if (todo == null) return;

        try
        {
            if (todo.IsCompleted)
                await _todoService.MarkAsPendingAsync(todo.Id);
            else
                await _todoService.MarkAsCompletedAsync(todo.Id);

            await LoadTodosAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling todo: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DeleteTodoAsync(TodoItem todo)
    {
        if (todo == null) return;

        try
        {
            await _todoService.DeleteAsync(todo.Id);
            await LoadTodosAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting todo: {ex.Message}");
        }
    }
}
*/

// ============================================
// 6. APP.AXAML.CS (Cập nhật phần DI)
// ============================================
/*
// Thêm vào OnFrameworkInitializationCompleted():

// Cấu hình DbContext
collection.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));

// Đăng ký Service
collection.AddScoped<ITodoService, TodoService>();

// Đăng ký ViewModel
collection.AddScoped<TodoViewModel>();
*/

// ============================================
// 7. CAMERA.CSPROJ (Thêm packages)
// ============================================
/*
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
</ItemGroup>
*/

// ============================================
// 8. MIGRATIONS COMMANDS
// ============================================
/*
// Tạo migration
dotnet ef migrations add InitialCreate --project camera --startup-project camera

// Áp dụng migration
dotnet ef database update --project camera --startup-project camera
*/

// ============================================
// 9. TEST CODE (Console App Example)
// ============================================
/*
using camera.Data;
using camera.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));
services.AddScoped<ITodoService, TodoService>();

var serviceProvider = services.BuildServiceProvider();
var todoService = serviceProvider.GetRequiredService<ITodoService>();

// Test CREATE
var todo1 = await todoService.CreateAsync("Học EF Core", "Hoàn thành guide", 3);
var todo2 = await todoService.CreateAsync("Làm bài tập", null, 2);

// Test READ
var allTodos = await todoService.GetAllAsync();
Console.WriteLine($"Total todos: {allTodos.Count}");

// Test UPDATE
await todoService.MarkAsCompletedAsync(todo1.Id);

// Test DELETE
await todoService.DeleteAsync(todo2.Id);
*/


