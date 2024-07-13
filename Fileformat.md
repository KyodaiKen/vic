# VIC File structure

All words are little endian.

```ps
[main header]
for each image object {
    [image object header]
    for each layer {
        [layer header]
        for each tile {
            [tile header]
            [compressed tile data]
        }
    }
}
```

# Main header
| Data Type | Field Name            | Contents                             |
| ------  | ----------------------- | ------------------------------------ |
| uint    | MAGIC_WORD              | For Animation: `0x56494341 VICA`, <br>for Image Collection `0x56494343 VICC` |
| uint    | METADATA_FLD_CNT        | Number of metadata fields            |
| uint[]  | METADATA_FLD_SZ         |                                      |
|         | **For each field**      |                                      |
| byte[]  | METADATA_DATA           |                                      |
|         | **End for each**        |                                      |

# Metadata header
| Data Type | Field Name            | Contents                          |
| ------- | -----------             | --------------------------------- |
| byte    | METADATA_TYPE_LEN       | Length of type field              |
| byte[]  | METADATA_TYPE           | Type (UTF-8 string)               |
| byte[]  | METADATA                | Metadata bytes, text is UTF-8     |

# Image Object header
| Data Type | Field Name             | Contents                             | Extended Info |
| ------  | -----------              | ---------------------------------    | -- |
| uint    | IOBJ_MAGIC_WORD          | `0x494F424A IOBJ`                    | |
| ulong   | IOBJ_SEQ_NBR             | Frame sequence number                | |
| byte    | IOBJ_TYPE                | Frame Type (see table)               | enum |
| uint    | IOBJ_WIDTH               |                                      | |
| uint    | IOBJ_HEIGHT              |                                      | |
| byte    | IOBJ_COLOR_SPACE         | Color Space (RGB, RGBA...)           | enum |
| byte    | IOBJ_CH_DATA_FORMAT      | Channel Data Format                  | enum |
| double  | IOBJ_PPI_RES_H           | Horizontal resolution in pixels/in   | |
| double  | IOBJ_PPI_RES_V           | Vertical resolution in pixels/in     | |
| ushort  | IOBJ_TILE_BASE_DIM       | Time base dimension in pixels        | Describes horizontal and vertical size with a single value |
| uint    | IOBJ_DISPL_DUR           | Frame display duration in usec       | |
| ulong   | IOBJ_NUM_LAYERS          | Frame layer count                    | |
| uint    | IOBJ_METADATA_FLD_CNT    | Number of metadata fields            | |
| uint[]  | IOBJ_METADATA_FLD_SZ     | Metadata field sizes                 | |
|         | **For each field**       |                                      | |
| byte[]  | IOBJ_METADATA_DATA       |                                      | |
|         | **End for each**         |                                      | |

## Image Object Type Table
| Name                       | Value |
| -------------------------- | ----- |
| KeyFrame                   | 0     |
| Predicted                  | 1     |
| BiDirectionalPred          | 2     |

## Color Space Table
| Name                       | Value |
| -------------------------- | ----- |
| GRAY                       | 0     |
| GRAYA_Straight             | 1     |
| GRAYA_PreMult              | 2     |
| RGB                        | 3     |
| RGBA_Straight              | 4     |
| RGBA_PreMult               | 5     |
| CMYK                       | 6     |
| CMYKA_Straight             | 7     |
| CMYKA_PreMult              | 8     |
| YCrCb                      | 9     |
| YCrCbA_Straight            | 10    |
| YCrCbA_PreMult             | 11    |

## Channel Data Format Table
| Name                       | Value | No. Bits | Data Type    |
| -------------------------- | ----- | -------- | ------------ |
| UINT8                      | 0     | 8        | Unsigned Int |
| UINT16                     | 1     | 16       | Unsigned Int |
| UINT32                     | 2     | 32       | Unsigned Int |
| UINT64                     | 3     | 64       | Unsigned Int |
| UINT128                    | 4     | 128      | Unsigned Int |
| FLOAT16                    | 5     | 16       | IEEE Float   |
| FLOAT32                    | 6     | 32       | IEEE Float   |
| FLOAT64                    | 7     | 64       | IEEE Float   |
| FLOAT128                   | 8     | 128      | IEEE Float   |

# Layer header (after IOBJ header for each layer)
| Data Type | Field Name            | Contents                             |
| ------  | -----------             | ---------------------------------    |
| uint    | LAYER_WIDTH             |                                      |
| uint    | LAYER_HEIGHT            |                                      |
| uint    | LAYER_OFFSET_X          | Pixel offset where layer is placed   |
| uint    | LAYER_OFFSET_Y          | Pixel offset where layer is placed   |
| byte    | LAYER_BLEND_MODE        |                                      |
| double  | LAYER_OPACITY           | between 0 and 1                      |
| uint    | LAYER_METADATA_FLD_CNT  | Number of metadata fields            |
| uint[]  | LAYER_METADATA_FIELD_SZ | Metadata field sizes                 |
|         | **For each field**      |                                      |
| byte[]  | LAYER_METADATA_DATA     |                                      |
|         | **End for each**        |                                      |

## Layer blend mode table
| Name                       | Value |
| -------------------------- | ----- |
| Normal                     | 0     |
| Multiply                   | 1     |
| Divide                     | 2     |
| Add                        | 3     |
| Subtract                   | 4     |

# Tile data with header (after layer header for each tile)
| Data Type | Field Name            | Contents                             |
| ------  | -----------             | ---------------------------------    |
| byte    | TILE_ALGORITHM          | Compression algorithm used on this tile |
| byte    | TILE_FLAGS              | Extended information about the compression used |
| uint    | TILE_DATA_LENGTH        | Length of tile data                  |

## Tile Algorithm Table
| Name                       | Value |
| -------------------------- | ----- |
| JPEG                       | 0     |
| WEBP                       | 1     |
| AVIF                       | 2     |
| JXL                        | 3     |
| FFV1                       | 4     |
| HEIC                       | 5     |
| VICLL_LZW                  | 128   |
| VICLL_LZ4                  | 129   |
| VICLL_SNAPPY               | 130   |
| VICLL_AC                   | 131   |
| VICLL_ZLIB                 | 132   |
| VICLL_BROTLI               | 133   |
| VICLL_XZ                   | 134   |
| VICLL_LZMA                 | 135   |

## Tile Flags Table
| Name                       | Value |
| -------------------------- | ----- |
| JPEG                       | 0     |
| WEBP                       | 1     |
| AVIF                       | 2     |
| JXL                        | 3     |
| FFV1                       | 4     |
| HEIC                       | 5     |
| VICLL                      | 255   |

# Metadata for image collectionss
## Naming and directory structure

| Location       | Metadata Field Type | Content description |
| -------------- | ------------------- | ------------------- |
| Image Object   | name                | Data bytes of the name represented in UTF-8 |
| Image Object   | path                | Data bytes of the path (separated using /) represented in UTF-8 |
| Image Object   | description         | Data bytes of the description represented in UTF-8 |

* `path` can be missing to place an image in the root directory
* Empty directories are not supported.