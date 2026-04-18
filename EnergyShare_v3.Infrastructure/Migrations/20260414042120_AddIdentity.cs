using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyShare_v3.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organismes_publics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organismes_publics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SocieteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    NumeroEntreprise = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    OrganismePublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_organismes_publics_OrganismePublicId",
                        column: x => x.OrganismePublicId,
                        principalTable: "organismes_publics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EnergieType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Perimetre = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    VendeurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GestionnairePartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partages_users_GestionnairePartageId",
                        column: x => x.GestionnairePartageId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_partages_users_VendeurId",
                        column: x => x.VendeurId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "point_accesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdresseLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CodePostal = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsInjectionPoint = table.Column<bool>(type: "bit", nullable: false),
                    Fournisseur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmartMeter_Encrypted = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EAN_Encrypted = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccordConsentement = table.Column<bool>(type: "bit", nullable: false),
                    DateAccordConsentement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRetraitConsentement = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_accesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_point_accesses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "data_partage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VolumePartage_kWh = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ImportedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_partage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_data_partage_partages_PartageId",
                        column: x => x.PartageId,
                        principalTable: "partages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "demandes_grd",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateDemande = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReponse = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DetailsDemande = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CommentaireReponseGRD = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResponseStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DemandeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerimetreConfirme = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DemandeurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganismePublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgentTraitantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_demandes_grd", x => x.Id);
                    table.ForeignKey(
                        name: "FK_demandes_grd_organismes_publics_OrganismePublicId",
                        column: x => x.OrganismePublicId,
                        principalTable: "organismes_publics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demandes_grd_partages_PartageId",
                        column: x => x.PartageId,
                        principalTable: "partages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_demandes_grd_users_AgentTraitantId",
                        column: x => x.AgentTraitantId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_demandes_grd_users_DemandeurId",
                        column: x => x.DemandeurId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents_partage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomFichier = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CheminStockage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsSigned = table.Column<bool>(type: "bit", nullable: false),
                    TypeDocument = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents_partage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documents_partage_partages_PartageId",
                        column: x => x.PartageId,
                        principalTable: "partages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documents_partage_users_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tarifs_accord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Montant = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true),
                    PartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifs_accord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tarifs_accord_partages_PartageId",
                        column: x => x.PartageId,
                        principalTable: "partages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistanceCalculee = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    PointAccessVendeurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointAccessAcheteurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matches_point_accesses_PointAccessAcheteurId",
                        column: x => x.PointAccessAcheteurId,
                        principalTable: "point_accesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matches_point_accesses_PointAccessVendeurId",
                        column: x => x.PointAccessVendeurId,
                        principalTable: "point_accesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "participations_partage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsInterlocuteurUnique = table.Column<bool>(type: "bit", nullable: false),
                    UserRolePartage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExitAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCommunicationPreavis = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateSortiePlanifiee = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participations_partage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_participations_partage_partages_PartageId",
                        column: x => x.PartageId,
                        principalTable: "partages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_participations_partage_point_accesses_PointAccessId",
                        column: x => x.PointAccessId,
                        principalTable: "point_accesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "profils_energie",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DemandeEnergie_kWh = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    OffreEnergie_kWh = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PrixAchatCible_Eur = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PrixVenteCible_Eur = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    PointAccessId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Audit_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Audit_UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profils_energie", x => x.Id);
                    table.ForeignKey(
                        name: "FK_profils_energie_point_accesses_PointAccessId",
                        column: x => x.PointAccessId,
                        principalTable: "point_accesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ObjetMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contenu = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    DateEnvoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLu = table.Column<bool>(type: "bit", nullable: false),
                    ExpediteurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinataireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_messages_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_messages_users_DestinataireId",
                        column: x => x.DestinataireId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_ExpediteurId",
                        column: x => x.ExpediteurId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_data_partage_PartageId",
                table: "data_partage",
                column: "PartageId");

            migrationBuilder.CreateIndex(
                name: "IX_demandes_grd_AgentTraitantId",
                table: "demandes_grd",
                column: "AgentTraitantId");

            migrationBuilder.CreateIndex(
                name: "IX_demandes_grd_DemandeurId",
                table: "demandes_grd",
                column: "DemandeurId");

            migrationBuilder.CreateIndex(
                name: "IX_demandes_grd_OrganismePublicId",
                table: "demandes_grd",
                column: "OrganismePublicId");

            migrationBuilder.CreateIndex(
                name: "IX_demandes_grd_PartageId",
                table: "demandes_grd",
                column: "PartageId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_partage_PartageId",
                table: "documents_partage",
                column: "PartageId");

            migrationBuilder.CreateIndex(
                name: "IX_documents_partage_UploadedById",
                table: "documents_partage",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_matches_PointAccessAcheteurId",
                table: "matches",
                column: "PointAccessAcheteurId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_PointAccessVendeurId_PointAccessAcheteurId",
                table: "matches",
                columns: new[] { "PointAccessVendeurId", "PointAccessAcheteurId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_DestinataireId",
                table: "messages",
                column: "DestinataireId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_ExpediteurId",
                table: "messages",
                column: "ExpediteurId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_MatchId",
                table: "messages",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_partages_GestionnairePartageId",
                table: "partages",
                column: "GestionnairePartageId");

            migrationBuilder.CreateIndex(
                name: "IX_partages_VendeurId",
                table: "partages",
                column: "VendeurId");

            migrationBuilder.CreateIndex(
                name: "IX_participations_partage_PartageId_PointAccessId",
                table: "participations_partage",
                columns: new[] { "PartageId", "PointAccessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_participations_partage_PointAccessId",
                table: "participations_partage",
                column: "PointAccessId");

            migrationBuilder.CreateIndex(
                name: "IX_point_accesses_UserId",
                table: "point_accesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_profils_energie_PointAccessId",
                table: "profils_energie",
                column: "PointAccessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tarifs_accord_PartageId",
                table: "tarifs_accord",
                column: "PartageId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_OrganismePublicId",
                table: "users",
                column: "OrganismePublicId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "data_partage");

            migrationBuilder.DropTable(
                name: "demandes_grd");

            migrationBuilder.DropTable(
                name: "documents_partage");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "participations_partage");

            migrationBuilder.DropTable(
                name: "profils_energie");

            migrationBuilder.DropTable(
                name: "tarifs_accord");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "partages");

            migrationBuilder.DropTable(
                name: "point_accesses");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "organismes_publics");
        }
    }
}
