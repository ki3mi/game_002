# Nivel 1 · Primera reparación · Caja de fusibles

Implementar en `Level01.unity` el primer encargo de la **sala de limpieza**. El jugador abre una caja de fusibles, comprueba tres piezas con el multímetro, encuentra vacía la caja de repuestos y sale a buscar un fusible compatible. Termina al verificar el repuesto, montar las tres piezas y volver a cerrar la caja. El colegio permanece desenergizado durante todo el encargo. [Mapa del nivel](Nivel_1_Etapa_1.md) · [Mecánicas de reparación](Nivel_1_Mecanicas_Reparacion.md)

## Montaje y recursos

| Elemento | Requisito |
| --- | --- |
| Caja mural | Seleccionar un modelo del proyecto. Revisar `Assets/ResourcePack/CajaConexion/Prefabs/` para reutilizar la carcasa. Crear una variante de juego con tapa y tornillos separados, tres alojamientos etiquetados **A, B y C** y anclajes para el visor. |
| Fusibles | Localizar el modelo disponible en Unity antes de crear otro. Preparar cada fusible como instancia independiente, con terminales en ambos extremos e inscripción legible al girarlo. La referencia define la compatibilidad. El estado sano o abierto se descubre al medir. |
| Repuestos | Colocar una caja de repuestos inicialmente vacía en la sala de limpieza. Debe contener una pista legible que señale un escondite concreto, por ejemplo el armario del corredor de aulas. Colocar allí el repuesto compatible. |
| Instrumentos | Reutilizar la cara del multímetro, las dos puntas 3D y el visor de la linterna. Incorporar un destornillador visible en la animación de los tornillos. [Referencia del inspector](Nivel_1_Correcciones_Linterna_Multimetro.md) |

Usar un caso fijo para la primera entrega. **A y C sanos, B abierto**. El repuesto comparte la referencia de B y tiene identidad propia. Dejar los estados iniciales y las referencias en la configuración del encargo para poder reutilizar la caja en otros niveles.

## Inspector y manual

- **Izquierda.** Multímetro 2D con perilla manipulable, LCD y puntas 3D. El selector conserva **OFF**, **V⎓** y **continuidad**. La comprobación de los fusibles requiere continuidad.
- **Centro.** Caja como objeto principal. El jugador puede rotar o acercar el modelo. Al sacar un fusible, la pieza seleccionada pasa a una zona de prueba dentro del visor para que se vean sus dos terminales. Reservar una bandeja para los tornillos y las piezas retiradas, con sus identificadores a la vista.
- **Derecha.** Una hoja asoma de la pila de documentos. Al hacer clic, se despliega sin tapar el objeto. Presenta la referencia exigida por ranura y una tabla breve que explica **pitido = continuidad** y **OL = circuito abierto en el ejercicio**. Las lecturas obtenidas se registran en la ficha de la tarea. El manual conserva sus valores de referencia.

## Secuencia de la reparación

1. **Verificar el estado del circuito.** La interacción consulta el estado de suministro de la sesión antes de habilitar el desmontaje y las pruebas. El corte se enseña en mantenimiento. Si falta esa condición, el visor explica qué debe comprobar el jugador. El estado se comparte con el encargo y la escena interior.
2. **Desatornillar la tapa.** Con el destornillador equipado, hacer clic directamente en un tornillo visible. La herramienta se alinea con su cabeza. El tornillo gira y avanza hacia fuera por su eje, luego se desplaza animado hasta la bandeja. Cada clic válido actúa sobre un tornillo. El jugador puede retirarlos en cualquier orden. Durante la animación, bloquear clics repetidos sobre esa pieza. La tapa se puede retirar al quedar libre el último tornillo. Al hacer clic sobre ella se anima su separación y queda visible la caja interior.
3. **Extraer y probar A, B y C.** Hacer clic en un fusible para llevarlo a la zona de prueba. Seleccionar **continuidad**, colocar una punta en un terminal y la otra en el terminal opuesto del mismo fusible retirado. La primera queda apoyada mientras se coloca la segunda. Con contacto válido, el LCD muestra la lectura y suena el pitido si el fusible está sano. Si está abierto, muestra **OL** y permanece en silencio. Una punta suelta, puntas en piezas distintas o un modo diferente no completan la comprobación. Al levantar una punta se pierde el contacto, pero se conserva el último resultado válido asociado a esa pieza.
4. **Buscar un repuesto.** Tras detectar el fusible abierto, registrar su referencia. Al inspeccionar la caja de repuestos vacía, añadir la pista al listado de tareas y permitir salir del visor. Al volver, la tapa, los tornillos, las ranuras y las lecturas conservan su estado. El jugador puede revisar la referencia del fusible encontrado al girarlo en el inventario.
5. **Verificar y montar.** Comprobar la continuidad del repuesto en el visor y colocarlo en la ranura B. Reinstalar los originales sanos en sus ranuras correspondientes si siguen fuera. Una pieza incompatible recibe una indicación antes de insertarse. Mantener el fusible abierto identificado como pieza retirada. El montaje requiere tres fusibles compatibles con comprobaciones válidas.
6. **Cerrar la caja.** Al hacer clic en la tapa, vuelve a su posición mediante una animación. Colocar cada tornillo de la bandeja mediante un clic sobre su punto de fijación. Animar la inserción y el giro en sentido inverso. Marcar el objetivo como terminado cuando la tapa esté fijada y las tres ranuras estén verificadas. El resultado aparece en la lista de tareas y la sala permanece oscura.

## Estado y reutilización

- Separar los estados de la caja, los tornillos, las ranuras, las piezas y las mediciones de sus animaciones. Guardar las lecturas por identidad de fusible para que sigan siendo correctas si el jugador cambia una pieza de sitio. Mantener una sola ubicación por pieza entre ranura, visor, bandeja e inventario.
- Compartir selector, LCD y contactos 3D con la inspección de la linterna. La batería responde a V⎓ y el fusible retirado a continuidad. El objeto inspeccionado define qué lectura proporciona la simulación. Los puntos de contacto se anclan a la geometría para seguirla al girar.
- Al cerrar el visor o pausar, conservar las acciones completadas y limpiar los contactos del instrumento al salir. Dejar el estado consistente si se interrumpe una animación. El reloj y el miedo siguen las reglas vigentes para el haz operativo durante la intervención.

## Pruebas de aceptación

| Caso | Resultado esperado |
| --- | --- |
| Desmontaje | Se retiran los tornillos uno a uno con clic y animación, en cualquier orden. La tapa permanece fija mientras quede un tornillo colocado. |
| Diagnóstico | A y C dan pitido, B da OL. Probar una punta suelta y el modo incorrecto. Esas acciones no marcan el fusible como comprobado. |
| Búsqueda e interrupción | La caja vacía revela una pista útil. Se puede salir, recoger el repuesto y volver sin perder ni duplicar tornillos o fusibles. |
| Montaje correcto e incorrecto | Una pieza incompatible o sin verificar deja el encargo pendiente. El repuesto compatible y verificado, los originales sanos y el cierre completo finalizan la tarea con el suministro cortado. |
| Legibilidad y regresión | El multímetro se ve a la izquierda, el objeto al centro y el manual a la derecha. Es posible rotar, seleccionar terminales y leer el LCD con claridad. La inspección de baterías y su medición en V⎓ siguen funcionando. |
