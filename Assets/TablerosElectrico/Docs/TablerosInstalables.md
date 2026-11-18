# Tableros instalables: integración en Unity

## Paquetes y puesta en marcha

Importe conservando las rutas y los archivos `.meta` las carpetas `Interruptor/Unity`, `ComponentesTableros/Unity` y `TablerosElectrico/Unity`. Los prefabs usan URP y referencian los prefabs del termomagnético y del seccionador existentes. Requieren los scripts de las tres carpetas; no se deben copiar solamente los FBX. Coloque uno de los tres prefabs de `TablerosElectrico/Unity/Prefabs` en la escena. Su raíz tiene transformación identidad y los anclajes están vacíos, con sus tapas. El registro de red se crea automáticamente al entrar en Play.

La instalación se realiza en ejecución. Cada instancia dispone de identidad propia. Los interruptores de pared continúan siendo controles independientes. Las puertas se accionan mediante `BoardDoorInteractable.Toggle()`. El tablero secundario abre lateralmente.

## Posiciones y compatibilidad

Las coordenadas son locales a la raíz, en metros y en ejes de Unity, expresadas como `(X, Y, Z)`. Las filas se describen por su primera posición, paso y cantidad; el índice aumenta con el paso indicado. El componente se coloca con transformación local identidad en el socket. Los identificadores distinguen mayúsculas y minúsculas.

| Tablero | Identificador lógico / socket en el modelo | Pieza | Posición local o distribución |
|---|---|---|---|
| Secundario | `TD_Circuito_01` … `TD_Circuito_06` | `Breaker` | primera `(0.160, 0.250, 0.105)`; paso X `-0.064`; 6 |
| Secundario | `TD_DIFERENCIAL` | `Differential` | `(0.170, 0.380, 0.105)` |
| Secundario | `TD_BORNERAS` | `Terminals` | `(-0.030, 0.374, 0.052)` |
| General | `TGD_Fila1_01` … `TGD_Fila1_08` | `Breaker` | primera `(0.203, 0.345, 0.075)`; paso X `-0.058`; 8. Las primeras cinco tienen identificadores lógicos indicados debajo. |
| General | `TGD_Fila2_01` … `TGD_Fila2_08` | `Breaker` | primera `(0.203, 0.205, 0.075)`; paso X `-0.058`; 8 |
| General | `TGD_MAIN` | `Disconnector` | `(0, 0.510, 0.025)` |
| General | `TGD_BORNERAS` | `Terminals` | `(0, 0.098, -0.055)` |
| Automatización | `CTRL_Circuito_01` … `CTRL_Circuito_06` | `Breaker` | primera `(-0.045, 1.790, -0.105)`; paso X `-0.058`; 6 |
| Automatización | `CTRL_Circuito_07` … `CTRL_Circuito_12` | `Breaker` | primera `(-0.045, 1.585, -0.105)`; paso X `-0.058`; 6 |
| Automatización | `CTRL_CONTACTOR` | `Contactor` | `(0.250, 1.300, -0.105)` |
| Automatización | `CTRL_MEDIDOR` | `Meter` | `(0.323, 1.788, -0.068)` |
| Automatización | `CTRL_BORNERAS` | `Terminals` | `(0, 0.500, -0.105)` |

En el general, los sockets físicos `TGD_Fila1_01` a `05` corresponden, respectivamente, a los IDs lógicos `TGD_AULA1`, `TGD_AULA2`, `TGD_LAB`, `TGD_AREAS` y `TGD_AUTOM`. Use los IDs lógicos con la API. Los `Socket` y `Port` del FBX no se deben desplazar para corregir escala u orientación. La entrada usa tres puertos L/N/PE; el general añade cinco salidas de alimentador. La vista `Preview_Montaje_*` de cada `.blend` sirve para inspeccionar ajustes y no forma parte del gabinete exportado.

## Instalación, retirada y eventos

```csharp
using TablerosEnergia;

// Ejecutar durante Play. El inventario real comprueba existencias antes.
BoardSlot slot = tablero.FindSlot("TD_Circuito_01");
InstallResult resultado = slot.TryInstall(BoardPartKind.Breaker);
if (resultado == InstallResult.Installed) {
    // Descontar una unidad del inventario solamente aquí.
    slot.SwitchControl.SetState(true);
}

// Occupied, Incompatible, MissingPrefab y RequiresPlayMode no consumen piezas.
if (slot.TryRemove(out BoardPartKind piezaDevuelta)) {
    // Devolver piezaDevuelta al inventario.
}
```

`BoardSlot.Changed` y `ModularBoard.Changed` notifican cambios; `EnergyGridRegistry.Instance.Changed` permite observar la red después de crearse el registro. Consulte `RequestedOn` para la posición del mando e `IsEnergized` para el estado efectivo. `SetEstimatedWatts(float)` modifica la carga estimada de un circuito final. El montaje muestra su cableado y la retirada lo oculta. Las conexiones visuales de entrada dependen también de que exista un enlace de alimentación.

## Fuentes, alimentadores y medición

```csharp
// Referencias a instancias concretas de ModularBoard, no a los assets prefab.
general.SetExternalSupply(connected: true, powered: true);
bool aulaConectada = aula1.ConnectFeed(general, "TGD_AULA1");
bool automatizacionConectada = automatizacion.ConnectFeed(general, "TGD_AUTOM");
// Un false indica salida inexistente/incompatible o un ciclo.
general.SetExternalSupply(connected: true, powered: false); // cable sigue conectado
general.SetExternalSupply(connected: true, powered: true);  // restablece
// aula1.DisconnectSupply(); // desconecta y retira el cable visual entre tableros
```

Instale bornes en cada tablero, el seccionador en el general, el diferencial en el secundario y el contactor en automatización. Conecte la fuente y los alimentadores, y encienda los mandos necesarios. Una alimentación ausente o interrumpida desenergiza las salidas sin alterar las posiciones de los mandos. Una conexión explícita crea tres tramos visuales diferenciados de fase, neutro y tierra; retirarla los oculta. El diferencial admite prueba y rearme, y el contactor acepta `SetControlSignal(bool)`; consulte el manual de componentes.

El medidor instalado usa `TextMesh` y muestra `SIN RED` sin alimentación. Suma una sola vez las cargas finales energizadas de los tableros conectados a la misma raíz de red. Los alimentadores no se cuentan como cargas. Los valores configurados son estimaciones de juego, no especificaciones eléctricas: con todas las piezas de los tres tableros de ejemplo instaladas y activas se obtienen 29 cargas y 23,29 kW. No se simulan sobrecargas ni generación.

## Control de prueba

`BoardDemoPlayer` es un adaptador opcional: añádalo a un objeto con `CharacterController` y cámara hija. Seleccione `1` termomagnético, `2` seccionador, `3` bornes, `4` diferencial, `5` contactor o `6` medidor. Apunte al anclaje y pulse `E` para instalar; con una pieza instalada, `E` opera su mando, y `R` la retira y devuelve al inventario de prueba. Apunte al botón de prueba del diferencial o a una puerta y pulse `E`. El inventario inicial es solamente de prueba; sustitúyalo por la API anterior. `WASD` desplaza, ratón orienta, espacio salta y Escape libera el cursor.

## Referencias de diseño

El modelo simplifica para juego el montaje en riel y la separación de conductores: [ABB, distribución modular](https://library.e.abb.com/public/95b5d614fffd4e469d7143f21aac4956/2CSC423013B0201.pdf), [Rittal, entradas de cable](https://www.rittal.com/in-en/products/PG20231215ZUB101/PG20240405ZUB003/PG20240405ZUB007/PRO13270?variantId=8800120) y [ABB, contactores](https://library.e.abb.com/public/589073c96e10473c8ccda73fd1fcd2ca/2CDC103022M0201.pdf). No constituye un plano de instalación eléctrica real.
