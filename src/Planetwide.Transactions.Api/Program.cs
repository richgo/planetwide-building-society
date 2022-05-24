using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Planetwide.Graphql.Shared.Extensions;
using Planetwide.Shared;
using Planetwide.Shared.Extensions;
using Planetwide.Transactions.Api.Daemons;
using Planetwide.Transactions.Api.Features;
using Planetwide.Transactions.Api.Features.Transactions;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(_ =>
    {
        BsonClassMap.RegisterClassMap<BasicTransaction>();
        BsonClassMap.RegisterClassMap<DirectDebitTransaction>();
        
        var connectionString = builder.Configuration["Database:Mongo"];
        ArgumentNullException.ThrowIfNull(connectionString, "Mongo db connection string");
        return new MongoClient(connectionString);
    })
    .AddSingleton<IMongoDatabase>(sp =>
    {
        var mongo = sp.GetRequiredService<MongoClient>();
        return mongo.GetDatabase(WellKnown.Database.MongoDatabase);
    })
    .AddSingleton<IMongoCollection<TransactionBase>>(sp =>
    {
        var database = sp.GetRequiredService<IMongoDatabase>();
        return database.GetCollection<TransactionBase>("transactions");
    })
    .AddHostedService<MigrationBackgroundJob>()
    .AddAuthorization()
    .RegisterRedis()
    .RegisterOpenTelemetry("Planetwide.Transactions", builder.Configuration["Database:Zipkin"]);

builder.Services
    .AddHealthChecks()
    .AddRedis(builder.Configuration["Database:Redis"])
    .AddMongoDb(builder.Configuration["Database:Mongo"]);

builder.Services
    .AddMemoryCache()
    .AddGraphQLServer()
    .AddPlanetwideDefaults()
    .PublishSchemaDefinition(opt => opt
        .SetName(WellKnown.Schemas.SchemaKey)
        .PublishToRedis(WellKnown.Schemas.Transactions, sp => sp
            .GetRequiredService<ConnectionMultiplexer>()))
    .AddMongoDbFiltering()
    .AddMongoDbProjections()
    .AddMongoDbSorting()
    .AddMongoDbPagingProviders()
    .BindRuntimeType<ObjectId, IdType>()
    .AddTypeConverter<ObjectId, string>(x => x.ToString())
    .AddTypeConverter<string, ObjectId>(x => ObjectId.Parse(x))
    .AddQueryType<QueryRoot>()
    .AddMutationType<MutationRoot>()
    .AddSubscriptionType<SubscriptionRoot>()
    .RegisterObjectExtensions(typeof(Program).Assembly)
    .AddType<TransactionBase>()
    .AddType<BasicTransaction>()
    .AddType<DirectDebitTransaction>();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.UseWebSockets();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGraphQL();
    endpoints.MapDetailedHealthChecks();
});

app.Run();