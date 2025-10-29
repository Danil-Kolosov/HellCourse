
// Создание репозиториев
//using FamilyTree;

//var personRepo = new PersonRepository();
//var connectionTypeRepo = new ConnectionTypeRepository();
//var familyConnectionRepo = new FamilyConnectionRepository();

//// Добавление персон
//var person1 = new Person(1, "Иванов", "Иван", "Иванович", "М",
//    new DateOnly(1980, 5, 15), null, "Биография...");
//var person2 = new Person(2, "Иванова", "Мария", "Петровна", "Ж",
//    new DateOnly(1985, 3, 20), null, "Биография...");

//personRepo.Add(person1);
//personRepo.Add(person2);

//// Создание связи
//var marriage = new FamilyConnection(1, 2, 1); // 1 = супружество
//familyConnectionRepo.Add(marriage);

//// Поиск связей
//var ivanConnections = familyConnectionRepo.FindConnectionsByPerson(1);




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
