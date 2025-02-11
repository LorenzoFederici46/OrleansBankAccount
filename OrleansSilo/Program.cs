using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
await Host.CreateDefaultBuilder(args)
    .UseOrleans(siloBuilder =>
    {
        siloBuilder.UseAzureStorageClustering(configureOptions: options =>
        {
            options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "OrleansCluster";
            options.ServiceId = "OrleansService";
        });

        siloBuilder.AddAzureTableGrainStorage("tableStorage", configureOptions: options =>
        {
            options.TableServiceClient = new Azure.Data.Tables.TableServiceClient("UseDevelopmentStorage=true;");
        });

        siloBuilder.AddAzureBlobGrainStorage("blobStorage", configureOptions: options =>
        {
            options.BlobServiceClient = new Azure.Storage.Blobs.BlobServiceClient("UseDevelopmentStorage=true;");
        });

        //siloBuilder.Configure<GrainCollectionOptions>(options =>
        //{
        //    options.CollectionQuantum = TimeSpan.FromSeconds(20);
        //    options.CollectionAge = TimeSpan.FromSeconds(20);
        //});
    }).RunConsoleAsync();

