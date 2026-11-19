# Arquitectura de desarrollo de EcoElectry

El proyecto usa Unity `6000.6.0f1` y ya tiene instalados Input System, uGUI y Unity Test Framework. Esta especificación organiza el código, las escenas, los datos y los modelos 3D para que cada cambio tenga un lugar claro. [Mecánicas del juego](Mecanicas.md) · [Nivel 01](Nivel01-LaPrimeraAula.md)

## Estructura del proyecto

El código nuevo y su contenido se incorporan bajo `Assets/EcoElectry/`. Los scripts se agrupan por responsabilidad. Dentro de cada capa se abre una subcarpeta por sistema cuando existan archivos que colocar en ella.

```text
Docs/
Assets/EcoElectry/
  Runtime/
    Domain/          Reglas y estado del juego en C#
    Application/     Sesión de nivel, acciones del jugador y flujos de campaña
    Unity/           Composición, entrada, cámaras, interacción, HUD y guardado
  Content/
    Definitions/     Niveles, diálogos, tareas, catálogos y habilidades
    Models/          Modelos 3D, materiales y texturas
    Prefabs/         Jugador, NPC, estructuras, equipos y pantallas reutilizables
    Scenes/          Inicio, niveles jugables y escenas de prueba
  Editor/            Herramientas de edición y validación de contenido
  Tests/
    EditMode/        Reglas, datos y operaciones de guardado
    PlayMode/        Integración de escenas, cámara, interacción y HUD
```

Usar nombres de archivo y espacios de nombres coherentes con su función, como `EcoElectry.Domain.Electrical` o `EcoElectry.Unity.Player`. Un componente reutilizable se implementa una vez y se configura por instancia, mediante datos o referencias serializadas. Los nombres de carpetas y archivos técnicos se escriben sin espacios ni tildes. Los textos que ve el jugador conservan el español.

## Capas y dependencias

| Capa | Responsabilidad | Dependencias del proyecto |
| --- | --- | --- |
| `EcoElectry.Domain` | Reglas de encaje, conexiones, simulación eléctrica, requisitos y estrellas, ingresos y habilidades. | C# sin referencias a Unity. |
| `EcoElectry.Application` | Ejecuta acciones como instalar, verificar, probar, entregar y restaurar el nivel. Coordina el estado de la sesión y define el contrato de guardado. | `Domain`. |
| `EcoElectry.Unity` | Compone la sesión, traduce entrada y colisiones en acciones, presenta modelos y UI, y escribe el guardado en disco. | `Application` y `Domain`. |
| `EcoElectry.Editor` | Valida escenas, prefabs y definiciones durante la edición. | Capas necesarias. Se compila solo en el Editor. |

Usar un `.asmdef` para cada capa y ensamblados separados para las pruebas. `Domain` y `Application` se configuran con `No Engine References`. Las referencias siguen la tabla y se declaran explícitamente para impedir dependencias circulares. [Manual de Unity sobre definiciones de ensamblado](https://docs.unity3d.com/es/current/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

### Responsabilidad de los sistemas

| Sistema | Dónde vive la decisión |
| --- | --- |
| Jugador e interacción | Un controlador de entrada y cámara en Unity. El modo de interacción distingue exploración, diálogo, intervención y prueba. Al cerrar la intervención, la cámara restaura la vista anterior. |
| Construcción y cableado | Unity propone la posición, el encaje y los datos de colisión. El dominio valida y registra las piezas, sus dependencias y conexiones mediante identificadores. La vista previa, el cálculo de costo y la confirmación usan las mismas reglas. |
| Tablero, herramientas y fallas | Las interacciones leen y cambian el estado del circuito a través de la sesión. El dominio resuelve continuidad, cortocircuitos y descarga durante una corrección. El sonido, la animación y el aviso muestran el resultado. |
| NPC, tutorial y HUD | Las definiciones del nivel contienen diálogos, tareas y sus condiciones. La sesión marca las tareas cuando se confirma la acción correspondiente. El HUD dibuja los diálogos abajo y las tareas arriba a la izquierda a partir del estado de la sesión. |
| Simulación y calificación | Al pulsar **Probar**, la sesión toma una copia de la instalación y ejecuta la simulación por intervalos definidos, independiente de los fotogramas. El resultado indica cuándo y por qué falla un servicio. El mismo evaluador determina la calificación para el informe y la entrega. |
| Campaña y habilidades | El progreso es la fuente de verdad sobre encargos completados, saldo, mejoras, potenciadores y mejor calificación. El presupuesto de cada encargo pertenece a su definición. |

## Escenas, datos y ciclo de vida

- Una escena de inicio crea los servicios de campaña y carga el nivel. Cada escena jugable tiene un `LevelRoot` que conecta su definición con los NPC, puntos de interacción, zonas de construcción y puntos de reaparición. Al salir, se libera la sesión del nivel y se desconectan sus suscripciones. Las dependencias se suministran desde estos puntos de composición.
- Las escenas contienen la disposición del escenario y las referencias locales. Los prefabs se componen con referencias a sus hijos y configuración reutilizable. Las reglas compartidas viven en los sistemas, sin búsquedas globales de objetos para resolverlas.
- Las definiciones editables de niveles, diálogos, piezas, cables y habilidades se guardan como `ScriptableObject`. Al iniciar un nivel, se convierten en datos que leen las reglas de C#. El estado mutable de la partida se crea por separado. Por ejemplo, el presupuesto y la reserva exigida de *La primera aula* se configuran en su definición, mientras que el diseño construido y la carga actual pertenecen a la sesión.
- Los niveles, las piezas, los bornes, las tareas y los puntos de guardado tienen identificadores estables y únicos dentro del catálogo o nivel correspondiente. Cambiar un nombre visible o mover un objeto dentro de la jerarquía conserva su identificador.

## Modelos 3D y prefabs de juego

Organizar `Content/Models/` por familias, como `Environment`, `Structures`, `Electrical` y `Characters`. Dentro de cada familia, mantener juntos el modelo exportado, sus materiales y sus texturas. Compartir un material cuando se use en varios modelos. El responsable de 3D mantiene el archivo fuente y entrega un FBX para integrar en Unity. Verificar antes de integrar que la escala sea coherente con el nivel, que el pivote sirva para colocar la pieza y que sus variantes tengan la misma orientación. Para estructuras encajables también se comprueban las dimensiones con el módulo base.

El **modelo importado** proporciona la geometría. El **prefab de juego** lo contiene como hijo visual y define sus colliders, interacciones, bornes y puntos de encaje. Los puntos que usa la lógica se crean en el prefab y mantienen su identidad al actualizar el FBX. Por ejemplo, `SM_ElectricalPanel` identifica el modelo y `PF_ElectricalPanel` el tablero interactivo. Al reexportar, conservar la ruta y el `.meta` del archivo y revisar los materiales y las referencias si cambia la jerarquía importada. [Tipos de prefab en Unity](https://docs.unity3d.com/ja/current/ScriptReference/PrefabAssetType.html) · [Flujo de archivos y metadatos de Unity](https://docs.unity3d.com/ru/current/Manual/AssetWorkflow.html)

El responsable de desarrollo integra el modelo en el prefab, configura sus conexiones y lo registra en el catálogo si es instalable. La validación comprueba referencias, colliders y puntos requeridos antes de utilizar el prefab en un nivel. Versionar cada archivo de Unity con su `.meta` y respetar las reglas de `.gitattributes` para los binarios.

## Guardado y recuperación

- **Sesión de nivel.** El punto de control automático conserva el diseño, el estado del tablero y del circuito, las tareas y el identificador de reaparición. En el nivel 01 se registra al cerrar el informe de una prueba fallida. Ante una descarga se restaura esa captura y se vuelve a presentar la tarea pendiente.
- **Progreso de campaña.** La primera entrega válida registra en una sola operación el encargo completado, el día y sus créditos. Una entrega repetida puede actualizar el mejor diseño, las estrellas o un potenciador pendiente. El registro del encargo y de los potenciadores ya concedidos evita que se cobren dos veces al reintentar.
- **Almacenamiento.** El adaptador de guardado escribe datos con versión de esquema e identificadores estables en `Application.persistentDataPath`. Escribe primero un archivo temporal y conserva una copia válida anterior hasta confirmar el reemplazo. Al cargar, valida el esquema y los identificadores antes de aplicar el estado. Si falla, ofrece recuperar la copia anterior y conserva el archivo original para diagnóstico.

## Migración del proyecto actual

1. Usar `Assets/ResourcePack/Scenes/SampleScene.unity` como referencia para comprobar el comportamiento existente. Incorporar el código nuevo en la estructura propuesta y validar primero un recorrido completo de *La primera aula*.
2. Revisar `FPSPlayer` y `BoardDemoPlayer` como antecedentes del controlador común. Integrar sus interacciones útiles con el controlador de entrada y cámara definido para el juego. Mantener un solo controlador activo en cada escena jugable.
3. Revisar `ModularBoard`, `BoardSlot`, `EnergyCircuit`, `EnergyGridRegistry` y los componentes de tablero como un grupo con referencias cruzadas. Reutilizar los componentes que controlan la presentación y aislar las decisiones de simulación en una sesión por nivel. Mientras exista código en `Assembly-CSharp`, las capas nuevas declaran sus propias dependencias y la integración con los prototipos se hace desde el código existente hasta migrar el grupo. [Referencias desde ensamblados personalizados en Unity](https://docs.unity3d.com/es/current/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
4. Mover scripts, escenas y prefabs en cambios pequeños desde Unity para conservar sus `.meta`. Separar reubicaciones de cambios de nombre, espacio de nombres o formato serializado. Después de cada cambio, reabrir las escenas afectadas y comprobar componentes, referencias y comportamiento antes de retirar la versión anterior.

## Comprobaciones por entrega

- **Edit Mode.** Verificar las cuatro soluciones del nivel 01, encajes incompatibles, cortes por fallas, cálculo de estrellas y abonos de campaña sin duplicados. Comprobar que una partida guardada se restaura con el tablero y las tareas esperados.
- **Play Mode.** Recorrer diálogo, tarea, cámara en primera persona, instalación, prueba, descarga y reaparición. Cambiar de escena y confirmar que hay una sola sesión activa y que la entrada vuelve a funcionar.
- **Contenido.** Comprobar identificadores duplicados, referencias faltantes, objetivos sin tareas asociadas y prefabs con anclajes requeridos ausentes. Revisar el nivel en el Editor después de modificar un FBX usado por modelos interactivos.
