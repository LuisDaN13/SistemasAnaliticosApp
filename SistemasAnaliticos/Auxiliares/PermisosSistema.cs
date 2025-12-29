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
            ClavesPermiso.Crear(Modulos.Rol, Acciones.CambiarRol),

            // Permiso
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.CambiarEstado),
            ClavesPermiso.Crear(Modulos.Permiso, Acciones.Descargar),

            // Constancia
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.CambiarEstado),
            ClavesPermiso.Crear(Modulos.Constancia, Acciones.Descargar),

            // Beneficio
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Beneficio, Acciones.CambiarEstado),

            // Liquidación
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.LiquidacionViatico, Acciones.CambiarEstado),

            // Foto
            ClavesPermiso.Crear(Modulos.Foto, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Foto, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Foto, Acciones.Eliminar),
            ClavesPermiso.Crear(Modulos.Foto, Acciones.Inactivar),

            // Noticias
            ClavesPermiso.Crear(Modulos.Noticia, Acciones.Ver),
            ClavesPermiso.Crear(Modulos.Noticia, Acciones.Crear),
            ClavesPermiso.Crear(Modulos.Noticia, Acciones.Detalles),
            ClavesPermiso.Crear(Modulos.Noticia, Acciones.Eliminar),
            ClavesPermiso.Crear(Modulos.Noticia, Acciones.Inactivar),
        };
    }
}
