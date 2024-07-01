# VIC - Versatile Image Coding / Container

This is an image container format for JPEG, WEBP, AVIF, JXL, FLV1 and potentially more image formats to add features to them beyond the capabilities of even TIFF.

## Features
- Tiling images for viewing extremely large images with as little memory usage as possible
- Adding more efficient multi threading capabilities
- Pages or Frames - Photo gallery or animation? You decide.
- Layer support with layer blend modes and layer offsets
- Relational metadata capabilities
- Streamable format layout
- Supports all pixel formats and color spaces like the wrapped compression formats do

## Limitations
|                                     | Limit                                       |
| ----------------------------------- | ------------------------------------------- |
| Maximum number of frames            | Limited by storage and memory constraints   |
| Maximum number of layers            | Limited by storage and memory constraints   |
| Max tiles per layer                 | Limited by storage and memory constraints   |
| Max image, frame & layer dimensions | 4294967295 x 4294967295 (Limited to 16 EiB) |
| Max tile dimensions                 | 65535 x 65535 (Limited to 4 GiB)            |
| Max tile data size                  | 4 GiB                                       |
| Max number of metadata fields       | 4294967295                                  |
| Max size of metadata field          | 4 GiB                                       |

## Format information
### Mime types
| Mime type   | File extensio   |
| ---------   | --------------- |
| `image/vic` | `.vic`          |

### Format structure
See: [File format](Fileformat.md)
