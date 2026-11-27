-- B4: elimina los datos de prueba (no toca las ofertas reales)
DELETE FROM oferta WHERE cargo LIKE 'PRUEBA B4 - %';
ANALYZE oferta;
-- Para volver al estado "sin indice" y repetir la prueba, descomenta:
-- DROP INDEX IF EXISTS ix_oferta_publicada_area_fecha;