var builder = DistributedApplication.CreateBuilder(args);

var mysql = builder.AddMySql("mysql")
                   .WithPhpMyAdmin()
                   .WithLifetime(ContainerLifetime.Persistent)
                   .WithDataVolume();

var mysqldb = mysql.AddDatabase("mysqldb");


builder.AddProject<Projects.SafeVault>("safevault")
    .WithReference(mysqldb)
    .WaitFor(mysqldb);

builder.Build().Run();
