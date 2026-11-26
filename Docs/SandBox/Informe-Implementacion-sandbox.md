# Informe de implementación · EcoElectry

El proyecto cuenta con una base de desarrollo en Unity `6000.6.0f1` y un sandbox jugable desde el Editor. El sandbox reúne el ciclo de montaje y diagnóstico eléctrico, la colocación de modelos en una grilla y el cambio de cámara. [Arquitectura](Arquitectura.md) · [Plan del sandbox](Plan-SandboxElectrico.md)

## Base del proyecto

- El contenido de juego está en `Assets/EcoElectry/`. Los ensamblados de `Domain` y `Application` usan C# puro. `Unity` conecta los datos, las escenas, la entrada y la presentación. El proyecto dispone de ensamblados separados para Editor, Edit Mode y Play Mode.
- La escena de inicio carga la escena base de *La primera aula*. Su definición editable configura un presupuesto de 100 créditos y una reserva mínima del 20 % para el objetivo de excelencia. [Startup.unity](../Assets/EcoElectry/Content/Scenes/Startup.unity) · [Nivel 01](../Assets/EcoElectry/Content/Scenes/Level01-LaPrimeraAula.unity)
- `SampleScene` sigue disponible como referencia en el Editor. El sandbox reutiliza modelos y prefabs del proyecto y los archivos originales de `Assets/ResourcePack/` se conservaron. [SampleScene.unity](../Assets/ResourcePack/Scenes/SampleScene.unity)

## Sandbox eléctrico

Abre [SandboxElectrical.unity](../Assets/EcoElectry/Content/Scenes/SandboxElectrical.unity) y pulsa **Play**. Desde el panel lateral se instalan el panel solar, la batería y la luminaria en sus anclajes. El taller permite trazar y retirar cables fotovoltaicos e interiores, validar bornes y rutas, preparar y fijar extremos y medir continuidad con el circuito desenergizado. Los recorridos ocultos se dibujan con líneas punteadas.

**Probar** simula la jornada con unidades ficticias y evalúa si la luminaria presta servicio continuo de 18:00 a 20:00. El informe indica la causa del fallo, la hora de interrupción y el tramo afectado cuando corresponde. Al cerrar un informe fallido se guarda un punto de control en memoria. Si se manipula una conexión bajo tensión, el jugador reaparece cerca del tablero y recupera el estado guardado de la instalación y la tarea pendiente. [Definición del sandbox](../Assets/EcoElectry/Content/Definitions/Levels/SandboxElectrical.asset)

| Caso | Resultado comprobable |
| --- | --- |
| Instalación desde cero | Los tres enlaces pueden trazarse, conectarse y verificarse antes de superar la prueba de iluminación. |
| Conexión abierta | El multímetro indica falta de continuidad. Fijar el extremo suelto permite repetir la prueba con el circuito corregido. |
| Cortocircuito | La protección abre el ramal afectado. Tras aislar las fuentes, el jugador puede corregir el contacto y rearmarla. |
| Reserva insuficiente | La luminaria estándar deja de funcionar a las 19:00. La variante eficiente permite completar la franja. |

## Colocación de modelos y cámara

La barra inferior **Modelos** incluye 122 prefabs, con navegación por familias y páginas. La grilla del suelo tiene 30 × 16 celdas de 0,5 × 0,5 m. La huella se calcula a partir de las dimensiones del modelo. Por ejemplo, **Panel Solar 1** ocupa 3 × 2 celdas. Los modelos de gran tamaño se reducen proporcionalmente para que quepan en el taller.

La vista previa muestra si la huella cabe en la grilla y si las celdas están disponibles. Haz clic para colocar la pieza, pulsa **R** para girarla 90°, **Esc** para cancelar o **Deshacer** para retirar la última colocación. La barra se usa para componer el escenario. Los equipos que participan en la simulación se montan desde el panel eléctrico. **Reiniciar caso** restaura la instalación inicial y retira las piezas colocadas en la grilla. [Catálogo de modelos](../Assets/EcoElectry/Content/Definitions/Construction/GridModelCatalog.asset)

El botón **Cámara** y la tecla **C** alternan entre primera y tercera persona. En las intervenciones eléctricas, la cámara pasa automáticamente a primera persona y recupera la vista anterior al salir. Se recorre el taller con **WASD** y se mira arrastrando con el botón derecho. [Guía para reproducir los casos](../README.md#sandbox-eléctrico)

## Verificación registrada

| Verificación | Resultado |
| --- | --- |
| Edit Mode | 10 pruebas aprobadas. Cubren la configuración base, las reglas de cableado y simulación, la recuperación, la ocupación de celdas y las referencias del catálogo. |
| Play Mode del sandbox | 3 pruebas aprobadas. Cubren la carga de la escena, el montaje eléctrico, la descarga y reaparición, y la colocación en grilla con cambio de cámara, deshacer y reinicio. |
| Revisión manual en Unity | Se comprobó el informe de conexión abierta, el filtrado por familias, la colocación con clic y el botón de cámara en Game View. La escena se validó sin scripts faltantes, prefabs rotos ni errores de consola. |

## Alcance pendiente

*La primera aula* dispone de una escena base. Quedan pendientes su tutorial con diálogos y tareas, la integración del recorrido eléctrico en el nivel, la progresión de campaña y el guardado persistente. El punto de control del sandbox funciona en memoria durante la sesión de juego. [Diseño de La primera aula](Nivel01-LaPrimeraAula.md) · [Arquitectura de guardado](Arquitectura.md#guardado-y-recuperación)
