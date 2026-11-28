# Inventario de audios · Llamada durante la conducción

Guille llama en cuanto el jugador recibe el volante. El receptor muestra la llamada entrante y repite un timbre hasta que el jugador pulsa **E**. Al contestar, empieza el primer clip. Los siguientes se activan según el avance por la carretera. Los archivos de voz están en `Assets/EcoElectry/Content/Audio/Level00/SoundTracks` y el filtro telefónico se aplica durante la reproducción.

| Orden | Archivo de audio | Activación | Duración |
| --- | --- | --- | --- |
| 1 | `L00_Llamada_01_Asignacion.wav` | Al contestar con E | 6,99 s |
| 2 | `L00_Llamada_02_Colegio_Aislado.wav` | 22 % del recorrido | 9,49 s |
| 3 | `L00_Llamada_03_Contrato_y_Estancia.wav` | 39 % del recorrido | 9,52 s |
| 4 | `L00_Llamada_04_Turno_Nocturno.wav` | 56 % del recorrido | 11,61 s |
| 5 | `L00_Llamada_05_Advertencia_y_Llegada.wav` | 80 % del recorrido, unos 550 m antes de la cinemática de llegada | 13,08 s |
| 6 | `L00_Llamada_06_Despues_de_Estacionar.wav` | Al estacionar y apagar el motor | 6,68 s |
| 7 | `L00_Llamada_07_Indicaciones_Caseta.wav` | Al continuar a pie, después del clip anterior | 9,84 s |

Los siete archivos suman aproximadamente **67,21 segundos**. La llamada continúa durante la llegada y termina con un tono de cierre tras las indicaciones para la caseta.

## Guiones para Voice Studio

### 01 · Asignación

> ¿Me escuchas? Bien. Te llamo para confirmar que vas rumbo al colegio. El viaje es largo, así que aprovecho para explicarte el trabajo antes de que llegues.

### 02 · Colegio aislado

> Es un colegio rural grande, bastante lejos de la ciudad. No hay talleres ni proveedores cerca. Si algo falla, tendrás que resolverlo allí mismo con las herramientas y repuestos que encuentres.

### 03 · Contrato y estancia

> Por eso te quedarás en el recinto durante todo el contrato. Ya deben tenerte preparado un cuarto. Organiza tus cosas para pasar allí las semanas de trabajo; no cuentes con volver a la ciudad cada noche.

### 04 · Turno nocturno

> Harás el mantenimiento de noche. Durante el día las clases siguen sin interrupción y no podemos meternos en medio. Cuando empieza el toque de queda, los estudiantes se quedan dentro y tienes vía libre para revisar las instalaciones y hacer tus tareas.

### 05 · Advertencia y llegada

> Una cosa más: la gente de la zona cuenta que pasan cosas raras alrededor del colegio. No sé cuánto creerles, pero si oyes algo fuera, mantente alerta y avisa antes de salir a investigar. Ya casi llegas. Busca la entrada iluminada junto a la caseta.

## Notas de grabación

- Interpretación: llamada laboral al principio, práctica y directa; la última advertencia baja un poco el tono, sin convertirla en una amenaza explícita.
- Mantener el mismo nivel, distancia de micrófono y filtro telefónico en todos los clips.
- Exportar una toma limpia por archivo, con silencios cortos al inicio y al final; aplicar el efecto de teléfono dentro del juego para poder ajustar su intensidad.
