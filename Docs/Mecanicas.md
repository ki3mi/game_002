# Mecánicas de Just a Wiring Job?

El jugador explora secciones del colegio y repara sus averías antes del amanecer. [Premisa y dirección artística](Context.md)

## Llegada e inventario

- Durante la conducción inicial, el jefe comunica el contexto y los objetivos del encargo por WhatsApp dentro del juego. Las notas de voz permiten seguir las indicaciones durante el trayecto.
- Al estacionar, el jugador abre el inventario, gira el llavero en 3D y confirma al jefe que lo tiene. Usa las llaves para entrar. La misma inspección permite reconocer las piezas que encuentre durante el turno.

## Ciclo de una sección

| Tarea | Acción del jugador | Resultado |
| --- | --- | --- |
| **Diagnosticar** | Recorre la sección, comprueba qué servicio falló e inspecciona el punto de intervención. | Identifica las conexiones pendientes y las piezas que necesita recuperar. |
| **Buscar piezas** | Sigue pistas hacia otras aulas, armarios o dependencias de la misma sección. | Recupera componentes que los estudiantes escondieron durante el día y los reconoce en el inventario. |
| **Reparar** | Instala las piezas y empareja cables por color y etiqueta en primera persona. | Cada conexión correcta queda hecha aunque el jugador interrumpa el trabajo para mirar a su alrededor. |
| **Verificar** | Con el colegio desenergizado, realiza la comprobación que pide el encargo y registra la lectura. | Las tareas resueltas se marcan como completadas. La sección se supera cuando se verifican todas las reparaciones obligatorias. |

El [diseño de reparación del Nivel 1](Nivel%201/Nivel_1_Mecanicas_Reparacion.md) recoge el tutorial en mantenimiento, la inspección 3D y las comprobaciones disponibles.

Una pieza necesaria tiene una pista que acota la búsqueda. Por ejemplo, el tablero revela que falta un conector y una nota en el aula contigua señala el armario donde lo escondieron. El tablero puede ocupar una sola pared y aun así dar una razón para recorrer la sección antes de terminar el montaje.

## Cámara en primera persona

El jugador explora y repara en primera persona. El movimiento de cámara sigue los pasos con un balanceo leve que se estabiliza al detenerse. Cuando oye algo extraño, puede dejar de manipular el tablero, girarse, apuntar la linterna y después retomar la tarea. El prototipo de pasillos ofrece marcha con **WASD**, mirada con el ratón y encendido o apagado de la linterna con **F**. [Prueba del prototipo](Prueba-Prototipo-Pasillos.md)

## Inspección de objetos en 3D

El objeto ocupa el centro del visor, iluminado sobre un fondo oscuro. El jugador puede **rotarlo, acercarlo, restablecer la vista y salir**. Las acciones aparecen en una fila inferior y el nombre y la descripción breve del objeto en una ficha lateral. La interfaz usa la estética del colegio, con fichas de mantenimiento, grano sutil y una tipografía legible.

La rotación permite descubrir referencias, etiquetas, cierres y ranuras. Cuando un punto interactivo queda a la vista, se muestra la acción correspondiente, como abrir el pestillo de una caja o seleccionar una batería. Para la linterna, la ficha lateral deja espacio para las lecturas del multímetro y la guía de voltaje. El medidor puede aparecer durante el cambio si el miedo alcanza el umbral de alerta.

## Linterna y baterías

La linterna funciona con **una o dos baterías instaladas**. El jugador empieza con una batería llena y una ranura vacía. Puede encenderla y apagarla con **F** y llevar repuestos en el inventario. Usar la primera antes de instalar la segunda genera diferencias de carga que puede comparar más adelante. [Plan de implementación](Nivel%201/Nivel_1_Plan_Linterna_Miedo.md)

### Carga e indicador

- El indicador muestra **cuatro barras de carga total**. Cada batería nueva aporta dos barras. Una ranura vacía o agotada aporta cero. La interfaz muestra la suma sin asociar cada barra a una ranura, para que medir una batería aporte información útil.
- La linterna consume **una batería a la vez** mientras está encendida. Al agotarse la activa, cambia automáticamente a la otra si tiene carga. Cuando se agotan todas, el haz se apaga. Apagar la linterna conserva la carga restante.
- Cada batería conserva su propia carga. Cambiar la de una ranura recalcula el total. La vista de inspección muestra qué ranuras están ocupadas y permite consultar el voltaje de cada batería mediante una medición opcional.

Por ejemplo, con una batería llena y otra a media carga, el indicador muestra **tres barras**. Si se reemplaza la batería a media carga por una nueva, vuelve a mostrar **cuatro**. La batería retirada se descarta con la carga que le quedaba.

### Búsqueda e interacción con el entorno

En el **Nivel 0** se explica que los estudiantes jugaron con las baterías y las dejaron repartidas por el colegio. En el **Nivel 1** esa información se da por conocida y el jugador empieza a buscar repuestos durante sus recorridos.

| Escondite | Interacción |
| --- | --- |
| **Cajuelas y compartimentos** | Abrir el compartimento y recoger la batería si está dentro. |
| **Debajo de muebles** | Acercarse, mirar debajo y recogerla cuando quede a la vista. |
| **Estantes y entre libros** | Revisar los espacios visibles entre objetos hasta encontrarla. |
| **Cajas** | Entrar en la vista de inspección 3D, rotar la caja hasta localizar el cierre e interactuar con él para abrirla y recoger el contenido. |
| **Pasillos** | Recoger directamente las baterías que quedaron esparcidas en el suelo o junto a una pared. |

El mapa debe incorporar objetos interactuables para abrir, inspeccionar o recoger según el escondite. Una caja cerrada oculta la batería hasta que el jugador encuentre y accione su cierre. Al recoger una batería, desaparece de su ubicación y entra al inventario como repuesto.

### Colocación y reemplazo

1. **Iniciar.** Con un repuesto disponible, el jugador elige cambiar la batería. El haz se apaga temporalmente, se abre la vista de inspección 3D de la linterna y se muestran las ranuras.
2. **Elegir la ranura.** Si hay dos baterías instaladas, el jugador selecciona cuál retirar. Si solo hay una, puede reemplazarla o usar la ranura libre para llevar dos. Colocar una batería en una ranura libre conserva la que ya estaba montada.
3. **Confirmar.** Se usa un repuesto del inventario y se instala en la ranura elegida con dos barras de carga. **Toda batería retirada se elimina de forma definitiva**, aunque aún tenga energía. La otra batería conserva su carga.
4. **Volver al juego.** Se cierra la inspección y se actualiza el indicador. Si la linterna estaba encendida al iniciar el cambio y ahora tiene carga, el haz vuelve a encenderse. Si estaba apagada, el jugador puede encenderla con **F**. Cancelar antes de confirmar restaura el estado anterior y conserva el inventario y las baterías instaladas.

La duración de la carga y la cantidad de repuestos de cada sección se ajustarán con pruebas de recorrido.

El miedo sube lentamente mientras la linterna permanece apagada, ya sea por decisión del jugador, por agotamiento o durante un recambio. Medir las pilas y consultar el manual prolonga ese intervalo. Al volver a tener un haz operativo, el miedo desciende gradualmente desde el valor acumulado. El medidor aparece al llegar al umbral de alerta y se oculta al bajar al rango seguro. Puede volver a aparecer durante un episodio posterior.

### Comprobación opcional con el multímetro

Si hay dos baterías, el jugador puede elegir directamente cuál retirar o compararlas antes de confirmar. En la vista de inspección, la linterna permanece apagada. El jugador selecciona **voltaje de corriente continua (V⎓)** y el juego permite medir cada celda por separado. La pantalla conserva ambas lecturas junto a sus ranuras hasta que se cierra la inspección. Medir toma unos segundos y deja intactos la carga y el inventario.

Usar celdas del mismo tipo. Junto al objeto aparece un **manual de la linterna** que permite consultar qué porcentaje aproximado corresponde a cada intervalo de voltaje. El instrumento muestra la lectura en voltios y el jugador puede contrastarla con el manual o elegir directamente la batería que quiere retirar.

| Lectura de ejemplo | Estimación inicial para calibrar el juego |
| --- | ---: |
| **1,42 V** | **50–75 %** |
| **1,18 V** | **10–25 %** |

Los intervalos se ajustan a las pilas simuladas del juego. En una pila real, el voltaje sin carga solo permite estimar su estado de forma aproximada. [Manual de pilas alcalinas de Energizer](https://energizer.com/wp-content/uploads/2023/12/alkaline_appman.pdf)

## Anomalías estáticas y reacción

Una anomalía estática aparece como una silueta difusa en la oscuridad, por ejemplo tras el vidrio de una puerta. Un sonido puede advertir de su presencia mientras el jugador está concentrado. La ventana de reacción empieza cuando fija la mirada en la silueta.

| Reacción | Desenlace | Efecto |
| --- | --- | --- |
| **Apunta a tiempo** | Justo antes de que el haz alcance la silueta, la linterna parpadea. Al volver la luz, la anomalía ha desaparecido. | Baja un poco el miedo. |
| **Mira demasiado tiempo** | Toda la cámara se oscurece un instante, como un parpadeo involuntario. Al volver la visión, la anomalía ha desaparecido. | Sube un poco el miedo. |

La aparición permanece en su lugar hasta resolverse. La presión proviene de cuánto tarda el jugador en reaccionar y de lo que deja pendiente mientras investiga. La duración de la ventana y la magnitud de los cambios de miedo se ajustarán en pruebas. Los demás tipos de anomalías quedan por definir.

## Miedo, pánico y fin del turno

El **medidor de miedo** sube al ignorar señales y al mirar una anomalía durante demasiado tiempo. Revisar una aparición a tiempo permite recuperar parte de la calma. Al subir el miedo, el pulso se hace más audible. En pánico, la visión pierde claridad y el temblor dificulta apuntar a un borne o elegir un cable. Los colores y las etiquetas deben seguir siendo legibles bajo la luz de trabajo.

- **Jornada completada.** El jugador verifica todas las reparaciones obligatorias antes de las **06:00**. La primera jornada comienza a las **18:00** y las siguientes comienzan a las **17:00** del día siguiente.
- **Derrota por desmayo.** El miedo llega al máximo antes de terminar el trabajo.
- **Derrota por tiempo.** Llegan las **06:00** con reparaciones obligatorias pendientes.

El reloj de la jornada avanza durante la exploración, la búsqueda de piezas, las reparaciones y las comprobaciones. Interrumpir una conexión para mirar alrededor conserva el montaje hecho y consume parte del tiempo del turno.
