using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace Capa_Modelo_Navegador
{
    public class sentencias
    {
        conexion cn = new conexion();

        //******************************************** CODIGO HECHO POR BRAYAN HERNANDEZ ***************************** 
        // Método que llena una tabla con datos relacionados a otra tabla si es necesario.
        public OdbcDataAdapter LlenaTbl(string sTabla, List<Tuple<string, string, string, string>> relacionesForaneas)
        {
            OdbcConnection conn = cn.ProbarConexion();

            try
            {
                // Verifica que las relaciones no sean nulas
                if (relacionesForaneas == null)
                {
                    relacionesForaneas = new List<Tuple<string, string, string, string>>();
                }

                // Verificar que la conexión esté activa
                if (conn == null)
                {
                    throw new InvalidOperationException("La conexión a la base de datos no está disponible.");
                }

                // Obtener los campos de la tabla principal de forma dinámica
                string[] sCamposDesc = ObtenerCampos(sTabla);
                if (sCamposDesc == null || sCamposDesc.Length == 0)
                {
                    throw new InvalidOperationException("No se pudieron obtener los campos de la tabla principal.");
                }

                // Inicia con el primer campo de la tabla
                string sCamposSelect = sTabla + "." + sCamposDesc[0];

                // Diccionario para evitar duplicados de columnas
                Dictionary<string, int> dicColumnasRegistradas = new Dictionary<string, int>();
                dicColumnasRegistradas[sCamposDesc[0]] = 1;

                // Obtener las propiedades de las columnas de la tabla principal
                var vColumnasPropiedades = ObtenerColumnasYPropiedades(sTabla);
                if (vColumnasPropiedades == null)
                {
                    throw new InvalidOperationException("No se pudieron obtener las propiedades de las columnas de la tabla.");
                }

                // Recorrer los campos de la tabla principal
                foreach (var (sNombreColumna, bEsAutoIncremental, bEsClaveForanea, bEsTinyInt) in vColumnasPropiedades)
                {
                    // Evitar agregar la columna principal dos veces
                    if (sNombreColumna == sCamposDesc[0])
                        continue;

                    // Si es una clave foránea, buscar si hay una relación foránea que la reemplace
                    bool columnaReemplazada = false;

                    foreach (var relacion in relacionesForaneas)
                    {
                        if (string.IsNullOrEmpty(relacion.Item1) || string.IsNullOrEmpty(relacion.Item2) || string.IsNullOrEmpty(relacion.Item3) || string.IsNullOrEmpty(relacion.Item4))
                        {
                            throw new ArgumentException("Uno de los valores en las relaciones foráneas es nulo o vacío.");
                        }

                        string sTablaRelacionada = relacion.Item1;
                        string sCampoDescriptivo = relacion.Item2;
                        string sColumnaForanea = relacion.Item3;

                        // Si la columna actual es una clave foránea, la reemplazamos por su campo descriptivo
                        if (sNombreColumna == sColumnaForanea)
                        {
                            sCamposSelect += ", " + sTablaRelacionada + "." + sCampoDescriptivo + " AS " + sCampoDescriptivo;
                            dicColumnasRegistradas[sCampoDescriptivo] = 1;
                            columnaReemplazada = true;
                            break;
                        }
                    }

                    // Si no fue reemplazada como clave foránea, agregarla como está
                    if (!columnaReemplazada)
                    {
                        sCamposSelect += ", " + sTabla + "." + sNombreColumna;
                        dicColumnasRegistradas[sNombreColumna] = 1;
                    }
                }

                // Crear el comando SQL para seleccionar los campos
                string sSql = "SELECT " + sCamposSelect + " FROM " + sTabla;

                // Agregar los LEFT JOIN para cada relación foránea
                foreach (var relacion in relacionesForaneas)
                {
                    string sTablaRelacionada = relacion.Item1;
                    string sColumnaForanea = relacion.Item3;
                    string sColumnaPrimariaRelacionada = relacion.Item4;

                    // Añadir el LEFT JOIN con la tabla relacionada
                    sSql += " LEFT JOIN " + sTablaRelacionada + " ON " + sTabla + "." + sColumnaForanea + " = " + sTablaRelacionada + "." + sColumnaPrimariaRelacionada;
                }

                // Filtrar por estado (activo o inactivo)
                sSql += " WHERE " + sTabla + ".estado = 0 OR " + sTabla + ".estado = 1";

                // Ordenar por la columna principal en orden descendente
                sSql += " ORDER BY " + sCamposDesc[0] + " DESC;";

                Console.WriteLine(sSql); // Imprimir la consulta SQL generada para debugging

                // Crear un adaptador de datos para ejecutar la consulta
                OdbcDataAdapter dataTable = new OdbcDataAdapter(sSql, conn);

                return dataTable;
            }
            finally
            {
                // Cerrar la conexión después de ejecutar la consulta
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    Console.WriteLine("Conexión cerrada después de llenar la tabla");
                }
            }
        }

        public string ObtenerValorClave(string sTabla, string sCampoClave, string sCampoDescriptivo, string sValorDescriptivo)
        {
            string sConsulta = $"SELECT {sCampoClave} FROM {sTabla} WHERE {sCampoDescriptivo} = '{sValorDescriptivo}'";
            string sResultado = null;

            using (OdbcConnection oConexion = cn.ProbarConexion())
            {
                using (OdbcCommand oComando = new OdbcCommand(sConsulta, oConexion))
                {
                    sResultado = oComando.ExecuteScalar()?.ToString();
                }
            }

            Console.WriteLine(sConsulta);
            return sResultado;
        }
        //******************************************** CODIGO HECHO POR BRAYAN HERNANDEZ ***************************** 

        //******************************************** CODIGO HECHO POR EMANUEL BARAHONA ***************************** 
        // Método que obtiene el último ID de una tabla
        public string ObtenerId(string sTabla)
        {
            string[] sCamposDesc = ObtenerCampos(sTabla);
            string sSql = "SELECT MAX(" + sCamposDesc[0] + ") FROM " + sTabla + ";";
            string sId = "1";

            using (OdbcConnection oConexion = cn.ProbarConexion())
            {
                using (OdbcCommand oComando = new OdbcCommand(sSql, oConexion))
                {
                    using (OdbcDataReader oLector = oComando.ExecuteReader())
                    {
                        if (oLector.HasRows && oLector.Read())
                        {
                            string sValor = oLector.GetValue(0)?.ToString();
                            sId = string.IsNullOrEmpty(sValor) ? "1" : sValor;
                        }
                    }
                }
            }

            return sId;
        }

        // Método para obtener datos adicionales de una tabla (no se especifica para qué se usan)
        public string[] ObtenerExtra(string sTabla)
        {
            string[] sCampos = new string[30];
            int iIndex = 0;

            using (OdbcConnection oConexion = cn.ProbarConexion())
            {
                using (OdbcCommand oComando = new OdbcCommand("DESCRIBE " + sTabla, oConexion))
                {
                    using (OdbcDataReader oLector = oComando.ExecuteReader())
                    {
                        while (oLector.Read())
                        {
                            sCampos[iIndex] = oLector.GetValue(5)?.ToString();
                            iIndex++;
                        }
                    }
                }
            }

            return sCampos;
        }
        //******************************************** CODIGO HECHO POR EMANUEL BARAHONA ***************************** 

        //******************************************** CODIGO HECHO POR ANIKA ESCOTO ***************************** 
        // Método para obtener el ID de usuario basado en su nombre de usuario
        public string ObtenerIdUsuarioPorUsername(string sUsername)
        {
            string sSql = "SELECT Pk_id_usuario FROM tbl_usuarios WHERE username_usuario = ?";

            using (OdbcConnection oConexion = cn.ProbarConexion())
            {
                using (OdbcCommand oComando = new OdbcCommand(sSql, oConexion))
                {
                    oComando.Parameters.AddWithValue("@username", sUsername);

                    using (OdbcDataReader oLector = oComando.ExecuteReader())
                    {
                        return oLector.Read() ? oLector["Pk_id_usuario"].ToString() : "-1";
                    }
                }
            }
        }
        // Método que cuenta los campos en una tabla
        public int ContarAlias(string sTabla)
        {
            int iCampos = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("DESCRIBE " + sTabla, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                iCampos++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " \nError en obtenerTipo, revise los parámetros de la tabla  \n -" + sTabla.ToUpper() + "\n -");
                }
            }

            return iCampos;
        }

        public int ContarReg(string sIdIndice)
        {
            int iCampos = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT * FROM ayuda WHERE id_ayuda = " + sIdIndice + ";", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                iCampos++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " \nError en obtenerTipo, revise los parámetros de la tabla  \n -" + sIdIndice.ToUpper() + "\n -");
                }
            }

            return iCampos;
        }

        public string ModRuta(string sIdAyuda)
        {
            string sRuta = "";
            string sQuery = "SELECT Ruta FROM ayuda WHERE Id_ayuda = ?";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                {
                    command.Parameters.AddWithValue("id_ayuda", sIdAyuda);
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sRuta = reader.GetString(0);
                        }
                    }
                }
            }

            return sRuta;
        }

        public string RutaReporte(string sIdIndice)
        {
            string sIndice2 = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                using (OdbcCommand command = new OdbcCommand("SELECT ruta FROM tbl_aplicaciones WHERE Pk_id_aplicacion = " + sIdIndice + ";", connection))
                {
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sIndice2 = reader["ruta"].ToString();
                        }
                    }
                }
            }

            return sIndice2;
        }

        public string ModIndice(string sIdAyuda)
        {
            string sIndice = "";
            string sQuery = "SELECT indice FROM ayuda WHERE id_ayuda = ?";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                {
                    command.Parameters.AddWithValue("Id_ayuda", sIdAyuda);
                    using (OdbcDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            sIndice = reader.GetString(0);
                        }
                    }
                }
            }

            return sIndice;
        }

        public string ProbarTabla(string sTabla)
        {
            string sError = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT * FROM " + sTabla + ";", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                        }
                    }
                }
                catch (Exception)
                {
                    sError = "La tabla " + sTabla.ToUpper() + " no existe.";
                }
            }

            return sError;
        }

        public string ProbarEstado(string sTabla)
        {
            string sError = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT estado FROM " + sTabla + ";", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                        }
                    }
                }
                catch (Exception)
                {
                    sError = "La tabla " + sTabla.ToUpper() + " no contiene el campo de ESTADO";
                }
            }

            return sError;
        }

        public int ProbarRegistros(string sTabla)
        {
            int iRegistros = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT * FROM " + sTabla + " where estado=1;", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                iRegistros++;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return iRegistros;
        }

        public string[] ObtenerCampos(string sTabla)
        {
            string[] sCampos = new string[30];
            int iIndex = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("DESCRIBE " + sTabla, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sCampos[iIndex] = reader.GetValue(0).ToString();
                                iIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " \nError en asignarCombo, revise los parámetros \n -" + sTabla);
                }
            }

            return sCampos;
        }

        public List<(string nombreColumna, bool esAutoIncremental, bool esClaveForanea, bool esTinyInt)> ObtenerColumnasYPropiedades(string sNombreTabla)
        {
            List<(string, bool, bool, bool)> lColumnas = new List<(string, bool, bool, bool)>();

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    string sQueryColumnas = $"SHOW COLUMNS FROM {sNombreTabla};";
                    using (OdbcCommand comando = new OdbcCommand(sQueryColumnas, connection))
                    {
                        using (OdbcDataReader lector = comando.ExecuteReader())
                        {
                            HashSet<string> clavesForaneas = new HashSet<string>();

                            string sQueryClavesForaneas = $@"
                        SELECT COLUMN_NAME
                        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                        WHERE TABLE_NAME = '{sNombreTabla}' AND REFERENCED_TABLE_NAME IS NOT NULL;";
                            using (OdbcCommand comandoClaves = new OdbcCommand(sQueryClavesForaneas, connection))
                            {
                                using (OdbcDataReader lectorClaves = comandoClaves.ExecuteReader())
                                {
                                    while (lectorClaves.Read())
                                    {
                                        string sNombreColumnaForanea = lectorClaves.GetString(0);
                                        clavesForaneas.Add(sNombreColumnaForanea);
                                    }
                                }
                            }

                            while (lector.Read())
                            {
                                string sNombreColumna = lector.GetString(0);
                                string sTipoColumna = lector.GetString(1);
                                string sColumnaExtra = lector.GetString(5);

                                bool bEsAutoIncremental = sColumnaExtra.Contains("auto_increment");
                                bool bEsClaveForanea = clavesForaneas.Contains(sNombreColumna);
                                bool bEsTinyInt = sTipoColumna.StartsWith("tinyint");

                                lColumnas.Add((sNombreColumna, bEsAutoIncremental, bEsClaveForanea, bEsTinyInt));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener columnas: " + ex.Message);
                }
            }

            return lColumnas;
        }

        public void EjecutarQueryConTransaccion(List<string> sQueries)
        {
            using (OdbcConnection connection = cn.ProbarConexion())
            {
                OdbcTransaction transaction = null;

                try
                {
                    transaction = connection.BeginTransaction();

                    foreach (string sQuery in sQueries)
                    {
                        using (OdbcCommand command = new OdbcCommand(sQuery, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    Console.WriteLine("Error en la transacción: " + ex.Message);
                }
            }
        }
        public string[] ObtenerTipo(string sTabla)
        {
            string[] sCampos = new string[30];
            int iIndex = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("DESCRIBE " + sTabla, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sCampos[iIndex] = LimpiarTipo(reader.GetValue(1).ToString());
                                iIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " \nError en obtenerTipo, revise los parámetros de la tabla  \n -" + sTabla.ToUpper() + "\n -");
                }
            }

            return sCampos;
        }

        public string[] ObtenerLLave(string sTabla)
        {
            string[] sCampos = new string[30];
            int iIndex = 0;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("DESCRIBE " + sTabla, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sCampos[iIndex] = reader.GetValue(3).ToString();
                                iIndex++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString() + " \nError en obtenerTipo, revise los parámetros de la tabla  \n -" + sTabla + "\n -");
                }
            }

            return sCampos;
        }

        public Dictionary<string, string> ObtenerItems(string sTabla, string sCampoClave, string sCampoDisplay)
        {
            Dictionary<string, string> dicItems = new Dictionary<string, string>();

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand($"SELECT {sCampoClave}, {sCampoDisplay} FROM {sTabla} WHERE estado = 1", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dicItems.Add(reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " \nError en obtenerItems, revise los parámetros \n -" + sTabla + "\n -" + sCampoClave);
                }
            }

            return dicItems;
        }
        // Método que limpia el tipo de dato de una cadena, eliminando la dimensión del campo
        string LimpiarTipo(string sCadena)
        {
            bool bDim = false;
            string sNuevaCadena = "";

            for (int iJIndex = 0; iJIndex < sCadena.Length; iJIndex++)
            {
                if (sCadena[iJIndex] == '(')
                {
                    bDim = true;
                }
            }

            if (bDim == true)
            {
                int iIndex = 0;

                int iTam = sCadena.Length;

                while (sCadena[iIndex] != '(')
                {
                    sNuevaCadena += sCadena[iIndex];
                    iIndex++;
                }

            }
            else
            {
                return sCadena;
            }

            return sNuevaCadena;
        }
        public string LlaveCampo(string sTabla, string sCampo, string sValor)
        {
            string sLlave = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT * FROM " + sTabla + " where " + sCampo + " = '" + sValor + "' ;", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sLlave = reader.GetValue(0).ToString();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

            return sLlave;
        }

        public string LlaveCampoReverso(string sTabla, string sCampo, string sValor)
        {
            string sLlave = "";
            string[] sCampos = ObtenerCampos(sTabla);

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    string sValorFormateado = "'" + sValor + "'";
                    string sQuery = $"SELECT {sCampo} FROM {sTabla} WHERE {sCampos[0]} = {sValorFormateado};";

                    using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sLlave = reader.GetValue(0).ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Dio errore: " + ex.ToString());
                }
            }

            return sLlave;
        }

        public string IdModulo(string sAplicacion)
        {
            string sLlave = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand command = new OdbcCommand("SELECT * FROM tbl_aplicacion where PK_id_aplicacion= " + sAplicacion + " ;", connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sLlave = reader.GetValue(0).ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Dio errore " + "SELECT * FROM tbl_aplicacion where PK_id_aplicacion= " + sAplicacion + " ;" + ex.ToString());
                }
            }

            return sLlave;
        }

        public void EjecutarQuery(string sQuery)
        {
            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    using (OdbcCommand consulta = new OdbcCommand(sQuery, connection))
                    {
                        consulta.ExecuteNonQuery();
                    }
                }
                catch (OdbcException ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public string ObtenerClavePrimaria(string sNombreTabla)
        {
            string sClavePrimaria = "";

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    string sQuery = $"SHOW KEYS FROM {sNombreTabla} WHERE Key_name = 'PRIMARY';";

                    using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sClavePrimaria = reader["Column_name"].ToString();
                                Console.WriteLine($"Clave primaria de {sNombreTabla}: {sClavePrimaria}");
                            }
                            else
                            {
                                throw new Exception("No se encontró una clave primaria para la tabla: " + sNombreTabla);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener la clave primaria de la tabla " + sNombreTabla + ": " + ex.ToString());
                }
            }

            return sClavePrimaria;
        }

        public string ObtenerClaveForanea(string sTablaOrigen, string sTablaReferencia)
        {
            string sClaveForanea = null;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    string sQuery = $@"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = '{sTablaOrigen}' 
                AND REFERENCED_TABLE_NAME = '{sTablaReferencia}';";

                    using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sClaveForanea = reader.GetString(0);
                                Console.WriteLine($"Clave foránea de {sTablaOrigen} que referencia a {sTablaReferencia}: {sClaveForanea}");
                            }
                            else
                            {
                                Console.WriteLine($"No se encontró clave foránea en {sTablaOrigen} que referencia a {sTablaReferencia}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener clave foránea: " + ex.Message);
                }
            }

            return sClaveForanea;
        }

        public (string tablaRelacionada, string campoClave, string campoDisplay) ObtenerRelacionesForaneas(string sTablaOrigen, string sCampo)
        {
            string tablaRelacionada = null;
            string campoClave = null;

            using (OdbcConnection connection = cn.ProbarConexion())
            {
                try
                {
                    string sQuery = $@"
                SELECT REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE 
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = '{sTablaOrigen}' 
                AND COLUMN_NAME = '{sCampo}';";

                    using (OdbcCommand command = new OdbcCommand(sQuery, connection))
                    {
                        using (OdbcDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tablaRelacionada = reader.GetString(0);
                                campoClave = reader.GetString(1);

                                Console.WriteLine($"Clave foránea de {sTablaOrigen} que referencia a {tablaRelacionada}: {campoClave}");

                                string campoDisplay = campoClave;

                                return (tablaRelacionada, campoClave, campoDisplay);
                            }
                            else
                            {
                                Console.WriteLine($"No se encontró clave foránea en {sTablaOrigen} para el campo {sCampo}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al obtener clave foránea: " + ex.Message);
                }
            }

            return (tablaRelacionada, campoClave, campoClave);
        }

        public OdbcDataAdapter llenarTblAyuda(string tabla)
        {
            string sql = "SELECT * FROM " + tabla + " ;";
            using (OdbcConnection connection = cn.ProbarConexion())
            {
                return new OdbcDataAdapter(sql, connection);
            }
        }
        //******************************************** CODIGO HECHO POR VICTOR CASTELLANOS ***************************** 
    }
}