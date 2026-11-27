# Nivel 0 · Etapa 2 · Conducción nocturna y llegada a pie

La salida manual con **E** descrita aquí corresponde a la etapa 2. La [etapa 4](Nivel_0_Etapa_4.md) la sustituye por una llegada cinematográfica que termina directamente a pie.

Implementar en `Assets/EcoElectry/Content/Scenes/Level00-Introduction.unity` el recorrido **conducir desde el inicio de la carretera → detenerse en la entrada del colegio → bajar con E → continuar a pie**. Toda la escena transcurre de noche y ambas perspectivas son en primera persona. El atardecer dinámico queda para una etapa posterior. [Contexto del nivel 0](Nivel_0_Contexto.md) · [Etapa 1](Nivel_0_Etapa_1.md) · [Integración de la cámara de horror](../Integracion-Camara-Pasillos.md)

## Punto de partida

El mapa construido contiene una camioneta estática `ElectricTruck_Reference_Scale1`, los anclajes `VehicleStart`, `VehicleParking` y `OnFootExit`, dos farolas de estacionamiento y `CasetaVigilancia`. El montaje actual también incluye un explorador con avatar y luz diurna. Editar la escena existente para integrar esta etapa. El constructor de la etapa 1 vuelve a montar el jugador y la iluminación cuando se ejecuta, por lo que la integración debe quedar guardada en la escena y poder probarse abriéndola directamente.

| Estado | Cámara y entrada | Iluminación del jugador |
| --- | --- | --- |
| **Conduciendo** | Cámara dentro de la cabina, al nivel del conductor. Se leen los controles del vehículo. **E** se habilita en la zona de llegada con el vehículo detenido. | Faros delanteros encendidos. El componente y la acción de la linterna a pie quedan inactivos. |
| **A pie** | Cámara de primera persona del prototipo. **WASD** mueve al jugador y el ratón orienta la vista. El vehículo queda estacionado y deja de recibir entrada. | **F** alterna la linterna. Como ajuste inicial, los faros se apagan al salir y el estacionamiento queda iluminado por sus farolas y la caseta. |

## 1. Preparar la camioneta y la conducción

1. Reutilizar el [prefab de la Electric Truck](../../Assets/EcoElectry/ThirdParty/Ezereal/ElectricTruck/Prefabs/Electric%20Truck.prefab) y dejar una sola instancia de la camioneta. Moverla al anclaje `VehicleStart`, orientada con la carretera. Ajustar su altura inicial para que las cuatro ruedas apoyen sobre la calzada. Restaurar la física dinámica y la gravedad que desactivó el montaje estático de la etapa 1.
2. Revisar sus `WheelCollider`, `Rigidbody`, referencias visuales de ruedas y componentes serializados. El conjunto de Ezereal que se encontró en el repositorio incluye el modelo y las ruedas físicas. Verificar qué controlador está realmente disponible y completar la conducción con un adaptador o controlador propio bajo `Runtime/Unity/` si el prefab carece de lógica de entrada. Mantener una sola lógica de dirección, motor y freno activa sobre las ruedas.
3. Con Input System, comenzar con **W** para acelerar, **S** para frenar y retroceder desde reposo, y **A/D** para girar. Aplicar fuerzas y frenos en el paso de física. Ajustar aceleración, giro y límite de velocidad en la ruta real hasta que sea posible tomar las curvas y frenar para entrar al colegio. Comprobar que los roces con barreras y las uniones entre tramos no vuelquen ni dejen atrapada la camioneta durante una maniobra normal.
4. Montar una cámara de cabina en un anclaje fijo a la altura de los ojos del conductor. Ajustar campo visual y plano cercano para que se lean el parabrisas, parte del tablero y las barreras al girar. La pose debe seguir la camioneta con estabilidad entre pasos de física. El balanceo de marcha del explorador se aplica solo a pie.

## 2. Bajar del vehículo con E

Delimitar una zona de llegada alrededor de `VehicleParking`. Permitir **E** únicamente cuando la camioneta esté dentro de la zona y prácticamente inmóvil. Usar como punto de partida un umbral de **0,5 m/s** y ajustarlo en pruebas. Mostrar **«E · Bajar del vehículo»** mientras la salida esté disponible. El marcador indica dónde orientar el estacionamiento y admite varias poses válidas dentro de la zona.

Antes de confirmar la salida, comprobar que haya suelo y espacio libre para la cápsula junto a la cabina. Usar `OnFootExit` como alternativa cercana si la puerta del conductor queda obstruida y el marcador es alcanzable desde el estacionamiento. Si ambas salidas están bloqueadas, mantener el modo de conducción y pedir al jugador que vuelva a maniobrar.

Al confirmar, aplicar el freno de estacionamiento, deshabilitar los controles de conducción y colocar el explorador en el punto validado mirando hacia la entrada. Transferir la imagen, el `AudioListener` y la entrada en la misma transición. Activar el control de linterna después de habilitar la vista a pie. Una pulsación larga de **E** produce una sola salida.

## 3. Integrar la cámara del prototipo

Reutilizar `HorrorCorridorExplorer` y la jerarquía de cámara descrita en la [guía de integración](../Integracion-Camara-Pasillos.md). Montar la vista de cabina y la del explorador como modos distintos de primera persona, con una sola cámara jugable y un solo `AudioListener` activos en cada modo. El explorador a pie usa `CharacterController`, con el cuerpo representado por la cápsula de colisión.

- **Cabina.** Usar el perfil visual del prototipo como base para el grano, el viñeteado y el color. Ajustar la distancia de visión a la carretera y comprobar la vista desde dentro del parabrisas. Mantener estable el haz de los faros durante la conducción.
- **A pie.** Configurar `Controls`, `View`, `Flashlight`, volumen y máscara de cámara según la guía. Adaptar el plano lejano, la exposición y la niebla al exterior. Mantener el balanceo del prototipo al caminar. Usar un perfil propio del nivel si necesita ajustes distintos.
- **Linterna y ciclo de vida.** Mantener deshabilitada tanto la luz portátil como su acción **F** mientras se conduce. `HorrorCorridorExplorer` enciende la linterna al inicializarse, por lo que el estado de salida debe quedar definido explícitamente. Para la primera prueba, proponer que empiece apagada y que **F** la encienda tras bajar.

## 4. Configurar la noche, los faros y el acceso

Convertir la iluminación diurna actual en una noche fija. Usar ambiente frío de baja intensidad para conservar la silueta de la fachada y los bordes de la carretera. Ajustar la niebla con la escena activa y desde la cabina. La calzada debe ser legible a la velocidad de prueba, incluso en una curva entre árboles.

| Luz | Montaje y comprobación |
| --- | --- |
| **Faros de la camioneta** | Revisar los anclajes `Headlight Low Beam Left/Right`. Reutilizar sus focos si están disponibles o montar dos `Spot` en su posición. Apuntarlos hacia la calzada, con un área de solapamiento delante del auto. Deben iluminar el primer giro desde el inicio y acompañar el auto sin filtrarse por el tablero, los taludes ni las barreras. |
| **Farolas del colegio** | Añadir luz desde las luminarias que ya están colocadas en el mapa. Priorizar los postes del estacionamiento y el recorrido hacia la entrada. Combinar emisión visible en la luminaria con una luz que realmente alcance el suelo. |
| **Caseta de vigilancia** | Colocar una luz cálida interior visible a través de la ventana y ajustar su alcance para que la caseta destaque desde el estacionamiento. La zona peatonal debe seguir siendo reconocible después de apagar los faros. |

Probar con la calidad **PC** y observar cómo se comportan las sombras cuando los faros y las farolas coinciden. Mantener la escena nocturna desde la aparición hasta el final del recorrido a pie.

## Criterios de aceptación

| Prueba en Play Mode | Resultado esperado |
| --- | --- |
| Abrir directamente la escena | Se inicia al principio de la carretera, dentro de la camioneta y con los faros encendidos. |
| Conducir hasta el colegio | Se puede acelerar, girar, frenar y maniobrar. La vista permanece en la cabina y los faros permiten seguir la ruta. **F** no activa una linterna portátil. |
| Pulsar **E** en carretera, en movimiento y en la zona de llegada | La salida solo se permite en una posición válida y con la camioneta detenida. La transición coloca al jugador en suelo libre y deja el vehículo inmóvil. |
| Probar una salida obstruida | El jugador aparece en una alternativa cercana válida o permanece al volante con indicación para maniobrar. |
| Caminar desde el estacionamiento hasta la caseta | La cámara a pie conserva el estilo del prototipo. **F** enciende y apaga la linterna y solo hay una cámara y un `AudioListener` activos. |
| Revisar el estacionamiento y recargar la escena | Farolas y caseta iluminan el entorno. La noche se mantiene y la recarga devuelve al jugador al inicio dentro del vehículo. |
