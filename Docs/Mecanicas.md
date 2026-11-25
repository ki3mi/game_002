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
| **Verificar** | Prueba el suministro tras la intervención y revisa los servicios del encargo. | Las tareas resueltas se marcan como completadas. La sección se supera cuando se verifican todas las reparaciones obligatorias. |

Una pieza necesaria tiene una pista que acota la búsqueda. Por ejemplo, el tablero revela que falta un conector y una nota en el aula contigua señala el armario donde lo escondieron. El tablero puede ocupar una sola pared y aun así dar una razón para recorrer la sección antes de terminar el montaje.

## Cámara y linterna

El jugador explora y repara en primera persona. El movimiento de cámara sigue los pasos con un balanceo leve que se estabiliza al detenerse. Cuando oye algo extraño, puede dejar de manipular el tablero, girarse, apuntar la linterna y después retomar la tarea. El prototipo de pasillos ofrece marcha con **WASD**, mirada con el ratón y encendido o apagado de la linterna con **F**. [Prueba del prototipo](Prueba-Prototipo-Pasillos.md)

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
