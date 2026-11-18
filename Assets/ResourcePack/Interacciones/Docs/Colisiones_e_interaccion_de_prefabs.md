# Colisiones e interacción de prefabs

## Cobertura

La corrección actualiza los 99 prefabs de las 33 familias indicadas en esta auditoría. Cada prefab tiene al menos un collider. Se conservaron los archivos meta originales de los prefabs y se agregaron únicamente los scripts y meshes de colisión necesarios.

Las colisiones usan cajas ajustadas a piezas y hojas móviles; cápsulas para árboles y personajes; esferas para arbustos; y meshes combinados no convexos para edificios, salas, cercas, cables, caminos, suelos, montañas y techos. Las hojas de puertas quedan con su collider en el pivote para que acompañe la apertura. El sensor de presencia suma un volumen esférico trigger y un Rigidbody cinemático.

## Uso desde el jugador

Los controladores FPSPlayer, BoardDemoPlayer e inventario de pruebas deben buscar AssetInteraction.IPlayerInteractable en el collider apuntado. Al pulsar E, llaman a Interact(). La interfaz expone también InteractionPrompt para presentar la acción. El contrato compartido y la interacción base están en Assets/Interacciones/Unity/Scripts.

## Componentes con comportamiento

| Familia | Prefabs | Comportamiento |
| --- | --- | --- |
| AccesoEscolar | 3 | Abre y cierra hojas con bisagra. |
| Puerta | 3 | Abre y cierra hojas con bisagra. |
| Ventana | Corredera, Proyectante | Desplaza hojas o abre la hoja superior; VentanaFija permanece estática. |
| Ventilador | 3 | E activa/desactiva el giro de los rotores configurados. |
| Sensor | 3 | Presencia detecta actores en el volumen trigger; luminosidad muestrea luces cercanas; consumo consulta la red energética. |
| Computadora, Monitor, Proyector, Inversor, ControladorCarga | 3 por familia | E alterna el estado de energía y actualiza los indicadores asignados. |

Los prefabs estáticos tienen collider y no reciben un script de interacción que no les corresponda.

## Dependencias

- SensorConsumo requiere TablerosEnergia.EnergyGridRegistry; crea o consulta la instancia en ejecución y expone potencia estimada y número de cargas activas.
- Los indicadores de los dispositivos de energía usan MaterialPropertyBlock, sin duplicar materiales compartidos.
- Los meshes de colisión de edificios y geometría estática están en Unity/Models/Collision dentro de la familia correspondiente. Sus archivos meta se mantienen junto a cada asset.
- Cada script nuevo está en Unity/Scripts del objeto al que corresponde. Los tipos compartidos están en Assets/Interacciones/Unity/Scripts.

## Verificación

Unity 6000.6.0f1 compiló sin errores. La pasada de editor guardó 99/99 prefabs, encontró 0 errores de escritura y registró 29 componentes interactivos. La prueba de comportamiento pasó con 0 fallos y 0 advertencias. VentanaFija quedó solo con collider. La escena Assets/Scenes/MAIN.unity no fue editada.

