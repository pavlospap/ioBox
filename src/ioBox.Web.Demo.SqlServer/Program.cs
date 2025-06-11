using System.Transactions;

using Dapper;

using EasyNetQ;

using IOBox;
using IOBox.Persistence;
using IOBox.SqlServer;
using IOBox.Web.Demo.SqlServer;

using Microsoft.Data.SqlClient;

using UUIDNext;

var builder = WebApplication.CreateBuilder(args);

builder.Services

    .AddIOBox(builder.Configuration)

    .AddIOBoxSqlServer(builder.Configuration)

    .AddSingleton<IMessageProcessor, MessageProcessor>()

    .AddHostedService<MessageListener>()

    .AddSingleton(RabbitHutch.CreateBus(
        builder.Configuration.GetConnectionString("RabbitMQConnection")));

var app = builder.Build();

app.UseHttpsRedirection();

app.UseIOBox(builder.Configuration);

MigrateDb(builder.Configuration);

app.MapPost("members", async (
    IConfiguration configuration,
    IDbContext dbContext,
    IDbStore dbStore,
    NewMemberInsertDto member,
    CancellationToken cancellationToken = default) =>
{
    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

    using var connection = new SqlConnection(
        configuration.GetConnectionString("DbConnection"));

    var memberId = Uuid.NewDatabaseFriendly(Database.SqlServer);

    var sql = @"
        INSERT INTO App.Members (Id, Email, HasBeenWelcomed, HasBeenGivenBonus) 
        VALUES (@memberId, @Email, 0, 0);";

    var command = new CommandDefinition(
        sql,
        new { memberId, member.Email },
        cancellationToken: cancellationToken);

    await connection.ExecuteAsync(command);

    var welcomeMessageId = Uuid.NewDatabaseFriendly(Database.SqlServer)
        .ToString();

    await dbStore.AddNewMessageAsync(
        messageId: welcomeMessageId,
        message: JsonSerializer.Serialize(new NewMemberWelcomeMessage(
            welcomeMessageId,
            memberId,
            member.Email)),
        ioName: "Outbox01",
        contextInfo: "POST: /members",
        cancellationToken: cancellationToken);

    var bonusMessageId = Uuid.NewDatabaseFriendly(Database.SqlServer)
        .ToString();

    await dbStore.AddNewMessageAsync(
        messageId: bonusMessageId,
        message: JsonSerializer.Serialize(new NewMemberBonusMessage(
            bonusMessageId,
            memberId,
            member.Email)),
        ioName: "Outbox02",
        contextInfo: "POST: /members",
        cancellationToken: cancellationToken);

    scope.Complete();

    return Results.Ok();
});

app.Run();

static void MigrateDb(IConfiguration configuration)
{
    using var connection = new SqlConnection(
        configuration.GetConnectionString("DbConnection"));

    var sql = @"
        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'App')
        BEGIN
            EXEC('CREATE SCHEMA App');
        END

        IF NOT EXISTS (
            SELECT * FROM sys.objects 
            WHERE object_id = OBJECT_ID(N'App.Members')
        )
        BEGIN
            CREATE TABLE App.Members (
                Id UNIQUEIDENTIFIER NOT NULL,
                Email NVARCHAR(50) NOT NULL,
                HasBeenWelcomed tinyint NOT NULL,
                HasBeenGivenBonus tinyint NOT NULL,
                CONSTRAINT PK_Members PRIMARY KEY (Id)
            );

            CREATE UNIQUE INDEX UX_Members_Email 
                ON App.Members(Email);
        END";

    connection.Execute(sql);
}

record NewMemberInsertDto(string Email);

record NewMemberWelcomeMessage(
    string MessageId,
    Guid MemberId,
    string MemberEmail);

record NewMemberBonusMessage(
    string MessageId,
    Guid MemberId,
    string MemberEmail);
