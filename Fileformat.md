# KURIF File structure

All words are little endian.

```
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
| Data Type | Field Name            | Contents                          | Extended Info |
| ------  | ----------------------- | --------------------------------- | -- |
| uint    | MAGIC_WORD              | Byte 0-2: KIF; Byte 3: Version byte  | |
| UUID    | UUID                    | Universally Unique ID of the file | |
| byte    | Usage                   | Gallery / Pages or Animation      | |
| uint    | WIDTH                   |                                   | |
| uint    | HEIGHT                  |                                   | |
| byte    | COMOPSITING_COLOR_SPACE | Color Space (RGB, RGBA...)        | enum |
| byte    | CHANNEL_DATA_FORMAT     | Channel Data Format               | enum |
| double  | PPI_RES_H               | Horizontal resolution in pixels/in| |
| double  | PPI_RES_V               | Vertical resolution in pixels/in  | |
| ushort  | TILE_BASE_DIM           | Time base dimension in pixels     | Describes horizontal and vertical size with a single value |
| uint    | METADATA_NUM_FIELDS     | Number of metadata fields         | |
| uint[]  | METADATA_FIELD_LENGTHS  |                                   | |
|         | **For each field**      |                                   | |
| byte[]  | METADATA_DATA           |                                   | |
|         | **End for each**        |                                   | |

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

## Usage table
| Name                       | Value |
| -------------------------- | ----- |
| Gallery                    | 0     |
| Animation                  | 1     |

# Metadata header
| Data Type | Field Name            | Contents                          | Extended Info |
| ------- | -----------             | --------------------------------- | -- |
| uint    | METADATA_FIELD_ID       | Metadata field ID                 | |
| uint    | METADATA_REF_FIELD_ID   | Field ID referencing to another   | |
| byte[8] | METADATA_TYPE           | Type (8 characters max)           | |
| byte[]  | METADATA                | Metadata bytes, text is UTF-8     | Max size is 65528 |

# Frame header
| Data Type | Field Name          | Contents                             | Extended Info |
| ------  | -----------           | ---------------------------------    | -- |
| uint    | FRM_MAGIC_WORD        | Byte 0-2: KFR; Byte 3: RESERVED      | Reserved byte must be FF in Version 0 |
| ulong   | FRM_SEQ_NBR           | Frame sequence number                | |
| byte    | FRM_NAME_LEN          | Length of frame name in bytes        | |
| ushort  | FRM_DESCR_LEN         | Length of frame description in bytes | |
| uint    | FRM_DISPL_DUR         | Frame display duration in ms         | |
| uint    | METADATA_NUM_FIELDS   | Number of metadata fields            | |
| uint[]  | METADATA_FIELD_LENGTHS|                                      | |
| ulong   | FRM_NUM_LAYERS        | Frame layer count                    | |
| byte[]  | FRM_NAME_STR          | Frame name as byte array for UTF-8   | |
| byte[]  | FRM_DESCR_STR         | Frame descr as byte array for UTF-8  | |
|         | **For each field**    |                                      | |
| byte[]  | METADATA_DATA         |                                      | |
|         | **End for each**      |                                      | |

# Layer header (within FRM_LAYER_DATA starting at layer offset 0)
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | -----------             | ---------------------------------    | -- |
| uint    | LAYER_MAGIC_WORD        | Byte 0-2: KLR; Byte 3: RESERVED      | Reserved byte must be FF in Version 0 |
| byte    | LAYER_NAME_LEN          | Length of layer name in bytes        | |
| ushort  | LAYER_DESCR_LEN         | Length of layer description in bytes | |
| uint    | LAYER_WIDRH             |                                      | |
| uint    | LAYER_HEIGHT            |                                      | |
| uint    | LAYER_OFFSET_X          | Pixel offset where layer is placed   | |
| uint    | LAYER_OFFSET_Y          | Pixel offset where layer is placed   | |
| byte    | LAYER_BLEND_MODE        |                                      | enum |
| double  | LAYER_OPACITY           | between 0 and 1                      | |
| uint    | METADATA_NUM_FIELDS     | Number of metadata fields            | |
| uint[]  | METADATA_FIELD_LENGTHS  |                                      | |
| byte[]  | LAYER_NAME_STR          | Layer name as byte array for UTF-8   | |
| byte[]  | LAYER_DESCR_STR         | Layer name as byte array for UTF-8   | |
|         | **For each field**      |                                      | |
| byte[]  | METADATA_DATA           |                                      | |
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
| uint    | TILE_DATA_LENGTH        | Length of tile data                  | |
| byte    | TILE_COMPRESSION        | Compression algorithm used on this tile | enum |

## Tile compression table
| Name                       | Value |
| -------------------------- | ----- |
| None                       | 0     |
| JPEG                       | 1     |
| WEBP                       | 2     |
| AVIF                       | 3     |
| JXL                        | 4     |
| KURIF                      | 255   |
