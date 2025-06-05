using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Capa_Vista_RRHH
{
    public partial class Mantenimiento_Alumnos : Form
    {
        String idUsuario;
        public Mantenimiento_Alumnos()
        {
            InitializeComponent();
            string[] alias = { "Carnet Alumno", "Nombre Alumno", "Direccion", "Telefono", "Email", "Estado" };
            navegador1.AsignarAlias(alias);
            navegador1.AsignarSalida(this);
            navegador1.AsignarColorFondo(ColorTranslator.FromHtml("#B4D2F0"));
            navegador1.AsignarColorFuente(Color.Black);
            navegador1.AsignarTabla("alumnos");
            navegador1.ObtenerIdAplicacion("1000");
            navegador1.ObtenerIdUsuario(idUsuario);
            navegador1.AsignarAyuda("1");
            navegador1.AsignarNombreForm("");

            navegador1.AsignarComboConTabla("alumnos", "carnet_alumno", "carnet_alumno", 1);
            
        }
    }
}
