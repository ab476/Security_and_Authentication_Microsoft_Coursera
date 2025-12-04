var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                   .WithLifetime(ContainerLifetime.Persistent)
                   .WithDataVolume();

var database = sql.AddDatabase("safevaultdb");


builder.AddProject<Projects.SafeVault>("safevault")
    .WithReference(database)
    .WaitFor(database);

builder.Build().Run();
