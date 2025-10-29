using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MetalfluxApi.Server.Migrations
{
    /// <inheritdoc />
    public partial class Edit_ids_from_int_to_long_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<long>(
                    name: "id",
                    table: "media",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "integer"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AlterColumn<long>(
                    name: "id",
                    table: "app_user",
                    type: "bigint",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "integer"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterColumn<int>(
                    name: "id",
                    table: "media",
                    type: "integer",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );

            migrationBuilder
                .AlterColumn<int>(
                    name: "id",
                    table: "app_user",
                    type: "integer",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "bigint"
                )
                .Annotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                )
                .OldAnnotation(
                    "Npgsql:ValueGenerationStrategy",
                    NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                );
        }
    }
}
