ALTER TABLE tbl_usuarios ADD COLUMN fk_empleado INT;

ALTER TABLE tbl_usuarios
ADD CONSTRAINT fk_empleado_foreign
FOREIGN KEY (fk_empleado) REFERENCES tbl_empleados(pk_clave);

CREATE TABLE tbl_quejas_reclamos (
    Id INT AUTO_INCREMENT PRIMARY KEY,            -- Identificador único, autoincremental
    Nombre VARCHAR(100) NOT NULL,                  -- Nombre completo del empleado
    IdEmpleado VARCHAR(50) NOT NULL,               -- ID del empleado
    Departamento VARCHAR(100),                     -- Departamento (opcional)
    Correo VARCHAR(100) NOT NULL,                  -- Correo electrónico
    Tipo VARCHAR(100) NOT NULL,                    -- Tipo de queja o reclamo
    Descripcion TEXT NOT NULL,                     -- Descripción detallada de la queja
    Urgencia VARCHAR(20),                          -- Urgencia: Baja, Media, Alta
    Confidencial BOOLEAN NOT NULL,                 -- Indica si la queja es confidencial
    Terminos BOOLEAN NOT NULL,                     -- Aceptación de los términos y condiciones
    Fecha DATETIME DEFAULT CURRENT_TIMESTAMP,      -- Fecha de creación, por defecto la fecha actual
    ArchivosAdjuntos TEXT                          -- Archivos adjuntos (opcional)
);
CREATE TABLE IF NOT EXISTS Tbl_postulante(
    Pk_id_postulante INT AUTO_INCREMENT PRIMARY KEY,
    Fk_puesto_aplica_postulante INT NULL,
    nombre_postulante VARCHAR(50) NOT NULL,
    apellido_postulante VARCHAR(50) NOT NULL,
    email_postulante VARCHAR(50) NOT NULL,
    telefono_postulante VARCHAR(15) NOT NULL,
    anios_experiencia INT NOT NULL DEFAULT 0,
    trabajo_anterior VARCHAR(100) NULL,
    puesto_anterior VARCHAR(50) NULL,
    Fk_nivel_educativo INT NOT NULL,
    titulo_obtenido VARCHAR(100) NULL,
    Fk_disponibilidad INT NOT NULL,
    salario_pretendido DECIMAL(10,2) NULL,
    fecha_postulacion DATETIME,
    estado TINYINT(1) NOT NULL DEFAULT 1,
    
    CONSTRAINT Fk_puesto_aplica_postulante FOREIGN KEY (Fk_puesto_aplica_postulante) REFERENCES tbl_puestos_trabajo(pk_id_puestos) ON DELETE SET NULL,
    CONSTRAINT Fk_nivel_educativo FOREIGN KEY (Fk_nivel_educativo) REFERENCES Tbl_nivel_educativo(Pk_id_nivel) ON DELETE RESTRICT,
    CONSTRAINT Fk_disponibilidad FOREIGN KEY (Fk_disponibilidad) REFERENCES Tbl_disponibilidad(Pk_id_disponibilidad) ON DELETE RESTRICT
);

-- Lo siguiente es una sola tabla
DROP TABLE IF EXISTS Tbl_expedientes;
CREATE TABLE IF NOT EXISTS Tbl_expedientes(
	Pk_id_expediente INT AUTO_INCREMENT PRIMARY KEY,
    Fk_id_postulante INT NULL,
    curriculum LONGBLOB,
    pruebas_psicometricas LONGBLOB,
    pruebas_psicologicas LONGBLOB,
    pruebas_aptitud LONGBLOB,
    estado TINYINT(1) NOT NULL DEFAULT 1
);
ALTER TABLE Tbl_expedientes ADD CONSTRAINT Fk_id_postulante FOREIGN KEY (Fk_id_postulante) REFERENCES Tbl_postulante(Pk_id_postulante) ON DELETE SET NULL;
ALTER TABLE Tbl_expedientes
DROP COLUMN pruebas_psicometricas,
DROP COLUMN pruebas_psicologicas,
DROP COLUMN pruebas_aptitud,
DROP COLUMN estado;

ALTER TABLE Tbl_expedientes
ADD COLUMN documento_entrevista LONGBLOB,
ADD COLUMN prueba_logica DECIMAL(5,2),
ADD COLUMN prueba_numerica DECIMAL(5,2),
ADD COLUMN prueba_verbal DECIMAL(5,2),
ADD COLUMN razonamiento DECIMAL(5,2),
ADD COLUMN prueba_tecnologica DECIMAL(5,2),
ADD COLUMN prueba_personalidad VARCHAR(255),
ADD COLUMN pruebas_psicometricas LONGBLOB;

CREATE TABLE IF NOT EXISTS Tbl_nivel_educativo (
    Pk_id_nivel INT AUTO_INCREMENT PRIMARY KEY,
    nivel VARCHAR(50) NOT NULL UNIQUE,
    estado TINYINT(1) NOT NULL DEFAULT 1
);

-- Tabla para disponibilidad
CREATE TABLE IF NOT EXISTS Tbl_disponibilidad (
    Pk_id_disponibilidad INT AUTO_INCREMENT PRIMARY KEY,
    disponibilidad VARCHAR(50) NOT NULL UNIQUE,
    estado TINYINT(1) NOT NULL DEFAULT 1
);

-- hasta aqui
select *from Tbl_nivel_educativo;
INSERT INTO Tbl_nivel_educativo (nivel) VALUES 
('Primaria'), ('Secundaria'), ('Técnico'), ('Universitario'), ('Postgrado');

-- Insertar opciones en Tbl_disponibilidad
INSERT INTO Tbl_disponibilidad (disponibilidad) VALUES 
('Inmediata'), ('1 semana'), ('1 mes'), ('Otro');
INSERT INTO tbl_planilla_Encabezado (
    encabezado_correlativo_planilla,
    encabezado_fecha_inicio,
    encabezado_fecha_final,
    encabezado_total_mes,
    estado
) VALUES (
    1,                         -- Correlativo de planilla
    '2025-05-01',              -- Fecha de inicio del período
    '2025-05-15',              -- Fecha final del período
    10000.00,                  -- Total del mes (puedes ajustar según tus datos)
    1                          -- Estado activo
);
INSERT INTO tbl_planilla_Detalle (
    detalle_total_Percepciones,
    detalle_total_Deducciones,
    detalle_total_liquido,
    fk_clave_empleado,
    fk_id_contrato,
    fk_id_registro_planilla_Encabezado,
    estado
) VALUES (
    500.00,     -- Percepciones
    100.00,     -- Deducciones
    3900.00,    -- Líquido (ej. 3500 base + 500 - 100)
    1,          -- fk_clave_empleado
    1,          -- fk_id_contrato
    1,          -- fk_id_registro_planilla_Encabezado (debe existir en tbl_planilla_Encabezado)
    1           -- estado activo
);
DROP TABLE IF EXISTS `tbl_asistencias`;
CREATE TABLE `tbl_asistencias` (
  `pk_id_asistencia` int NOT NULL AUTO_INCREMENT,
  `fecha` date NOT NULL,
  `hora_entrada` time DEFAULT NULL,
  `hora_salida` time DEFAULT NULL,
  `estado_asistencia` varchar(255) NOT NULL,
  `observaciones` text,
  `fk_clave_empleado` int NOT NULL,
  PRIMARY KEY (`pk_id_asistencia`),
  KEY `fk_clave_empleado` (`fk_clave_empleado`),
  CONSTRAINT `tbl_asistencias_ibfk_1` FOREIGN KEY (`fk_clave_empleado`) REFERENCES `tbl_empleados` (`pk_clave`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

