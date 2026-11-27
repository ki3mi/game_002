# Nivel 1 · Etapa 1 · Mapa de la planta baja

Crear `Assets/EcoElectry/Content/Scenes/Level01.unity` como una escena interior independiente. El jugador entra desde el acceso del colegio y explora en primera persona. La planta baja debe sentirse laberíntica al descubrirla y fácil de recorrer cuando el jugador reconoce los cruces. Los bucles, los atajos y las vistas parciales organizan la exploración. El interior toma su dirección visual del prototipo de horror de pasillos. [Introducción del nivel 0](../Nivel%200%20Introducci%C3%B3n/Nivel_0_Contexto.md) · [Prototipo de horror](../../Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity) · [Arquitectura](../Arquitectura.md)

## Dirección visual y atmósfera

- **Arquitectura y materiales.** Usar geometría low poly con tramos largos, esquinas que cortan la vista, puertas con vidrio y marcos repetibles. Partir del yeso beige apagado, el zócalo verde grisáceo y el terrazo oscuro mate del prototipo. Los baños pueden distinguirse por sus azulejos, la biblioteca por la madera de las estanterías y el comedor por su superficie lavable y sus mesas alineadas. Mantener la paleta desaturada en las tres salas.
- **Luz por tramos.** Alternar zonas de fluorescentes tenues con giros en penumbra y entradas de luz exterior azul. Destacar el vestíbulo y el cruce central con focos localizados para que se reconozcan al volver. Reservar los acentos de emisión para tubos y pilotos. Usar las ventanas, los marcos y la geometría de puertas para recortar los haces y crear sombras reconocibles.
- **Cámara y linterna.** Usar `HorrorCorridorProfile.asset` como base de grano fino, viñeteado y desaturación. Si el nivel requiere ajustes, guardarlos en un perfil propio. Conservar la marcha en primera persona con balanceo sutil y el haz cálido de la linterna. La prueba del mapa debe permitir distinguir puertas, marcos y escalones con luz ambiental y leer rótulos al acercarse con la linterna.

Montar y revisar primero un tramo entre el vestíbulo y el cruce central desde la altura de la cámara jugable. Una vez definida la relación entre materiales, iluminación y contraste en ese tramo, aplicarla a las demás alas. [Perfil visual del prototipo](../../Assets/EcoElectry/Content/Rendering/HorrorCorridorProfile.asset) · [Guía de cámara e iluminación](../Integracion-Camara-Pasillos.md)

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
4. **Aplicar el acabado del prototipo.** Usar el tramo de referencia para ajustar el contraste y extender los materiales y la iluminación por zonas. Variar el mobiliario, las señales y las vistas por vidrio para que cada ala tenga puntos reconocibles. Revisar las puertas en penumbra y las vistas hacia el cruce con la linterna encendida y apagada.
5. **Preparar la escena para el siguiente paso.** Agrupar `Entrance`, `MainCorridor`, `Classrooms`, `Restrooms`, `FacultyRoom`, `Library`, `Cafeteria`, `Stairs` y `Markers`. Reutilizar el explorador en primera persona del prototipo para las pruebas de recorrido. Registrar los puntos candidatos de reparación, búsqueda de piezas y apariciones como marcadores. La conexión desde el nivel 0 se habilitará cuando exista la escena nueva y se valide su punto de entrada. [Integración de cámara y linterna](../Integracion-Camara-Pasillos.md)

## Criterios de aceptación

| Prueba | Resultado esperado |
| --- | --- |
| **Entrada y regreso** | Al aparecer, se entiende cómo alcanzar el corredor principal. Se puede volver al vestíbulo desde cualquiera de las alas. |
| **Orientación y bucles** | Se completa la vuelta por aulas, comedor y biblioteca. Al repetirla, los espacios y el cruce permiten reconocer el camino sin depender de flechas en pantalla. |
| **Salas transitables** | Se puede acceder a las aulas, baños, comedor, sala de docentes y biblioteca a pie, con suelo continuo y sin puertas o muebles atravesados. |
| **Atmósfera y lectura** | Los materiales mates, el grano fino y la alternancia de luz fría y penumbra recuerdan al prototipo. Con la linterna apagada se reconocen los accesos cercanos y las referencias principales. Al encenderla se leen los rótulos y se revelan detalles en las zonas oscuras. |
| **Escalera** | Se alcanza el descansillo inferior. La barrera impide subir al segundo piso y permite continuar por el corredor. |
| **Escena independiente** | `Level01.unity` se abre y recorre directamente. Usa sus propios marcadores y recursos compartidos. Las averías, anomalías y objetivos se añadirán en las etapas de jugabilidad. |
