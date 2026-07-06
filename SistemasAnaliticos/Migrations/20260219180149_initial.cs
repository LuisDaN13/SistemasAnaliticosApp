using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemasAnaliticos.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlcanceUsuario",
                columns: table => new
                {
                    idAlcance = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    alcance = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlcanceUsuario", x => x.idAlcance);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    estado = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    primerNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    segundoNombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    primerApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    segundoApellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    noEmpleado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    fechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    genero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    estadoCivil = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    tipoSangre = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    hijos = table.Column<bool>(type: "bit", nullable: false),
                    cantidadHijos = table.Column<int>(type: "int", nullable: true),
                    provincia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    canton = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    distrito = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    direccionExacta = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    profesion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    puesto = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    fechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    correoEmpresa = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    celularOficina = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    jefeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    jefeNombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    salario = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    cuentaIBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    celularPersonal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    correoPersonal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    telefonoHabitacion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    licencias = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    tipoPariente = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    contactoEmergencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    telefonoEmergencia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    padecimientosAlergias = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false),
                    sessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    lastActivityUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    diasVacaciones = table.Column<int>(type: "int", nullable: true),
                    ultimaFechaCalculoVacaciones = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetUsers_jefeId",
                        column: x => x.jefeId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Auditoria",
                columns: table => new
                {
                    idAudit = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Hora = table.Column<TimeSpan>(type: "time", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Tabla = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoria", x => x.idAudit);
                });

            migrationBuilder.CreateTable(
                name: "Beneficio",
                columns: table => new
                {
                    idBeneficio = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beneficio", x => x.idBeneficio);
                });

            migrationBuilder.CreateTable(
                name: "Constancia",
                columns: table => new
                {
                    idConstancia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    dirijido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    fechaRequerida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    datosAdjuntos = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Constancia", x => x.idConstancia);
                });

            migrationBuilder.CreateTable(
                name: "Extras",
                columns: table => new
                {
                    idExtra = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipoExtra = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    jefe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    totalHoras = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Extras", x => x.idExtra);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    idFoto = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.idFoto);
                });

            migrationBuilder.CreateTable(
                name: "Garantia",
                columns: table => new
                {
                    idGarantia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    moneda = table.Column<string>(type: "nvarchar(1)", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    aFavorDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    nombreLicitacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prorroga = table.Column<bool>(type: "bit", nullable: false),
                    numeroGarantia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    numeroLicitacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoLicitacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    plazo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    datosAdjuntos1 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo1 = table.Column<long>(type: "bigint", nullable: true),
                    datosAdjuntos2 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo2 = table.Column<long>(type: "bigint", nullable: true),
                    datosAdjuntos3 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo3 = table.Column<long>(type: "bigint", nullable: true),
                    datosAdjuntos4 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo4 = table.Column<long>(type: "bigint", nullable: true),
                    datosAdjuntos5 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo5 = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garantia", x => x.idGarantia);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionViatico",
                columns: table => new
                {
                    idViatico = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    jefe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionViatico", x => x.idViatico);
                });

            migrationBuilder.CreateTable(
                name: "Noticias",
                columns: table => new
                {
                    idNoticia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaPublicacion = table.Column<DateOnly>(type: "date", nullable: false),
                    horaPublicacion = table.Column<TimeSpan>(type: "time", nullable: false),
                    autor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    contenidoTexto = table.Column<string>(type: "varchar(max)", nullable: true),
                    foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticias", x => x.idNoticia);
                });

            migrationBuilder.CreateTable(
                name: "Permiso",
                columns: table => new
                {
                    idPermiso = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaCreacion = table.Column<DateOnly>(type: "date", nullable: false),
                    nombreEmpleado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    departamento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    fechaRegresoLaboral = table.Column<DateTime>(type: "datetime2", nullable: true),
                    horaCita = table.Column<TimeSpan>(type: "time", nullable: true),
                    motivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    comentarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    foto = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    datosAdjuntos = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    nombreArchivo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tipoMIME = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tamanoArchivo = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permiso", x => x.idPermiso);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
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
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioSesion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LoginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioSesion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioSesion_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtrasDetalle",
                columns: table => new
                {
                    idExtrasDetalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idExtra = table.Column<long>(type: "bigint", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    horaInicio = table.Column<TimeSpan>(type: "time", nullable: true),
                    horaFin = table.Column<TimeSpan>(type: "time", nullable: true),
                    detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    atm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noCaso = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    noBoleta = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lugar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sucursal = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtrasDetalle", x => x.idExtrasDetalle);
                    table.ForeignKey(
                        name: "FK_ExtrasDetalle_Extras_idExtra",
                        column: x => x.idExtra,
                        principalTable: "Extras",
                        principalColumn: "idExtra",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidacionViaticoDetalle",
                columns: table => new
                {
                    idViaticoDetalle = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idViatico = table.Column<long>(type: "bigint", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    detalle = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidacionViaticoDetalle", x => x.idViaticoDetalle);
                    table.ForeignKey(
                        name: "FK_LiquidacionViaticoDetalle_LiquidacionViatico_idViatico",
                        column: x => x.idViatico,
                        principalTable: "LiquidacionViatico",
                        principalColumn: "idViatico",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlcanceUsuario_rolId",
                table: "AlcanceUsuario",
                column: "rolId",
                unique: true);

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
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_cedula",
                table: "AspNetUsers",
                column: "cedula",
                unique: true,
                filter: "[cedula] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_correoEmpresa",
                table: "AspNetUsers",
                column: "correoEmpresa",
                unique: true,
                filter: "[correoEmpresa] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_departamento",
                table: "AspNetUsers",
                column: "departamento");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_estado",
                table: "AspNetUsers",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_fechaIngreso",
                table: "AspNetUsers",
                column: "fechaIngreso");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_jefeId",
                table: "AspNetUsers",
                column: "jefeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_noEmpleado",
                table: "AspNetUsers",
                column: "noEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_primerNombre",
                table: "AspNetUsers",
                column: "primerNombre");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_puesto",
                table: "AspNetUsers",
                column: "puesto");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Fecha",
                table: "Auditoria",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Tabla",
                table: "Auditoria",
                column: "Tabla");

            migrationBuilder.CreateIndex(
                name: "IX_Auditoria_Usuario",
                table: "Auditoria",
                column: "Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_ExtrasDetalle_idExtra",
                table: "ExtrasDetalle",
                column: "idExtra");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidacionViaticoDetalle_idViatico",
                table: "LiquidacionViaticoDetalle",
                column: "idViatico");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_RolId_Clave",
                table: "RolPermisos",
                columns: new[] { "RolId", "Clave" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioSesion_UserId",
                table: "UsuarioSesion",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlcanceUsuario");

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
                name: "Auditoria");

            migrationBuilder.DropTable(
                name: "Beneficio");

            migrationBuilder.DropTable(
                name: "Constancia");

            migrationBuilder.DropTable(
                name: "ExtrasDetalle");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "Garantia");

            migrationBuilder.DropTable(
                name: "LiquidacionViaticoDetalle");

            migrationBuilder.DropTable(
                name: "Noticias");

            migrationBuilder.DropTable(
                name: "Permiso");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "UsuarioSesion");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Extras");

            migrationBuilder.DropTable(
                name: "LiquidacionViatico");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
