var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMySqlDataSource(connectionName: "mysqldb");

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();

