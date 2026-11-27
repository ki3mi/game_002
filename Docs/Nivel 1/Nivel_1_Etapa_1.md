# Nivel 1 · Etapa 1 · Mapa de la planta baja

Crear `Assets/EcoElectry/Content/Scenes/Level01.unity` como una escena interior independiente. El jugador entra desde el acceso del colegio y explora en primera persona. La planta baja debe sentirse laberíntica al descubrirla y fácil de recorrer cuando el jugador reconoce los cruces. Usar como referencia de ritmo los bucles, los atajos y las vistas parciales de un mapa de terror. La distribución del colegio será propia. [Introducción del nivel 0](../Nivel%200%20Introducci%C3%B3n/Nivel_0_Contexto.md) · [Arquitectura](../Arquitectura.md)

## Distribución propuesta

El **corredor principal** sale del vestíbulo y llega a un cruce central desde el que se distingue la escalera al fondo. Dos alas se conectan por un pasillo transversal cercano al comedor. Esa conexión permite regresar al corredor principal por un camino distinto al de ida.

| Zona | Ubicación y conexiones | Referencia para orientarse |
| --- | --- | --- |
| **Entrada y vestíbulo** | Conectan con la puerta del colegio en el nivel 0. El punto de aparición mira hacia el corredor principal. | Vidrio de la entrada y tablón de avisos. |
| **Corredor principal** | Une vestíbulo, cruce central y descansillo de la escalera. Desde el cruce salen los accesos a ambas alas. | Reloj en el cruce y baranda al fondo. |
| **Aulas y baños** | Dos o tres aulas y un bloque de baños se abren al pasillo de un ala. El pasillo gira al fondo para enlazar con el comedor. | Puertas numeradas, filas de pupitres y revestimiento distinto en los baños. |
| **Sala de docentes y biblioteca** | La sala de docentes queda cerca del cruce. La biblioteca, más al fondo, tiene un acceso desde su ala y otro hacia el pasillo transversal. | Placa de docentes y estanterías visibles desde la entrada de la biblioteca. |
| **Comedor estudiantil** | Ubicado al fondo de la planta baja, junto al pasillo transversal. Se llega desde el ala de aulas o desde la biblioteca. | Espacio más ancho, filas de mesas y mostrador. |
| **Escalera al segundo piso** | Visible desde el corredor principal. Se puede llegar al descansillo inferior. Una barrera invisible provisional bloquea el inicio de la subida y deja libre el paso de planta baja. | Baranda y señal de piso. |

**Vuelta de exploración:** entrada → cruce central → aulas y baños → comedor → pasillo transversal → biblioteca → cruce central. Incluir un giro que oculte la siguiente puerta y un acceso corto entre el comedor y el corredor principal, preparado como futuro atajo. Durante las primeras pruebas del mapa, dejar ese acceso identificable para evaluar su utilidad. Reservar espacio alrededor de los puntos de reparación y detrás de esquinas o vidrios para futuras apariciones.

## Orden de montaje

1. **Establecer la entrada.** Alinear la altura del umbral y la dirección de llegada con la puerta de sección del nivel 0. Colocar un marcador de aparición interior y otro de conexión junto a la puerta. Mantener el acceso visible desde el vestíbulo para entender de dónde viene el jugador.
2. **Construir el bloque de prueba.** Levantar suelos, paredes y techos sencillos. Montar primero el corredor principal, las dos alas y el pasillo transversal. Hacer un recorrido completo en primera persona para ajustar anchos, esquinas y distancias. Delimitar el hueco de escalera con la barrera provisional sobre una capa que colisione con el jugador.
3. **Montar puertas y ambientes.** Mantener transitables los accesos a las salas previstas y revisar la apertura de hojas, el umbral y el espacio alrededor de los muebles. Reutilizar los prefabs compartidos de puertas, ventanas, pupitres, pizarras y mobiliario docente. Usar volúmenes rotulados para sanitarios, estanterías o mobiliario de comedor que requieran modelos específicos.
4. **Dar identidad a cada ala.** Variar la distribución del mobiliario, las señales, las vistas por vidrio y los puntos de luz provisionales. Mantener la geometría low poly y probar la lectura de las salidas con la cámara granulada. El corredor principal debe ofrecer referencias suficientes para que el jugador pueda volver a orientarse.
5. **Preparar la escena para el siguiente paso.** Agrupar `Entrance`, `MainCorridor`, `Classrooms`, `Restrooms`, `FacultyRoom`, `Library`, `Cafeteria`, `Stairs` y `Markers`. Reutilizar el explorador en primera persona del prototipo para las pruebas de recorrido. Registrar los puntos candidatos de reparación, búsqueda de piezas y apariciones como marcadores. La conexión desde el nivel 0 se habilitará cuando exista la escena nueva y se valide su punto de entrada. [Integración de cámara y linterna](../Integracion-Camara-Pasillos.md)

## Criterios de aceptación

| Prueba | Resultado esperado |
| --- | --- |
| **Entrada y regreso** | Al aparecer, se entiende cómo alcanzar el corredor principal. Se puede volver al vestíbulo desde cualquiera de las alas. |
| **Orientación y bucles** | Se completa la vuelta por aulas, comedor y biblioteca. Al repetirla, los espacios y el cruce permiten reconocer el camino sin depender de flechas en pantalla. |
| **Salas transitables** | Se puede acceder a las aulas, baños, comedor, sala de docentes y biblioteca a pie, con suelo continuo y sin puertas o muebles atravesados. |
| **Escalera** | Se alcanza el descansillo inferior. La barrera impide subir al segundo piso y permite continuar por el corredor. |
| **Escena independiente** | `Level01.unity` se abre y recorre directamente. Usa sus propios marcadores y recursos compartidos. Las averías, anomalías y objetivos se añadirán en las etapas de jugabilidad. |
