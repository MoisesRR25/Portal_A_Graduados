-- B4: normaliza estados legacy de oferta a los valores canonicos de la API.
-- Script solo para datos de prueba/legacy; no crea el esquema.
UPDATE oferta SET estado = 'Publicada'
WHERE estado IN ('Activa', 'activa', 'publicada', 'PUBLICADA');

UPDATE oferta SET estado = 'En revision'
WHERE estado IN ('En Revision', 'En revisión', 'en revision', 'EN REVISION');

-- El indice parcial anterior usaba estado = 'Activa'; se reemplaza por el nuevo (b4_03)
DROP INDEX IF EXISTS ix_oferta_activa_area_fecha;

ANALYZE oferta;