# Nivel 0 · Etapa 4 · Cinemáticas de salida y llegada

Implementar dos secuencias en `Level00-Introduction.unity`. La primera sigue a la camioneta en movimiento y entrega el control desde la cabina. La segunda muestra el colegio al estacionarse y entrega el control a pie junto al vehículo. Ambas usan el mapa, el vehículo y la iluminación de la escena jugable. [Conducción](Nivel_0_Etapa_2.md) · [Ciclo del cielo](Nivel_0_Etapa_3.md)

## Subetapa 4.1 · Inicio de la conducción

**Duración inicial propuesta de 6–8 segundos.** La secuencia comienza a las **16:00** con la camioneta en movimiento desde `VehicleStart`, dentro del tercer chunk. Usar el eje de la carretera para guiar el vehículo durante el plano y emitir el mismo progreso que durante la conducción jugable.

| Plano | Dirección de cámara |
| --- | --- |
| **Seguimiento entre árboles** | Empezar detrás y a un lado de la camioneta, con altura suficiente para mostrar la carretera y las barreras. Acompañar su avance con un movimiento suave. Los troncos cercanos pasan por los bordes del encuadre y dejan despejada la silueta del vehículo. |
| **Acercamiento a la cabina** | Acercarse al lateral delantero manteniendo el sentido de marcha. Cortar a la vista interior antes de que la cámara exterior alcance la carrocería. Mantener continuidad en el sonido del motor y en el movimiento del paisaje tras el parabrisas. |
| **Entrega del volante** | Sostener brevemente el mismo encuadre que usará la cámara jugable. Al terminar el plano, activar la entrada de conducción cuando la camioneta aún avanza. Conservar su posición, orientación y velocidad para evitar un salto al tomar el control. |

Durante la secuencia, la entrada del jugador queda bloqueada. La animación y el controlador de conducción se turnan para mover la misma camioneta. El plano exterior funciona como una cámara cinematográfica. La conducción jugable empieza en primera persona.

## Subetapa 4.2 · Estacionamiento y llegada al colegio

**Duración inicial propuesta de 9–12 segundos.** Activar la secuencia una sola vez cuando el jugador entre despacio en la zona de llegada y se valide que la camioneta puede quedar estacionada. Usar `VehicleParking` como referencia de orientación. Si la camioneta necesita un ajuste final, animarlo desde su posición real por una trayectoria despejada. Si la maniobra queda obstruida, el jugador conserva el volante hasta colocarse en una posición válida.

| Plano | Dirección de cámara |
| --- | --- |
| **La camioneta se detiene y aparece el colegio** | Salir de la cabina con un corte a un plano exterior amplio. Acompañar la última parte de la maniobra y abrir el encuadre hacia `BuildingA`. Mostrar la fachada entre los árboles y dejar la camioneta en el cuadro para que se entienda dónde quedó estacionada. |
| **Caseta de vigilancia** | Cortar a un plano medio de `CasetaVigilancia`, con la ventana iluminada y el acceso peatonal en el encuadre. El cristal y la luz marcan el punto al que se acercará el jugador. |
| **Camino y sala de mantenimiento** | Desplazar la cámara a lo largo de `CaminoMantenimiento` y cerrar el plano en el rótulo y la puerta de `SalaDeMantenimiento`. Mostrar cómo se conecta el sendero con la caseta para que el jugador reconozca la ruta a pie. |
| **Entrega a pie** | Cortar a una vista a la altura de los ojos junto a la puerta del conductor, mirando hacia el acceso. Activar el explorador de primera persona y la acción de linterna. |

Tras la maniobra, mantener aplicado el freno durante el resto de la cinemática. Antes de entregar el control a pie, validar el suelo y el espacio para la cápsula junto a la puerta. Si ese punto está bloqueado, usar `OnFootExit` cuando sea una alternativa despejada y cercana. **El jugador aparece a pie sin pulsar E y se retira el aviso para bajar.**

La llegada fija las **18:00**. El oscurecimiento nocturno continúa durante los planos y conserva su avance al devolver el control a pie. Mantener el sonido del motor hasta que se detenga, y dar paso al ambiente del bosque y del colegio en los planos de la caseta y el sendero.

## Integración y comprobaciones

- Montar dos secuencias de **Unity Timeline** en la escena actual. Usar pistas de animación para las cámaras y señales al cierre para transferir el control. Reutilizar [la conducción](../../Assets/EcoElectry/Runtime/Unity/Player/IntroductionTruckDrive.cs) y adaptar [la transición de llegada](../../Assets/EcoElectry/Runtime/Unity/Levels/VehicleArrivalTransition.cs) para disparar la secuencia en una llegada válida y activar al explorador desde la señal final.
- Mantener cuatro estados consecutivos, **cinemática inicial → conducción → cinemática de llegada → exploración a pie**. En cada estado, comprobar que haya una sola cámara y un solo `AudioListener` activos. Durante los cortes, evitar referencias a cámaras o personajes de otras escenas.
- Verificar en Play Mode que el inicio siga a una camioneta en movimiento, que el cambio a cabina conserve la marcha y que las teclas solo conduzcan después del primer plano interior.
- Probar una llegada válida y otra con el espacio de estacionamiento o salida obstruido. La llegada válida muestra los tres puntos del colegio y termina con el jugador al lado de la camioneta, con control a pie inmediato. Pulsar **E** carece de función de desembarque y volver al estacionamiento no repite la cinemática.
- Revisar los encuadres con el cielo y las luces del nivel activo. La fachada, la caseta y la puerta de mantenimiento deben ser reconocibles en el plano y al retomar el recorrido jugable.
