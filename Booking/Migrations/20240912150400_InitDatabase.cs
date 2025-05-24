using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberAdult = table.Column<int>(type: "int", nullable: false),
                    NumberChildren = table.Column<int>(type: "int", nullable: false),
                    NumberInfant = table.Column<int>(type: "int", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ContentRequest = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    Tax = table.Column<decimal>(type: "money", nullable: true),
                    Discount = table.Column<decimal>(type: "money", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "money", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StatusBill = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryHotels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOutstanding = table.Column<bool>(type: "bit", nullable: false),
                    IdParent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryHotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryHotels_CategoryHotels_IdParent",
                        column: x => x.IdParent,
                        principalTable: "CategoryHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryTours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOutstanding = table.Column<bool>(type: "bit", nullable: false),
                    IdParent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryTours_CategoryTours_IdParent",
                        column: x => x.IdParent,
                        principalTable: "CategoryTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CityCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    CityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CodeIATA = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IdParent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityCodes_CityCodes_IdParent",
                        column: x => x.IdParent,
                        principalTable: "CityCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Medias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsAvatar = table.Column<bool>(type: "bit", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    AuthorType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AuthorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BirthDay = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<int>(type: "int", maxLength: 10, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UtilityName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountPayment = table.Column<int>(type: "int", nullable: false),
                    AmountPaid = table.Column<int>(type: "int", nullable: true),
                    NumberAccountReceived = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberAccountPayment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAccountPayment = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillPayments_Bills_BillId",
                        column: x => x.BillId,
                        principalTable: "Bills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Star = table.Column<int>(type: "int", nullable: false),
                    ResidenceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Slug = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Outstanding = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCateHotel = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hotels_CategoryHotels_IdCateHotel",
                        column: x => x.IdCateHotel,
                        principalTable: "CategoryHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TourName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Departure = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sightseeing = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Vehicle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    Discount = table.Column<decimal>(type: "money", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Overview = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TravelSchedule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceInclude = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ServiceNotInclude = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IdCateTour = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tours_CategoryTours_IdCateTour",
                        column: x => x.IdCateTour,
                        principalTable: "CategoryTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelUtilities",
                columns: table => new
                {
                    IdHotel = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUtility = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelUtilities", x => new { x.IdHotel, x.IdUtility });
                    table.ForeignKey(
                        name: "FK_HotelUtilities_Hotels_IdHotel",
                        column: x => x.IdHotel,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HotelUtilities_Utilities_IdUtility",
                        column: x => x.IdUtility,
                        principalTable: "Utilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Style = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    Discount = table.Column<decimal>(type: "money", nullable: true),
                    PriceShow = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Area = table.Column<int>(type: "int", nullable: false),
                    AmountPeople = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Bed = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    BedMore = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IdHotel = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Hotels_IdHotel",
                        column: x => x.IdHotel,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TravelSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTour = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TravelSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TravelSchedules_Tours_IdTour",
                        column: x => x.IdTour,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillPayments_BillId",
                table: "BillPayments",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "Index_Name",
                table: "CategoryHotels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryHotels_IdParent",
                table: "CategoryHotels",
                column: "IdParent");

            migrationBuilder.CreateIndex(
                name: "Index_Name",
                table: "CategoryTours",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTours_IdParent",
                table: "CategoryTours",
                column: "IdParent");

            migrationBuilder.CreateIndex(
                name: "Index_CityName_CodeIATA",
                table: "CityCodes",
                columns: new[] { "CityName", "CodeIATA" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityCodes_IdParent",
                table: "CityCodes",
                column: "IdParent");

            migrationBuilder.CreateIndex(
                name: "Index_Slug",
                table: "Hotels",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_IdCateHotel",
                table: "Hotels",
                column: "IdCateHotel");

            migrationBuilder.CreateIndex(
                name: "IX_HotelUtilities_IdUtility",
                table: "HotelUtilities",
                column: "IdUtility");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "Index_RoomName",
                table: "Rooms",
                column: "RoomName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_IdHotel",
                table: "Rooms",
                column: "IdHotel");

            migrationBuilder.CreateIndex(
                name: "Index_Slug",
                table: "Tours",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tours_IdCateTour",
                table: "Tours",
                column: "IdCateTour");

            migrationBuilder.CreateIndex(
                name: "IX_TravelSchedules_IdTour",
                table: "TravelSchedules",
                column: "IdTour");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "Index_Email_PhoneNumber",
                table: "Users",
                columns: new[] { "Email", "PhoneNumber" },
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "Index_UtilityName",
                table: "Utilities",
                column: "UtilityName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillPayments");

            migrationBuilder.DropTable(
                name: "CityCodes");

            migrationBuilder.DropTable(
                name: "HotelUtilities");

            migrationBuilder.DropTable(
                name: "Medias");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "TravelSchedules");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Bills");

            migrationBuilder.DropTable(
                name: "Utilities");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CategoryHotels");

            migrationBuilder.DropTable(
                name: "CategoryTours");
        }
    }
}
