using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMPS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpgradePayrollSalaryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxDeductions",
                table: "Payrolls",
                newName: "OvertimeRatePerHour");

            migrationBuilder.RenameColumn(
                name: "OvertimePay",
                table: "Payrolls",
                newName: "OvertimeHours");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Payrolls",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "Payrolls",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Payrolls");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Payrolls");

            migrationBuilder.RenameColumn(
                name: "OvertimeRatePerHour",
                table: "Payrolls",
                newName: "TaxDeductions");

            migrationBuilder.RenameColumn(
                name: "OvertimeHours",
                table: "Payrolls",
                newName: "OvertimePay");

            migrationBuilder.AlterColumn<string>(
                name: "EntityId",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
