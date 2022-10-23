using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Alerting.Domain.Migrations
{
    public partial class StateTypesAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alertings_telegramBots_BotId",
                table: "alertings");

            migrationBuilder.DropForeignKey(
                name: "FK_alertings_clients_ClientId",
                table: "alertings");

            migrationBuilder.DropForeignKey(
                name: "FK_telegramBots_clients_ClientId",
                table: "telegramBots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_telegramBots",
                table: "telegramBots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_clients",
                table: "clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alertings",
                table: "alertings");

            migrationBuilder.DropIndex(
                name: "IX_alertings_ClientId",
                table: "alertings");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "alertings");

            migrationBuilder.DropColumn(
                name: "LastAlerted",
                table: "alertings");

            migrationBuilder.RenameTable(
                name: "telegramBots",
                newName: "TelegramBots");

            migrationBuilder.RenameTable(
                name: "clients",
                newName: "Clients");

            migrationBuilder.RenameTable(
                name: "alertings",
                newName: "Alertings");

            migrationBuilder.RenameIndex(
                name: "IX_telegramBots_ClientId",
                table: "TelegramBots",
                newName: "IX_TelegramBots_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_alertings_BotId",
                table: "Alertings",
                newName: "IX_Alertings_BotId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientStateId",
                table: "Alertings",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TelegramBots",
                table: "TelegramBots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clients",
                table: "Clients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alertings",
                table: "Alertings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StateTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: true),
                    LastActive = table.Column<DateTime>(nullable: false),
                    LastAlerted = table.Column<DateTime>(nullable: false),
                    TypeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientStates_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientStates_StateTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "StateTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertings_ClientStateId",
                table: "Alertings",
                column: "ClientStateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientStates_ClientId",
                table: "ClientStates",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientStates_TypeId",
                table: "ClientStates",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alertings_TelegramBots_BotId",
                table: "Alertings",
                column: "BotId",
                principalTable: "TelegramBots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Alertings_ClientStates_ClientStateId",
                table: "Alertings",
                column: "ClientStateId",
                principalTable: "ClientStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramBots_Clients_ClientId",
                table: "TelegramBots",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alertings_TelegramBots_BotId",
                table: "Alertings");

            migrationBuilder.DropForeignKey(
                name: "FK_Alertings_ClientStates_ClientStateId",
                table: "Alertings");

            migrationBuilder.DropForeignKey(
                name: "FK_TelegramBots_Clients_ClientId",
                table: "TelegramBots");

            migrationBuilder.DropTable(
                name: "ClientStates");

            migrationBuilder.DropTable(
                name: "StateTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TelegramBots",
                table: "TelegramBots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clients",
                table: "Clients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Alertings",
                table: "Alertings");

            migrationBuilder.DropIndex(
                name: "IX_Alertings_ClientStateId",
                table: "Alertings");

            migrationBuilder.DropColumn(
                name: "ClientStateId",
                table: "Alertings");

            migrationBuilder.RenameTable(
                name: "TelegramBots",
                newName: "telegramBots");

            migrationBuilder.RenameTable(
                name: "Clients",
                newName: "clients");

            migrationBuilder.RenameTable(
                name: "Alertings",
                newName: "alertings");

            migrationBuilder.RenameIndex(
                name: "IX_TelegramBots_ClientId",
                table: "telegramBots",
                newName: "IX_telegramBots_ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Alertings_BotId",
                table: "alertings",
                newName: "IX_alertings_BotId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "alertings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAlerted",
                table: "alertings",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_telegramBots",
                table: "telegramBots",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_clients",
                table: "clients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_alertings",
                table: "alertings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_alertings_ClientId",
                table: "alertings",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_alertings_telegramBots_BotId",
                table: "alertings",
                column: "BotId",
                principalTable: "telegramBots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_alertings_clients_ClientId",
                table: "alertings",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_telegramBots_clients_ClientId",
                table: "telegramBots",
                column: "ClientId",
                principalTable: "clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
