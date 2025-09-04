using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GorevNet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTaskTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "UserTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "UserTasks",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "UserTasks");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "UserTasks");
        }
    }
}
