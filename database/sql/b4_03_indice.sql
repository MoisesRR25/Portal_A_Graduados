-- B4: indice parcial sobre la expresion lower(area), ordenado como la consulta
-- El predicado debe coincidir exactamente con el estado canonico 'Publicada'.
-- Se recrea porque CREATE INDEX IF NOT EXISTS no actualiza una definicion existente.
DROP INDEX IF EXISTS ix_oferta_publicada_area_fecha;
CREATE INDEX ix_oferta_publicada_area_fecha
    ON oferta (lower(area), fecha_publicacion DESC, id_oferta DESC)
    WHERE estado = 'Publicada';

ANALYZE oferta;