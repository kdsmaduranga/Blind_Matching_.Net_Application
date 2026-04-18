var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/index.html");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapFallbackToFile("index.html");

app.Run();