-- Alinea los estados de oferta con el diccionario de datos del SRS
UPDATE oferta SET estado = 'Publicada' WHERE estado = 'Activa';

-- El indice parcial anterior usaba estado = 'Activa'; se reemplaza por el nuevo (b4_03)
DROP INDEX IF EXISTS ix_oferta_activa_area_fecha;

ANALYZE oferta;