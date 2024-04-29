using ProtoPivot;
using ProtoPivot.ExampleWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddRouting();
builder.Services.AddProtoPivotServices();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapProtoPivotService<IProductService>("product");
});

app.MapGet("/", (d) =>
{
    //return "Hello World!";
    return Task.CompletedTask;
});

app.Run();
