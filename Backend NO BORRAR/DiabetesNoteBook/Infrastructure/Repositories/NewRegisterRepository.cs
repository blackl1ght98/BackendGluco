using DiabetesNoteBook.Domain.Models;
using DiabetesNoteBook.Infrastructure.Interfaces;

namespace DiabetesNoteBook.Infrastructure.Repositories
{
    //Hemos creado una interfaz para que el componente sea reutilizable por eso esta clase se ha
    //vinculado a una interfaz
    public class NewRegisterRepository : INewRegisterRepository
    {
        //Llamamos a la base de datos para poder usarla

        private readonly DiabetesNoteBookContext _context;
        //Creamos el contructor

        public NewRegisterRepository(DiabetesNoteBookContext context)
        {
            _context = context;
        }
        //Agregamos el metodo que esta en la interfaz junto a  la tabla usuarios

        public async Task SaveNewRegisterUser(Usuario newUsuario)
        {
            //Agregamos los datos directamente a la tabla usuarios

            await _context.Usuarios.AddAsync(newUsuario);
            //Guardamos los cambios

            await _context.SaveChangesAsync();
        }
        //Agregamos el metodo que esta en la interfaz junto a  la tabla Personas

      
    }
}
