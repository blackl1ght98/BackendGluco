using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using DiabetesNoteBook.Application.DTOs;
using DiabetesNoteBook.Application.Interfaces;
using DiabetesNoteBook.Application.Services;
using DiabetesNoteBook.Domain.Models;
using DiabetesNoteBook.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiabetesNoteBook.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //En este controlador se llaman a los servicios necesarios para poder operar

    public class MedicionesController : ControllerBase
    {
        private readonly DiabetesNoteBookContext _context;
        private readonly IOperationsService _operationsService;
        private readonly INuevaMedicionService _medicion;
        private readonly IDeleteMedicionService _deleteMedicion;

        //Se realiza el constructor

        public MedicionesController(DiabetesNoteBookContext context, IOperationsService operationsService, INuevaMedicionService nuevaMedicion, IDeleteMedicionService deleteMedicion)
        {
            _context = context;
            _operationsService = operationsService;
            _medicion = nuevaMedicion;
            _deleteMedicion = deleteMedicion;

        }
        //En este endpoint se realiza el agregado de las mediciones para agregar necesitan los datos que
        ////hay en DTOMediciones.
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> PostMediciones1(DTOMediciones mediciones)
        {
            //Buscamos si la persona existe en base de datos ya que las mediciones estan asociadas a una persona

            var existeUsuario = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == mediciones.Id_Usuario);
            //Si la persona no existe devolvemos el mensaje contenido en NotFound.

            if (existeUsuario == null)
            {
                return NotFound("La persona a la que intenta poner la medicion no existe");
            }
            //Llamamos al servicio medicion que contiene el metodo NuevaMedicion este metodo necesita un 
            //DTOMediciones que contiene los datos necesarios para agregar la medicion a esa persona
            await _medicion.NuevaMedicion(new DTOMediciones
            {
                Fecha = mediciones.Fecha,
                Regimen = mediciones.Regimen,
                PreMedicion = mediciones.PreMedicion,
                GlucemiaCapilar = mediciones.GlucemiaCapilar,
                BolusComida = mediciones.BolusComida,
                BolusCorrector = mediciones.BolusCorrector,
                PreDeporte = mediciones.PreDeporte,
                DuranteDeporte = mediciones.DuranteDeporte,
                PostDeporte = mediciones.PostDeporte,
                RacionHC = mediciones.RacionHC,
                Notas = mediciones.Notas,
                Id_Usuario = mediciones.Id_Usuario


            });
            //Se agrega la operacion llamando al servicio  _operationsService el cual tiene un metodo
            //_operationsService dicho metodo se alimenta de un DTOOperation que contiene los datos necesarios para 
            //agregar la operacion
            await _operationsService.AddOperacion(new DTOOperation
            {
                Operacion = "Persona agregada",
                UserId = existeUsuario.Id
            });
            //Si todo va bien devuel un ok.

            return Ok("Medicion guardada con exito ");
        }
        ////Este endpoint permite al usuario eliminar una medicion el cual se alimenta de un DTOEliminarMedicion
        ////que contiene los datos necesarios para poder eliminar esa medicion
        [AllowAnonymous]
        [HttpDelete("eliminarmedicion")]
        public async Task<ActionResult> DeleteMedicion(DTOEliminarMedicion Id)
        {
            //Buscamos la medicion por id en base de datos
            try
            {
                var medicionExist = await _context.Mediciones.FirstOrDefaultAsync(x => x.Id == Id.Id);
                //Si la medicion no existe devolvemos el mensaje contenido en BadRequest

                if (medicionExist == null)
                {
                    return BadRequest("La medicion que intenta eliminar no se encuentra");
                }
                //Llamamos al servicio _deleteMedicion que tiene un metodo DeleteMedicion el cual
                //necesita un DTOEliminarMedicion que  contiene los datos necesarios para eliminar la medicion
                await _deleteMedicion.DeleteMedicion(new DTOEliminarMedicion
                {
                    Id = Id.Id
                });
                //Agregamos la operacion  llamando  al servicio _operationsService el cual tiene un
                //metodo AddOperacion este metodo necesita un DTOOperation el cual tiene los datos necesarios 
                //para agregar la operacion
               
                await _operationsService.AddOperacion(new DTOOperation
                {
                    Operacion = "Eliminar medicion",
                    UserId = medicionExist.IdUsuario
                });
              
                //Devolvemos un ok si todo va bien

                return Ok("Eliminacion realizada con exito");
            }
            catch 
            {
                return BadRequest("En estos momentos no se ha podido consultar los datos de la persona, por favor, intentelo más tarde.");

            }
            
        }
        ////En este endpoint obtenemos las mediciones en base a la id del usuario para este endpoint necesita un
        ////DTOById que contiene los datos necesarios para hacer el get
        [AllowAnonymous]
        [HttpGet("getmedicionesporidusuario/{Id}")]
        public async Task<ActionResult<IEnumerable<Medicione>>> GetMedicionesPorIdUsuario([FromRoute] DTOById userData)
        {

            try
            {
                //Buscamos en base de datos la id del usuario el cual tiene asociadas mediciones

                var mediciones = await _context.Mediciones.Where(m => m.IdUsuarioNavigation.Id == userData.Id).ToListAsync();
                //Si la id del usuario que se le pasa no existe no encuentra las mediciones asociadas

                if (mediciones == null)
                {
                    return NotFound("Datos de medicion no encontrados");
                }
                //Agregamos la operacion usando el servicio _operationsService que tiene un metodo
                //AddOperacion dicho metodo necesita un DTOOperation que contiene los datos necesarios
                //para realizar la operacion.
                await _operationsService.AddOperacion(new DTOOperation
                {
                    Operacion = "Consulta medicion por id de usuario",
                    UserId = userData.Id
                });

                //Si todo va bien se devuelve un ok

                return Ok(mediciones);
            }
            catch
            {
                return BadRequest("En estos momentos no se ha podido consultar los datos de la persona, por favor, intentelo más tarde.");
            }

        }

        [Authorize]
        [HttpGet("descargarMedicionesPDF")]

        public async Task<IActionResult> DescargarMedicionesPDF()
        {
            // Obtener el ID del usuario actualmente autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           
            // Buscar las mediciones asociadas al usuario actual
            var mediciones = await _context.Mediciones
                .Where(m => m.IdUsuarioNavigation.Id.ToString() == userId)
                .ToListAsync();
            if (mediciones == null || mediciones.Count == 0)
            {
                return BadRequest("Datos de medicion no encontrados");
            }

            // Crear un documento PDF con orientación horizontal
            Document document = new Document();
            //Margenes y tamaño del documento
            document.PageInfo.Width = Aspose.Pdf.PageSize.PageLetter.Width;
            document.PageInfo.Height = Aspose.Pdf.PageSize.PageLetter.Height;
            document.PageInfo.Margin = new MarginInfo(1, 10, 10, 10); // Ajustar márgenes


            // Agregar una nueva página al documento con orientación horizontal
            Page page = document.Pages.Add();
            //Control de margenes

            page.PageInfo.Margin.Left = 35;
            page.PageInfo.Margin.Right = 0;
            //Controla la horientacion actualmente es horizontal
            page.SetPageSize(Aspose.Pdf.PageSize.PageLetter.Width, Aspose.Pdf.PageSize.PageLetter.Height);

            // Crear una tabla para mostrar las mediciones
            Aspose.Pdf.Table table = new Aspose.Pdf.Table();

            table.VerticalAlignment = VerticalAlignment.Center;
            table.Alignment = HorizontalAlignment.Left;



            table.DefaultCellBorder = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 0.1F);
            table.Border = new Aspose.Pdf.BorderInfo(Aspose.Pdf.BorderSide.All, 1F);
            table.ColumnWidths = "55 50 45 45 45 35 45 45 45 45 35 50"; // Ancho de cada columna

            page.Paragraphs.Add(table);

            // Agregar fila de encabezado a la tabla
            Aspose.Pdf.Row headerRow = table.Rows.Add();

            headerRow.Cells.Add("Fecha").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Regimen").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Pre Medicion").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Post Medicion").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Glucemia Capilar").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Bolus Comida").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Bolus Corrector").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Pre Deporte").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Durante Deporte").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Post Deporte").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Racion HC").Alignment = HorizontalAlignment.Center; 
            headerRow.Cells.Add("Notas").Alignment = HorizontalAlignment.Center; 

            // Agregar contenido de mediciones a la tabla
            foreach (var medicion in mediciones)
            {
                
                Aspose.Pdf.Row dataRow = table.Rows.Add();
                dataRow.Cells.Add($"{medicion.Fecha}").Alignment = HorizontalAlignment.Center; 
                dataRow.Cells.Add($"{medicion.Regimen}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.PreMedicion}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.PostMedicion}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.GlucemiaCapilar}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.BolusComida}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.BolusCorrector}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.PreDeporte}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.DuranteDeporte}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.PostDeporte}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.RacionHc}").Alignment = HorizontalAlignment.Center;
                dataRow.Cells.Add($"{medicion.Notas}").Alignment = HorizontalAlignment.Center;
            }

            // Crear un flujo de memoria para guardar el documento PDF
            MemoryStream memoryStream = new MemoryStream();

            // Guardar el documento en el flujo de memoria
            document.Save(memoryStream);

            // Convertir el documento a un arreglo de bytes
            byte[] bytes = memoryStream.ToArray();

            // Liberar los recursos de la memoria
            memoryStream.Close();
            memoryStream.Dispose();

            // Devolver el archivo PDF para descargar
            return File(bytes, "application/pdf", "mediciones.pdf");
        }





    }
}
