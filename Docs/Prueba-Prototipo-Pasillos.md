# Prueba de los pasillos de noche

Para montar esta cámara en otra escena y consultar sus campos del Inspector, abre la [guía de integración de la cámara](Integracion-Camara-Pasillos.md).

Abre [HorrorCorridorPrototype.unity](../Assets/EcoElectry/Content/Scenes/HorrorCorridorPrototype.unity) en Unity y pulsa **Play**. También puedes abrirla desde **Tools → EcoElectry → Prototipos → Abrir pasillos de noche**.

Camina con **WASD**, mira con el **ratón** y pulsa **F** para apagar o encender la linterna. **Esc** libera el cursor y un clic en la vista Game lo captura de nuevo.

Pulsa **B** para abrir los ajustes de imagen y arrastra las barras con el ratón. Al **1 %** de brillo, el pasillo queda casi a oscuras con la linterna apagada. Enciéndela con **F** para iluminar el recorrido con su intensidad habitual. El grano de la cámara se puede ajustar del **0 %** al **100 %**. **Restablecer** recupera el brillo del **100 %** y el grano del **29 %**. Cierra el panel con **B** o **Esc** para volver a caminar. Ambos valores se guardan para la próxima sesión del prototipo.

## Recorrido

1. Avanza hasta la puerta cerrada de la izquierda. Acércate hasta tocar la hoja y mira de lado hacia el marco.
2. Sigue por el pasillo y mira por la puerta entreabierta. Alterna la linterna para comparar la lectura del interior.
3. Gira a la derecha al final del pasillo, cruza la puerta abierta y entra en el aula. Vuelve al inicio con la linterna apagada.

## Ajustes de referencia

| Elemento | Valores iniciales | Dónde se ajusta |
| --- | --- | --- |
| Marcha | 1,7 m/s, aceleración y frenada de 0,16 s | Explorador → Horror Corridor Explorer |
| Mirada | Altura de 1,65 m, campo visual vertical de 67° | Explorador y su cámara |
| Pasos | Un paso cada 0,72 m, hasta 1,5 cm verticales y 0,28° de inclinación | Horror Corridor Explorer → Bob Intensity permite comparar con cero |
| Linterna | Encendida al aparecer, alcance de 11,5 m, conos interior y exterior de 28° y 64°, sombras suaves | Camara → Linterna |
| Imagen | Grano fino Thin1 a 0,29, viñeta a 0,24 y saturación a −29 | [Perfil URP](../Assets/EcoElectry/Content/Rendering/HorrorCorridorProfile.asset) |
| Grano alto | Por encima del 65 %, la interferencia aumenta progresivamente. Cerca del 100 % aparecen granulado grueso y pequeñas distorsiones horizontales | B → Grano de la cámara |
| Música | **Untitled #9 (Smáskífa 1)** se reproduce en bucle al abrir la escena en Play | Camara → Audio Source |
| Glitch | Banda horizontal con separación de color, desplazamiento máximo del 0,45 % del ancho. Tras el primer pulso, dura 0,17 s cada 3–6 s | Camara → Corridor Signal Glitch |
| Parpadeos | Caídas breves de 0,13–0,24 s. La imagen parpadea cada 3–7 s y la linterna encendida cada 4–9 s, con ritmos independientes | Corridor Signal Glitch → Flicker y Horror Corridor Explorer → Flashlight Flicker. La profundidad en cero desactiva el efecto correspondiente |

## Comprobaciones en Unity

En dos segundos desde reposo, el controlador avanzó 3,263 m con 30 pasos de simulación por segundo y 3,264 m con 60. Al frenar, la cámara volvió a 1,65 m. La puerta cerrada detuvo al personaje y la abierta permitió entrar y salir del aula. El plano cercano de la cámara quedó libre al mirar hacia el marco y al retroceder contra el muro.

Dos pulsaciones de **F** apagaron y volvieron a encender la linterna. Se revisaron la entrada con el haz encendido y la esquina con el haz apagado. En una comparación de dos fotogramas con la cámara fija y el grano desactivado para la medición, el glitch sólo alteró la banda horizontal.
