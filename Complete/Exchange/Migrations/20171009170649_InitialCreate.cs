using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Exchange.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExecutionReport",
                columns: table => new
                {
                    ExecId = table.Column<string>(type: "varchar(127)", nullable: false),
                    AvgPx = table.Column<long>(type: "bigint", nullable: false),
                    ClOrdID = table.Column<string>(type: "longtext", nullable: true),
                    ClientID = table.Column<string>(type: "longtext", nullable: true),
                    CumQty = table.Column<long>(type: "bigint", nullable: false),
                    CummCommission = table.Column<long>(type: "bigint", nullable: false),
                    CxlRejReason = table.Column<int>(type: "int", nullable: true),
                    EscrowRestricted = table.Column<bool>(type: "bit", nullable: false),
                    ExecRefID = table.Column<string>(type: "longtext", nullable: true),
                    ExecType = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<long>(type: "bigint", nullable: false),
                    LastCommission = table.Column<long>(type: "bigint", nullable: false),
                    LastLiquidityInd = table.Column<int>(type: "int", nullable: false),
                    LastPx = table.Column<long>(type: "bigint", nullable: false),
                    LastQty = table.Column<long>(type: "bigint", nullable: false),
                    LeavesEscrow = table.Column<long>(type: "bigint", nullable: false),
                    MassStatusReqID = table.Column<string>(type: "longtext", nullable: true),
                    OrdRejReason = table.Column<int>(type: "int", nullable: true),
                    OrdStatus = table.Column<int>(type: "int", nullable: false),
                    OrdStatusReqID = table.Column<string>(type: "longtext", nullable: true),
                    OrdType = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<string>(type: "longtext", nullable: true),
                    OrderQty = table.Column<long>(type: "bigint", nullable: false),
                    OrigClOrdID = table.Column<string>(type: "longtext", nullable: true),
                    OrigEscrow = table.Column<long>(type: "bigint", nullable: false),
                    PegOffset = table.Column<long>(type: "bigint", nullable: true),
                    PegPriceType = table.Column<int>(type: "int", nullable: true),
                    PegScope = table.Column<int>(type: "int", nullable: true),
                    PeggedPrice = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    SecondaryOrderId = table.Column<string>(type: "longtext", nullable: true),
                    SeqNum = table.Column<long>(type: "bigint", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    StopPx = table.Column<long>(type: "bigint", nullable: false),
                    SubmitTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Symbol = table.Column<string>(type: "longtext", nullable: true),
                    TimeInForce = table.Column<int>(type: "int", nullable: false),
                    TotalNumReports = table.Column<int>(type: "int", nullable: true),
                    TradeId = table.Column<string>(type: "longtext", nullable: true),
                    TransactTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TrdMatchID = table.Column<string>(type: "longtext", nullable: true),
                    TriggerPriceType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionReport", x => x.ExecId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionReport_OrdStatus",
                table: "ExecutionReport",
                column: "OrdStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExecutionReport");
        }
    }
}
