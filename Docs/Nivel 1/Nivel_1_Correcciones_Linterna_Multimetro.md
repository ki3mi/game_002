# Nivel 1 · Correcciones de la inspección de la linterna y el multímetro

Completar la inspección con el multímetro a la izquierda, una linterna 3D mejorada en el centro, el manual a la derecha y las baterías disponibles arriba. El jugador mide con dos puntas 3D, colocadas una a una, y mueve las baterías entre la linterna y la bandeja superior. La comparación sigue siendo opcional. [Plan de la etapa](Nivel_1_Plan_Linterna_Miedo.md) · [Reglas de reparación](Nivel_1_Mecanicas_Reparacion.md)

## Punto de partida

La inspección ya dibuja la linterna y sus ranuras en un visor 3D. En el panel lateral, un interruptor de voltaje habilita los botones **Medir A** y **Medir B**, que inician un temporizador y consultan la lectura. Reutilizar la sesión, el visor y la fuente de voltaje existente. Sustituir el inicio de la medición por el selector visible y el contacto de las dos puntas. La elección de ranura, el manual y la confirmación del cambio siguen disponibles. [Panel actual](../../Assets/EcoElectry/Runtime/Unity/Survival/SurvivalScene.Hud.cs) · [Visor](../../Assets/EcoElectry/Runtime/Unity/Survival/InspectionPreview.cs) · [Sesión](../../Assets/EcoElectry/Runtime/Application/Survival/SurvivalSession.cs)

## Distribución del inspector

| Elemento | Indicaciones |
| --- | --- |
| **Izquierda · Multímetro 2D** | Usar una ilustración frontal con carcasa, perilla giratoria y LCD independientes. Dejar accesibles las puntas 3D roja y negra junto al instrumento. |
| **Centro · Linterna 3D** | Mejorar el modelo actual con cabezal, lente, empuñadura, interruptor y tapa de compartimento reconocibles. Modelar dos ranuras separadas, contactos metálicos y marcas `+` y `−`. Las pilas deben verse como piezas independientes. Revisar materiales mate y metal, silueta y legibilidad de los terminales al rotar o acercar. |
| **Derecha · Manual** | Mostrar una hoja de papel que asoma de una pila de documentos. Al hacer clic, la hoja se despliega por completo en el lado derecho. Su contenido se limita a una tabla ordenada de voltaje y porcentaje aproximado para las pilas del juego. Otro clic permite guardarla. |
| **Arriba · Bandeja de baterías** | Mostrar el modelo 3D del repuesto del inventario. Reservar posiciones separadas para las baterías que el jugador retire durante la inspección. Las piezas se pueden seleccionar directamente. |
| **Lecturas registradas** | Mostrar en el LCD la lectura actual y conservar los dos resultados junto a las baterías medidas. La hoja de referencia muestra valores fijos y permanece independiente de esas lecturas. |

## Interacción con el multímetro

| Elemento | Indicaciones |
| --- | --- |
| **Selector** | Hacer que la perilla encaje visualmente en **OFF**, **V⎓** y **continuidad**, con el modo activo legible. **V⎓** permite medir baterías. Si el jugador selecciona continuidad y toca una pila, mostrar «Selecciona V⎓ para comparar baterías». El selector queda preparado para reutilizar continuidad en las reparaciones. |
| **Pantalla** | Mostrar el modo activo y el estado de la medición. En **OFF**, pantalla apagada. En **V⎓** sin contacto válido, mostrar `—`. Durante una lectura, indicar que se está estabilizando y, al concluir, presentar el valor en voltios en la propia pantalla, por ejemplo `1,42 V`. |
| **Puntas 3D** | Crear dos modelos low poly con mango, aislamiento y punta metálica. Diferenciarlos por color y una indicación de polaridad. Colocarlos en el visor existente con anclajes a los terminales de cada pila. Sus posiciones deben seguir al objeto cuando se rota o se acerca el compartimento. |

## Secuencia de medición

1. **Abrir e inspeccionar.** Al abrir el compartimento, la linterna queda apagada y el jugador puede girarla o acercarla. Si tiene dos baterías, puede abrir el multímetro o pasar directamente a elegir qué ranura cambiar.
2. **Seleccionar V⎓.** El jugador mueve la perilla y el LCD refleja el modo. Para colocar cada punta, hace clic en ella y luego en el terminal. La punta se desplaza hasta el contacto y queda allí mientras coloca la otra. La primera punta por sí sola no inicia una lectura. Distinguir un clic sobre una punta o batería del arrastre usado para rotar el objeto.
3. **Establecer contacto.** Simular cada pila aislada del circuito de la linterna, con el haz apagado. El cable negro sale de **COM** y el rojo de la entrada de voltaje **V/Ω**. La lectura positiva requiere negro en `−` y rojo en `+` de **la misma pila**. Si se invierten, mostrar el mismo valor con signo negativo y una indicación para corregir la polaridad antes de fijar la comparación. Con una punta suelta o contactos en pilas distintas, mostrar `—`. Si ambas puntas tocan el mismo terminal, mostrar una diferencia cercana a `0 V` sin registrarla como voltaje de la pila. [Medición de voltaje DC en Fluke](https://www.fluke.com/en-us/learn/blog/digital-multimeters/how-to-measure-dc-voltage-with-a-digital-multimeter)
4. **Leer y repetir.** Mantener los contactos durante una espera breve. Usar los **1,5 s** actuales como punto de partida, conservando tiempo de juego y aumento de miedo durante la espera. Mostrar la lectura en el LCD y registrarla junto a la ranura. Al llevar las puntas a la siguiente pila, conservar la lectura anterior en la ficha de comparación.
5. **Terminar o cancelar.** Permitir guardar las puntas, consultar el manual y confirmar el cambio. Si se mueve una punta, se cambia de modo, se pausa o se abandona la inspección, gestionar el intento pendiente de forma explícita. Mover una punta o cambiar de modo cancela la lectura pendiente. La pausa la congela y permite retomarla al continuar si el contacto sigue válido. Salir limpia los contactos y detiene el intento. Cambiar de ranura para elegir reemplazo conserva las lecturas ya registradas.

## Extracción y colocación de baterías

1. **Sacar.** Al hacer clic en una batería instalada, se anima su desplazamiento desde la ranura hasta una posición libre de la bandeja superior. La ranura queda vacía. Si había puntas tocando esa pila, se separan y se cancela la lectura en curso.
2. **Poner.** Al hacer clic en otra batería de la bandeja, se selecciona para colocarla. Si hay una sola ranura libre, se desplaza a ella. Si hay dos, el jugador elige la ranura de destino. La animación debe seguir la dirección del compartimento hasta que la pila quede asentada.
3. **Elegir el orden.** El jugador puede retirar cualquiera primero, extraer ambas antes de poner otra, reinsertar una pila retirada o intercambiar las dos. La bandeja conserva cada modelo y su carga. Evitar operaciones simultáneas sobre la misma pieza durante una animación y permitir la siguiente acción cuando termine.
4. **Confirmar o cancelar.** Los movimientos preparan el montaje. Al confirmar, se guardan las ranuras finales, se descuentan los repuestos instalados y se descartan las pilas retiradas que quedaron fuera. Cancelar restaura las ranuras y el inventario de entrada y conserva el miedo acumulado.

## Integración y acabado

- Mantener la medición basada en `SurvivalSession.MeasureSlot`. El selector, la posición de las puntas y el tiempo de estabilización controlan cuándo se solicita la lectura. El voltaje se obtiene de la batería instalada, con el mismo resultado al repetir la prueba en iguales condiciones.
- Mantener el visor y el panel reutilizables. Identificar cada pila por su identidad y asociar contactos y lecturas a esa pieza para que sigan siendo correctos si cambia de ranura. Usar la misma presentación en `Level01.unity` y `FlashlightFearTest.unity`. Confinar luces, cámara y puntas al visor y liberar sus recursos al salir de escena.
- Comprobar que la perilla, el LCD, las puntas y sus etiquetas se distingan con la atmósfera oscura del nivel. Dar respuesta visual al contacto y a la lectura completada. Mantener legibles el manual, las barras de linterna y el miedo cuando llegue a su umbral. Comprobar al menos **1280 × 720** y **1920 × 1080**, con ambas pilas y el manual abierto.
- Conservar el apagado temporal de la linterna y el aumento de miedo durante toda la inspección. La medición deja intactas las cargas. Solo confirmar el montaje gasta un repuesto y descarta la batería elegida. Al cancelar, se conserva el miedo acumulado junto con las pilas y el inventario.

## Criterios de cierre de la etapa

| Prueba | Resultado esperado |
| --- | --- |
| **Medir dos baterías** | Girar el selector a V⎓, colocar ambas puntas en la primera pila y leer el LCD. Repetir en la segunda y consultar ambas lecturas en el manual. La medición depende del contacto 3D. |
| **Validar el selector y el contacto** | Colocar las puntas una a una. El LCD muestra el signo negativo si se invierte la polaridad. Una punta suelta o contactos en pilas distintas no producen una lectura de comparación. |
| **Interrumpir una medición** | Al cambiar de modo o levantar una punta, se cancela la lectura pendiente. Pausar congela la espera. Salir de la inspección devuelve el control de exploración y limpia la presentación del instrumento. |
| **Abrir el manual** | La hoja asoma a la derecha. Al abrirla, solo aparece la tabla ordenada de voltaje y porcentajes aproximados. El multímetro queda a la izquierda y la linterna permanece visible en el centro. |
| **Cambiar con y sin medición** | Las extracciones y colocaciones se animan. Se puede retirar primero cualquiera de las pilas, sacar ambas e invertir su orden sin duplicarlas. Las reservas del inventario solo se descuentan al confirmar. |
| **Revisar la etapa completa** | En el Nivel 1 se puede empezar con una pila, encontrar un repuesto, instalarlo, comparar ambas pilas y salir. Se comprueban consumo, traspaso automático, miedo, pausa y derrota. En la escena de prueba se reproduce el mismo flujo. |

Para entregar, adjuntar el multímetro 2D, las puntas 3D, el modelo mejorado de la linterna, la hoja del manual, las referencias necesarias en ambas escenas y el resultado de las pruebas. El alcance de medición de esta entrega es **V⎓ para las baterías**. La comprobación de continuidad sobre instalaciones se integra en la etapa de reparaciones.
