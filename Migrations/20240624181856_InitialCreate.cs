using System;
using Microsoft.EntityFrameworkCore.Migrations;
using WooperUtility.Datacontext;

#nullable disable

namespace WooperUtility.Migrations;

#pragma warning disable CA1814
/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

        migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AdminSince = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

        migrationBuilder.CreateTable(
                name: "BannedUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BannedById = table.Column<long>(type: "INTEGER", nullable: false),
                    BanDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannedUsers_Users_BannedById",
                        column: x => x.BannedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BannedUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

        migrationBuilder.CreateTable(
            name: "BotRequests",
            columns: table => new
            {
                Id = table.Column<long>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RequestDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                RequestType = table.Column<string>(type: "TEXT", nullable: false),
                UserId = table.Column<long>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BotRequests", x => x.Id);
                table.ForeignKey(
                    name: "FK_BotRequests_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Admins_UserId",
            table: "Admins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BannedUsers_BannedById",
            table: "BannedUsers",
            column: "BannedById");

        migrationBuilder.CreateIndex(
            name: "IX_BannedUsers_UserId",
            table: "BannedUsers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_BotRequests_UserId",
            table: "BotRequests",
            column: "UserId");


        migrationBuilder.InsertData("Users",
                                columns: ["Id", "Username", "JoinDate", "LastActivity"],
                                values: new object[,] {
                                    { 123123, "xxxxxx", DateTime.UtcNow, DateTime.UtcNow }
                                });
        migrationBuilder.InsertData("Admins",
                                columns: ["AdminSince", "UserId"],
                                values: new object[,] {
                                    { DateTime.UtcNow, 123123 }
                                });

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);
        migrationBuilder.DropTable(
            name: "Admins");

        migrationBuilder.DropTable(
            name: "BannedUsers");

        migrationBuilder.DropTable(
            name: "BotRequests");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
#pragma warning restore CA1814

