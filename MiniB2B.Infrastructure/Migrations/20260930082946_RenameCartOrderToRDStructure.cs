using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniB2B.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCartOrderToRDStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut Cart/Order tablolarını veri kaybını önlemek için silip yeniden oluşturmak yerine R/D isimlerine rename ederek uyarladım.
            migrationBuilder.DropForeignKey(name: "FK_CartItems_Carts_CartId", table: "CartItems");
            migrationBuilder.DropForeignKey(name: "FK_CartItems_Products_ProductId", table: "CartItems");
            migrationBuilder.DropForeignKey(name: "FK_Carts_AspNetUsers_UserId", table: "Carts");
            migrationBuilder.DropForeignKey(name: "FK_OrderItems_Orders_OrderId", table: "OrderItems");
            migrationBuilder.DropForeignKey(name: "FK_OrderItems_Products_ProductId", table: "OrderItems");
            migrationBuilder.DropForeignKey(name: "FK_Orders_AspNetUsers_UserId", table: "Orders");

            migrationBuilder.DropCheckConstraint(name: "CK_CartItems_Quantity_Positive", table: "CartItems");
            migrationBuilder.DropCheckConstraint(name: "CK_OrderItems_Quantity_Positive", table: "OrderItems");
            migrationBuilder.DropCheckConstraint(name: "CK_OrderItems_TotalPrice_NonNegative", table: "OrderItems");
            migrationBuilder.DropCheckConstraint(name: "CK_OrderItems_UnitPrice_NonNegative", table: "OrderItems");
            migrationBuilder.DropCheckConstraint(name: "CK_Orders_TotalAmount_NonNegative", table: "Orders");

            migrationBuilder.DropPrimaryKey(name: "PK_CartItems", table: "CartItems");
            migrationBuilder.DropPrimaryKey(name: "PK_Carts", table: "Carts");
            migrationBuilder.DropPrimaryKey(name: "PK_OrderItems", table: "OrderItems");
            migrationBuilder.DropPrimaryKey(name: "PK_Orders", table: "Orders");

            migrationBuilder.DropIndex(name: "IX_CartItems_CartId_ProductId", table: "CartItems");
            migrationBuilder.DropIndex(name: "IX_CartItems_ProductId", table: "CartItems");
            migrationBuilder.DropIndex(name: "IX_Carts_UserId", table: "Carts");
            migrationBuilder.DropIndex(name: "IX_OrderItems_OrderId", table: "OrderItems");
            migrationBuilder.DropIndex(name: "IX_OrderItems_ProductId", table: "OrderItems");
            migrationBuilder.DropIndex(name: "IX_Orders_OrderNumber", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_Status", table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_UserId_OrderDate", table: "Orders");

            migrationBuilder.RenameTable(name: "Carts", newName: "SepetR");
            migrationBuilder.RenameTable(name: "CartItems", newName: "SepetD");
            migrationBuilder.RenameTable(name: "Orders", newName: "SiparisR");
            migrationBuilder.RenameTable(name: "OrderItems", newName: "SiparisD");

            migrationBuilder.RenameColumn(name: "CartId", table: "SepetD", newName: "SepetRId");
            migrationBuilder.RenameColumn(name: "OrderId", table: "SiparisD", newName: "SiparisRId");

            // SepetR ana kaydında sepet toplamını tutmak için TotalAmount alanını ekledim.
            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "SepetR",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // SiparisR kaydını kaynak SepetR ile takip etmek için nullable SepetId bağlantısını ekledim.
            migrationBuilder.AddColumn<int>(
                name: "SepetId",
                table: "SiparisR",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE sr
                SET TotalAmount = COALESCE(t.TotalAmount, 0)
                FROM SepetR sr
                OUTER APPLY
                (
                    SELECT SUM(CAST(p.Price * sd.Quantity AS decimal(18,2))) AS TotalAmount
                    FROM SepetD sd
                    INNER JOIN Products p ON p.Id = sd.ProductId
                    WHERE sd.SepetRId = sr.Id
                ) t
                """);

            migrationBuilder.AddPrimaryKey(name: "PK_SepetD", table: "SepetD", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_SepetR", table: "SepetR", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_SiparisD", table: "SiparisD", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_SiparisR", table: "SiparisR", column: "Id");

            migrationBuilder.AddCheckConstraint(name: "CK_SepetD_Quantity_Positive", table: "SepetD", sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_SepetR_TotalAmount_NonNegative", table: "SepetR", sql: "[TotalAmount] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_SiparisD_Quantity_Positive", table: "SiparisD", sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_SiparisD_TotalPrice_NonNegative", table: "SiparisD", sql: "[TotalPrice] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_SiparisD_UnitPrice_NonNegative", table: "SiparisD", sql: "[UnitPrice] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_SiparisR_TotalAmount_NonNegative", table: "SiparisR", sql: "[TotalAmount] >= 0");

            migrationBuilder.CreateIndex(name: "IX_SepetD_ProductId", table: "SepetD", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_SepetD_SepetRId_ProductId", table: "SepetD", columns: new[] { "SepetRId", "ProductId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_SepetR_UserId", table: "SepetR", column: "UserId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_SiparisD_ProductId", table: "SiparisD", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_SiparisD_SiparisRId", table: "SiparisD", column: "SiparisRId");
            migrationBuilder.CreateIndex(name: "IX_SiparisR_OrderNumber", table: "SiparisR", column: "OrderNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_SiparisR_SepetId", table: "SiparisR", column: "SepetId");
            migrationBuilder.CreateIndex(name: "IX_SiparisR_Status", table: "SiparisR", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_SiparisR_UserId_OrderDate", table: "SiparisR", columns: new[] { "UserId", "OrderDate" });

            migrationBuilder.AddForeignKey(name: "FK_SepetD_Products_ProductId", table: "SepetD", column: "ProductId", principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_SepetD_SepetR_SepetRId", table: "SepetD", column: "SepetRId", principalTable: "SepetR", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_SepetR_AspNetUsers_UserId", table: "SepetR", column: "UserId", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_SiparisD_Products_ProductId", table: "SiparisD", column: "ProductId", principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
            migrationBuilder.AddForeignKey(name: "FK_SiparisD_SiparisR_SiparisRId", table: "SiparisD", column: "SiparisRId", principalTable: "SiparisR", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_SiparisR_AspNetUsers_UserId", table: "SiparisR", column: "UserId", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_SiparisR_SepetR_SepetId", table: "SiparisR", column: "SepetId", principalTable: "SepetR", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_SepetD_Products_ProductId", table: "SepetD");
            migrationBuilder.DropForeignKey(name: "FK_SepetD_SepetR_SepetRId", table: "SepetD");
            migrationBuilder.DropForeignKey(name: "FK_SepetR_AspNetUsers_UserId", table: "SepetR");
            migrationBuilder.DropForeignKey(name: "FK_SiparisD_Products_ProductId", table: "SiparisD");
            migrationBuilder.DropForeignKey(name: "FK_SiparisD_SiparisR_SiparisRId", table: "SiparisD");
            migrationBuilder.DropForeignKey(name: "FK_SiparisR_AspNetUsers_UserId", table: "SiparisR");
            migrationBuilder.DropForeignKey(name: "FK_SiparisR_SepetR_SepetId", table: "SiparisR");

            migrationBuilder.DropCheckConstraint(name: "CK_SepetD_Quantity_Positive", table: "SepetD");
            migrationBuilder.DropCheckConstraint(name: "CK_SepetR_TotalAmount_NonNegative", table: "SepetR");
            migrationBuilder.DropCheckConstraint(name: "CK_SiparisD_Quantity_Positive", table: "SiparisD");
            migrationBuilder.DropCheckConstraint(name: "CK_SiparisD_TotalPrice_NonNegative", table: "SiparisD");
            migrationBuilder.DropCheckConstraint(name: "CK_SiparisD_UnitPrice_NonNegative", table: "SiparisD");
            migrationBuilder.DropCheckConstraint(name: "CK_SiparisR_TotalAmount_NonNegative", table: "SiparisR");

            migrationBuilder.DropPrimaryKey(name: "PK_SepetD", table: "SepetD");
            migrationBuilder.DropPrimaryKey(name: "PK_SepetR", table: "SepetR");
            migrationBuilder.DropPrimaryKey(name: "PK_SiparisD", table: "SiparisD");
            migrationBuilder.DropPrimaryKey(name: "PK_SiparisR", table: "SiparisR");

            migrationBuilder.DropIndex(name: "IX_SepetD_ProductId", table: "SepetD");
            migrationBuilder.DropIndex(name: "IX_SepetD_SepetRId_ProductId", table: "SepetD");
            migrationBuilder.DropIndex(name: "IX_SepetR_UserId", table: "SepetR");
            migrationBuilder.DropIndex(name: "IX_SiparisD_ProductId", table: "SiparisD");
            migrationBuilder.DropIndex(name: "IX_SiparisD_SiparisRId", table: "SiparisD");
            migrationBuilder.DropIndex(name: "IX_SiparisR_OrderNumber", table: "SiparisR");
            migrationBuilder.DropIndex(name: "IX_SiparisR_SepetId", table: "SiparisR");
            migrationBuilder.DropIndex(name: "IX_SiparisR_Status", table: "SiparisR");
            migrationBuilder.DropIndex(name: "IX_SiparisR_UserId_OrderDate", table: "SiparisR");

            migrationBuilder.DropColumn(name: "TotalAmount", table: "SepetR");
            migrationBuilder.DropColumn(name: "SepetId", table: "SiparisR");

            migrationBuilder.RenameColumn(name: "SepetRId", table: "SepetD", newName: "CartId");
            migrationBuilder.RenameColumn(name: "SiparisRId", table: "SiparisD", newName: "OrderId");

            migrationBuilder.RenameTable(name: "SepetR", newName: "Carts");
            migrationBuilder.RenameTable(name: "SepetD", newName: "CartItems");
            migrationBuilder.RenameTable(name: "SiparisR", newName: "Orders");
            migrationBuilder.RenameTable(name: "SiparisD", newName: "OrderItems");

            migrationBuilder.AddPrimaryKey(name: "PK_CartItems", table: "CartItems", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_Carts", table: "Carts", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_OrderItems", table: "OrderItems", column: "Id");
            migrationBuilder.AddPrimaryKey(name: "PK_Orders", table: "Orders", column: "Id");

            migrationBuilder.AddCheckConstraint(name: "CK_CartItems_Quantity_Positive", table: "CartItems", sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_OrderItems_Quantity_Positive", table: "OrderItems", sql: "[Quantity] > 0");
            migrationBuilder.AddCheckConstraint(name: "CK_OrderItems_TotalPrice_NonNegative", table: "OrderItems", sql: "[TotalPrice] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_OrderItems_UnitPrice_NonNegative", table: "OrderItems", sql: "[UnitPrice] >= 0");
            migrationBuilder.AddCheckConstraint(name: "CK_Orders_TotalAmount_NonNegative", table: "Orders", sql: "[TotalAmount] >= 0");

            migrationBuilder.CreateIndex(name: "IX_CartItems_CartId_ProductId", table: "CartItems", columns: new[] { "CartId", "ProductId" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_CartItems_ProductId", table: "CartItems", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_Carts_UserId", table: "Carts", column: "UserId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_OrderItems_OrderId", table: "OrderItems", column: "OrderId");
            migrationBuilder.CreateIndex(name: "IX_OrderItems_ProductId", table: "OrderItems", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_Orders_OrderNumber", table: "Orders", column: "OrderNumber", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Orders_Status", table: "Orders", column: "Status");
            migrationBuilder.CreateIndex(name: "IX_Orders_UserId_OrderDate", table: "Orders", columns: new[] { "UserId", "OrderDate" });

            migrationBuilder.AddForeignKey(name: "FK_CartItems_Carts_CartId", table: "CartItems", column: "CartId", principalTable: "Carts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_CartItems_Products_ProductId", table: "CartItems", column: "ProductId", principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Carts_AspNetUsers_UserId", table: "Carts", column: "UserId", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_OrderItems_Orders_OrderId", table: "OrderItems", column: "OrderId", principalTable: "Orders", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_OrderItems_Products_ProductId", table: "OrderItems", column: "ProductId", principalTable: "Products", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
            migrationBuilder.AddForeignKey(name: "FK_Orders_AspNetUsers_UserId", table: "Orders", column: "UserId", principalTable: "AspNetUsers", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }
    }
}
