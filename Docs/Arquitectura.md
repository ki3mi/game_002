# Arquitectura de desarrollo de Just a Wiring Job?

El proyecto usa Unity `6000.6.0f1` con Input System, uGUI y Unity Test Framework. La arquitectura cubre el prólogo, las jornadas de reparación y los sistemas de terror. `EcoElectry` identifica las rutas, espacios de nombres y ensamblados técnicos existentes. [Contexto](Context.md) · [Mecánicas del juego](Mecanicas.md)

## Estructura del proyecto

El código nuevo y su contenido se incorporan bajo `Assets/EcoElectry/`. Los scripts se agrupan por responsabilidad. Dentro de cada capa se abre una subcarpeta por sistema cuando existan archivos que colocar en ella.

```text
Docs/
Assets/EcoElectry/
  Runtime/
    Domain/          Reglas y estado del juego en C#
    Application/     Sesión de jornada, acciones del jugador y avance entre secciones
    Unity/           Composición, entrada, cámaras, interacción, HUD y guardado
  Content/
    Definitions/     Secciones, mensajes, tareas, piezas y apariciones
    Models/          Modelos 3D, materiales y texturas
    Prefabs/         Jugador, NPC, estructuras, equipos y pantallas reutilizables
    Scenes/          Inicio, exterior, secciones interiores y escenas de prueba
  ThirdParty/        Assets importados y dependencias, agrupados por paquete
  Editor/            Herramientas de edición y validación de contenido
  Tests/
    EditMode/        Reglas, datos y operaciones de guardado
    PlayMode/        Integración de escenas, cámara, interacción y HUD
```

Usar nombres de archivo y espacios de nombres coherentes con su función, como `EcoElectry.Domain.Electrical` o `EcoElectry.Unity.Player`. Un componente reutilizable se implementa una vez y se configura por instancia, mediante datos o referencias serializadas. Los nombres de carpetas y archivos técnicos se escriben sin espacios ni tildes. Los textos que ve el jugador conservan el español.

Los recursos importados de Ezereal, Fantasy Forest, Kansai e Idyllic Fantasy Nature se organizan en `Assets/EcoElectry/ThirdParty/`. Las variantes y escenas propias siguen en `Content/`. Idyllic Fantasy Nature se conserva completo para niveles futuros. Los paquetes originales de esta reorganización quedan como respaldo local en `Papelera/`, junto a `Assets/` y fuera de la importación de Unity.

## Capas y dependencias

| Capa | Responsabilidad | Dependencias del proyecto |
| --- | --- | --- |
| `EcoElectry.Domain` | Valida conexiones, inventario y objetivos. Resuelve miedo, reacciones a anomalías y condiciones de fin de turno. | C# sin referencias a Unity. |
| `EcoElectry.Application` | Coordina la sesión de jornada, las acciones de reparación, el reloj y el avance entre secciones. Define el contrato de guardado. | `Domain`. |
| `EcoElectry.Unity` | Compone las escenas, traduce entrada, colisiones y detección visual en acciones, y presenta cámara, luz, audio e interfaz. Implementa el guardado en disco. | `Application` y `Domain`. |
| `EcoElectry.Editor` | Valida escenas, prefabs y definiciones durante la edición. | Capas necesarias. Se compila solo en el Editor. |

Usar un `.asmdef` para cada capa y ensamblados separados para las pruebas. `Domain` y `Application` se configuran con `No Engine References`. Las referencias siguen la tabla y se declaran explícitamente para impedir dependencias circulares. [Manual de Unity sobre definiciones de ensamblado](https://docs.unity3d.com/es/current/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

### Responsabilidad de los sistemas

| Sistema | Dónde vive la decisión |
| --- | --- |
| Jugador, cámara y linterna | Unity controla conducción, exploración e intervención según el modo activo. En el interior, la cámara conserva la primera persona al entrar y salir del tablero. La linterna distingue la orden de encendido del jugador de un parpadeo temporal. |
| Inventario y mensajes | La sesión registra las llaves y piezas por identificador. Unity presenta la inspección 3D y los mensajes del jefe como contenido guionizado del juego. |
| Reparación y verificación | El dominio valida piezas y conexiones. La sesión conserva cada conexión confirmada aunque el jugador mire fuera del tablero, y marca el objetivo cuando se verifica el servicio. |
| Anomalías y miedo | Unity detecta mirada y dirección del haz. La sesión controla la ventana de reacción y aplica un solo desenlace por aparición. La presentación ejecuta el efecto correspondiente en la linterna o en la cámara y refleja el estado de miedo en el pulso, la visión y el temblor. |
| Reloj y avance | La sesión mantiene el tiempo de juego y resuelve éxito, desmayo o vencimiento del turno. La interfaz muestra ese resultado. La transición de jornada se confirma desde la sesión. |

## Escenas, datos y ciclo de vida

- La ruta prevista empieza con una escena exterior que reúne conducción, estacionamiento, fachada y entrada. Cada sección interior tiene su propia escena y un punto de composición local, como `LevelRoot`. Ya dentro del colegio, las puertas de acceso resuelven la sección de destino. Al cargar una sección, se inicializa la sesión de esa jornada. Al salir, se liberan la sesión y sus suscripciones. El progreso entre días pertenece a la partida.
- Las escenas contienen la disposición del escenario y las referencias locales. Los prefabs se componen con referencias a sus hijos y configuración reutilizable. Las reglas compartidas viven en los sistemas, sin búsquedas globales de objetos para resolverlas.
- Las definiciones editables de secciones, mensajes, piezas, objetivos y apariciones se guardan como `ScriptableObject`. Al iniciar una escena se convierten en datos que leen las reglas de C#. El reloj, el miedo, las piezas recuperadas y las conexiones confirmadas pertenecen a la sesión.
- Las secciones, puertas, piezas, bornes, tareas y apariciones tienen identificadores estables y únicos dentro de su catálogo o escena. Cambiar un nombre visible o mover un objeto dentro de la jerarquía conserva su identificador.

## Modelos 3D y prefabs de juego

Organizar `Content/Models/` por familias, como `Environment`, `Structures`, `Electrical` y `Characters`. Dentro de cada familia, mantener juntos el modelo exportado, sus materiales y sus texturas. Compartir un material cuando se use en varios modelos. El responsable de 3D mantiene el archivo fuente y entrega un FBX para integrar en Unity. Verificar antes de integrar que la escala sea coherente con el nivel, que el pivote sirva para colocar la pieza y que sus variantes tengan la misma orientación. Para estructuras encajables también se comprueban las dimensiones con el módulo base.

El **modelo importado** proporciona la geometría. El **prefab de juego** lo contiene como hijo visual y define sus colliders, interacciones, bornes y puntos de encaje. Los puntos que usa la lógica se crean en el prefab y mantienen su identidad al actualizar el FBX. Por ejemplo, `SM_ElectricalPanel` identifica el modelo y `PF_ElectricalPanel` el tablero interactivo. Al reexportar, conservar la ruta y el `.meta` del archivo y revisar los materiales y las referencias si cambia la jerarquía importada. [Tipos de prefab en Unity](https://docs.unity3d.com/ja/current/ScriptReference/PrefabAssetType.html) · [Flujo de archivos y metadatos de Unity](https://docs.unity3d.com/ru/current/Manual/AssetWorkflow.html)

El responsable de desarrollo integra el modelo en el prefab, configura sus conexiones y lo registra en el catálogo si es instalable. La validación comprueba referencias, colliders y puntos requeridos antes de utilizar el prefab en un nivel. Versionar cada archivo de Unity con su `.meta` y respetar las reglas de `.gitattributes` para los binarios.

## Guardado y recuperación

- **Sesión de jornada.** Mantener juntos el reloj, el miedo, el inventario de la jornada, el estado de las conexiones y las tareas verificadas. El comportamiento tras una derrota y la posibilidad de guardar a mitad de turno quedan por definir.
- **Progreso de partida.** Registrar la sección completada y el siguiente turno como una operación conjunta cuando se confirme el cierre de la jornada. Reintentar la operación debe dejar un solo avance en el registro.
- **Almacenamiento.** El adaptador de guardado escribe datos con versión de esquema e identificadores estables en `Application.persistentDataPath`. Escribe primero un archivo temporal y conserva una copia válida anterior hasta confirmar el reemplazo. Al cargar, valida el esquema y los identificadores antes de aplicar el estado. Si falla, ofrece recuperar la copia anterior y conserva el archivo original para diagnóstico.

## Migración del proyecto actual

1. Usar [HorrorCorridorPrototype.unity](../Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity) como referencia jugable de marcha, cámara, linterna y posprocesado. Sus parpadeos ambientales ofrecen una referencia visual. En la nueva mecánica, el desenlace de una anomalía y su efecto en el miedo los determina la sesión. [Integración de la cámara](Integracion-Camara-Pasillos.md)
2. La escena `Startup.unity` carga actualmente `Level01-LaPrimeraAula.unity`. El prólogo, las secciones interiores y su orden de carga corresponden a la implementación del nuevo recorrido.
3. Evaluar `ModularBoard`, `BoardSlot` y los componentes de tablero existentes para reutilizar los elementos de interacción que sirvan para reparar. Las reglas de las jornadas se integran a través de los ensamblados vigentes. Mientras exista código en `Assembly-CSharp`, mantener la dirección de referencias descrita en la documentación de Unity. [Referencias desde ensamblados personalizados en Unity](https://docs.unity3d.com/es/current/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
4. Mover scripts, escenas y prefabs en cambios pequeños desde Unity para conservar sus `.meta`. Separar reubicaciones de cambios de nombre, espacio de nombres o formato serializado. Después de cada cambio, reabrir las escenas afectadas y comprobar componentes, referencias y comportamiento antes de retirar la versión anterior.

## Comprobaciones por entrega

- **Edit Mode.** Verificar emparejamiento de cables, recuperación de piezas, conservación del montaje al interrumpir, desenlaces de anomalía y condiciones de fin del turno. Comprobar el avance al día siguiente sin duplicados.
- **Play Mode.** Recorrer el prólogo, inspeccionar las llaves, cargar una sección, recoger una pieza, interrumpir y retomar una reparación y comprobar los dos tipos de parpadeo. Al cruzar una puerta de sección, confirmar que hay una sola sesión activa y que la entrada vuelve a funcionar.
- **Contenido.** Comprobar identificadores duplicados, referencias faltantes, objetivos sin tareas asociadas y prefabs con anclajes requeridos ausentes. Revisar el nivel en el Editor después de modificar un FBX usado por modelos interactivos.
