# Componentes instalables de tableros

Los prefabs `InterruptorDiferencial`, `Contactor`, `BornerasCableado` y `MedidorEnergia` se asignan automáticamente a los anclajes compatibles de los prefabs de tablero. Los termomagnéticos y el seccionador pertenecen a `Interruptor/Unity`. Importe las tres familias con sus `.meta` y scripts; consulte `TablerosElectrico/Unity/Docs/TablerosInstalables.md` para coordenadas, conexión de red, inventario y API.

| Pieza | Tipo de anclaje | Comportamiento |
|---|---|---|
| Diferencial | `Differential` | Mando independiente y botón de prueba. Con alimentación presente y mando encendido, `DifferentialDevice.TryTest()` dispara el dispositivo. Restablezca con `BoardSlot.SwitchControl.SetState(true)`. |
| Contactor | `Contactor` | `ContactorDevice.SetControlSignal(bool)` controla su señal; `IsClosed` refleja si está cerrado con alimentación. Su mando e indicador son independientes. |
| Bornes | `Terminals` | Su instalación habilita la conexión de entrada del tablero. Los conductores de entrada aparecen cuando también existe enlace de alimentación. |
| Medidor | `Meter` | `EnergyMeterDisplay` presenta la demanda estimada y circuitos activos de la red asociada; no requiere paquetes de texto adicionales. |

Los componentes comienzan desmontados. `BoardSlot.TryInstall` solo instancia el prefab compatible y rechaza una posición ocupada; `TryRemove` retira la pieza y su cableado. El cableado entre tableros aparece al llamar `ConnectFeed` y desaparece al desconectar. El color y trazado de fase, neutro y protección son representaciones visuales de juego.

Los fuentes editables de diferencial y contactor se encuentran en sus respectivas carpetas dentro de `ComponentesTableros`, junto con los demás modelos de la familia. La prueba del diferencial requiere alimentación; véase también la [explicación de prueba del fabricante](https://www.se.com/in/en/faqs/FA401819/). Esta simulación no sustituye un diseño o una comprobación eléctrica real.
