# EcoElectry

EcoElectry es un juego de puzles de construcción y simulación en 3D low poly. Eres un electricista de un programa municipal de electrificación. Empiezas instalando energía solar en un colegio rural y avanzas hacia encargos más complejos, hasta abastecer los servicios de una ciudad con energía renovable.

El diseño aborda el **ODS 7** mediante decisiones sobre generación renovable, eficiencia y continuidad del servicio, y el **ODS 4** mediante prácticas guiadas con herramientas eléctricas.

## Cómo se juega

- Recorre cada establecimiento, conversa con sus responsables y reúne los requisitos del encargo.
- Construye con piezas encajables, elige los recorridos del cableado y pasa a primera persona para conectar y verificar equipos.
- Simula una jornada para comprobar el suministro. Si un servicio falla, usa el informe para corregir el diseño y volver a probar.
- Gana estrellas según los requisitos, el presupuesto y el objetivo de excelencia. Usa los créditos de campaña para comprar mejoras y vuelve a encargos anteriores para conseguir mejores resultados y potenciadores.

## Abrir el proyecto

1. Si obtuviste el repositorio mediante Git y faltan los archivos binarios de Git LFS, ejecuta `git lfs pull` en la raíz.
2. En Unity Hub, agrega la carpeta raíz y abre el proyecto con **Unity `6000.6.0f1`**, la versión registrada en [ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).
3. En el Editor, abre [`Startup.unity`](Assets/EcoElectry/Content/Scenes/Startup.unity) y pulsa **Play** para cargar el prólogo. También puedes abrir [`Level00-Introduction.unity`](Assets/EcoElectry/Content/Scenes/Level00-Introduction.unity) directamente.
4. En **Window > General > Test Runner** puedes ejecutar las pruebas de configuración en **Edit Mode** y **Play Mode**. Usa **Tools > EcoElectry > Validar definiciones de nivel** para revisar los datos y los identificadores de nivel.

El prólogo transcurre en la carretera del bosque y el exterior del colegio. La primera sección interior se montará en una escena propia. [Introducción](Docs/Nivel%200%20Introducci%C3%B3n/Nivel_0_Contexto.md) · [Plan del mapa del nivel 1](Docs/Nivel%201/Nivel_1_Etapa_1.md)

## Sandbox eléctrico

Abre [SandboxElectrical.unity](Assets/EcoElectry/Content/Scenes/SandboxElectrical.unity) y pulsa **Play**. Usa **WASD** para caminar, arrastra con el botón derecho para mirar y usa el botón **Cámara** o la tecla **C** para alternar entre primera y tercera persona. Pulsa **Esc** para salir de una intervención. El panel lateral conserva las acciones de montaje eléctrico, cableado y herramientas.

En la barra inferior **Modelos**, elige una familia o cambia de página para encontrar una pieza. Apunta a la grilla del suelo y haz clic cuando la huella esté verde. Cada celda mide **0,5 × 0,5 m**. Pulsa **R** para girar, **Esc** para cancelar o **Deshacer** para retirar la última pieza. La barra sirve para componer el escenario. El montaje eléctrico usa los anclajes del panel lateral. **Reiniciar caso** retira las piezas de la grilla y restaura la instalación del caso.

El menú **Tools > EcoElectry > Actualizar catálogo de modelos** incorpora los recursos de `ResourcePack` y del contenido de EcoElectry. Calcula la huella según las dimensiones de cada prefab y reduce proporcionalmente los modelos muy grandes para que quepan en el taller.

| Caso | Cómo reproducirlo |
| --- | --- |
| Instalación desde cero | Coloca el panel, la batería y la luminaria. Traza **Salida solar → Entrada FV** con cable fotovoltaico. Usa cable interior para **Borne de batería → Entrada batería** y **Salida aula → Borne de luminaria**. En el ramal del aula añade **Suelo izq. → Muro izq. → Techo izq. → Techo der. → Muro der.** entre los dos bornes. Pela y fija ambos extremos de cada tramo y verifica continuidad. Activa el panel y la batería, cierra las cuchillas y pulsa **Probar**. |
| Conexión abierta y descarga | Pulsa **Probar** y cierra el informe. Intenta fijar el borne de la luminaria con el tablero en servicio para comprobar la reaparición. Abre las cuchillas, aísla el panel y la batería, y mide continuidad. Fija el extremo de la luminaria y repite la prueba con la instalación en servicio. |
| Cortocircuito | Prueba el caso y comprueba el disparo de la protección. Cierra el informe, abre las cuchillas, aísla las fuentes, corrige el contacto en **Salida aula** y rearma la protección. Pon la instalación en servicio y vuelve a probar. |
| Reserva insuficiente | Prueba con la luminaria estándar para observar el corte a las 19:00. Cambia a la eficiente y repite. El cruce en planta de los cables de entrada mantiene el circuito operativo. |

Los datos y las referencias a los prefabs del circuito están en [SandboxElectrical.asset](Assets/EcoElectry/Content/Definitions/Levels/SandboxElectrical.asset).

## Documentación

| Documento | Contenido |
| --- | --- |
| [Contexto](Docs/Context.md) | Premisa, identidad visual, ODS y progresión de la campaña. |
| [Mecánicas](Docs/Mecanicas.md) | Construcción, cableado, simulación, estrellas, economía y habilidades. |
| [Nivel 1 · Planta baja](Docs/Nivel%201/Nivel_1_Etapa_1.md) | Distribución del colegio, orden de montaje y pruebas de recorrido. |
| [Arquitectura](Docs/Arquitectura.md) | Organización del código, escenas, datos, prefabs y modelos 3D. |
