namespace SistemasAnaliticos.Auxiliares
{
    public static class PermisosSistema
    {
        public static readonly List<string> Todos = new()
        {
            // Usuarios
            ClavesPermiso.Crear(Modulos.Usuarios, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Usuarios, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Usuarios, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Usuarios, Acciones.Editar),
            ClavesPermiso.Crear(Modulos.Usuarios, Acciones.Inactivar),

            // Rol
            ClavesPermiso.Crear(Modulos.Rol, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Rol, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Rol, Acciones.Inactivar),
            ClavesPermiso.Crear(Modulos.Rol, Acciones.Eliminar),

            // Permiso
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Editar),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.CambiarEstado),

            // Constancia
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Editar),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.CambiarEstado),

            // Beneficio
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Editar),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.CambiarEstado),

            // Liquidación
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Editar),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.CambiarEstado),

            // Fotos
            ClavesPermiso.Crear(Modulos.Fotos, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Fotos, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Fotos, Acciones.Eliminar),
            ClavesPermiso.Crear(Modulos.Fotos, Acciones.Inactivar),

            // Noticias
            ClavesPermiso.Crear(Modulos.Noticias, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Noticias, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Noticias, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Noticias, Acciones.Eliminar),
            ClavesPermiso.Crear(Modulos.Noticias, Acciones.Inactivar),
        };
    }
}
