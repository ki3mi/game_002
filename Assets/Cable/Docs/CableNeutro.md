# CableNeutro

## Brief
Conductor unipolar neutro para montaje eléctrico interior, estático, presentado como un tramo flexible en U con dos puntas de cobre expuestas. No incluye conectores, marcas ni datos eléctricos impresos.

## Referencia dimensional
Fuente: [Prysmian, ficha Wirepol CPRO Rígido](https://es-stage.prysmian.com/sites/es.prysmian.com/files/media/documents/Wirepol-Rigido-H07V-U-H07V-R-Eca-MAR23.pdf). La ficha incluye una construcción unipolar de 1 × 1,5 mm², espesor nominal de aislamiento de 0,7 mm, diámetro exterior aproximado de 3,2 mm y entre los colores disponibles incluye gris. Se usa como ancla dimensional y cromática; el modelo no representa un producto concreto.

## Supuestos y escala
- Longitud de referencia del tramo: 1,0 m, supuesto de producción porque la ficha no prescribe la longitud de cada pieza.
- Aislamiento: 0,95 m; puntas peladas: 0,025 m por extremo.
- El área de cobre de 1,5 mm² se representa mediante una sección circular equivalente.
- Factor visual uniforme: ×2 para lectura en cámara de juego. No es una especificación de fabricante.
- Dimensiones finales: 2,0 m de longitud total, aislamiento de 6,4 mm de diámetro, puntas peladas de 0,05 m por extremo.
- Pivote: centro de la huella en XY y suelo en Z=0; raíz identidad; Z vertical en Blender.

## Geometría y cotas
Partes exportables: CableNeutro_Aislante, CableNeutro_PuntaInicio, CableNeutro_PuntaFin; raíz CableNeutro_Root. Dos materiales exportables: M_CableNeutro_Aislante y M_CableNeutro_Cobre.

Bounds medidos en Blender (m):
- Mínimo XYZ: (-0,607979; -0,327394; 0,000000)
- Máximo XYZ: (0,607979; 0,330594; 0,006400)
- Dimensiones XYZ: 1,215958 × 0,657989 × 0,006400 m
- Triángulos: 420

## Poly Haven
Se intentó buscar un cable mediante Blender MCP. La búsqueda no pudo completarse: el addon conectado usa protocolo 15 frente al 16 requerido y reporta Poly Haven desactivado. No se importó ningún recurso externo; el cable eléctrico se modeló con bpy conforme al protocolo.

## Archivos
- Modelo: CableNeutro.blend
- Script: Scripts/CableNeutro.py
- Vista de revisión: Previews/CableNeutro_Revision.png
- Exportación: Unity/Models/CableNeutro.fbx
