using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Alerting.Domain.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telegramBots",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Token = table.Column<string>(nullable: true),
                    ClientId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegramBots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telegramBots_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alertings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ClientId = table.Column<Guid>(nullable: true),
                    BotId = table.Column<Guid>(nullable: true),
                    ChatId = table.Column<string>(nullable: true),
                    WaitingInterval = table.Column<int>(nullable: false),
                    LastAlerted = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alertings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alertings_telegramBots_BotId",
                        column: x => x.BotId,
                        principalTable: "telegramBots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_alertings_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alertings_BotId",
                table: "alertings",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_alertings_ClientId",
                table: "alertings",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_telegramBots_ClientId",
                table: "telegramBots",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alertings");

            migrationBuilder.DropTable(
                name: "telegramBots");

            migrationBuilder.DropTable(
                name: "clients");
        }
    }
}
