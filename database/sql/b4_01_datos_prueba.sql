-- B4: 50 000 ofertas de prueba, identificables por el prefijo 'PRUEBA B4 -'
-- Script solo para la prueba de rendimiento B4; no crea el esquema.
-- Usa los estados canonicos de la API: 'Publicada' y 'Cerrada'.
-- Cerca del 30 % quedan publicadas y vigentes; el resto, cerradas y vencidas.
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM empresa) THEN
        RAISE EXCEPTION 'b4_01 requiere al menos una empresa registrada.';
    END IF;
END $$;

INSERT INTO oferta (id_empresa, cargo, area, ubicacion, descripcion_corta, descripcion,
                    requisitos, rango_salarial, modalidad, clasificacion_oportunidad,
                    fecha_publicacion, fecha_cierre, estado)
SELECT (SELECT id_empresa FROM empresa ORDER BY id_empresa LIMIT 1),
       'PRUEBA B4 - Cargo ' || g,
       (ARRAY['Desarrollo de software','Datos','Redes','Soporte tecnico',
              'Diseno','Marketing','Administracion','Finanzas'])[1 + (g % 8)],
       (ARRAY['San Salvador','Santa Ana','San Miguel','La Libertad','Usulutan'])[1 + (g % 5)],
       'Oferta de prueba ' || g,
       'Descripcion de prueba para la oferta ' || g,
       'Requisitos de prueba para la oferta ' || g,
       NULL,
       (ARRAY['Presencial','Remoto','Hibrido'])[1 + (g % 3)],
       (ARRAY['Empleo','Pasantia'])[1 + (g % 2)],
       (now() AT TIME ZONE 'utc') - (g % 365) * interval '1 day',
       CASE WHEN g % 10 < 3
            THEN (now() AT TIME ZONE 'utc') + ((g % 90) + 1) * interval '1 day'
            ELSE (now() AT TIME ZONE 'utc') - ((g % 90) + 1) * interval '1 day'
       END,
       CASE WHEN g % 10 < 3 THEN 'Publicada' ELSE 'Cerrada' END
FROM generate_series(1, 50000) AS g;

ANALYZE oferta;