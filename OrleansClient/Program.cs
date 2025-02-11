
using Orleans.Configuration;
using OrleansClient.Contracts;
using OrleansGrains.Abstraction;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseOrleansClient((context, client) =>
{
    client.UseAzureStorageClustering(configureOptions: options =>
    {
        options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
    });

    client.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "OrleansCluster";
        options.ServiceId = "OrleansService";
    });
});
var app = builder.Build();

app.MapGet("checkingaccount/{checkingAccountID}/balance", 
    async (IClusterClient clusterClient, Guid checkingAccountID) =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        var balance = await checkingAccountGrain.getBalance();
        return TypedResults.Ok(balance);
    });

app.MapPost("checkingaccount", async (IClusterClient clusterClient, CreateAccount createAccount) =>
{
    var checkingAccountID = Guid.NewGuid();
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
    await checkingAccountGrain.initialize(createAccount.OpeningBalance);

    return TypedResults.Created($"checkingaccount/{checkingAccountID}");
});

app.MapPost("checkingaccount/{checkingAccountID}/debit", async (
    Guid checkingAccountID,
    Debit debit,
    IClusterClient clusterClient) =>
{
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Debit(debit.Amount);

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountID}/credit", async (
    Guid checkingAccountID,
    Credit credit,
    IClusterClient clusterClient) =>
{
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Credit(credit.Amount);

    return TypedResults.NoContent();
});

app.Run();
