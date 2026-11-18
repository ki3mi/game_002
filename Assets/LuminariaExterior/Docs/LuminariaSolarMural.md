# LuminariaSolarMural

## Brief y estructura

- **Uso:** instalación eléctrica exterior en pared.
- **Forma:** carcasa compacta con panel fotovoltaico inclinado, placa de anclaje, soporte, fijaciones, óptica frontal, difusor inferior y sensor visual.
- **Partes exportables independientes:** Carcasa, PanelSolar, PlacaMural, Soporte, dos Fijaciones, Optica, Difusor y Sensor. Sin marca, texto, cableado ni certificación ficticia.
- **Pivote:** centro de la placa mural, sobre la cara de contacto con la pared (Y=0). El frente mira hacia -Y.

## Referencia de escala

Ancla dimensional: ficha del fabricante [TPK Lighting SWL-1006](https://www.tpk-led.com/products/solar-wall-light/swl-1006/), que publica W150 × L235 × H130 mm. La ficha describe un equipo mural solar con panel superior inclinado, sensor y salida óptica frontal/inferior.

**Supuesto de ejes:** interpreto L=235 mm como ancho frontal X, W=150 mm como proyección Y desde pared y H=130 mm como altura Z. El modelo es una interpretación estilizada de esa envolvente, no una réplica de diseño comercial ni una declaración de prestaciones.

- Factor visual uniforme: ×1.0.
- Bounds locales finales Blender (X, Y, Z): **0.235 × 0.150 × 0.130 m**.
- Mínimos XYZ: **(-0.1175, -0.1500, -0.0325) m**.
- Máximos XYZ: **(0.1175, 0.0000, 0.0975) m**.
- Bounds Unity tras conversión Z-up→Y-up: **0.235 × 0.130 × 0.150 m**.

## Producción y revisión

- Blender: 9 mallas exportables, 440 triángulos estimados, 0 aristas no manifold; root y transforms de malla en identidad.
- Paleta/materiales: carcasa #568DB9, panel solar #355B9E, metal gris azulado #98A0A9, ópticas crema #F5D18D.
- Poly Haven MCP: se intentó una búsqueda de modelo solar mural. El addon conectado está en protocolo 15 y el servidor espera 16; reportó la librería Poly Haven deshabilitada. No se importó ningún recurso.
- Archivos de fuente: LuminariaSolarMural.blend, Scripts/Build_LuminariaSolarMural.py, Previews/LuminariaSolarMural_Revision.png.
- FBX estático: exportado desde la colección del asset, excluyendo Preview, a Models/LuminariaSolarMural.fbx.

## Validación

- Blender: copia guardada con save_as_mainfile(copy=True). La geometría se volvió a medir en la escena activa tras el guardado: bounds exactos, pivote identidad, 440 tris y 0 aristas no manifold. El modo seguro del Blender MCP no permite reabrir archivos; por eso no se hizo una segunda medición tras reabrir el archivo.
- UnityBuildTemp: instancia verificada en C:/Users/Rick/Documents/Blender/Assets/UnityBuildTemp; pipeline URP activo y shader Universal Render Pipeline/Lit disponible. Importer: scale=1, useFileScale=true, bakeAxisConversion=false, importAnimation=false.
- Prefab y cuatro materiales URP/Lit se crearon y validaron en la copia temporal: root en posición cero, rotación identidad y escala uno; 9 meshes, 440 triángulos, hijos con escala 1; bounds 0.235 × 0.130 × 0.150 m. GUID del FBX, los cuatro materiales y el prefab eran únicos dentro de la copia de prueba.
- La inspección visual se hizo en Prefab Stage y esa etapa se cerró. La captura temporal generada por Unity MCP fuera de la carpeta exclusiva se eliminó junto con su .meta. MAIN.unity quedó como escena activa y sin guardar cambios.
- La consola solo mostró el warning previo de MCP: WebSocket no inicializado; no hubo errores nuevos de importación/materiales/prefab.

## Estado de entrega

- Entrega final en `Models/LuminariaSolarMural.fbx`, cuatro materiales `.mat` en `Materials/` y `Prefabs/LuminariaSolarMural.prefab`, cada uno acompañado por el `.meta` generado por Unity.
- Los seis archivos y sus sidecars de GUID se copiaron desde `_LuminariaSolarMural_Validation` después de validarlos; la comparación SHA-256 de los assets coincide. GUID únicos en el proyecto de pruebas y 21 GUID distintos en los `.meta` de esta entrega familiar.
- El prefab validado usa URP/Lit en sus 9 slots, contiene 9 mallas y 440 triángulos, root en origen con rotación identidad y escala uno, y bounds Unity de **0.235000 × 0.130000 × 0.150000 m**.
- La vista temporal se cerró y eliminó su captura y `.meta`. `MAIN.unity` sigue activa y sin cambios pendientes; no se guardaron escenas. No hay Prefab Stage abierto.
- La consola no registró errores de importación, materiales ni prefab. Solo aparece el warning preexistente de MCP: WebSocket no inicializado.
- **Postflight Blender pendiente:** el Blender MCP seguro no permite reabrir el `.blend`. Se midió la escena activa después de guardar la copia, pero no se hizo la revisión independiente del archivo en un proceso Blender separado.
- Poly Haven no pudo consultarse: el addon conectado usa protocolo 15 mientras el servidor requiere 16 y reporta la biblioteca deshabilitada. No se importó ningún recurso externo.
