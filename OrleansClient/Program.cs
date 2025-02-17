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

    client.UseTransactions();
});
var app = builder.Build();

app.MapPost("checkingaccount", 
    async (IClusterClient clusterClient, 
           ITransactionClient transactionClient, 
           CreateAccount createAccount) => {

    var checkingAccountID = Guid.NewGuid();
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Initialize(createAccount.OpeningBalance);
    });

    return TypedResults.Created($"checkingaccounnt/{checkingAccountID}");
});

app.MapGet("checkingaccount/{checkingAccountID}/balance", 
    async (IClusterClient clusterClient, 
           ITransactionClient transactionClient,
           Guid checkingAccountID) => {

    decimal balance = 0;
    await transactionClient.RunTransaction(TransactionOption.Create, async () => 
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        balance = await checkingAccountGrain.GetBalance();
    });

    return TypedResults.Ok(balance);
});

app.MapPost("checkingaccount/{checkingAccountID}/debit", 
    async (Guid checkingAccountID, Debit debit,
           ITransactionClient transactionClient, 
           IClusterClient clusterClient) => {
   
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
            var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
            await checkingAccountGrain.Debit(debit.Amount);
    });

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountID}/credit", 
    async (Guid checkingAccountID, Credit credit,
           ITransactionClient transactionClient,
           IClusterClient clusterClient) => {

    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        await checkingAccountGrain.Credit(credit.Amount);
    });

    return TypedResults.NoContent();
});

app.MapPost("checkingaccount/{checkingAccountID}/transfer",
    async (Guid checkingAccountID, Transfer transfer,
           ITransactionClient transactionClient,
           IClusterClient clusterClient) => {
    
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var sender = clusterClient.GetGrain<ICheckingAccountGrain>(checkingAccountID);
        var receiver = clusterClient.GetGrain<ICheckingAccountGrain>(transfer.CheckingAccountID);
        await sender.Transfer(transfer.CheckingAccountID, transfer.Amount);
    });
        return TypedResults.NoContent();
    });

app.MapPost("atm", 
    async (CreateAtm createAtm,
           ITransactionClient transactionClient,
           IClusterClient clusterClient) => {
    
    var atmID = Guid.NewGuid();
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmID);
        await atmGrain.Initialize(createAtm.InitialAtmCashBalance);
    });

    return TypedResults.Created($"atm/{atmID}");
});

app.MapGet("atm/{atmID}/balance", 
    async (Guid atmID, ITransactionClient transactionClient,
           IClusterClient clusterClient) => {
    
    decimal balance = 0;
    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmID);
        balance = await atmGrain.GetBalance();
    });

    return TypedResults.Ok(balance);
});

app.MapPost("atm/{atmID}/withdraw", 
    async (Guid atmID, AtmWithdraw atmWithdraw,
           ITransactionClient transactionClient, 
           IClusterClient clusterClient) => {

    await transactionClient.RunTransaction(TransactionOption.Create, async () =>
    {
        var atmGrain = clusterClient.GetGrain<IAtmGrain>(atmID);
        var checkingAccountGrain = clusterClient.GetGrain<ICheckingAccountGrain>(atmWithdraw.CheckingAccountID);
        await atmGrain.Withdraw(atmWithdraw.CheckingAccountID, atmWithdraw.Amount);
        await checkingAccountGrain.Debit(atmWithdraw.Amount);
    });
});

app.MapGet("customer/{customerID}/networth",
    async (IClusterClient clusterClient,Guid customerID) => {

        var customerGrain = clusterClient.GetGrain<ICustomerGrain>(customerID);
        var networth = await customerGrain.GetNetWorth();

        return TypedResults.Ok(networth);
});

app.MapPost("customer/{customerID}/addcheckingaccount", async (
    Guid customerID,
    CustomerCheckingAccount customerCheckingAccount,
    IClusterClient clusterClient) =>
{
    var customerGrain = clusterClient.GetGrain<ICustomerGrain>(customerID);
    await customerGrain.AddCheckingAccount(customerCheckingAccount.AccountID);
    return TypedResults.NoContent();
});

app.Run();
