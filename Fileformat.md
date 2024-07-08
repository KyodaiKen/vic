# VIC File structure

All words are little endian.

```ps
[main header]
for each frame {
    [frame header]
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
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | ----------------------- | ------------------------------------ | -- |
| uint    | MAGIC_WORD              | 0x56494300 VIC + 0xBD                | |
| byte    | Usage                   | Gallery / Pages or Animation         | |
| uint    | METADATA_NUM_FIELDS     | Number of metadata fields            | |
| uint[]  | METADATA_FIELD_LENGTHS  |                                      | |
|         | **For each field**      |                                      | |
| byte[]  | METADATA_DATA           |                                      | |
|         | **End for each**        |                                      | |

For "Custom", the following metadata fields have to be added (example values):

```json
{
    "chinf": 
    {
        "count": 8,

        /* Identificators */
        "id:0": "red",
        "id:1": "green",
        "id:2": "blue",
        "id:3": "ir",
        "id:4": "uv",
        "id:5": "rgb_alpha",
        "id:6": "ir_alpha",
        "id:7": "uv_alpha",

        /* Alpha mode */
        "am:5": "s", //Straight
        "am:6": "p", //Premultiplied
        "am:7": "p"  //Premultiplied
    }
}
```
## Usage table
| Name                       | Value |
| -------------------------- | ----- |
| Gallery                    | 0     |
| Animation                  | 1     |

# Metadata header
| Data Type | Field Name            | Contents                          |
| ------- | -----------             | --------------------------------- |
| uint    | METADATA_FIELD_ID       | Metadata field ID                 |
| uint    | METADATA_REF_FIELD_ID   | Field ID referencing to another   |
| byte    | METADATA_TYPE_LEN       | Length of type field              |
| byte[]  | METADATA_TYPE           | Type (UTF-8 string)               |
| byte[]  | METADATA                | Metadata bytes, text is UTF-8     |

# Frame header
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | -----------             | ---------------------------------    | -- |
| uint    | FRM_MAGIC_WORD          | 0x564652FB / VFR + 0xFB              | |
| ulong   | FRM_SEQ_NBR             | Frame sequence number                | |
| byte    | FRM_TYPE                | Frame Type (see table)               | enum |
| uint    | FRM_WIDTH               |                                      | |
| uint    | FRM_HEIGHT              |                                      | |
| byte    | FRM_COLOR_SPACE         | Color Space (RGB, RGBA...)           | enum |
| byte    | FRM_CH_DATA_FORMAT      | Channel Data Format                  | enum |
| double  | FRM_PPI_RES_H           | Horizontal resolution in pixels/in   | |
| double  | FRM_PPI_RES_V           | Vertical resolution in pixels/in     | |
| ushort  | FRM_TILE_BASE_DIM       | Time base dimension in pixels        | Describes horizontal and vertical size with a single value |
| byte    | FRM_NAME_LEN            | Length of frame name in bytes        | |
| ushort  | FRM_DESCR_LEN           | Length of frame description in bytes | |
| uint    | FRM_DISPL_DUR           | Frame display duration in usec       | |
| uint    | FRM_METADATA_FLD_CNT    | Number of metadata fields            | |
| uint[]  | FRM_METADATA_FLD_SZ     | Metadata field sizes                 | |
| ulong   | FRM_NUM_LAYERS          | Frame layer count                    | |
| byte[]  | FRM_NAME_STR            | Frame name as byte array for UTF-8   | |
| byte[]  | FRM_DESCR_STR           | Frame descr as byte array for UTF-8  | |
|         | **For each field**      |                                      | |
| byte[]  | FRM_METADATA_DATA       |                                      | |
|         | **End for each**        |                                      | |

## Frame type table
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
| Custom                     | 12    |

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

# Layer header (within FRM_LAYER_DATA starting at layer offset 0)
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | -----------             | ---------------------------------    | -- |
| uint    | LAYER_MAGIC_WORD        | 0x564C52DB / VLR + 0xDB              | |
| byte    | LAYER_NAME_LEN          | Length of layer name in bytes        | |
| ushort  | LAYER_DESCR_LEN         | Length of layer description in bytes | |
| uint    | LAYER_WIDTH             |                                      | |
| uint    | LAYER_HEIGHT            |                                      | |
| uint    | LAYER_OFFSET_X          | Pixel offset where layer is placed   | |
| uint    | LAYER_OFFSET_Y          | Pixel offset where layer is placed   | |
| byte    | LAYER_BLEND_MODE        |                                      | enum |
| double  | LAYER_OPACITY           | between 0 and 1                      | |
| uint    | LAYER_METADATA_FLD_CNT  | Number of metadata fields            | |
| uint[]  | LAYER_METADATA_FIELD_SZ | Metadata field sizes                 | |
| byte[]  | LAYER_NAME_STR          | Layer name as byte array for UTF-8   | |
| byte[]  | LAYER_DESCR_STR         | Layer name as byte array for UTF-8   | |
|         | **For each field**      |                                      | |
| byte[]  | LAYER_METADATA_DATA     |                                      | |
|         | **End for each**        |                                      | |

## Layer blend mode table
| Name                       | Value |
| -------------------------- | ----- |
| Normal                     | 0     |
| Multiply                   | 1     |
| Divide                     | 2     |
| Add                        | 3     |
| Subtract                   | 4     |

# Tile data with header (after layer header)
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | -----------             | ---------------------------------    | -- |
| uint    | TILE_MAGIC_WORD         | Byte 0-2: KTL; Byte 3: RESERVED      | Reserved byte must be FF in Version 0 |
| byte    | TILE_ALGORITHM          | Compression algorithm used on this tile | enum |
| uint    | TILE_DATA_LENGTH        | Length of tile data                  | |

## Tile algorithm table
| Name                       | Value | Notes                                    |
| -------------------------- | ----- | ---------------------------------------- |
| JPEG                       | 0     | |
| WEBP                       | 1     | |
| AVIF                       | 2     | |
| JXL                        | 3     | |
| FFV1                       | 4     | |
| HEIC                       | 5     | |
| VICLL - None / LZW         | 128   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - None / ZLIB        | 129   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - None / BROTLI      | 130   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - None / XZ          | 131   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - None / LZMA        | 132   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - None / AC          | 133   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / LZW          | 134   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / ZLIB         | 135   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / BROTLI       | 136   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / XZ           | 137   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / LZMA         | 138   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Sub / AC           | 139   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / LZW           | 140   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / ZLIB          | 141   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / BROTLI        | 142   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / XZ            | 143   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / LZMA          | 144   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Up / AC            | 145   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / LZW      | 146   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / ZLIB     | 147   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / BROTLI   | 148   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / XZ       | 149   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / LZMA     | 150   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Average / AC       | 151   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / LZW        | 152   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / ZLIB       | 153   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / BROTLI     | 154   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / XZ         | 155   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / LZMA       | 156   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - Paeth / AC         | 157   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / LZW          | 158   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / ZLIB         | 159   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / BROTLI       | 160   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / XZ           | 161   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / LZMA         | 162   | VIC LOSSLESS CODEC: Filter / Compression |
| VICLL - JXL / AC           | 163   | VIC LOSSLESS CODEC: Filter / Compression |