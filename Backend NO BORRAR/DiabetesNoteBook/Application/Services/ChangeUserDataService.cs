using DiabetesNoteBook.Application.DTOs;
using DiabetesNoteBook.Application.Interfaces;
using DiabetesNoteBook.Domain.Models;
using DiabetesNoteBook.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DiabetesNoteBook.Application.Services
{
    //Hemos creado una interfaz para que el componente sea reutilizable por eso esta clase se ha
    //vinculado a una interfaz
    public class ChangeUserDataService : IChangeUserDataService
    {
        private readonly DiabetesNoteBookContext _context;
        private readonly IChangeUserData _changeUserData;
        //Creamos el constructor

        public ChangeUserDataService(DiabetesNoteBookContext context, IChangeUserData changeUserData)
        {
            _context = context;
            _changeUserData = changeUserData;
        }
        //Ponemos el metodo que se encuentra en la interfaz el cual tiene un DTOChangeUserData que contiene
        //datos para poder hacer que este metodo cumpla su funcion
        
        public async Task ChangeUserData(DTOChangeUserData changeUserData)
        {
            try
            {

                // Buscar el usuario en la base de datos por su ID
                var usuarioUpdate = await _context.Usuarios.Include(x=>x.UsuarioMedicacions).AsTracking().FirstOrDefaultAsync(x => x.Id == changeUserData.Id);
                if (usuarioUpdate != null)
                {
                    // Actualizar los datos del usuario con los proporcionados
                    usuarioUpdate.Avatar = changeUserData.Avatar;
                    usuarioUpdate.Nombre = changeUserData.Nombre;
                    usuarioUpdate.PrimerApellido = changeUserData.PrimerApellido;
                    usuarioUpdate.SegundoApellido = changeUserData.SegundoApellido;
                    usuarioUpdate.Sexo = changeUserData.Sexo;
                    usuarioUpdate.Edad = changeUserData.Edad;
                    usuarioUpdate.Peso = changeUserData.Peso;
                    usuarioUpdate.Altura = changeUserData.Altura;
                    usuarioUpdate.Actividad = changeUserData.Actividad;
                    usuarioUpdate.TipoDiabetes = changeUserData.TipoDiabetes;
                    usuarioUpdate.Insulina = changeUserData.Insulina;
                    //usuarioUpdate.Medicacion=String.Join(",", changeUserData.Medicacion);
                    // Guardar los cambios en la base de datos
                    await _changeUserData.SaveChangeUserData(usuarioUpdate);
                    //Eliminar medicacion usuario
                    _context.UsuarioMedicacions.RemoveRange(usuarioUpdate.UsuarioMedicacions);
                    // Guardar los cambios en la base de datos
                    _context.SaveChanges();
                  
                    
                    //toma los medicamentos asociados al usuario y los pone en una sola linea
                    var medicamentos = changeUserData.Medicacion.SelectMany(m => m.Split(','));
                    foreach (var medicacionNombre in medicamentos)
                    {
                        // Eliminar espacios en blanco alrededor del nombre de la medicación
                        var nombreMedicacion = medicacionNombre.Trim();
                        // Verificar si la medicación ya existe en la base de datos
                        var medicacionExistente = await _context.Medicaciones.FirstOrDefaultAsync(m => m.Nombre == nombreMedicacion);
                        if (medicacionExistente == null)
                        {
                            // Si la medicación no existe, se crea un nuevo registro en la tabla Medicaciones
                            var nuevaMedicacion = new Medicacione { Nombre = nombreMedicacion };
                            _context.Medicaciones.Add(nuevaMedicacion);
                        }
                    }
                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();
                    // Asociar las medicaciones con el usuario en la tabla UsuarioMedicaciones
                    foreach (var medicacionNombre in medicamentos)
                    {
                        // Eliminar espacios en blanco alrededor del nombre de la medicación
                        var nombreMedicacion = medicacionNombre.Trim();
                        // Obtener el objeto Medicacion correspondiente al nombre de la medicacion
                        var medicacion = await _context.Medicaciones.FirstOrDefaultAsync(m => m.Nombre == nombreMedicacion);

                        var usuarioMedicacion = new UsuarioMedicacion
                        {
                            // Asignar el ID del nuevo usuario
                            IdUsuario = usuarioUpdate.Id,
                            // Asignar el ID de la medicación
                            IdMedicacion = medicacion.IdMedicacion
                        };
                        // Agregar la relación a la tabla UsuarioMedicaciones
                        _context.UsuarioMedicacions.Add(usuarioMedicacion);
                    }
                    // Guardar los cambios en la base de datos
                    await _context.SaveChangesAsync();
                    // Eliminar las medicaciones que ya no están asociadas a ningún usuario
                    var medicacionesViejas = await _context.Medicaciones
                        .Where(m => !m.UsuarioMedicacions.Any())
                        .ToListAsync();

                    _context.Medicaciones.RemoveRange(medicacionesViejas);
                    await _context.SaveChangesAsync();

                }
            }
            catch
            {
               
            }
            /* public async Task ChangeUserData(DTOChangeUserData changeUserData)
        {
            try
            {
                var userIdString = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Convertir el valor de la Id a tipo numérico
                if (int.TryParse(userIdString, out int userId))
                {
                    // Resto del código para actualizar los datos del usuario usando userId...
                    var usuarioUpdate = await _context.Usuarios.Include(x => x.UsuarioMedicacions).AsTracking().FirstOrDefaultAsync(x => x.Id == userId);
                    if (usuarioUpdate != null)
                    {
                        // Actualizar los datos del usuario con los proporcionados

                        usuarioUpdate.Avatar = changeUserData.Avatar;
                        usuarioUpdate.Nombre = changeUserData.Nombre;
                        usuarioUpdate.PrimerApellido = changeUserData.PrimerApellido;
                        usuarioUpdate.SegundoApellido = changeUserData.SegundoApellido;
                        usuarioUpdate.Sexo = changeUserData.Sexo;
                        usuarioUpdate.Edad = changeUserData.Edad;
                        usuarioUpdate.Peso = changeUserData.Peso;
                        usuarioUpdate.Altura = changeUserData.Altura;
                        usuarioUpdate.Actividad = changeUserData.Actividad;
                        usuarioUpdate.TipoDiabetes = changeUserData.TipoDiabetes;
                        usuarioUpdate.Insulina = changeUserData.Insulina;
                        //usuarioUpdate.Medicacion=String.Join(",", changeUserData.Medicacion);
                        // Guardar los cambios en la base de datos
                        await _changeUserData.SaveChangeUserData(usuarioUpdate);
                        //Eliminar medicacion usuario
                        _context.UsuarioMedicacions.RemoveRange(usuarioUpdate.UsuarioMedicacions);
                        // Guardar los cambios en la base de datos
                        _context.SaveChanges();


                        //toma los medicamentos asociados al usuario y los pone en una sola linea
                        var medicamentos = changeUserData.Medicacion.SelectMany(m => m.Split(','));
                        foreach (var medicacionNombre in medicamentos)
                        {
                            // Eliminar espacios en blanco alrededor del nombre de la medicación
                            var nombreMedicacion = medicacionNombre.Trim();
                            // Verificar si la medicación ya existe en la base de datos
                            var medicacionExistente = await _context.Medicaciones.FirstOrDefaultAsync(m => m.Nombre == nombreMedicacion);
                            if (medicacionExistente == null)
                            {
                                // Si la medicación no existe, se crea un nuevo registro en la tabla Medicaciones
                                var nuevaMedicacion = new Medicacione { Nombre = nombreMedicacion };
                                _context.Medicaciones.Add(nuevaMedicacion);
                            }
                        }
                        // Guardar los cambios en la base de datos
                        await _context.SaveChangesAsync();
                        // Asociar las medicaciones con el usuario en la tabla UsuarioMedicaciones
                        foreach (var medicacionNombre in medicamentos)
                        {
                            // Eliminar espacios en blanco alrededor del nombre de la medicación
                            var nombreMedicacion = medicacionNombre.Trim();
                            // Obtener el objeto Medicacion correspondiente al nombre de la medicacion
                            var medicacion = await _context.Medicaciones.FirstOrDefaultAsync(m => m.Nombre == nombreMedicacion);

                            var usuarioMedicacion = new UsuarioMedicacion
                            {
                                // Asignar el ID del nuevo usuario
                                IdUsuario = usuarioUpdate.Id,
                                // Asignar el ID de la medicación
                                IdMedicacion = medicacion.IdMedicacion
                            };
                            // Agregar la relación a la tabla UsuarioMedicaciones
                            _context.UsuarioMedicacions.Add(usuarioMedicacion);
                        }
                        // Guardar los cambios en la base de datos
                        await _context.SaveChangesAsync();
                        // Eliminar las medicaciones que ya no están asociadas a ningún usuario
                        var medicacionesViejas = await _context.Medicaciones
                            .Where(m => !m.UsuarioMedicacions.Any())
                            .ToListAsync();

                        _context.Medicaciones.RemoveRange(medicacionesViejas);
                        await _context.SaveChangesAsync();
                    }
                    }
                else
                {
                    // Manejar el caso en el que la conversión falle
                    throw new InvalidOperationException("El valor de la Id no es un número válido.");
                }
                // Buscar el usuario en la base de datos por su ID
                

                
            }
            catch
            {
               
            }
        }
             */
        }


    }
}
