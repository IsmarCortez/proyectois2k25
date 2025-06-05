using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Portal_Web.Models
{
    [Table("Tbl_expedientes")]
    public class Expediente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Pk_id_expediente { get; set; }

        public int? Fk_id_postulante { get; set; }

        public byte[] curriculum { get; set; }

        public byte[] documento_entrevista { get; set; }

        public byte[] pruebas_psicometricas { get; set; }

        public decimal? prueba_logica { get; set; }
        public decimal? prueba_numerica { get; set; }
        public decimal? prueba_verbal { get; set; }
        public decimal? razonamiento { get; set; }
        public decimal? prueba_tecnologica { get; set; }

        [StringLength(255)]
        public string prueba_personalidad { get; set; }
    }
}
