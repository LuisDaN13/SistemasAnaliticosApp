using Microsoft.EntityFrameworkCore;
using SistemasAnaliticos.Entidades;
using SistemasAnaliticos.Models;

namespace SistemasAnaliticos.Services
{
    public class PermisoService 
    {
        private readonly DBContext _context;


        public PermisoService(DBContext context)
        {
            _context = context;
        }


        // IMPLEMENTACION DE METODOS DE TRAER PERMISOS
        public async Task<List<Permiso>> TraerPermisos()
        {
            var citas = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .ToListAsync();

            return citas;
        }
        public async Task<List<Permiso>> TraerVacaciones()
        {
            var vacaciones = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.tipo == "Vacaciones" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Select(x => new Permiso
                {
                    idPermiso = x.idPermiso,
                    nombreEmpleado = x.nombreEmpleado,
                    fechaIngreso = x.fechaIngreso,
                    fechaInicio = x.fechaInicio,
                    fechaFinalizacion = x.fechaFinalizacion,
                    fechaRegresoLaboral = x.fechaRegresoLaboral,
                    comentarios = x.comentarios,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return vacaciones;
        }
        public async Task<List<Permiso>> TraerIncapacidades()
        {
            var incapacidades = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.tipo == "Incapacidad" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Select(x => new Permiso
                {
                    idPermiso = x.idPermiso,
                    nombreEmpleado = x.nombreEmpleado,
                    fechaIngreso = x.fechaIngreso,
                    fechaInicio = x.fechaInicio,
                    fechaFinalizacion = x.fechaFinalizacion,
                    fechaRegresoLaboral = x.fechaRegresoLaboral,
                    motivo = x.motivo,
                    comentarios = x.comentarios,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return incapacidades;
        }
        public async Task<List<Permiso>> TraerTeletrabajo()
        {
            var teletrabajo = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.tipo == "Teletrabajo" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Select(x => new Permiso
                {
                    idPermiso = x.idPermiso,
                    nombreEmpleado = x.nombreEmpleado,
                    fechaIngreso = x.fechaIngreso,
                    fechaInicio = x.fechaInicio,
                    fechaFinalizacion = x.fechaFinalizacion,
                    fechaRegresoLaboral = x.fechaRegresoLaboral,
                    motivo = x.motivo,
                    comentarios = x.comentarios,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return teletrabajo;
        }
        public async Task<List<Permiso>> TraerPerEspecial()
        {
            var teletrabajo = await _context.Permiso
                .AsNoTracking()
                .Where(p => p.tipo == "Especial" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaIngreso)
                .Select(x => new Permiso
                {
                    idPermiso = x.idPermiso,
                    nombreEmpleado = x.nombreEmpleado,
                    fechaIngreso = x.fechaIngreso,
                    fechaInicio = x.fechaInicio,
                    fechaFinalizacion = x.fechaFinalizacion,
                    fechaRegresoLaboral = x.fechaRegresoLaboral,
                    motivo = x.motivo,
                    comentarios = x.comentarios,
                    foto = x.foto,
                    estado = x.estado
                })
                .ToListAsync();

            return teletrabajo;
        }

        public async Task<List<Constancia>> TraerConstanciaLaboral()
        {
            var laboral = await _context.Constancia
                .AsNoTracking()
                .Where(p => p.tipo == "Laboral" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaPedido)
                .Select(x => new Constancia
                {
                    idConstancia = x.idConstancia,
                    nombrePersona = x.nombrePersona,
                    fechaPedido = x.fechaPedido,
                    dirijido = x.dirijido,
                    fechaRequerida = x.fechaRequerida,
                    Comentarios = x.Comentarios,
                    estado = x.estado
                })
                .ToListAsync();

            return laboral;
        }
        public async Task<List<Constancia>> TraerConstanciaSalarial()
        {
            var laboral = await _context.Constancia
                .AsNoTracking()
                .Where(p => p.tipo == "Salarial" && p.estado == "Creada")
                .OrderByDescending(x => x.fechaPedido)
                .Select(x => new Constancia
                {
                    idConstancia = x.idConstancia,
                    nombrePersona = x.nombrePersona,
                    fechaPedido = x.fechaPedido,
                    dirijido = x.dirijido,
                    fechaRequerida = x.fechaRequerida,
                    Comentarios = x.Comentarios,
                    estado = x.estado
                })
                .ToListAsync();

            return laboral;
        }

        public Task<List<Beneficio>> TraerBenePrestamos()
        {
            throw new NotImplementedException();
        }
        public Task<List<Beneficio>> TraerBeneAdelanto()
        {
            throw new NotImplementedException();
        }
    }
}
