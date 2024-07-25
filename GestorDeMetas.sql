USE GestorDeMetas;

CREATE TABLE Meta (
    IdMeta INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(80) NOT NULL,
    Fecha DATE NOT NULL,
    Estatus INT NOT NULL DEFAULT 1     
);

CREATE TABLE Tarea (
    IdTarea INT IDENTITY(1,1) PRIMARY KEY,
    IdMeta INT NOT NULL,
    NombreTarea NVARCHAR(80) NOT NULL,
	Descripcion NVARCHAR(100) NOT NULL,
    Fecha DATE NOT NULL,
    Estatus INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Tarea_Meta FOREIGN KEY (IdMeta) REFERENCES Meta(IdMeta)
);

INSERT INTO dbo.Meta VALUES ('Primera Meta de prueba',GETDATE(),1)


INSERT INTO dbo.Tarea VALUES (1,'Primera tarea','Primera tarea de primer meta de prueba',GETDATE(),1)
INSERT INTO dbo.Tarea VALUES (1,'Segunda tarea','Segunda tarea de primer meta de prueba',GETDATE(),1)
INSERT INTO dbo.Tarea VALUES (1,'Tercera tarea','Tercera tarea de primer meta de prueba',GETDATE(),0)

SELECT * FROM dbo.Meta
SELECT * FROM dbo.Tarea

-- QUERY DE OBTENCION DE PORCENTAJES
SELECT m.*,
    ROUND(COALESCE(
        (SUM(CASE WHEN t.Estatus = 1 THEN 1 ELSE 0 END) * 100.0) / COUNT(t.IdTarea), 
        0
    ),2) AS porcentaje
FROM 
    dbo.Meta m
LEFT JOIN 
    dbo.Tarea t ON m.IdMeta = t.IdMeta
GROUP BY 
    m.IdMeta, m.Nombre, m.Fecha, m.Estatus

	SELECT m.*,  ROUND(COALESCE(    (SUM(CASE WHEN t.Estatus = 1 THEN 1 ELSE 0 END) * 100.0) / COUNT(t.IdTarea),       0   ),2) AS Porcentaje FROM    dbo.Meta m LEFT JOIN  dbo.Tarea t ON m.IdMeta = t.IdMeta GROUP BY   m.IdMeta, m.Nombre, m.Fecha, m.Estatus