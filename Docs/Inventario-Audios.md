# Inventario de audios de Just a Wiring Job?

Este documento define dónde se guardan los audios del juego y cómo se nombran. Describe el estado planificado sobre la rama `music`; no mueve ni renombra archivos por sí mismo. [Contexto del juego](Context.md) · [Mecánicas](Mecanicas.md) · [Introducción del nivel 0](Nivel%200%20Introducción/Nivel_0_Contexto.md) · [Mapa del nivel 1](Nivel%201/Nivel_1_Etapa_1.md)

## Ubicación y nombres

Todos los clips de audio, incluidos música, ambientes, voces y efectos, se ubican en `Assets/Music/`. Los nombres técnicos usan minúsculas, `snake_case`, sin espacios ni tildes. Los textos que ve el jugador siguen escribiéndose en español.

| Prefijo | Uso |
| --- | --- |
| `music_` | Música en bucle. |
| `amb_` | Ambientes continuos. |
| `sfx_` | Efectos reproducidos por acciones u objetos. |
| `vo_` | Notas de voz, guardia y grabaciones con diálogo. |
| `ui_` | Respuestas de menús e inventario. |

Cada archivo se versiona junto con su `.meta`. Si un clip ya está referenciado y se mueve o se renombra, se conserva su GUID o se actualizan las escenas y prefabs que lo usan. No se crea una copia duplicada para cambiar el nombre.

## Inventario actual y destino

| Nombre actual | Ruta actual | Nombre objetivo | Uso | Estado y referencias |
| --- | --- | --- | --- | --- |
| `door.wav` | `Assets/Music/door.wav` | `door.wav` | Apertura y cierre de puertas, incluidas las puertas de sección. | Nuevo en la rama `music`. Todavía no tiene referencias en escenas. |
| `Untitled #9 (Smáskífa 1).mp3` | `Assets/Music/Untitled #9 (Smáskífa 1).mp3` | `music_corridor.mp3` | Música en bucle del prototipo de pasillos y de la escena del bosque. | Referenciado en `HorrorCorridorPrototype.unity` y `demoScene_free.unity`. El cambio de nombre requiere actualizar esas referencias o conservar el GUID. |
| `BosqueNocturno.wav` | `Assets/EcoElectry/Content/Audio/Level00/BosqueNocturno.wav` | `amb_forest_night.wav` | Ambiente del bosque durante la llegada al colegio. | Referenciado en `Level00-Introduction.unity`. Bucle, `Play On Awake` desactivado y volumen `0,085`. Pendiente de mover a `Assets/Music/`. |
| `MotorElectrico.wav` | `Assets/EcoElectry/Content/Audio/Level00/MotorElectrico.wav` | `sfx_truck_engine_loop.wav` | Motor de la camioneta durante la conducción y las cinemáticas. | Referenciado en `Level00-Introduction.unity`. Bucle, `Play On Awake` desactivado y volumen `0,2`. Pendiente de mover a `Assets/Music/`. |

La ruta de destino de todas las filas es `Assets/Music/<nombre objetivo>`.

## Audios a incorporar

Los nombres siguientes reservan la ranura de cada clip. Los archivos marcados como «Por guion» se crean cuando exista el texto definitivo.

### Música y ambientes

| Nombre | Momento o lugar | Tipo |
| --- | --- | --- |
| `music_corridor.mp3` | Prototipo de pasillos y escenas que reutilicen su atmósfera. | Bucle 2D, volumen de referencia `0,32`. |
| `amb_forest_night.wav` | Carretera, estacionamiento y desvío hacia el bosque. | Bucle de ambiente. |
| `amb_school_hall.wav` | Vestíbulo, corredores y cruce central del nivel 1. | Bucle de ambiente. |
| `amb_fluorescent_hum.wav` | Zonas con fluorescentes tenues y sala de mantenimiento. | Bucle local. |
| `amb_vhs_hiss.wav` | Televisor CRT y cinta de instrucciones. | Bucle local. |

### Vehículo y exterior

| Nombre | Momento o lugar | Tipo |
| --- | --- | --- |
| `sfx_truck_engine_loop.wav` | Conducción, cinemática inicial y llegada. | Bucle mientras el motor esté activo. |
| `sfx_truck_start.wav` | Encendido del motor al comenzar la conducción. | One-shot. |
| `sfx_tires_asphalt.wav` | Neumáticos sobre la calzada a velocidad. | Bucle opcional. |
| `sfx_barrier_scrape.wav` | Roce con barreras laterales. | One-shot espacial. |

### Puertas, inventario y reparación

| Nombre | Momento o lugar | Tipo |
| --- | --- | --- |
| `door.wav` | Apertura, cierre y cruce de puertas interiores y de sección. | One-shot espacial. |
| `sfx_pickup_key.wav` | Recogida e inspección del llavero. | One-shot. |
| `sfx_pickup_part.wav` | Recogida de conectores y piezas. | One-shot. |
| `sfx_inventory_inspect.wav` | Giro del objeto en el visor 3D del inventario. | One-shot. |
| `sfx_flashlight_toggle.wav` | Encendido y apagado de la linterna con **F**. | One-shot. |
| `sfx_vhs_insert.wav` | Colocar la cinta en la videocasetera. | One-shot espacial. |
| `sfx_vhs_play.wav` | Inicio de la reproducción de la cinta. | One-shot. |
| `sfx_cable_connect.wav` | Emparejar un cable correctamente en el tablero. | One-shot. |
| `sfx_part_install.wav` | Instalar una pieza en su punto de encaje. | One-shot. |
| `sfx_panel_verify_ok.wav` | Verificación correcta del servicio. | One-shot. |
| `sfx_panel_verify_fail.wav` | Verificación con tareas obligatorias pendientes. | One-shot. |

### Terror, miedo y fin del turno

| Nombre | Momento o lugar | Tipo |
| --- | --- | --- |
| `sfx_anomaly_alert.wav` | Sonido que advierte de una anomalía mientras el jugador repara. | One-shot espacial. |
| `sfx_anomaly_stare.wav` | Mirada prolongada antes del oscurecimiento de cámara. | One-shot. |
| `sfx_heartbeat_loop.wav` | Pulso audible a medida que sube el miedo. | Bucle 2D; el volumen sigue el estado de miedo. |
| `sfx_camera_glitch.wav` | Banda horizontal, parpadeo de imagen y pérdida temporal de visión. | One-shot. |
| `sfx_faint.wav` | Derrota por miedo máximo. | One-shot. |

### Voces e interfaz

| Nombre | Momento o lugar | Tipo |
| --- | --- | --- |
| `vo_boss_road_01.wav` | Mensajes y notas de voz del jefe durante la conducción. | Por guion; 2D como teléfono. |
| `vo_guard_entrance_01.wav` | Voz del guardia desde la caseta. | Por guion; espacial y ligeramente amortiguada. |
| `vo_vhs_briefing_01.wav` | Instrucciones de la cinta de mantenimiento. | Por guion; 2D con subtítulos. |
| `ui_menu_open.wav` | Apertura del menú de imagen o del inventario. | One-shot 2D. |
| `ui_menu_close.wav` | Cierre de menús y paneles. | One-shot 2D. |
| `ui_confirm.wav` | Confirmación de una acción de interfaz. | One-shot 2D. |
| `ui_error.wav` | Acción no disponible, como bajar del vehículo fuera de la zona válida. | One-shot 2D. |

## Configuración de referencia

| Categoría | Spatial Blend | Loop | Volumen inicial |
| --- | --- | --- | --- |
| Música | `0` | Sí | `0,32` |
| Ambientes | `0` para globales; espacial para fuentes localizadas | Sí | Según escena; bosque `0,085` |
| Motor | `0` en la configuración actual | Sí | `0,2` |
| Efectos de mundo | Espacial | No | Ajustar en pruebas |
| Interfaz e inventario | `0` | No | Ajustar en pruebas |
| Voces de teléfono o grabación | `0` | No | Ajustar con subtítulos |
| Voz del guardia | Espacial desde la caseta | No | Ajustar con la distancia al acceso |

## Comprobación al incorporar un audio

1. Confirmar que el archivo y su `.meta` están en `Assets/Music/` con el nombre de este inventario.
2. Comprobar que el `AudioSource` use el clip correcto y que `Play On Awake`, `Loop`, `Spatial Blend` y volumen correspondan a su categoría.
3. Si el clip sustituye a otro ya referenciado, revisar las escenas `Level00-Introduction.unity`, `HorrorCorridorPrototype.unity` y `demoScene_free.unity` antes de retirar el original.
4. Probar el momento de activación en Play Mode: llegada, conducción, puertas, inventario, reparación, anomalías y menús.
