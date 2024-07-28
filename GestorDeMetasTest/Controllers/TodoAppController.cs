using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Security.AccessControl;

namespace GestorDeMetasTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoAppController : ControllerBase
    {
        private IConfiguration _configuration;
        public TodoAppController(IConfiguration configurartion)
        {
            _configuration = configurartion;
        }

        //Obtención de datos

        [HttpGet]
        [Route("GetMeta")]
        public JsonResult GetMeta()
        {
            string query = "SELECT m.IdMeta, m.Nombre, m.Fecha, m.Estatus, ROUND(COALESCE((SUM(CASE WHEN t.Estatus = 1 THEN 1 ELSE 0 END) * 100.0) / CASE WHEN COUNT(t.IdTarea) = 0 THEN 1 ELSE COUNT(t.IdTarea) END, 0), 2) AS Porcentaje FROM dbo.Meta m LEFT JOIN  dbo.Tarea t ON m.IdMeta = t.IdMeta  GROUP BY   m.IdMeta, m.Nombre, m.Fecha, m.Estatus";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult(table);

        }

        [HttpGet]
        [Route("GetTarea")]
        public IActionResult GetTarea(int IdMeta)
        {
            string query = "SELECT * FROM dbo.Tarea WHERE IdMeta= " + IdMeta;
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            SqlDataReader myReader;
            List<object> tareas = new List<object>();

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }

            // Convertir DataTable a una lista de objetos anónimos
            foreach (DataRow row in table.Rows)
            {
                var tarea = new
                {
                    IdTarea = Convert.ToInt32(row["IdTarea"]),
                    IdMeta = Convert.ToInt32(row["IdMeta"]),
                    NombreTarea = row["NombreTarea"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    Estatus = Convert.ToInt32(row["Estatus"]),
                    Prioridad = Convert.ToInt32(row["Prioridad"])
                };
                tareas.Add(tarea);
            }

            return new JsonResult(new { data = tareas });
        }

        //Insersión de datos

        [HttpPost]
        [Route("AddMeta")]
        public JsonResult AddMeta(string newMeta)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string queryCheck = "SELECT COUNT(1) FROM dbo.Meta WHERE Nombre = @newMeta";
            string queryInsert = "INSERT INTO dbo.Meta (Nombre, Fecha, Estatus) VALUES (@newMeta, @FechaInsercion, @Estatus)";
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Verificar si el nombre ya existe
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@newMeta", newMeta);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        myCon.Close();
                        return new JsonResult(new { success = false, message = "La meta ya existe" });
                    }
                }

                // Insertar nuevo registro
                using (SqlCommand myCommand = new SqlCommand(queryInsert, myCon))
                {
                    myCommand.Parameters.AddWithValue("@newMeta", newMeta);
                    myCommand.Parameters.AddWithValue("@FechaInsercion", currentDate);
                    myCommand.Parameters.AddWithValue("@Estatus", 1);

                    int rowsAffected = myCommand.ExecuteNonQuery();
                    myCon.Close();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult(new { success = true, message = "Meta agregada correctamente" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = "Error al agregar la meta" });
                    }
                }
            }
        }

        [HttpPost]
        [Route("AddTarea")]
        public JsonResult AddTarea(string newTarea, string desc, int IdMeta)
        {
            string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string queryCheck = "SELECT COUNT(1) FROM dbo.Tarea WHERE NombreTarea = @newTarea AND IdMeta = @IdMeta";
            string queryInsert = "INSERT INTO dbo.Tarea (IdMeta, NombreTarea, Descripcion, Fecha, Estatus, Prioridad) VALUES (@IdMeta, @newTarea, @desc, @FechaInsercion, @Estatus, @Prioridad)";
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Verificar si el nombre ya existe
                using (SqlCommand checkCommand = new SqlCommand(queryCheck, myCon))
                {
                    checkCommand.Parameters.AddWithValue("@newTarea", newTarea);
                    checkCommand.Parameters.AddWithValue("@IdMeta", IdMeta);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        myCon.Close();
                        return new JsonResult(new { success = false, message = "La tarea ya existe en la meta, intente con una nueva tarea" });
                    }
                }

                // Insertar nuevo registro
                using (SqlCommand myCommand = new SqlCommand(queryInsert, myCon))
                {
                    myCommand.Parameters.AddWithValue("@IdMeta", IdMeta);
                    myCommand.Parameters.AddWithValue("@newTarea", newTarea);
                    myCommand.Parameters.AddWithValue("@desc", desc);
                    myCommand.Parameters.AddWithValue("@FechaInsercion", currentDate);
                    myCommand.Parameters.AddWithValue("@Estatus", 0);
                    myCommand.Parameters.AddWithValue("@Prioridad", 0);

                    int rowsAffected = myCommand.ExecuteNonQuery();
                    myCon.Close();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult(new { success = true, message = "Tarea agregada correctamente" });
                    }
                    else
                    {
                        return new JsonResult(new { success = false, message = "Error al agregar la tarea" });
                    }
                }
            }
        }

        //Actualización de datos

        [HttpPut]
        [Route("UpdateMeta")]
        public IActionResult UpdateMetas(int id, string newName)
        {
            string query = "UPDATE dbo.Meta SET Nombre = @newName WHERE IdMeta = @id";

            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@newName", newName);
                    myCommand.Parameters.AddWithValue("@id", id);

                    int rowsAffected = myCommand.ExecuteNonQuery();

                    myCon.Close();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Meta actualizada correctamente");
                    }
                    else
                    {
                        return new JsonResult("No se encontró la meta con el ID especificado");
                    }
                }
            }
        }

        [HttpPut]
        [Route("UpdateTarea")]
        public IActionResult UpdateTarea(int id, string newName)
        {
            string query = "UPDATE dbo.Meta SET Nombre = @newName WHERE IdMeta = @id";

            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@newName", newName);
                    myCommand.Parameters.AddWithValue("@id", id);

                    int rowsAffected = myCommand.ExecuteNonQuery();

                    myCon.Close();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Meta actualizada correctamente");
                    }
                    else
                    {
                        return new JsonResult("No se encontró la meta con el ID especificado");
                    }
                }
            }
        }

        [HttpPut]
        [Route("CambiarPrioridad")]
        public IActionResult CambiarPrioridad(int id)
        {
            string querySelect = "SELECT Prioridad FROM dbo.Tarea WHERE IdTarea = @id";
            string queryUpdate = "UPDATE dbo.Tarea SET Prioridad = @prioridad WHERE IdTarea = @id";

            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");

            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Obtener el valor actual de Prioridad
                int currentPriority;
                using (SqlCommand myCommandSelect = new SqlCommand(querySelect, myCon))
                {
                    myCommandSelect.Parameters.AddWithValue("@id", id);
                    var result = myCommandSelect.ExecuteScalar();

                    if (result == null)
                    {
                        myCon.Close();
                        return new JsonResult("No se encontró la tarea con el ID especificado");
                    }

                    currentPriority = Convert.ToInt32(result);
                }

                // Establecer el nuevo valor de Prioridad
                int newPriority = currentPriority == 1 ? 0 : 1;

                // Actualizar el valor de Prioridad
                using (SqlCommand myCommandUpdate = new SqlCommand(queryUpdate, myCon))
                {
                    myCommandUpdate.Parameters.AddWithValue("@id", id);
                    myCommandUpdate.Parameters.AddWithValue("@prioridad", newPriority);

                    int rowsAffected = myCommandUpdate.ExecuteNonQuery();

                    myCon.Close();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Prioridad actualizada correctamente");
                    }
                    else
                    {
                        return new JsonResult("Error al actualizar la prioridad");
                    }
                }
            }
        }

        [HttpPut]
        [Route("CompletarTarea")]
        public IActionResult CompletarTarea(int id)
        {
            string query = "UPDATE dbo.Tarea SET Estatus = 1 WHERE IdTarea = @id";

            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@id", id);

                    int rowsAffected = myCommand.ExecuteNonQuery();

                    myCon.Close();
                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Tarea actualizada correctamente");
                    }
                    else
                    {
                        return new JsonResult("No se encontró la tarea con el ID especificado");
                    }
                }
            }
        }

        [HttpPut]
        [Route("editarTarea")]
        public IActionResult editarTarea(int id, int idMeta, string newName)
        {
            string queryCheck = "SELECT COUNT(*) FROM dbo.Tarea WHERE NombreTarea = @newName AND IdTarea = @id";
            string queryUpdate = "UPDATE dbo.Tarea SET NombreTarea = @newName WHERE IdTarea = @id AND IdMeta = @idMeta";

            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();

                // Verificar si ya existe una tarea con el nuevo nombre
                using (SqlCommand myCommandCheck = new SqlCommand(queryCheck, myCon))
                {
                    myCommandCheck.Parameters.AddWithValue("@id", id);
                    myCommandCheck.Parameters.AddWithValue("@idMeta", idMeta);
                    myCommandCheck.Parameters.AddWithValue("@newName", newName);

                    int count = (int)myCommandCheck.ExecuteScalar();
                    if (count > 0)
                    {
                        myCon.Close();
                        return new JsonResult("Ya existe una tarea con ese nombre para esta meta") { StatusCode = 400 };
                    }
                }

                // Actualizar el valor de Nombre
                using (SqlCommand myCommandUpdate = new SqlCommand(queryUpdate, myCon))
                {
                    myCommandUpdate.Parameters.AddWithValue("@id", id);
                    myCommandUpdate.Parameters.AddWithValue("@idMeta", idMeta);
                    myCommandUpdate.Parameters.AddWithValue("@newName", newName);

                    int rowsAffected = myCommandUpdate.ExecuteNonQuery();
                    myCon.Close();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Tarea actualizada correctamente");
                    }
                    else
                    {
                        return new JsonResult("No se encontró la tarea con el ID especificado");
                    }
                }
            }
        }

        //Eliminación de datos

        [HttpDelete]
        [Route("DeleteMeta")]
        public JsonResult DeleteMetas(int id)
        {
            string query = "DELETE FROM dbo.Meta WHERE IdMeta=@id";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@id", id);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Meta eliminada correctamente");

        }

        [HttpDelete]
        [Route("DeleteTarea")]
        public JsonResult DeleteTarea(int id)
        {
            string query = "DELETE FROM dbo.Tarea WHERE IdTarea=@id";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("todoAppDBCon");
            SqlDataReader myReader;
            using (SqlConnection myCon = new SqlConnection(sqlDataSource))
            {
                myCon.Open();
                using (SqlCommand myCommand = new SqlCommand(query, myCon))
                {
                    myCommand.Parameters.AddWithValue("@id", id);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);
                    myReader.Close();
                    myCon.Close();
                }
            }
            return new JsonResult("Tarea eliminada correctamente");

        }
    }
}
