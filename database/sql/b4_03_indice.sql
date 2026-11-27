-- B4: indice parcial sobre la expresion lower(area), ordenado como la consulta
CREATE INDEX IF NOT EXISTS ix_oferta_publicada_area_fecha
    ON oferta (lower(area), fecha_publicacion DESC, id_oferta DESC)
    WHERE estado = 'publicada';

ANALYZE oferta;