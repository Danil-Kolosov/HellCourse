using Microsoft.EntityFrameworkCore;
using buildingCompany.Data;
using buildingCompany.Models;

var builder = WebApplication.CreateBuilder(args);

// Добавляем Razor Pages
builder.Services.AddRazorPages();

// Добавляем контекст БД с SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Создаем БД при запуске (без удаления!)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Создаем базу данных, если её нет (НЕ удаляем существующую)
    dbContext.Database.EnsureCreated();

    // Проверяем и добавляем начальные данные, только если таблицы пустые
    try
    {
        if (!dbContext.BuildingSites.Any())
        {
            dbContext.BuildingSites.AddRange(
                new BuildingSite { Name = "ЖК Солнечный", Address = "ул. Ленина, 1" },
                new BuildingSite { Name = "Школа №5", Address = "ул. Школьная, 10" }
            );
        }

        if (!dbContext.WorkTypes.Any())
        {
            dbContext.WorkTypes.AddRange(
                new WorkType { Title = "Штукатурка стен", PricePerHour = 500 },
                new WorkType { Title = "Укладка плитки", PricePerHour = 700 }
            );
        }

        if (!dbContext.Employees.Any())
        {
            dbContext.Employees.AddRange(
                new Employee { FullName = "Иван Петров", Position = "Штукатур" },
                new Employee { FullName = "Сергей Иванов", Position = "Плиточник" }
            );
        }

        dbContext.SaveChanges();
    }
    catch (Exception ex)
    {
        // Логируем ошибку, но не прерываем запуск приложения
        Console.WriteLine($"Ошибка при инициализации БД: {ex.Message}");
    }
}

// Настройка конвейера обработки запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();