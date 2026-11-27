-- B4 (experimento): desactiva el escaneo por bitmap SOLO en esta sesion
SET enable_bitmapscan = off;

EXPLAIN (ANALYZE, BUFFERS)
SELECT o.id_oferta, o.cargo, o.area, o.ubicacion, o.fecha_publicacion
FROM oferta o
WHERE o.estado = 'publicada'
  AND o.fecha_cierre > (now() AT TIME ZONE 'utc')
  AND lower(o.area) = 'desarrollo de software'
ORDER BY o.fecha_publicacion DESC, o.id_oferta DESC
LIMIT 20;