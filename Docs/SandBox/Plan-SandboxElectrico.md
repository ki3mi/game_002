# Plan de desarrollo · Sandbox eléctrico de EcoElectry

**Destinatario:** agente de desarrollo Unity. **Entregable de implementación:** `Assets/EcoElectry/Content/Scenes/SandboxElectrical.unity`, una escena que se pueda abrir y probar directamente desde el Editor. Tomar como referencia la [arquitectura](Arquitectura.md), las [mecánicas](Mecanicas.md) y el [primer nivel](Nivel01-LaPrimeraAula.md).

## Alcance

Implementar el ciclo **colocar equipos → trazar y conectar cables → verificar → probar → corregir**. El sandbox debe permitir jugar el ciclo completo y aislar fallas concretas. Usar una simulación de juego con unidades ficticias. La campaña, la economía diaria, las estrellas, los diálogos, el tendido aéreo y la construcción por bloques quedan para sus propias etapas. Las superficies del sandbox pueden ser primitivas y los equipos pueden usar prefabs existentes o representaciones provisionales.

## Escena y controles

| Zona | Contenido y propósito |
| --- | --- |
| Patio | Punto inicial, acceso a la zona técnica y un anclaje para el panel solar. |
| Zona técnica | Tablero con cuchillas y protección, espacio para una batería y punto de reaparición cercano. |
| Aula de prueba | Una luminaria y anclajes para probar rutas ocultas por pared, techo y suelo, con al menos una transición entre superficies. |
| Panel de pruebas | Botones para cargar un caso, iniciar **Probar** y reiniciar. Muestra el estado del tablero, la tarea actual y el último resultado. |

En exploración, permitir el cambio manual entre primera y tercera persona. Al editar una conexión o usar una herramienta, cambiar automáticamente a primera persona y recuperar la vista anterior al salir. Documentar los controles usados en la escena o en una ayuda visible. Los tramos ocultos se dibujan únicamente con líneas punteadas. Los bornes y las conexiones accesibles se muestran como puntos de interacción.

## Orden de implementación

1. **Preparar la escena y su composición.** Crear la escena junto a `Startup.unity` y `Level01-LaPrimeraAula.unity`. Usar un punto de composición local basado en `LevelRoot` y una definición propia del sandbox. Debe funcionar al pulsar **Play** desde la escena, sin arrancar la campaña ni redirigir el inicio del juego. Reutilizar los `.asmdef` existentes de `Domain`, `Application` y `Unity`. Los prototipos de `Assets/ResourcePack/` sirven como referencia de interacción y presentación. Si se reutilizan, respetar la dirección de dependencias de la arquitectura y conservar sus `.meta`.
2. **Modelar y cablear la instalación.** En C# de dominio, representar equipos, bornes, tramos, cuchillas y protección mediante identificadores estables. En Unity, seleccionar un origen, añadir puntos de ruta, elegir un destino y previsualizar el tramo. En esta etapa se usan dos familias, fotovoltaica entre panel y equipo receptor, e interior para distribuir energía a la luminaria. Validar la compatibilidad de bornes, la ocupación y las rutas antes de confirmar. Cancelar una vista previa deja el diseño intacto. Retirar un tramo actualiza tanto el circuito como su representación.
3. **Implementar herramientas e intervención.** Introducir acciones sencillas de pelacables, destornillador y multímetro sobre puntos interactivos. Distinguir un cable trazado de uno con los extremos conectados. El multímetro devuelve continuidad en el tramo consultado o circuito abierto. La medición de continuidad se realiza con el circuito simulado desenergizado. Durante una corrección, si el jugador intenta manipular un cable o borne bajo tensión, registrar una descarga antes de aplicar el cambio y restaurar el último punto automático. El estado bajo tensión se deriva de la conectividad y de las fuentes activas, incluida la batería. Inspeccionar visualmente el tablero es una acción separada de tocar una conexión.
4. **Ejecutar la simulación e informar.** Al pulsar **Probar**, capturar el diseño y simular por franjas con el modelo de generación, consumo y almacenamiento del nivel 01. La instalación válida enciende la luminaria durante toda la franja exigida. El informe devuelve la franja y el tramo o servicio afectado ante una conexión abierta, un cortocircuito o falta de energía. Modelar el corto como contacto indebido en un tramo o borne del caso de prueba. El cruce visual de rutas aisladas conserva su funcionamiento. Ante un corto, la protección interrumpe el circuito correspondiente. El evento de descarga del jugador pertenece al momento de interacción, mientras que el disparo de la protección pertenece al circuito.
5. **Preparar la recuperación y los casos repetibles.** Cargar cada caso con un estado inicial conocido. Al cerrar un informe con correcciones pendientes, guardar automáticamente el diseño, el estado del circuito, la tarea y el punto de reaparición. La descarga restaura esa captura y muestra la causa. Para este sandbox basta un punto de control en memoria de la sesión. **Reiniciar** reconstruye el caso seleccionado y limpia el informe, las mediciones y los cambios del intento anterior.
6. **Demostrar la reutilización.** Configurar dos casos con posiciones o recorridos distintos mediante datos y anclajes, usando los mismos componentes y reglas. Mantener los valores de generación, batería y demanda en definiciones. Las acciones de `Application` las consumen Unity y las pruebas. Los botones exclusivos del sandbox se limitan a cargar casos y presentar sus resultados.

## Casos de aceptación

| Caso | Resultado verificable |
| --- | --- |
| Instalación desde cero | Se conectan panel, batería, tablero y luminaria. Se puede verificar continuidad y superar la prueba de iluminación. |
| Ruta por pared, techo y suelo | Los segmentos ocultos se dibujan con líneas punteadas. Al cancelar o retirar un tramo, desaparece su representación y el circuito refleja el estado correspondiente. |
| Conexión abierta | El multímetro indica falta de continuidad. El informe localiza el tramo y una nueva prueba funciona después de corregirlo. |
| Cortocircuito | La protección abre el circuito afectado. El informe distingue el corto de una conexión abierta. Dos rutas aisladas que se cruzan en la vista funcionan con normalidad. |
| Reserva insuficiente | La prueba indica en qué franja dejó de funcionar la luminaria y atribuye la interrupción a falta de energía. |
| Corrección bajo tensión | Tocar un borne energizado produce una descarga. El jugador reaparece cerca del tablero y recupera diseño, circuito y tarea. Tras aislar las fuentes simuladas, puede corregir y volver a probar. |
| Reinicio y reutilización | Tras cambiar de caso o reiniciar, la escena carga el estado inicial sin duplicar controles, conexiones ni suscripciones. Los dos casos usan el mismo código de instalación. |

## Cierre de la entrega

- Ejecutar pruebas **Edit Mode** para validación de conexiones, continuidad, cortocircuitos, simulación reproducible y restauración del punto de control.
- Ejecutar pruebas **Play Mode** para apertura directa, cambio de cámara, instalación válida, descarga, reaparición y reinicio.
- Entregar la escena, las definiciones y los prefabs que utiliza, junto con los resultados de prueba. Verificar que se abre sin referencias faltantes ni errores de consola. Indicar la ruta de la escena y los controles para que otra persona pueda repetir los casos.
