SELECT Provincias.* INTO dbo.Provincias FROM OPENROWSET(
BULK 'C:\Users\pmr_a\source\repos\Agapea-Blazor-2024\provincias.json', SINGLE_NCLOB
) AS [Json]
CROSS APPLY OPENJSON (BulkColumn, '$')
WITH (
CCOM nvarchar(max) '$.CCOM',
CPRO nvarchar(max) '$.CPRO',
PRO nvarchar(max) '$.PRO'
) AS [Provincias]


SELECT Municipios.* INTO dbo.Municipios FROM OPENROWSET(
BULK 'C:\Users\pmr_a\source\repos\Agapea-Blazor-2024\municipios.json', SINGLE_NCLOB
) AS [Json]
CROSS APPLY OPENJSON (BulkColumn, '$')
WITH (
CPRO nvarchar(max) '$.CPRO',
CMUM nvarchar(max) '$.CMUM',
CUN nvarchar(max) '$.CUN',
DMUN50 nvarchar(max) '$.DMUN50'
) AS [Municipios]


select * into dbo.Categorias from AgapeaDB2024.dbo.Categorias
select * into dbo.Libros from AgapeaDB2024.dbo.Libros