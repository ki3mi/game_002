# Nivel 1 · Plan de implementación de la linterna y el miedo

El [documento de correcciones](Nivel_1_Correcciones_Linterna_Multimetro.md) especifica el multímetro interactivo y las pruebas de cierre de esta etapa.

Integrar en `Level01.unity` una linterna con dos ranuras, búsqueda de baterías y un medidor de miedo sensible a la pérdida de luz. Las reglas se reutilizan en futuros niveles. Cada escena configura su inventario inicial y la ubicación de los repuestos. [Mecánicas de linterna y baterías](../Mecanicas.md#linterna-y-baterías) · [Arquitectura](../Arquitectura.md)

## Reglas de la primera entrega

| Situación | Linterna | Miedo |
| --- | --- | --- |
| **Iniciar una partida nueva** | Una batería inicial llena, equivalente a dos de las cuatro barras, y una ranura vacía. | Empieza en rango seguro. El medidor queda oculto. |
| **Encender con F** | Consume carga de una batería. Si se agota y la otra tiene carga, cambia automáticamente de ranura. | Baja gradualmente mientras hay un haz operativo. |
| **Apagar con F o quedarse sin carga** | El haz se apaga. El consumo se detiene. | Sube gradualmente mientras el jugador permanece sin el haz. |
| **Abrir el compartimento para cambiar una batería** | El haz se suspende durante la inspección. Elegir, medir o consultar el manual conserva ambas cargas. | Sube durante todo el proceso. Al salir conserva el valor alcanzado. |
| **Parpadeo visual breve** | El efecto modula el haz durante un instante y conserva el estado de encendido. | El parpadeo por sí solo no activa el aumento por quedarse sin luz. |

La subida se calcula por tiempo de juego, de modo que pulsar **F** varias veces no acumula penalizaciones instantáneas. Mostrar el medidor cuando el miedo alcance el umbral de alerta y ocultarlo cuando baje a un umbral seguro inferior. Esta separación evita que la barra parpadee cuando el valor fluctúa cerca del límite. Como punto de partida para pruebas, usar una escala de **0 a 100**, mostrar a partir de **12**, ocultar al bajar a **6**, subir **3 puntos/s** sin haz y recuperar **5 puntos/s** con el haz. Mantener estas cifras en configuración. A **100**, emitir una sola señal de desmayo para que la sesión resuelva la derrota. La pausa general detiene consumo y miedo. La inspección de inventario mantiene el tiempo activo.

## Separación de responsabilidades

| Capa | Qué implementa |
| --- | --- |
| **Dominio** | Estado de dos ranuras, carga individual, batería activa, encendido solicitado, consumo y reglas de sustitución. El estado de miedo recibe el tiempo transcurrido y la disponibilidad del haz, aplica los límites y resuelve cuándo mostrar la barra o emitir el desmayo. Estas reglas se prueban en C# sin referencias a objetos de escena. |
| **Aplicación** | Coordina las acciones de encender, recoger, inspeccionar, medir, confirmar y cancelar. Es la responsable de transferir la batería entre mundo, inventario y ranura, y de enviar al sistema de miedo el estado funcional del haz. |
| **Unity** | Conecta Input System, luz, visor 3D, HUD y objetos recogibles. El HUD lee el estado de la sesión. El controlador de cámara conserva movimiento y mirada. |
| **Configuración de escena** | Define si el sistema está habilitado, su configuración inicial, los objetos recogibles y las referencias al explorador y al HUD. La misma configuración se puede asignar a otra escena con distintos repuestos. |

Conservar una sola acción de encendido por jugador. `HorrorCorridorExplorer` controla actualmente **F** y el foco de su linterna. En la integración del sistema, dirigir esa acción al estado de la linterna y usar el foco para representar el resultado. El parpadeo del prototipo actúa sobre la presentación. Si se cambia de escena dentro de una partida, transferir las cargas, las ranuras y el miedo existentes a través de la sesión. La carga inicial se aplica al crear una partida nueva o reiniciar deliberadamente una prueba. [Explorador del prototipo](../../Assets/EcoElectry/Runtime/Unity/Player/HorrorCorridorExplorer.cs)

## Orden de implementación

1. **Carga y luz.** Implementar el consumo por segundo, la transferencia entre ranuras y el apagado por agotamiento. Conectar **F** y mostrar la carga total sobre cuatro barras. Verificar primero con la única batería inicial.
2. **Miedo y visibilidad.** Conectar el estado funcional del haz, el consumo y el tiempo activo de la sesión. Añadir los dos umbrales del HUD y las señales de cambios de estado. Actualizar carga y miedo desde un único avance de sesión para que los resultados se mantengan al variar los FPS.
3. **Recogida.** Montar un prefab reutilizable de batería que pueda recogerse en el suelo o aparecer como contenido de un objeto interactuable. Cada batería conserva su identidad y carga al pasar del mundo al inventario. En el Nivel 1, colocar al menos un repuesto temprano en el recorrido. La batería inicial se va usando antes de encontrar e instalar la segunda, lo que genera diferencias de carga de forma natural.
4. **Inspección y recambio.** Abrir el visor 3D con el haz suspendido. Mostrar las ranuras ocupadas y permitir elegir una libre o reemplazar una instalada. Si hay dos, habilitar la medición opcional en **V⎓** y el manual lateral. Derivar lecturas repetibles de la carga individual simulada. Confirmar el cambio en una sola operación. Retirar un repuesto del inventario, instalarlo con su carga y descartar definitivamente la batería retirada. Cancelar conserva inventario y ranuras. Al salir, recuperar el estado del interruptor previo y encender el haz si corresponde y hay carga.
5. **Integrar en el mapa y probar la reutilización.** Probar en `Level01.unity` con baterías en el corredor, bajo un mueble y dentro de una caja. Montar la misma linterna, el HUD y los recogibles en una escena de prueba independiente mediante referencias y configuración. Comprobar que las reglas funcionan con otros escondites y tiempos de consumo.

## Criterios de aceptación

| Prueba | Resultado esperado |
| --- | --- |
| **Inicio y consumo** | Se empieza con dos barras y una ranura vacía. Solo se descarga la batería activa. Al agotarse, la segunda mantiene el haz si tiene carga. |
| **Oscuridad y recuperación** | Apagar con **F**, agotar toda la carga o entrar en recambio eleva el miedo. Al encender, baja. La barra aparece en alerta, se oculta al recuperar el rango seguro y reaparece en un nuevo episodio. |
| **Cambio y cancelación** | Una ranura vacía acepta un repuesto. Seleccionar una ocupada descarta solo la batería elegida al confirmar. Cancelar conserva las baterías y el repuesto, además del miedo acumulado. |
| **Medición opcional** | Dos baterías con cargas distintas mantienen lecturas reproducibles al medirlas varias veces. El manual se consulta con el haz apagado y la elección también se puede confirmar directamente. |
| **Pausa y parpadeo** | La pausa congela carga y miedo. El parpadeo visual conserva el estado funcional del haz. Cuando el miedo llega a 100, la derrota se notifica una sola vez. |
| **Reutilización** | La misma lógica funciona en dos escenas con configuraciones y recogibles distintos. Al continuar una partida en otra escena, se recupera el estado de sus baterías y del miedo. |
