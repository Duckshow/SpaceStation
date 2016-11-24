using UnityEngine;
using System.Collections;

public class CachedAssets : MonoBehaviour {

    public static CachedAssets Instance;

    [System.Serializable]
    public struct OrientedAsset {
        [SerializeField] private Sprite Back;
        [SerializeField] private Sprite Front;
        [SerializeField] private Sprite Left;
        [SerializeField] private Sprite Right;

        public Sprite GetOrientedAsset(ActorOrientation.OrientationEnum _orientation) {
            switch (_orientation) {
                case ActorOrientation.OrientationEnum.Down:
                    return Front;
                case ActorOrientation.OrientationEnum.Up:
                    return Back;
                case ActorOrientation.OrientationEnum.Left:
                    return Left;
                case ActorOrientation.OrientationEnum.Right:
                    return Right;
            }

            return null;
        }
    }
    [System.Serializable]
    public class ShadedAsset {
        public Sprite Diffuse;
        public Sprite Normal;
        public Sprite Emissive;
        public Sprite Specular;

        public enum TextureType { Diffuse, Normal, Emissive, Specular }
        public void SetTexture(TextureType _type, Sprite _sprite) {
            switch (_type) {
                case TextureType.Diffuse:
                    Diffuse = _sprite;
                    break;
                case TextureType.Normal:
                    Normal = _sprite;
                    break;
                case TextureType.Emissive:
                    Emissive = _sprite;
                    break;
                case TextureType.Specular:
                    Specular = _sprite;
                    break;
            }
        }
    }

    [System.Serializable]
    public class WallSet {

        public Texture2D SpriteSheet_Diffuse;
        public Texture2D SpriteSheet_Normal;
        public Texture2D SpriteSheet_Emissive;
        public Texture2D SpriteSheet_Specular;

        [HideInInspector] public ShadedAsset Single;
        [HideInInspector] public ShadedAsset FourWay;

        [HideInInspector] public ShadedAsset Vertical_T;
        [HideInInspector] public ShadedAsset Vertical_M;
        [HideInInspector] public ShadedAsset Vertical_B;

        [HideInInspector] public ShadedAsset[] DoorVertical_Animation = new ShadedAsset[4];
        //[HideInInspector] public ShadedAsset DoorVertical_f0;
        //[HideInInspector] public ShadedAsset DoorVertical_f1;
        //[HideInInspector] public ShadedAsset DoorVertical_f2;
        //[HideInInspector] public ShadedAsset DoorVertical_f3;

        [HideInInspector] public ShadedAsset Horizontal_L;
        [HideInInspector] public ShadedAsset Horizontal_M;
        [HideInInspector] public ShadedAsset Horizontal_R;

        [HideInInspector] public ShadedAsset[] DoorHorizontal_Bottom_Animation = new ShadedAsset[4];
        //[HideInInspector] public ShadedAsset DoorHorizontal_Bottom_f0;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Bottom_f1;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Bottom_f2;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Bottom_f3;

        [HideInInspector] public ShadedAsset[] DoorHorizontal_Top_Animation = new ShadedAsset[4];
        //[HideInInspector] public ShadedAsset DoorHorizontal_Top_f0;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Top_f1;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Top_f2;
        //[HideInInspector] public ShadedAsset DoorHorizontal_Top_f3;

        [HideInInspector] public ShadedAsset Corner_TopLeft;
        [HideInInspector] public ShadedAsset Corner_TopRight;
        [HideInInspector] public ShadedAsset Corner_BottomRight;
        [HideInInspector] public ShadedAsset Corner_BottomLeft;

        [HideInInspector] public ShadedAsset Tee_Left;
        [HideInInspector] public ShadedAsset Tee_Top;
        [HideInInspector] public ShadedAsset Tee_Right;
        [HideInInspector] public ShadedAsset Tee_Bottom;

        [HideInInspector] public ShadedAsset Diagonal_TopLeft;
        [HideInInspector] public ShadedAsset Diagonal_TopRight;
        [HideInInspector] public ShadedAsset Diagonal_BottomRight;
        [HideInInspector] public ShadedAsset Diagonal_BottomLeft;
    }

    [Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;

    [Header("Grid Assets")]
    public WallSet[] WallSets;


    void Awake() {
        Instance = this;

        for (int i = 0; i < WallSets.Length; i++) {
            LoadAndAssignSprites(WallSets[i].SpriteSheet_Diffuse, i, ShadedAsset.TextureType.Diffuse);
            LoadAndAssignSprites(WallSets[i].SpriteSheet_Normal, i, ShadedAsset.TextureType.Normal);
            LoadAndAssignSprites(WallSets[i].SpriteSheet_Emissive, i, ShadedAsset.TextureType.Emissive);
            LoadAndAssignSprites(WallSets[i].SpriteSheet_Specular, i, ShadedAsset.TextureType.Specular);
        }
    }

    void LoadAndAssignSprites(Texture2D _spriteSheet, int _wallSetIndex, ShadedAsset.TextureType _type) {
        Sprite[] _sprites = Resources.LoadAll<Sprite>("Textures/" + _spriteSheet.name);
        if (_sprites == null || _sprites.Length == 0)
            throw new System.Exception(_spriteSheet.name + " failed to load properly!");

        for (int i = 0; i < _sprites.Length; i++) {
            if (_type == ShadedAsset.TextureType.Normal)
                _sprites[i] = ConvertNormalToUnityFriendly(_sprites[i]);

            switch (_sprites[i].name) {
                case "Single":
                    WallSets[_wallSetIndex].Single.SetTexture(_type, _sprites[i]);
                    break;
                case "FourWay":
                    WallSets[_wallSetIndex].FourWay.SetTexture(_type, _sprites[i]);
                    break;

                // Verticals
                case "Vertical_T":
                    WallSets[_wallSetIndex].Vertical_T.SetTexture(_type, _sprites[i]);
                    break;
                case "Vertical_M":
                    WallSets[_wallSetIndex].Vertical_M.SetTexture(_type, _sprites[i]);
                    break;
                case "Vertical_B":
                    WallSets[_wallSetIndex].Vertical_B.SetTexture(_type, _sprites[i]);
                    break;

                // DoorVerticals
                case "DoorVertical_f0":
                    WallSets[_wallSetIndex].DoorVertical_Animation[0].SetTexture(_type, _sprites[i]);

                    //WallSets[_wallSetIndex].DoorVertical_f0.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorVertical_f1":
                    WallSets[_wallSetIndex].DoorVertical_Animation[1].SetTexture(_type, _sprites[i]);

                    //WallSets[_wallSetIndex].DoorVertical_f1.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorVertical_f2":
                    WallSets[_wallSetIndex].DoorVertical_Animation[2].SetTexture(_type, _sprites[i]);

                    //WallSets[_wallSetIndex].DoorVertical_f2.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorVertical_f3":
                    WallSets[_wallSetIndex].DoorVertical_Animation[3].SetTexture(_type, _sprites[i]);

                    //WallSets[_wallSetIndex].DoorVertical_f3.SetTexture(_type, _sprites[i]);
                    break;

                // Horizontals
                case "Horizontal_L":
                    WallSets[_wallSetIndex].Horizontal_L.SetTexture(_type, _sprites[i]);
                    break;
                case "Horizontal_M":
                    WallSets[_wallSetIndex].Horizontal_M.SetTexture(_type, _sprites[i]);
                    break;
                case "Horizontal_R":
                    WallSets[_wallSetIndex].Horizontal_R.SetTexture(_type, _sprites[i]);
                    break;

                // DoorHorizontals
                case "DoorHorizontal_Bottom_f0":
                    WallSets[_wallSetIndex].DoorHorizontal_Bottom_Animation[0].SetTexture(_type, _sprites[i]);

      //              WallSets[_wallSetIndex].DoorHorizontal_Bottom_f0.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Bottom_f1":
                    WallSets[_wallSetIndex].DoorHorizontal_Bottom_Animation[1].SetTexture(_type, _sprites[i]);

    //                WallSets[_wallSetIndex].DoorHorizontal_Bottom_f1.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Bottom_f2":
                    WallSets[_wallSetIndex].DoorHorizontal_Bottom_Animation[2].SetTexture(_type, _sprites[i]);

  //                  WallSets[_wallSetIndex].DoorHorizontal_Bottom_f2.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Bottom_f3":
                    WallSets[_wallSetIndex].DoorHorizontal_Bottom_Animation[3].SetTexture(_type, _sprites[i]);

//                    WallSets[_wallSetIndex].DoorHorizontal_Bottom_f3.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Top_f0":
                    WallSets[_wallSetIndex].DoorHorizontal_Top_Animation[0].SetTexture(_type, _sprites[i]);

                    //WallSets[_wallSetIndex].DoorHorizontal_Top_f0.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Top_f1":
                    WallSets[_wallSetIndex].DoorHorizontal_Top_Animation[1].SetTexture(_type, _sprites[i]);

//                    WallSets[_wallSetIndex].DoorHorizontal_Top_f1.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Top_f2":
                    WallSets[_wallSetIndex].DoorHorizontal_Top_Animation[2].SetTexture(_type, _sprites[i]);

//                    WallSets[_wallSetIndex].DoorHorizontal_Top_f2.SetTexture(_type, _sprites[i]);
                    break;
                case "DoorHorizontal_Top_f3":
                    WallSets[_wallSetIndex].DoorHorizontal_Top_Animation[3].SetTexture(_type, _sprites[i]);

//                    WallSets[_wallSetIndex].DoorHorizontal_Top_f3.SetTexture(_type, _sprites[i]);
                    break;

                // Corners
                case "Corner_BottomLeft":
                    WallSets[_wallSetIndex].Corner_BottomLeft.SetTexture(_type, _sprites[i]);
                    break;
                case "Corner_TopLeft":
                    WallSets[_wallSetIndex].Corner_TopLeft.SetTexture(_type, _sprites[i]);
                    break;
                case "Corner_BottomRight":
                    WallSets[_wallSetIndex].Corner_BottomRight.SetTexture(_type, _sprites[i]);
                    break;
                case "Corner_TopRight":
                    WallSets[_wallSetIndex].Corner_TopRight.SetTexture(_type, _sprites[i]);
                    break;

                // Tees
                case "Tee_Bottom":
                    WallSets[_wallSetIndex].Tee_Bottom.SetTexture(_type, _sprites[i]);
                    break;
                case "Tee_Left":
                    WallSets[_wallSetIndex].Tee_Left.SetTexture(_type, _sprites[i]);
                    break;
                case "Tee_Right":
                    WallSets[_wallSetIndex].Tee_Right.SetTexture(_type, _sprites[i]);
                    break;
                case "Tee_Top":
                    WallSets[_wallSetIndex].Tee_Top.SetTexture(_type, _sprites[i]);
                    break;

                // Diagonals
                case "Diagonal_BottomLeft":
                    WallSets[_wallSetIndex].Diagonal_BottomLeft.SetTexture(_type, _sprites[i]);
                    break;
                case "Diagonal_TopLeft":
                    WallSets[_wallSetIndex].Diagonal_TopLeft.SetTexture(_type, _sprites[i]);
                    break;
                case "Diagonal_BottomRight":
                    WallSets[_wallSetIndex].Diagonal_BottomRight.SetTexture(_type, _sprites[i]);
                    break;
                case "Diagonal_TopRight":
                    WallSets[_wallSetIndex].Diagonal_TopRight.SetTexture(_type, _sprites[i]);
                    break;

                
                default:
                    throw new System.NotImplementedException(_sprites[i].name + "hasn't been properly implemented yet!");
            }
        }
    }

    private Texture2D spriteAsTexture;
    private Color[] cachedPixels;
    private Sprite textureAsSprite;
    Sprite ConvertNormalToUnityFriendly(Sprite _sprite) {

        spriteAsTexture = new Texture2D((int)_sprite.rect.width, (int)_sprite.rect.height);
        cachedPixels = _sprite.texture.GetPixels( (int)_sprite.rect.x, (int)_sprite.rect.y, (int)_sprite.rect.width, (int)_sprite.rect.height);
        spriteAsTexture.SetPixels(cachedPixels);
        spriteAsTexture.Apply();

        int _index = 0;
        for (int y = 0; y < spriteAsTexture.height; y++) {
            for (int x = 0; x < spriteAsTexture.width; x++) {
                _index = (y * spriteAsTexture.width) + x;

                cachedPixels[_index].r = spriteAsTexture.GetPixel(x, y).g;
                cachedPixels[_index].g = cachedPixels[_index].r;
                cachedPixels[_index].b = cachedPixels[_index].r;
                cachedPixels[_index].a = spriteAsTexture.GetPixel(x, y).r;
            }
        }

        spriteAsTexture.SetPixels(cachedPixels);
        spriteAsTexture.Apply();

        textureAsSprite = Sprite.Create(spriteAsTexture, new Rect(0, 0, spriteAsTexture.width, spriteAsTexture.height), _sprite.pivot, _sprite.pixelsPerUnit);
        textureAsSprite.name = _sprite.name;

        return textureAsSprite;
    }

    public Color[] GetCachedAssetPixels(Sprite _asset) {
        return _asset.texture.GetPixels(Mathf.RoundToInt(_asset.rect.xMin), Mathf.RoundToInt(_asset.rect.yMin), Mathf.RoundToInt(_asset.rect.width), Mathf.RoundToInt(_asset.rect.height));
    }

    public ShadedAsset GetAssetForTile(Tile.TileType _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _isOnGroundLevel, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
        switch (_tileType) {
            case Tile.TileType.Empty:
                break;
            case Tile.TileType.Wall:
                if (!_isOnGroundLevel) // for now at least
                    return null;

                if (_hasConnection_Left) {
                    if (_hasConnection_Top) {
                        if (_hasConnection_Right) {
                            if (_hasConnection_Bottom) return WallSets[_styleIndex].FourWay;
                            else return WallSets[_styleIndex].Tee_Bottom;
                        }
                        else if (_hasConnection_Bottom) return WallSets[_styleIndex].Tee_Right;
                        else return WallSets[_styleIndex].Corner_TopLeft;
                    }
                    else if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSets[_styleIndex].Tee_Top;
                        else return WallSets[_styleIndex].Horizontal_M;
                    }
                    else if (_hasConnection_Bottom) return WallSets[_styleIndex].Corner_BottomLeft;
                    else return WallSets[_styleIndex].Horizontal_R;
                }
                else if (_hasConnection_Top) {
                    if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSets[_styleIndex].Tee_Left;
                        else return WallSets[_styleIndex].Corner_TopRight;
                    }
                    else if (_hasConnection_Bottom) return WallSets[_styleIndex].Vertical_M;
                    else return WallSets[_styleIndex].Vertical_B;
                }
                else if (_hasConnection_Right) {
                    if (_hasConnection_Bottom) return WallSets[_styleIndex].Corner_BottomRight;
                    else return WallSets[_styleIndex].Horizontal_L;
                }
                else if (_hasConnection_Bottom) return WallSets[_styleIndex].Vertical_T;
                else return WallSets[_styleIndex].Single;

            case Tile.TileType.Diagonal:
                if (!_isOnGroundLevel) // for now at least
                    return null;

                switch (_tileOrientation) {
                    case Tile.TileOrientation.BottomLeft:
                        return WallSets[_styleIndex].Diagonal_BottomLeft;
                    case Tile.TileOrientation.TopLeft:
                        return WallSets[_styleIndex].Diagonal_TopLeft;
                    case Tile.TileOrientation.TopRight:
                        return WallSets[_styleIndex].Diagonal_TopRight;
                    case Tile.TileOrientation.BottomRight:
                        return WallSets[_styleIndex].Diagonal_BottomRight;
                }
                break;
            case Tile.TileType.Door:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _isOnGroundLevel ? null : WallSets[_styleIndex].DoorVertical_Animation[0];
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _isOnGroundLevel ? WallSets[_styleIndex].DoorHorizontal_Bottom_Animation[0] : WallSets[_styleIndex].DoorHorizontal_Top_Animation[0];
                }
                break;
            default:
                throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
        }

        return null;
    }

    public ShadedAsset[] GetAnimationForTile(Tile.TileType _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _getBottomLayer) {
        switch (_tileType) {
            case Tile.TileType.Door:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _getBottomLayer ? null : WallSets[_styleIndex].DoorVertical_Animation;
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _getBottomLayer ? WallSets[_styleIndex].DoorHorizontal_Bottom_Animation : WallSets[_styleIndex].DoorHorizontal_Top_Animation;
                }
                break;
            default:
                throw new System.NotImplementedException(_tileType + " doesn't appear to have an animation!");
        }

        return null;
    }
}
