# Plan de desarrollo del prototipo de pasillos de EcoElectry

Crear `Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity` para evaluar cómo se siente caminar de noche por el colegio en primera persona. La escena debe abrir directamente en **Play** desde Unity, con su propia cámara, iluminación y punto de aparición. Se prueba como escena independiente del nivel 01, la campaña y el sandbox eléctrico. Usar la [arquitectura del proyecto](Arquitectura.md) para ubicar los componentes reutilizables.

## Entorno de prueba

Montar un pasillo recto de unos 12–15 m que gire hacia otro tramo más corto. Colocar una puerta cerrada, una entreabierta que deje ver parte del interior y una abierta por la que se pueda entrar a un aula pequeña. Los tres estados de puerta se preparan en el Editor para probar huecos, colisiones y encuadres. Se puede reutilizar `PuertaInterior` de `ResourcePack`, conservando su pivote, apariencia y colliders. Ajustar la pose de cada instancia para que la hoja y su colisión queden alineadas.

Usar geometría low poly, paredes claras apagadas, un suelo mate, marcos reconocibles y una ventana que reciba luz exterior fría. Colocar una luminaria tenue al inicio y dejar la esquina y el aula en sombra. El recorrido debe permitir caminar por el centro, acercarse a un muro, girar, atravesar una puerta, mirar dentro del aula y volver al inicio.

## Movimiento y cámara

- **Controles.** Usar **WASD** para caminar, **ratón** para mirar y **F** para alternar la linterna. Reutilizar `Player/Move` y `Player/Look` del Input System. `ThirdPersonExplorer` sirve como referencia del movimiento existente. Ubicar el comportamiento nuevo en `Runtime/Unity/Player/` y montar un solo controlador en el prototipo.
- **Cuerpo.** Usar un `CharacterController` y una velocidad inicial de **1,7 m/s**. Suavizar el arranque y la frenada durante una fracción de segundo. Mantener la velocidad al caminar en diagonal y dejar la mirada bajo control directo del ratón.
- **Cámara.** Situarla a la altura de los ojos, cerca de **1,65 m** desde el suelo. Separar la orientación de la mirada del desplazamiento de los pasos. Calcular un balanceo suave a partir de la distancia que el cuerpo realmente recorre sobre el suelo y aplicar la pose después del movimiento. Empezar con **1–2 cm de desplazamiento vertical** y una inclinación lateral casi imperceptible. Al parar o quedar bloqueado contra una pared, el balanceo vuelve gradualmente al centro.
- **Espacios estrechos.** Limitar el desplazamiento de la cámara cerca de obstáculos para que el balanceo quede dentro del espacio que puede ocupar el jugador. Probarlo al mirar de costado junto a un marco y al retroceder hacia un muro. Dejar la intensidad del balanceo ajustable desde el Inspector, incluido el valor cero para comparar sensaciones.

## Imagen y linterna

Crear un perfil de volumen URP para la escena. Aplicar **grano fino**, viñeteado suave y colores desaturados. Empezar con una intensidad de grano baja y ajustarla observando las zonas en penumbra y el interior del haz. Los marcos y contornos deben seguir siendo legibles en ambos casos.

Montar un foco `Spot` orientado con la cámara, ligeramente por debajo de la mirada. Usar un haz cálido de borde suave, con alcance inicial de **10–12 m**. Comenzar con la linterna encendida. Cada pulsación de **F** cambia su estado una vez. Con la linterna apagada, la luz ambiental permite ubicar las salidas cercanas. Con ella encendida, se distinguen los detalles de la puerta y la luz queda ocluida por los muros y las hojas. El interruptor, la pose de cámara y el perfil visual se ajustan por separado.

## Orden de implementación

1. **Montar el recorrido.** Crear la escena con suelo, muros y puertas. Ajustar el punto de aparición y comprobar todos los pasos con colisiones.
2. **Ajustar la sensación de caminar.** Afinar la velocidad y la respuesta de la mirada antes de añadir el balanceo. Repetir el recorrido mirando al frente, desplazándose lateralmente y frenando junto a las puertas.
3. **Añadir la linterna y la imagen.** Verificar primero el encendido, el apagado y las sombras. Ajustar después el grano y la exposición con la linterna en ambos estados.
4. **Entregar la escena de prueba.** Registrar los controles y los valores con los que se evaluó. Las puertas, los contrastes de luz y el recorrido bastan para juzgar el movimiento y la atmósfera en esta etapa.

## Criterios de aceptación

| Prueba | Resultado esperado |
| --- | --- |
| Abrir la escena y pulsar **Play** | Aparece una vista en primera persona y se puede iniciar el recorrido directamente. |
| Caminar, girar y soltar el movimiento | Los pasos tienen un ritmo consistente y la vista se estabiliza al detenerse. Comparar la sensación a 30 y 60 FPS. |
| Avanzar contra un muro y atravesar una puerta abierta | El muro detiene al jugador, la puerta abierta permite el paso y la cámara conserva una vista libre de recortes contra paredes y marcos. |
| Pulsar **F** varias veces y apuntar a una puerta cerrada | El estado alterna una vez por pulsación. El haz sigue la mirada e ilumina únicamente el lado expuesto de la puerta. |
| Recorrer el pasillo con la linterna encendida y apagada | El grano se percibe sin dificultar la lectura de los bordes. El contraste entre el pasillo iluminado y el aula oscura permite evaluar la atmósfera. |
