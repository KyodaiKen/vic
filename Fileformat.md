# KURIF File structure

All words are little endian.

# Main header
| Data Type | Field Name            | Contents                          | Extended Info |
| ------  | ----------------------- | --------------------------------- | -- |
| uint    | MAGIC_WORD              | Byte 0-2: KIF; Byte 3: Info byte  | |
| UUID    | UUID                    | Universally Unique ID of the file | |
| uint    | WIDTH                   |                                   | |
| uint    | HEIGHT                  |                                   | |
| byte    | COMOPSITING_COLOR_SPACE | Color Space (RGB, RGBA...)        | enum |
| byte    | CHANNEL_DATA_FORMAT     | Channel Data Format               | enum |
| byte    | USAGE                   | Usage (Gallery or Animation)      | enum |
| double  | PPI_RES_H               | Horizontal resolution in pixels/in| |
| double  | PPI_RES_V               | Vertical resolution in pixels/in  | |
| ushort  | TILE_BASE_DIM           | Time base dimension in pixels     | Describes horizontal and vertical size with a single value |
| uint    | METADATA_NUM_FIELDS     | Number of metadata fields         | |
|         | **For each field**      |                                   | |
| byte[]  | METADATA_DATA           |                                   | |
|         | **End for each**        |                                   | |
| uint[]  | METADATA_FIELD_LENGTHS  |                                   | |

## Info byte
| Bit(s) | Field            | Notes                                     |
| ------ | ---------------- | ----------------------------------------- |
| 0-3    | Version 0 ... 15 | Currently 0                               |
| 4      | Usage            | See usage table                           |
| 5-7    | Reseved          | In Version 0, this always has to be 1     |

## Color Space Table
| Name                       | Value |
| -------------------------- | ----- |
| GRAY                       | 1     |
| GRAYA_Straight             | 2     |
| GRAYA_PreMult              | 3     |
| RGB                        | 4     |
| RGBA_Straight              | 5     |
| RGBA_PreMult               | 6     |
| CMYK                       | 7     |
| CMYKA_Straight             | 8     |
| CMYKA_PreMult              | 9     |
| YCrCb                      | 10    |
| YCrCbA_Straight            | 11    |
| YCrCbA_PreMult             | 11    |

## Channel Data Format Table
| Name                       | Value | No. Bits | Data Type    |
| -------------------------- | ----- | -------- | ------------ |
| UINT8                      | 1     | 8        | Unsigned Int |
| UINT16                     | 2     | 16       | Unsigned Int |
| UINT32                     | 3     | 32       | Unsigned Int |
| UINT64                     | 4     | 64       | Unsigned Int |
| FLOAT16                    | 5     | 16       | IEEE Float   |
| FLOAT32                    | 6     | 32       | IEEE Float   |
| FLOAT64                    | 7     | 64       | IEEE Float   |
| FLOAT128                   | 8     | 128      | IEEE Float   |

## Usage table
| Name                       | Value |
| -------------------------- | ----- |
| Gallery                    | 1     |
| Animation                  | 2     |

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
| byte[]  | FRM_NAME_STR          | Frame name as byte array for UTF-8   | |
| ushort  | FRM_DESCR_LEN         | Length of frame description in bytes | |
| byte[]  | FRM_DESCR_STR         | Frame name as byte array for UTF-8   | |
| uint    | FRM_DISPL_DUR         | Frame display duration in ms         | |
| uint    | METADATA_NUM_FIELDS   | Number of metadata fields            | |
|         | **For each field**    |                                      | |
| byte[]  | METADATA_DATA         |                                      | |
|         | **End for each**      |                                      | |
| uint[]  | METADATA_FIELD_LENGTHS|                                      | |
| ulong   | FRM_NUM_LAYERS        | Frame layer count                    | |
|         | **For each layer**    |                                      | |
| byte[]  | FRM_LAYER_DATA        |                                      | |
|         | **End for each**      |                                      | |
| ulong[] | FRM_LAYER_DATA_LENGTHS|                                      | |

# Layer header (within FRM_LAYER_DATA starting at layer offset 0)
| Data Type | Field Name            | Contents                             | Extended Info |
| ------  | -----------             | ---------------------------------    | -- |
| byte    | LAYER_NAME_LEN          | Length of layer name in bytes        | |
| byte[]  | LAYER_NAME_STR          | Layer name as byte array for UTF-8   | |
| ushort  | LAYER_DESCR_LEN         | Length of layer description in bytes | |
| byte[]  | LAYER_DESCR_STR         | Layer name as byte array for UTF-8   | |
| uint    | LAYER_WIDRH             |                                      | |
| uint    | LAYER_HEIGHT            |                                      | |
| uint    | LAYER_OFFSET_X          | Pixel offset where layer is placed   | |
| uint    | LAYER_OFFSET_Y          | Pixel offset where layer is placed   | |
| byte    | LAYER_BLEND_MODE        |                                      | enum |
| double  | LAYER_OPACITY           | between 0 and 1                      | |
| uint    | METADATA_NUM_FIELDS     | Number of metadata fields            | |
|         | **For each field**      |                                      | |
| byte[]  | METADATA_DATA           |                                      | |
|         | **End for each**        |                                      | |
| uint[]  | METADATA_FIELD_LENGTHS  |                                      | |
| ulong   | LAYER_NUM_TILES         | Frame layer count                    | |
|         | **For each tile**       |                                      | |
| byte[]  | LAYER_TILE_DATA         |                                      | |
|         | **End for each**        |                                      | |
| uint[]  | LAYER_TILE_DATA_LENGTHS |                                      | |

## Layer blend mode table
| Name                       | Value |
| -------------------------- | ----- |
| Normal                     | 0     |
| Multiply                   | 1     |
| Divide                     | 2     |
| Add                        | 3     |
| Subtract                   | 4     |


# Tile header (within LAYER_TILE_DATA starting at tile offset 0)
| Data Type | Field Name            | Contents                                | Extended Info |
| ------  | -----------             | ---------------------------------       | -- |
| byte    | TILE_COMPRESSION        | Compression algorithm used on this tile | enum |
| byte[]  | TILE_COMPRESSED_DATA    |                                         | |

## Tile compression table
| Name                       | Value |
| -------------------------- | ----- |
| None                       | 0     |
| JPEG                       | 1     |
| WEBP                       | 2     |
| AVIF                       | 3     |
| JXL                        | 4     |
| KURIF_FAST                 | 253   |
| KURIF_MEDIUM               | 254   |
| KURIF_SLOW                 | 255   |
