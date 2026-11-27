# Nivel 0 · Etapa 3 · Del atardecer a la noche

La conducción empieza a las **16:00** en el tercer chunk de la carretera y el colegio se alcanza a las **18:00**, cuando comienza la noche. Durante el trayecto, el cielo, la luz ambiental y la niebla cambian con el avance por la carretera. A partir de la llegada, el entorno sigue oscureciéndose progresivamente durante el tramo a pie. [Contexto del nivel 0](Nivel_0_Contexto.md) · [Conducción y llegada](Nivel_0_Etapa_2.md)

## Distancia y tiempo de referencia

Tomar como tercer chunk `Chunk_01_02`, al contar desde `Chunk_01_00`. Empieza en `Z = 500 m`. Para la medición, propongo mover `VehicleStart` a **`Z = 505,5 m`**, unos metros dentro del chunk. Colocar la camioneta en su carril sobre la calzada y orientarla según la tangente del camino. La coordenada `Z` identifica la sección transversal donde se coloca el inicio. Para medir la longitud se sigue el eje curvo de la carretera.

Recalculé el eje con los puntos de control del constructor y su muestreo cada **4 m**, que da **3.529,6 m** en total hasta el portón. Desde el comienzo del tercer chunk hay **3.029,0 m** hasta el portón. Desde el punto de aparición propuesto hay **3.023,5 m** hasta el portón, más **unos 40 m** de referencia por el acceso al estacionamiento. [Constructor de la carretera](../../Assets/EcoElectry/Editor/Levels/Level00IntroductionBuilder.cs)

| Tramo desde la aparición propuesta | Distancia | Tiempo teórico a 120 km/h |
| --- | ---: | ---: |
| Hasta el portón | 3.023,5 m | 90,7 s |
| Hasta `VehicleParking`, prolongando el eje por el acceso | ≈ 3.063,5 m | ≈ 91,9 s · **1 min 32 s** |

**120 km/h = 33,33 m/s.** El cálculo usa `tiempo = distancia / velocidad`. Los **91,9 s** sirven para calibrar la escena. El viaje jugado durará más al acelerar, frenar y maniobrar en el acceso.

## Reloj ligado al recorrido

1. Exponer en la escena el eje de la carretera con distancia acumulada y prolongarlo por el acceso hasta `VehicleParking`. Usar el mismo trazado que genera la calzada y conservar la referencia al actualizar el mapa.
2. Proyectar la posición de la camioneta sobre ese eje. Calcular el avance como la fracción de distancia entre `VehicleStart` y el final del acceso. Conservar el mayor avance válido alcanzado. Detenerse mantiene la hora y retroceder conserva el punto de atardecer alcanzado. Limitar la proyección a los tramos cercanos al vehículo para evitar saltos entre curvas.
3. Aplicar **`hora = 16:00 + 120 minutos × avance`**. La interpolación visual puede suavizarse entre hitos, con la misma referencia de hora para cielo y luces. Al validar la llegada al estacionamiento, fijar las **18:00** como fin del tramo por distancia e iniciar la fase nocturna una sola vez. Si se usa un salto de prueba al estacionamiento, empezar también en ese punto de la transición.

| Hora de juego | Avance | Distancia desde el inicio | Tiempo de referencia a 120 km/h | Resultado visual |
| --- | ---: | ---: | ---: | --- |
| **16:00** | 0 % | 0 m | 0 s | Atardecer cálido, horizonte ámbar y árboles aún legibles. |
| **16:45** | 37,5 % | ≈ 1.149 m | ≈ 34,5 s | La luz directa cae y el bosque empieza a enfriarse. |
| **17:20** | 66,7 % | ≈ 2.042 m | ≈ 61,3 s | Crepúsculo azul. Los faros resaltan el asfalto y los reflectantes. |
| **17:50** | 91,7 % | ≈ 2.808 m | ≈ 84,2 s | Queda una franja tenue en el horizonte y la fachada se distingue como silueta. |
| **18:00** | 100 % | ≈ 3.063 m | ≈ 91,9 s | Comienza la noche. Aún se distinguen el cielo y las copas de los árboles. Farolas y caseta orientan la llegada. |

## Oscurecimiento después de las 18:00

Al llegar, pasar de un avance por distancia a un avance por **tiempo de juego transcurrido desde la llegada**. La curva nocturna empieza con los mismos valores de cielo, ambiente, reflejos y niebla del último instante de conducción. Sigue avanzando durante la cinemática de llegada y cuando el jugador camina hacia la caseta o se queda quieto. Cambiar de cámara o volver a entrar en la zona de llegada conserva el avance acumulado. [Cinemáticas del nivel 0](Nivel_0_Etapa_4.md)

Como punto de partida, probar **120 segundos de juego activo** desde la llegada hasta la noche más oscura y dejar la duración configurable. A mitad de ese intervalo, el horizonte pierde casi todo el brillo. Al terminar, el cielo llega a su mínimo de luminancia y se mantiene allí. Ajustar la curva comparando las vistas desde el estacionamiento, la caseta y el camino a mantenimiento. Este temporizador controla el entorno del prólogo. El reloj de reparaciones se inicia al entrar al nivel 1, según el [contexto de la introducción](Nivel_0_Contexto.md).

## Adaptación del ciclo existente

Reutilizar `IntroductionSkyCycle` y el shader `IntroductionSkyBlend`. Hoy el componente recorre nueve fases que se repiten en un ciclo de 12 minutos reales. Configurar el nivel 0 con dos fases consecutivas, **atardecer por avance en carretera** y **oscurecimiento nocturno por tiempo desde la llegada**. Aplicar el primer estado antes del primer fotograma jugable y mantener el valor final de oscuridad hasta salir de la escena. El constructor nocturno también debe asignar esta configuración al volver a montar el nivel. [Controlador del cielo](../../Assets/EcoElectry/Runtime/Unity/Levels/IntroductionSkyCycle.cs) · [Shader de mezcla](../../Assets/EcoElectry/Content/Rendering/IntroductionSkyBlend.shader) · [Constructor de iluminación](../../Assets/EcoElectry/Editor/Levels/Level00IntroductionBuilder.Night.cs)

- **Skybox.** Probar `Cold Sunset`, `Deep Dusk` y `Cold Night` de los recursos ya conectados. Ajustar la orientación de los cubemaps para que el horizonte conserve su dirección entre mezclas. El shader actual mezcla texturas. Añadir un multiplicador de luminancia exclusivo del cielo y reducirlo progresivamente hasta el valor nocturno. Calibrar la mezcla y la luminancia juntos, porque dos texturas con nubes de brillo distinto pueden producir un repunte aunque el multiplicador baje.
- **Entorno.** Hacer descender la luz solar hasta cero, enfriar y reducir el ambiente, bajar la intensidad de los reflejos y oscurecer el color de la niebla. Mantener una contribución lunar tenue si ayuda a leer la silueta del colegio. Ajustar las reflexiones con actualizaciones espaciadas en lugar de recalcularlas en cada fotograma.
- **Luces locales.** Conservar los faros, las farolas y la caseta como fuentes de visibilidad. El oscurecimiento se aplica al cielo y a su aportación al entorno. La exposición de cámara se comprueba desde el parabrisas y a pie para que el estado final sea oscuro y el haz de cada luz siga siendo útil.

## Pruebas de aceptación

| Prueba | Resultado esperado |
| --- | --- |
| Iniciar desde el tercer chunk | La camioneta aparece orientada sobre la calzada y el primer fotograma muestra el atardecer de las 16:00. |
| Recorrer a 120 km/h de referencia | Los cambios se producen cerca de los hitos calculados. Comparar la duración con **1 min 32 s** y registrar la diferencia por frenado y maniobra. |
| Frenar o retroceder | La hora permanece en el mayor avance alcanzado y el cielo conserva la fase visual correspondiente. |
| Llegar despacio, esperar en la camioneta y bajar | Al validar la llegada empieza el oscurecimiento nocturno. El cambio de cámara conserva el mismo cielo y el oscurecimiento continúa a pie. |
| Volver al estacionamiento y esperar el final de la transición | El cielo sigue desde el punto alcanzado, llega a su luminancia mínima y permanece allí. La calzada y el acceso se leen con sus luces locales. |
| Recargar la escena | El primer fotograma vuelve a las **16:00**. Comprobar también que la transición no pasa por otras horas del ciclo completo. |
