-- B4: consulta de ofertas vigentes por area (la misma logica que GET /api/ofertas)
-- El estado canonico de la API es 'Publicada' (con mayuscula inicial).
EXPLAIN (ANALYZE, BUFFERS)
SELECT o.id_oferta, o.cargo, o.area, o.ubicacion, o.fecha_publicacion
FROM oferta o
WHERE o.estado = 'Publicada'
  AND o.fecha_cierre > (now() AT TIME ZONE 'utc')
  AND lower(o.area) = 'desarrollo de software'
ORDER BY o.fecha_publicacion DESC, o.id_oferta DESC
LIMIT 20;