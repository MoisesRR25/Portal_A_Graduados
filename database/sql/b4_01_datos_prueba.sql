-- B4: 50 000 ofertas de prueba, identificables por el prefijo 'PRUEBA B4 -'
-- Cerca del 30 % quedan activas y vigentes; el resto, cerradas y vencidas.
INSERT INTO oferta (id_empresa, cargo, area, ubicacion, descripcion_corta, descripcion,
                    requisitos, rango_salarial, modalidad, clasificacion_oportunidad,
                    fecha_publicacion, fecha_cierre, estado)
SELECT 1,
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
       CASE WHEN g % 10 < 3 THEN 'Activa' ELSE 'Cerrada' END
FROM generate_series(1, 50000) AS g;

ANALYZE oferta;