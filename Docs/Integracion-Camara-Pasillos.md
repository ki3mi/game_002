# Cámara de primera persona de EcoElectry

La escena de referencia es [HorrorCorridorPrototype.unity](../Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity). Abre la escena y pulsa **Play**, o usa **Tools → EcoElectry → Prototipos → Abrir pasillos de noche**. Este menú abre la escena existente y solo la construye si todavía no existe. La cámara del nivel 01 usa su propio controlador, [ThirdPersonExplorer](../Assets/EcoElectry/Runtime/Unity/Player/ThirdPersonExplorer.cs), que alterna entre primera y tercera persona con **C**.

## Controles

| Entrada | Acción |
| --- | --- |
| **WASD** o stick izquierdo | Caminar. La velocidad máxima se conserva en diagonal. |
| **Ratón** o stick derecho | Mirar. El ratón requiere que la vista Game tenga capturado el cursor. |
| **F** | Alternar la linterna. Empieza encendida. |
| **B** | Abrir o cerrar el menú de imagen. Al abrirlo se detiene el movimiento y se libera el cursor para arrastrar las barras. |
| **Esc** o clic en Game | **Esc** cierra el menú si está abierto o libera el cursor durante la exploración. Un clic en Game vuelve a capturarlo durante la exploración. |

## Integración en otra escena

El proyecto usa Unity **6000.6.0f1**, **URP 17.6.0** con Render Graph e **Input System 1.20.0**. Para reproducir la iluminación y las sombras de la linterna en el editor, usa la calidad **PC**. El asset URP de ese nivel admite sombras en luces adicionales. La configuración **Mobile** del proyecto las tiene desactivadas.

1. Abre la escena de origen y copia la jerarquía completa de **Explorador** a la escena de destino. Si la montas a mano, usa esta estructura. Mantén la escala del objeto raíz en `(1, 1, 1)` y colócalo con los pies sobre un suelo con colisión. La rotación inicial del objeto raíz determina hacia dónde mira al aparecer.

   ```text
   Explorador
   ├─ CharacterController
   ├─ HorrorCorridorExplorer
   └─ Camara
      ├─ Camera y UniversalAdditionalCameraData
      ├─ AudioListener
      ├─ CorridorSignalGlitch
      ├─ Linterna (Light de tipo Spot)
      └─ Perfil de imagen (Volume)
   ```

2. En **Horror Corridor Explorer**, asigna **Controls** al archivo de acciones que contiene `Player/Move` y `Player/Look`, **View** al componente `Camera` y **Flashlight** al componente `Light` hijo de esa cámara. El controlador necesita las tres referencias para habilitarse. El recurso de origen es [InputSystem_Actions.inputactions](../Assets/ResourcePack/InputSystem_Actions.inputactions).
3. En **Camara**, usa el tag `MainCamera`, proyección **Perspective**, cámara **Base** y el renderer URP predeterminado. Activa **Post Processing** y **Render Shadows**. En **Volume Mask**, selecciona `TransparentFX`. Deja una sola cámara jugable y un solo **Audio Listener** activos para esta configuración.
4. Pon **Perfil de imagen** en la capa `TransparentFX`. Configura su **Volume** como **Global**, con **Weight** = 1 y **Priority** = 0, y asigna [HorrorCorridorProfile.asset](../Assets/EcoElectry/Content/Rendering/HorrorCorridorProfile.asset). Mantén este volumen bajo la cámara para que el menú encuentre `FilmGrain`. Si necesitas valores de perfil distintos, duplica el asset para la escena de destino. Conserva la máscara de volumen restringida a la capa que usa el perfil.
5. Añade **Corridor Signal Glitch** al mismo objeto que la cámara y asigna [CorridorSignal.shader](../Assets/EcoElectry/Content/Rendering/CorridorSignal.shader) en **Signal Shader**. Esta referencia habilita tanto el glitch y el parpadeo de imagen como la interferencia de grano alto. Para reproducir la música, conserva también el **Audio Source** de la cámara con **Play On Awake**, **Loop**, **Spatial Blend** = 0 y **Volume** = 0,32. La música es opcional para la integración de cámara.
6. Prueba con la escena de destino como **escena activa**. El controlador toma las luces y las emisiones de los objetos presentes en esa escena al inicializarse. El ajuste de brillo aplica la luz ambiental, la niebla y los reflejos globales cuando la escena de la cámara es la activa. Para un montaje aditivo, activa la escena de la cámara antes de inicializar el controlador y comprueba que el volumen de otra escena no compita en la misma capa.

## Ajustes en el Inspector

### Explorador y cámara

| Componente o campo | Valor de referencia | Efecto |
| --- | --- | --- |
| Character Controller | Height 1,85 m, Radius 0,28 m, Center Y 0,925 m con X y Z en 0 | Cápsula del jugador. `Center Y` equivale a media altura. |
| Character Controller | Slope Limit 45°, Step Offset 0,16 m, Skin Width 0,025 m, Min Move Distance 0 | Pendientes, escalones y contacto con superficies. |
| Walk Speed y Acceleration Time | 1,7 m/s y 0,16 s | Velocidad de marcha y transición al acelerar o frenar. |
| Mouse Sensitivity | 0,12 | Grados por unidad de desplazamiento del ratón. El stick usa 110°/s a entrada completa. |
| Eye Height | 1,65 m | Altura de la cámara sobre el origen del explorador. |
| Bob Intensity | 1, rango 0 a 1 | Multiplicador del balanceo. En 0 queda desactivado. |
| Step Length, Vertical Bob y Roll Degrees | 0,72 m, 0,015 m y 0,28° | Cadencia y amplitudes. El desplazamiento lateral es de hasta 0,006 m antes de limitarlo por la cápsula. |
| Camera | FOV vertical 67°, Near 0,04 m, Far 55 m | Encuadre y distancia visible. Revisa Far en espacios abiertos. Mantén Near pequeño para evitar recortes junto a los marcos. |
| Camera | Solid Color `RGBA(0.04, 0.05, 0.07, 1)`, HDR activado | Fondo del pasillo y rango de imagen. La cámara usa el viewport completo. |
| Flashlight Binding | `<Keyboard>/f` | Ruta de la acción de linterna en Input System. |
| Brightness Menu Key | `B` | Tecla de apertura y cierre. `None` deshabilita el acceso por tecla. |
| Brightness Preference Key | Vacío en el prototipo | Usa `EcoElectry.Corridor.Brightness` cuando está vacío. Asigna una clave única si otra escena necesita un brillo guardado independiente. |

El controlador rota el cuerpo en horizontal y aplica el cabeceo en la cámara con un límite de ±85°. El balanceo avanza con la distancia que recorre la cápsula en el suelo. Al detenerse o empujar un muro, vuelve al centro. La pose se actualiza en `LateUpdate` y limita el desplazamiento para que el plano cercano quede dentro de la cápsula. Revisa en conjunto altura, radio, FOV y Near al cambiar el tamaño del jugador.

### Linterna y señal de cámara

| Ajuste | Valor en el prototipo |
| --- | --- |
| Linterna → Transform | Posición local `(0.10, -0.12, 0.02)` m, rotación local cero |
| Linterna → Light | Spot, intensidad 7,4, alcance 11,5 m, ángulos interior 28° y exterior 64°, color `RGB(1, 0.86, 0.68)` |
| Linterna → Shadows | Soft Shadows, fuerza 1, Bias 0,025, Normal Bias 0,08 y Near Plane 0,10 m en la escena guardada |
| Explorer → Flashlight Flicker Depth / Interval | 0,88, pausa aleatoria de 4–9 s entre pulsos. Solo actúa con la linterna encendida. |
| Signal → Displacement | 0,0045 del ancho de la imagen, equivalente al 0,45 %. Desplaza una banda horizontal y separa ligeramente los canales de color. |
| Signal → Minimum Interval / Maximum Interval / Pulse Duration | 3 s, 6 s y 0,17 s. El primer glitch comienza a los 1,4 s. |
| Signal → Flicker Depth / Flicker Interval | 0,55, pausa aleatoria de 3–7 s entre pulsos de oscurecimiento de la imagen. |

Los dos parpadeos tienen ritmos independientes y cada descenso dura entre 0,13 y 0,24 s. Pon la profundidad correspondiente en **0** para desactivarlo. Pon **Displacement** en **0** para desactivar el glitch de banda y conservar el grano alto. Al alternar la linterna con **F**, se restablece su intensidad base y se programa una nueva pausa antes del siguiente parpadeo.

### Perfil de volumen y menú de imagen

| Ajuste | Configuración y comportamiento |
| --- | --- |
| Film Grain | Tipo `Thin1`, intensidad base 0,29 y respuesta 0,72. Activa el override de intensidad y conserva el componente activo en el perfil. |
| Vignette | Intensidad 0,24 y suavidad 0,48. |
| Color Adjustments | Saturación −29 y Post Exposure 0. |
| Brillo del entorno | **1–220 %**, base **100 %**. Escala las luces de la escena, la emisión de los materiales compatibles, el color de fondo, la luz ambiental, la niebla y la intensidad de los reflejos hasta su tope. El haz de la linterna se excluye del multiplicador. |
| Grano de la cámara | **0–100 %**. Ajusta la intensidad de Film Grain en la instancia del volumen. Por encima del **65 %**, un refuerzo cuadrático añade ruido animado de dos escalas y pequeñas distorsiones horizontales. El máximo produce un granulado deliberadamente intenso. |
| Restablecer | Devuelve el brillo al **100 %** y el grano al valor inicial del perfil activo al entrar, **29 %** con el perfil del prototipo. |

Los cambios se ven al mover las barras y se guardan con `PlayerPrefs` al cerrar el panel o desactivar el controlador. La clave de grano `EcoElectry.Corridor.FilmGrain` se comparte entre las escenas que reutilizan este menú en el mismo proyecto. La clave de brillo se puede configurar por escena. El shader de señal se ejecuta antes del posprocesado URP en Render Graph. Con grano de hasta 65 % solo se habilita durante un pulso de glitch o parpadeo. Con grano superior se procesa cada fotograma de la cámara.

## Iluminación y entorno nocturno

Reproduce primero la iluminación de la escena con el **brillo del entorno al 100 %**, el **grano al 29 %** y la **linterna apagada**. Así puedes comparar la luz base, la niebla y el contraste con los valores guardados en el prototipo. El perfil de cámara aporta el grano, la viñeta y la desaturación. La luz y la niebla se configuran en la escena de destino.

### Ajustes globales de la escena

En **Window → Rendering → Lighting → Environment**, configura los siguientes valores. Los colores hexadecimales corresponden a los valores RGB del selector de Unity.

| Campo | Valor de referencia |
| --- | --- |
| Skybox Material | `None` |
| Sun Source | `None`. El prototipo ilumina el interior con luces locales. |
| Environment Lighting → Source | **Color**, equivalente a `AmbientMode.Flat`. |
| Ambient Color | Azul grisáceo oscuro `#222A36`, RGB 34, 42, 54. |
| Environment Reflections | Source **Skybox**, Intensity Multiplier **1**, Bounces **1**, resolución **128**. La barra de brillo también atenúa la intensidad de reflejos. |
| Fog | Activado, Mode **Exponential Squared**, color `#1A2029` y densidad **0,032**. |
| Cámara → Background | **Solid Color** con `RGBA(0.04, 0.05, 0.07, 1)`. Usa este fondo si el encuadre deja ver el exterior. |

### Luces locales y geometría

Distribuye la luz por zonas. El pasillo comienza con un fluorescente tenue, recibe luz azul a través de la ventana y termina en una esquina más oscura con un piloto rojo. Estas posiciones están en coordenadas de la escena original. En otro mapa, conserva la relación de cada luz con la entrada, la ventana y el giro.

| Luz | Tipo y colocación | Intensidad, alcance y color |
| --- | --- | --- |
| Fluorescente de guardia | **Spot** hacia abajo desde `(0, 2.83, 1.6)`. Conos interior **85°** y exterior **132°**. | **2,6**, **9 m**, RGB 208, 224, 200. |
| Luna en el patio | **Spot** fuera de la ventana, en `(3.1, 3.10, 5.15)` y con dirección `(-1, -0.58, 0.33)`. Conos interior **51,6°** y exterior **86°**. | **5,3**, **12 m**, RGB 154, 192, 241. |
| Resplandor de emergencia | **Point** junto al piloto de la esquina, 0,21 m delante de su soporte. | **0,46**, **2,2 m**, RGB 255, 99, 66. |

Usa **Soft Shadows** en los dos focos, con **Bias 0,035** y **Normal Bias 0,15**. Coloca el foco azul de modo que la abertura y los montantes de la ventana recorten el haz. Mantén las paredes, puertas y marcos como geometría opaca que proyecte sombras y revisa desde ambos lados que la linterna solo ilumine la cara expuesta.

Usa materiales **URP/Lit** mates para el yeso, el zócalo y el suelo. El prototipo usa **Smoothness 0,16** en estos materiales, yeso beige apagado, zócalo verde grisáceo y suelo de terrazo oscuro. Reserva la emisión para pequeños acentos como el tubo encendido y el piloto. Los rótulos del pasillo usan un shader con iluminación y prueba de profundidad, de modo que el texto responde a la linterna y queda oculto por las paredes. [Constructor del entorno](../Assets/EcoElectry/Editor/Levels/HorrorCorridorEnvironment.cs) · [Materiales del pasillo](../Assets/EcoElectry/Content/Models/HorrorCorridor) · [Shader de rótulos](../Assets/EcoElectry/Content/Rendering/CorridorSignText.shader)

Cuando la luz base esté lista, baja el entorno al **1 %** y alterna **F**. Con la linterna apagada, el espacio debe quedar casi a oscuras. Encendida, su haz debe seguir mostrando el suelo y las superficies cercanas. En escenas con iluminación horneada, revisa este contraste por separado. La barra ajusta parámetros y luces en tiempo de ejecución y las contribuciones ya almacenadas en lightmaps pueden seguir visibles.

## Ejemplo en un escenario abierto

La escena de prueba del bosque reutiliza la misma jerarquía. Usa **Far = 1000 m**, el perfil **FantasyForestNightProfile** con Film Grain, Vignette y Color Adjustments, y la clave de brillo `EcoElectry.FantasyForest.Brightness`. La cámara usa **Displacement = 0,0045** para el glitch de banda horizontal y **Flicker Depth = 0** para el oscurecimiento periódico. El shader también aplica la interferencia de grano alto. La linterna conserva su parpadeo. [Escena del bosque](../Assets/EcoElectry/ThirdParty/FantasyForest/Scenes/demoScene_free.unity) · [Perfil del bosque](../Assets/EcoElectry/ThirdParty/FantasyForest/Scenes/FantasyForestNightProfile.asset)

El bosque usa iluminación ambiental **Color** `#2B3548`, sin material de skybox, y reduce los reflejos a **0,15**. Su niebla **Exponential Squared** es más azul y densa que la del interior, con color `#0A111F` y densidad **0,042**.

## Comprobación después de integrar

1. Pulsa **Play**, camina y mira a ambos lados. Repite junto a una puerta y comprueba que la cámara vuelva al centro al soltar el movimiento.
2. Abre **B**, deja el brillo en **1 %** y cierra el menú. Alterna **F** para comprobar que el haz sigue iluminando y que proyecta sombras con calidad **PC**. Con la linterna apagada, espera un ciclo para comprobar que el parpadeo no la encienda.
3. Compara el grano al **0 %**, **29 %** y **100 %**. Si falta la barra, revisa la jerarquía del volumen y que el perfil incluya **Film Grain**. Si la barra aparece pero el grano fino no se ve, revisa **Post Processing**, **Volume Mask**, capa y peso. Si falta el refuerzo al máximo, revisa el componente de señal, su shader y Render Graph.
4. Cierra el menú y vuelve a entrar en Play para comprobar que se recuperen los valores. Usa **Restablecer** para volver a la referencia y prueba la escena con la consola abierta para detectar errores de referencias o shader.

[Controlador del explorador](../Assets/EcoElectry/Runtime/Unity/Player/HorrorCorridorExplorer.cs) · [Menú de imagen](../Assets/EcoElectry/Runtime/Unity/Player/CorridorBrightness.cs) · [Pase de señal](../Assets/EcoElectry/Runtime/Unity/Player/CorridorSignalGlitch.cs) · [Constructor de escena](../Assets/EcoElectry/Editor/Levels/HorrorCorridorBuilder.cs) · [Montaje de imagen](../Assets/EcoElectry/Editor/Levels/HorrorCorridorImage.cs)
