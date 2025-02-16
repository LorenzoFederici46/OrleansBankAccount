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

app.MapPost("checkingaccount", 
    async (IClusterClient clusterClient, CreateAccount createAccount) =>
{
    var checkingAccountID = Guid.NewGuid();
    var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
    await checkingAccountGrain.Initialize(createAccount.OpeningBalance);

    return TypedResults.Created($"checkingaccount/{checkingAccountID}");
});

app.MapGet("checkingaccount/{checkingAccountID}/balance", 
    async (IClusterClient clusterClient, Guid checkingAccountID) =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        var balance = await checkingAccountGrain.GetBalance();
        return TypedResults.Ok(balance);
    });

app.MapPost("checkingaccount/{checkingAccountID}/debit", 
    async (Guid checkingAccountID, Debit debit, IClusterClient clusterClient) =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Debit(debit.Amount);
        return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountID}/credit", 
    async (Guid checkingAccountID, Credit credit, IClusterClient clusterClient) =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Credit(credit.Amount);
        return TypedResults.NoContent();
});

app.MapPost("atm", 
    async (CreateAtm createAtm, IClusterClient clusterClient) =>
    {
        var atmID = Guid.NewGuid();
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmID);
        await atmGrain.Initialize(createAtm.InitialAtmCashBalance);
        return TypedResults.Created($"atm/{atmID}");
});

app.MapPost("atm/{atmID}/withdraw", 
    async (Guid atmID, AtmWithdraw atmWithdraw, IClusterClient clusterClient) =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmID);
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(atmWithdraw.CheckingAccountID);

        await atmGrain.Withdraw(atmWithdraw.CheckingAccountID, atmWithdraw.Amount);

});

app.MapPost("checkingaccount/{checkingAccountID}/transfer",
    async (Guid checkingAccountID, Transfer transfer, IClusterClient clusterClient) =>
    {
        var sender = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        var receiver = clusterClient.GetGrain<ICheckingAccountGrain>(transfer.CheckingAccountID);

        await sender.Transfer(transfer.CheckingAccountID, transfer.Amount);
        return TypedResults.NoContent();
    });

app.Run();
