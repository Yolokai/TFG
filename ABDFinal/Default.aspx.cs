using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Web;
using System.Text;
using System.Linq;

namespace ABDFinal
{
    public partial class WebForm1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            switch (Request["op"])
            {
                case "1":
                    EnviarFichero();
                    break;
            }
        }

        public Dictionary<string, string> idenToIndex = new Dictionary<string, string>();

        private void EnviarFichero()
        {
            string Respuesta = string.Empty;
            HttpFileCollection files = Request.Files;
            string NombreTempFichero = string.Empty;
            HttpPostedFile file = files[0];
            var respuesta = new { Estado = "ok" };
            if (files.Count > 0)
            {
                try
                {
                    // Guardar el archivo en el servidor
                    string folderpath = Server.MapPath("~/Uploads/");
                    string filepath = folderpath + Path.GetFileName(files[0].FileName);
                    if (!Directory.Exists(folderpath))
                    {
                        // Si no existe, créalo
                        Directory.CreateDirectory(folderpath);
                    }
                    files[0].SaveAs(filepath);

                    // Diccionario para almacenar los valores, ahora vacío
                    Dictionary<string, List<string>> datosTabla = new Dictionary<string, List<string>>();
                    Dictionary<string, string> entradas = new Dictionary<string, string>();
                    Dictionary<string, string> salidas = new Dictionary<string, string>();
                    Dictionary<string, List<string>> proceso = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> indicadores = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> usuarios = new Dictionary<string, List<string>>();
                    Dictionary<string, List<string>> procedimiento = new Dictionary<string, List<string>>();

                    LeerTablaExcel(filepath, datosTabla);

                    idenToIndex.Clear();

                    GrafoData grafo = BpmnBuilder.CrearGrafoDesdeExcel(datosTabla);
                    entradas = LeerEntradas(filepath, entradas);
                    salidas = LeerSalidas(filepath, salidas);
                    proceso = LeerProceso(filepath, proceso);
                    indicadores = LeerIndicadores(filepath, indicadores);
                    usuarios = LeerUsuarios(filepath, usuarios);
                    procedimiento = Procedimiento(filepath, procedimiento);

                    // Convertir los datos a formato JSON
                    var jsonresult = new
                    {
                        datosExcell = datosTabla,
                        Grafo = grafo,
                        Entradas = entradas,
                        Salidas = salidas,
                        Proceso = proceso,
                        Usuarios = usuarios,
                        Procedimiento = procedimiento,
                        Estado = "ok"
                    };

                    string JsonRespuesta = string.Empty;

                    JsonRespuesta = JsonConvert.SerializeObject(jsonresult, Formatting.Indented);

                    Response.ContentType = "application/json";
                    // Response.AddHeader("Content-Disposition", "attachment;filename=data.json");
                    Response.Write(JsonRespuesta);
                    try { Response.End(); }
                    catch { }
                }
                catch (Exception ex)
                {
                    var jsonresult = new
                    {
                        error = ex.Message,
                        // Estado = "nok"
                    };
                }
            }
        }
        public enum NodeType
        {
            Inicio, //0
            Final, //1
            Normal, //2
            Decision, //3
            Aprobacion, //4
            AND, //5
            OR //6
        }

        public class GrafoData
        {
            public List<BpmnNode> Nodes { get; set; } = new List<BpmnNode>();
            public List<BpmnEdge> Edges { get; set; } = new List<BpmnEdge>();
        }

        public class BpmnNode
        {
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public List<string> predecessors { get; set; } = new List<string>(); // Predecesores para puertas AND
        }

        public class BpmnEdge
        {
            public string id { get; set; }
            public string source { get; set; }
            public string target { get; set; }
            public string name { get; set; }
            public string puerta { get; set; }
        }

        public static class BpmnBuilder
        {
            public static GrafoData CrearGrafoDesdeExcel(Dictionary<string, List<string>> datosTabla)
            {
                var grafo = new GrafoData();
                int totalFilas = datosTabla.First().Value.Count;

                var nodoInicio = new BpmnNode
                {
                    id = "inicio",
                    name = "Inicio",
                    type = "StartEvent"
                };
                grafo.Nodes.Add(nodoInicio);
                var nodoFin = new BpmnNode
                {
                    id = "fin",
                    name = "Fin",
                    type = "EndEvent"
                };
                grafo.Nodes.Add(nodoFin);

                for (int fila = 0; fila < totalFilas; fila++)
                {
                    var nodo = new BpmnNode();
                    foreach (var kvp in datosTabla)
                    {
                        string nombreColumna = kvp.Key;
                        string valorCelda = kvp.Value[fila];
                        // Crear una nueva instancia de BpmnNode en cada iteración

                        if (nombreColumna == "Num. Identificador")
                        {
                            nodo.id = valorCelda;
                        }
                        else if (nombreColumna == "Actividad / Tarea")
                        {
                            nodo.name = valorCelda;
                        }
                        else if (nombreColumna == "Tipo")
                        {
                            nodo.type = TipoActividadBpmn(valorCelda);
                        }
                        else if (nombreColumna == "Act. Predecesora" && valorCelda != "")
                        {
                            if (valorCelda.Contains(';'))
                            {
                                string[] predecesoras = valorCelda.Split(';');
                                foreach (string predecesora in predecesoras)
                                {
                                    nodo.predecessors.Add(predecesora);
                                }
                            }
                            else if (valorCelda.Contains('*'))
                            {
                                string[] predecesoras = valorCelda.Split('*');
                                string nodoAndId = "AND_" + nodo.id;

                                var nodoAnd = new BpmnNode
                                {
                                    id = nodoAndId,
                                    name = "AND"+ nodo.id,
                                    type = "ParallelGateway"
                                };

                                foreach (string predecesora in predecesoras)
                                {
                                    nodoAnd.predecessors.Add(predecesora);
                                    var enlaceToAnd = new BpmnEdge
                                    {
                                        id = "enlace_" + predecesora + "_" + nodoAndId,
                                        source = predecesora,
                                        target = nodoAndId,
                                    };

                                    grafo.Edges.Add(enlaceToAnd);
                                }

                                grafo.Nodes.Add(nodoAnd);
                                nodo.predecessors.Add(nodoAndId);
                            }
                            else if (valorCelda.Contains('+'))
                            {
                                string[] predecesoras = valorCelda.Split('+');
                                string nodoOrId = "OR_" + nodo.id;

                                var nodOr = new BpmnNode
                                {
                                    id = nodoOrId,
                                    name = "OR" + nodo.id,
                                    type = "ExclusiveGateway"
                                };

                                foreach (string predecesora in predecesoras)
                                {
                                    nodOr.predecessors.Add(predecesora);
                                    var enlaceToOr = new BpmnEdge
                                    {
                                        id = "enlace_" + predecesora + "_" + nodoOrId,
                                        source = predecesora,
                                        target = nodoOrId
                                    };

                                    grafo.Edges.Add(enlaceToOr);
                                }
                                grafo.Nodes.Add(nodOr);
                                nodo.predecessors.Add(nodoOrId);
                            } 
                            else if (valorCelda == "")
                            {

                            }
                            else
                            {
                                nodo.predecessors.Add(valorCelda);
                            }
                        }
                        else if(nombreColumna== "Act. Posterior")
                        {
                            if(valorCelda=="fin")
                            {
                                grafo.Edges.Add(new BpmnEdge { id = "fin"+nodo.id, source = nodo.id , target = "fin" });
                            }
                        }
                    }
                    grafo.Nodes.Add(nodo);

                    if (fila > 0)
                    {
                        for (int i = 0; i < nodo.predecessors.Count; i++)
                        {
                            // Creamos un enlace desde el predecesor al nodo actual
                            var enlace = new BpmnEdge
                            {
                                id = "enlace_" + nodo.predecessors[i].ToString()+grafo.Nodes.Last().id, // Generamos un ID único para el enlace
                                source = nodo.predecessors[i], // ID del nodo anterior
                                target = grafo.Nodes.Last().id, // ID del nodo actual
                            };
                            for (int j = 0; j < 2; j++)
                            {
                                if (i == 0)
                                {
                                    enlace.name = "Si";
                                }
                                else
                                {
                                    enlace.name = "No";
                                }
                            }
                            grafo.Edges.Add(enlace);
                        }
                    }
                }

                // Añadir enlace desde nodo de inicio al primer nodo real
                grafo.Edges.Add(new BpmnEdge { id = "enlace_inicio", source = nodoInicio.id, target = grafo.Nodes[2].id });

                return grafo;
            }

            private static string TipoActividadBpmn(string tipo)
            {
                switch (tipo)
                {
                    case "Inicio":
                        return "StartEvent";
                    case "Final":
                        return "EndEvent";
                    case "Decisión":
                        return "ExclusiveGateway";
                    case "Aprobación":
                        return "ExclusiveGateway"; // Puedes ajustar esto según los tipos de actividades de tu proceso
                    case "AND":
                        return "ParallelGateway";
                    case "OR":
                        return "ExclusiveGateway";
                    default:
                        return "Task"; // Por defecto, consideramos todas las actividades como tareas
                }
            }
        }

        private static void LeerTablaExcel(string filePath, Dictionary<string, List<string>> datos)
        {
            // Cargar el archivo Excel
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(0); // Queremos la primera hoja de excel

                // Obtener el número total de columnas y filas
                int totalFilas = worksheet.PhysicalNumberOfRows;
                int totalColumnas = worksheet.GetRow(2).PhysicalNumberOfCells;

                // Iterar sobre las columnas de la tercera fila (nombres de las columnas)
                for (int col = 1; col < totalColumnas; col++)
                {
                    string nombreColumna = worksheet.GetRow(2).GetCell(col).StringCellValue;
                    if (nombreColumna == "Num. Identificador" || nombreColumna == "Actividad / Tarea" || nombreColumna == "Tipo" || nombreColumna == "Responsable" || nombreColumna == "Act. Predecesora" || nombreColumna == "Act. Posterior" || nombreColumna == "Opción SI" || nombreColumna == "Opción NO")
                    {
                        datos[nombreColumna] = new List<string>();
                    }

                }

                // Iterar sobre las filas (empezando desde la cuarta fila)
                for (int fila = 3; fila <= totalFilas; fila++)
                {
                    // Iterar sobre las columnas empezando desde la segunda columna
                    for (int col = 1; col <= totalColumnas; col++)
                    {
                        string nombreColumna = worksheet.GetRow(2)?.GetCell(col)?.StringCellValue;
                        string ValorCelda = worksheet.GetRow(fila)?.GetCell(col)?.ToString();

                        if (nombreColumna == "Num. Identificador" || nombreColumna == "Actividad / Tarea" || nombreColumna == "Tipo" || nombreColumna == "Responsable" || nombreColumna == "Act. Predecesora" || nombreColumna == "Act. Posterior" || nombreColumna == "Opción SI" || nombreColumna == "Opción NO")
                        {

                            if (ValorCelda != "" && ValorCelda != null)
                            {
                                // Agregar el valor al diccionario bajo el nombre de la columna correspondiente
                                datos[nombreColumna].Add(ValorCelda);
                            }
                            else if (nombreColumna == "Actividad / Tarea" && ValorCelda == "")
                            {
                                datos["Num. Identificador"].Remove(worksheet.GetRow(fila).GetCell(col - 1).ToString());
                                fila++;
                                col = 0;
                            }
                            else if (ValorCelda == null)
                            {
                                //No hace nada
                            }
                            else
                            {
                                datos[nombreColumna].Add(ValorCelda);
                            }
                        }
                    }
                }
            }
        }
        private static Dictionary<string, string> LeerEntradas(string filePath, Dictionary<string, string> entradas)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(3); // Queremos la cuarta hoja de excel
                int totalFilas = worksheet.PhysicalNumberOfRows;
                int col = 1;
                for (int fil = 4; fil <= totalFilas; fil++)
                {
                    string Fuente = worksheet.GetRow(fil).GetCell(col).StringCellValue;
                    string Entrada = worksheet.GetRow(fil).GetCell(col + 1).StringCellValue;
                    if (Fuente != "" && Entrada != "")
                    {
                        entradas.Add(Fuente, Entrada);
                    }
                }

            }
            return entradas;
        }
        private static Dictionary<string, string> LeerSalidas(string filePath, Dictionary<string, string> salidas)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(3); // Queremos la cuarta hoja de excel
                int totalFilas = worksheet.PhysicalNumberOfRows;
                int col = 4;
                for (int fil = 4; fil <= totalFilas; fil++)
                {
                    string Salidas = worksheet.GetRow(fil).GetCell(col).StringCellValue;
                    string Receptores = worksheet.GetRow(fil).GetCell(col + 1).StringCellValue;
                    if (Salidas != "" && Receptores != "")
                    {
                        salidas.Add(Salidas, Receptores);
                    }

                }
            }
            return salidas;
        }
        private static Dictionary<string, List<string>> LeerProceso(string filePath, Dictionary<string, List<string>> proceso)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(2);
                int totalFilas = worksheet.PhysicalNumberOfRows;
                int col = 1;
                List<string> detalles = new List<string>();
                string dato;
                string dato2;
                for (int fil = 2; fil < 5; fil++)
                {
                    dato = worksheet.GetRow(fil).GetCell(col).StringCellValue;
                    dato2 = worksheet.GetRow(fil).GetCell(2).StringCellValue;
                    detalles.Insert(0, dato2);
                    proceso.Add(dato, detalles);
                    detalles.Clear();
                }

                dato = worksheet.GetRow(6).GetCell(1).StringCellValue;
                dato2 = worksheet.GetRow(7).GetCell(1).StringCellValue;
                detalles.Insert(0, dato2);
                proceso.Add(dato, detalles);
                detalles.Clear();

                dato = worksheet.GetRow(10).GetCell(1).StringCellValue;
                int i = 0;
                for (int fil = 11; fil < totalFilas; fil++)
                {
                    dato2 = worksheet.GetRow(fil).GetCell(1).StringCellValue;
                    detalles.Insert(i, dato2);
                    i++;
                }
                proceso.Add(dato, detalles);
                return proceso;
            }
        }
        private static Dictionary<string, List<string>> LeerIndicadores(string filePath, Dictionary<string, List<string>> indicadores)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(4); // Queremos la cuarta hoja de excel
                int totalFilas = worksheet.PhysicalNumberOfRows;

                for (int col = 1; col < 6; col++)
                {
                    string nombreColumna = worksheet.GetRow(1).GetCell(col).StringCellValue;
                    List<string> detalles = new List<string>(); // Nueva instancia de List<string> para cada columna

                    for (int fil = 2; fil < totalFilas; fil++)
                    {
                        var cell = worksheet.GetRow(fil).GetCell(col);
                        string dato;

                        if (cell == null)
                        {
                            // No hacer nada si la celda está vacía
                        }
                        else if (cell.CellType == CellType.Numeric)
                        {
                            // Convierte el valor numérico a string
                            dato = cell.NumericCellValue.ToString();
                            detalles.Add(dato);
                        }
                        else if (cell.StringCellValue == "" || cell.StringCellValue == "Nota: Se puede definir un máximo de 4 indicadores por año, uno de cada tipo (U.medida), para cada proceso.")
                        {
                            // No hacer nada si el valor de la celda es vacío o igual al mensaje específico
                        }
                        else
                        {
                            // Si no es numérico, obtiene el valor como string directamente
                            dato = cell.StringCellValue;
                            detalles.Add(dato);
                        }
                    }
                    indicadores.Add(nombreColumna, detalles);
                }
            }
            return indicadores;
        }
        private static Dictionary<string, List<string>> LeerUsuarios(string filePath, Dictionary<string, List<string>> usuarios)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(5);
                int totalFilas = worksheet.PhysicalNumberOfRows;
                for (int col = 1; col < 4; col++)
                {
                    string nombreColumna = worksheet.GetRow(1).GetCell(col).StringCellValue;
                    List<string> detalles = new List<string>();
                    for (int fil = 2; fil <= totalFilas; fil++)
                    {
                        string cell = worksheet.GetRow(fil).GetCell(col).StringCellValue;
                        string dato;

                        if (cell == "")
                        {
                            // No hacer nada si la celda está vacía
                        }
                        else
                        {
                            // Si no es numérico, obtiene el valor como string directamente
                            dato = cell;
                            detalles.Add(dato);
                        }
                    }
                    usuarios.Add(nombreColumna, detalles);
                }
                return usuarios;
            }
        }
        private static Dictionary<string, List<string>> Procedimiento(string filePath, Dictionary<string, List<string>> datos)
        {
            // Cargar el archivo Excel
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fileStream);

                // Obtener la hoja de trabajo (worksheet)
                ISheet worksheet = workbook.GetSheetAt(0); // Queremos la primera hoja de excel

                // Obtener el número total de columnas y filas
                int totalFilas = worksheet.PhysicalNumberOfRows;
                int totalColumnas = worksheet.GetRow(2).PhysicalNumberOfCells;

                // Iterar sobre las columnas de la tercera fila (nombres de las columnas)
                for (int col = 1; col <= totalColumnas; col++)
                {
                    string nombreColumna = worksheet.GetRow(2).GetCell(col).StringCellValue;
                    datos[nombreColumna] = new List<string>();
                }

                // Iterar sobre las filas (empezando desde la cuarta fila)
                for (int fila = 3; fila <= totalFilas; fila++)
                {
                    // Iterar sobre las columnas empezando desde la segunda columna
                    for (int col = 1; col <= totalColumnas; col++)
                    {
                        string nombreColumna = worksheet.GetRow(2)?.GetCell(col)?.StringCellValue;
                        string ValorCelda = worksheet.GetRow(fila)?.GetCell(col)?.ToString();

                        if (ValorCelda != "" && ValorCelda != null)
                        {
                            // Agregar el valor al diccionario bajo el nombre de la columna correspondiente
                            datos[nombreColumna].Add(ValorCelda);
                        }
                        else if (nombreColumna == "Actividad / Tarea" && ValorCelda == "")
                        {
                            datos["Num. Identificador"].Remove(worksheet.GetRow(fila).GetCell(col - 1).ToString());
                            fila++;
                            col = 0;
                        }
                        else if (ValorCelda == null)
                        {
                            //No hace nada
                        }
                        else
                        {
                            datos[nombreColumna].Add(ValorCelda);
                        }
                    }
                }
                return datos;
            }
        }
    }
}
