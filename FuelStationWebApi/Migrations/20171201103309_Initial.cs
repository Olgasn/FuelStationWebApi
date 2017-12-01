using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FuelStationWebApi.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fuels",
                columns: table => new
                {
                    FuelID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FuelDensity = table.Column<float>(nullable: false),
                    FuelType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fuels", x => x.FuelID);
                });

            migrationBuilder.CreateTable(
                name: "Tanks",
                columns: table => new
                {
                    TankID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TankMaterial = table.Column<string>(nullable: true),
                    TankPicture = table.Column<string>(nullable: true),
                    TankType = table.Column<string>(nullable: true),
                    TankVolume = table.Column<float>(nullable: false),
                    TankWeight = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tanks", x => x.TankID);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    OperationID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    FuelID = table.Column<int>(nullable: false),
                    Inc_Exp = table.Column<float>(nullable: true),
                    TankID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.OperationID);
                    table.ForeignKey(
                        name: "FK_Operations_Fuels_FuelID",
                        column: x => x.FuelID,
                        principalTable: "Fuels",
                        principalColumn: "FuelID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Operations_Tanks_TankID",
                        column: x => x.TankID,
                        principalTable: "Tanks",
                        principalColumn: "TankID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_FuelID",
                table: "Operations",
                column: "FuelID");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_TankID",
                table: "Operations",
                column: "TankID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations");

            migrationBuilder.DropTable(
                name: "Fuels");

            migrationBuilder.DropTable(
                name: "Tanks");
        }
    }
}
