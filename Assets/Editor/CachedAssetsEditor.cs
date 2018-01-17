using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(CachedAssets))]
public class CachedAssetsEditor : Editor {

    private CachedAssets thisCA;
    public override void OnInspectorGUI() {
        thisCA = (CachedAssets)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Collider Paths"))
            GenerateShadowColliders();
        // if (GUILayout.Button("Test Apply Collider"))
        //     TestApplyCollider();
    }

    // public void TestApplyCollider(){
    //     // //Debug.Log(thisCA.WallSets);
    //     // //Debug.Log(thisCA.WallSets[0].wall_Single_shadow);
    //     thisCA.ShadowCollider = new CachedAssets.MovableCollider(thisCA.WallSets[0].wall_Single_shadow.Paths.Length);
    //     for (int i = 0; i < thisCA.WallSets[0].wall_Single_shadow.Paths.Length; i++) {
    //         // //Debug.Log("hello? " + thisCA.WallSets[0].wall_Single_shadow[i].Length);
    //         // for (int j = 0; j < thisCA.WallSets[0].wall_Single_shadow[i].Length; i++)
    //         //     //Debug.Log(thisCA.WallSets[0].wall_Single_shadow[i][j]);
    //         thisCA.ShadowCollider.SetPath(i, thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices);
    //         for (int j = 0; j < thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices.Length; j++) {
    //             Debug.Log(thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices[j]);
    //         }
    //     }
    // }

    private const string LOADPATH_SHADOWCOLLIDER_BLUEPRINT = "Assets/Prefabs/ShadowColliders/_ShadowColliderBlueprint.prefab";
    private const string SAVEPATH_SHADOWCOLLIDER = "Assets/Prefabs/ShadowColliders/";

    private int currentWallSetIndex;
    private const int AMOUNT_OF_SHIT = 93;
    private int shitCount = 0;
    private bool stopThePresses = false;
    private delegate void GenerateShadowCollidersForAnim(int _wallSetIndex, CachedAssets.WallSet.Purpose _type, Vector2i[] _anim, ref PolygonCollider2D[] _colliders);
    public void GenerateShadowColliders() {
        shitCount = 0;
        shadowPixels = null;
        stopThePresses = false;

        for (int i = 0; i < thisCA.WallSets.Length; i++) {

            // generals
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Single, CachedAssets.WallSet.wall_Single, ref thisCA.WallSets[i].wall_Single_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Fourway, CachedAssets.WallSet.wall_FourWay, ref thisCA.WallSets[i].wall_FourWay_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Vertical_T, CachedAssets.WallSet.wall_Vertical_T, ref thisCA.WallSets[i].wall_Vertical_T_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Vertical_M, CachedAssets.WallSet.wall_Vertical_M, ref thisCA.WallSets[i].wall_Vertical_M_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Vertical_B, CachedAssets.WallSet.wall_Vertical_B, ref thisCA.WallSets[i].wall_Vertical_B_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Horizontal_L, CachedAssets.WallSet.wall_Horizontal_L, ref thisCA.WallSets[i].wall_Horizontal_L_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Horizontal_M, CachedAssets.WallSet.wall_Horizontal_M, ref thisCA.WallSets[i].wall_Horizontal_M_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Horizontal_R, CachedAssets.WallSet.wall_Horizontal_R, ref thisCA.WallSets[i].wall_Horizontal_R_shadow);

            // corners
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Corner_TR, CachedAssets.WallSet.wall_Corner_TopRight, ref thisCA.WallSets[i].wall_Corner_TopRight_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Corner_TL, CachedAssets.WallSet.wall_Corner_TopLeft, ref thisCA.WallSets[i].wall_Corner_TopLeft_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Corner_BR, CachedAssets.WallSet.wall_Corner_BottomRight, ref thisCA.WallSets[i].wall_Corner_BottomRight_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Corner_BL, CachedAssets.WallSet.wall_Corner_BottomLeft, ref thisCA.WallSets[i].wall_Corner_BottomLeft_shadow);
            
            // tees
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Tee_R, CachedAssets.WallSet.wall_Tee_Right, ref thisCA.WallSets[i].wall_Tee_Right_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Tee_L, CachedAssets.WallSet.wall_Tee_Left, ref thisCA.WallSets[i].wall_Tee_Left_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Tee_T, CachedAssets.WallSet.wall_Tee_Top, ref thisCA.WallSets[i].wall_Tee_Top_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Tee_B, CachedAssets.WallSet.wall_Tee_Bottom, ref thisCA.WallSets[i].wall_Tee_Bottom_shadow);

            // diagonals
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TR, CachedAssets.WallSet.wall_Diagonal_TopRight, ref thisCA.WallSets[i].wall_Diagonal_TopRight_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TR_T, CachedAssets.WallSet.wall_Diagonal_TopRight_T, ref thisCA.WallSets[i].wall_Diagonal_TopRight_T_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TR_R, CachedAssets.WallSet.wall_Diagonal_TopRight_R, ref thisCA.WallSets[i].wall_Diagonal_TopRight_R_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TR_TR, CachedAssets.WallSet.wall_Diagonal_TopRight_TR, ref thisCA.WallSets[i].wall_Diagonal_TopRight_TR_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TL, CachedAssets.WallSet.wall_Diagonal_TopLeft, ref thisCA.WallSets[i].wall_Diagonal_TopLeft_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TL_T, CachedAssets.WallSet.wall_Diagonal_TopLeft_T, ref thisCA.WallSets[i].wall_Diagonal_TopLeft_T_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TL_L, CachedAssets.WallSet.wall_Diagonal_TopLeft_L, ref thisCA.WallSets[i].wall_Diagonal_TopLeft_L_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_TL_TL, CachedAssets.WallSet.wall_Diagonal_TopLeft_TL, ref thisCA.WallSets[i].wall_Diagonal_TopLeft_TL_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BR, CachedAssets.WallSet.wall_Diagonal_BottomRight, ref thisCA.WallSets[i].wall_Diagonal_BottomRight_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BR_B, CachedAssets.WallSet.wall_Diagonal_BottomRight_B, ref thisCA.WallSets[i].wall_Diagonal_BottomRight_B_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BR_R, CachedAssets.WallSet.wall_Diagonal_BottomRight_R, ref thisCA.WallSets[i].wall_Diagonal_BottomRight_R_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BR_BR, CachedAssets.WallSet.wall_Diagonal_BottomRight_BR, ref thisCA.WallSets[i].wall_Diagonal_BottomRight_BR_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BL, CachedAssets.WallSet.wall_Diagonal_BottomLeft, ref thisCA.WallSets[i].wall_Diagonal_BottomLeft_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BL_B, CachedAssets.WallSet.wall_Diagonal_BottomLeft_B, ref thisCA.WallSets[i].wall_Diagonal_BottomLeft_B_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BL_L, CachedAssets.WallSet.wall_Diagonal_BottomLeft_L, ref thisCA.WallSets[i].wall_Diagonal_BottomLeft_L_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.Wall_Diagonal_BL_BL, CachedAssets.WallSet.wall_Diagonal_BottomLeft_BL, ref thisCA.WallSets[i].wall_Diagonal_BottomLeft_BL_shadow);

            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.DoorVertical, CachedAssets.WallSet.anim_DoorVertical_Open.Forward()[0], ref thisCA.WallSets[i].anim_DoorVertical_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.DoorHorizontal, CachedAssets.WallSet.anim_DoorHorizontal_Open.Forward()[0], ref thisCA.WallSets[i].anim_DoorHorizontal_shadow);
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.AirlockVertical, CachedAssets.WallSet.anim_AirlockVertical_Open_L_Bottom.Forward()[0], ref thisCA.WallSets[i].anim_AirlockVertical_shadow, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Open_L, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Open_L_TOP, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Open_R, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Open_R_TOP, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Wait, 
                CachedAssets.WallSet.Purpose.AirlockVertical_Wait_TOP
            );
            GenerateShadowCollider(i, CachedAssets.WallSet.Purpose.AirlockHorizontal, CachedAssets.WallSet.anim_AirlockHorizontal_Open_B_Bottom.Forward()[0], ref thisCA.WallSets[i].anim_AirlockHorizontal_shadow,
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Open_B, 
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Open_B_TOP, 
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Open_T, 
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Open_T_TOP, 
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Wait, 
                CachedAssets.WallSet.Purpose.AirlockHorizontal_Wait_TOP 
            );

            if (stopThePresses){
                Debug.Log("Collider generation was cancelled!".Color(Color.red));
                break;
            }
            else
                Debug.Log(("Finished Wallset #" + i + "!").Color(Color.green));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }
    private List<Color32> shadowPixels;
    private const int TILE_PIXEL_RES = Tile.RESOLUTION;
    private const int maxIndexY = (TILE_PIXEL_RES * 2) - 1;
    private const int maxIndexX = TILE_PIXEL_RES - 1;
    private Color32[][] assetPixels;
    private Dictionary<int, Vector2> startingPixels = new Dictionary<int, Vector2>();
    private int currentX = -1;
    private int currentY = -1;
    private int prevX = -1;
    private int prevY = -1;
    private List<Vector2>[] vertices;
    private int startingPixelindex;
    private int amountOfPixelsInPaths = 0;
    private int pixelsIterated = 0;
    private bool canVisit_L = false;
    private bool canVisit_TL = false;
    private bool canVisit_T = false;
    private bool canVisit_TR = false;
    private bool canVisit_R = false;
    private bool canVisit_BR = false;
    private bool canVisit_B = false;
    private bool canVisit_BL = false;
    private Color32 pixel_L;
    private Color32 pixel_TL;
    private Color32 pixel_T;
    private Color32 pixel_TR;
    private Color32 pixel_R;
    private Color32 pixel_BR;
    private Color32 pixel_B;
    private Color32 pixel_BL;
    private Vector2 newVertex = new Vector2();
    void GenerateShadowCollider(int _wallSetIndex, CachedAssets.WallSet.Purpose _colliderDefinition, Vector2i _texturePos, ref PolygonCollider2D _colliderPrefab, params CachedAssets.WallSet.Purpose[] _additionalIDs) { 
        if(stopThePresses)
            return;

        shitCount++;
        EditorUtility.DisplayProgressBar("Generating Collider Paths", "Collider #" + shitCount.ToString(), (float)shitCount / (float)AMOUNT_OF_SHIT);

        if (shadowPixels == null)
            shadowPixels = new List<Color32>(thisCA.WallSets[_wallSetIndex].ShadowMap.GetPixels32());
        
        assetPixels = new Color32[TILE_PIXEL_RES * 2][];
        for (int y = 0; y < assetPixels.Length; y++)
            assetPixels[y] = new Color32[TILE_PIXEL_RES];
        
        for (int y = 0; y < assetPixels.Length; y++) {
            for (int x = 0; x < assetPixels[y].Length; x++) {
                assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((CachedAssets.WallSet.TEXTURE_SIZE_X * ((_texturePos.y * TILE_PIXEL_RES) + y)) + ((_texturePos.x * TILE_PIXEL_RES) + x))];
            }
        }

        startingPixels.Clear();
        amountOfPixelsInPaths = 0;
        for (int y = 0; y < assetPixels.Length; y++) {
            for (int x = 0; x < assetPixels[y].Length; x++) {
                if(assetPixels[y][x].r > 0)
                    amountOfPixelsInPaths++;
                if (assetPixels[y][x].r == 255 && !startingPixels.ContainsKey(assetPixels[y][x].g)) // only look for 255 for fewer ContainsKey-calls
                    startingPixels.Add(assetPixels[y][x].g, new Vector2(x, y));
            }
        }

        startingPixelindex = 0;
        vertices = new List<Vector2>[startingPixels.Count];
        pixelsIterated = 0;
        foreach (KeyValuePair<int, Vector2> pixel in startingPixels) {

            vertices[startingPixelindex] = new List<Vector2>();
            currentX = (int)pixel.Value.x;
            currentY = (int)pixel.Value.y;
            prevX = currentX;
            prevY = currentY;
            do{
                pixelsIterated++;
                if(EditorUtility.DisplayCancelableProgressBar("Generating Collider Paths", "Collider #" + shitCount.ToString() + "/" + AMOUNT_OF_SHIT + " (Pixel #" + pixelsIterated + "/" + amountOfPixelsInPaths + ")", (float)pixelsIterated / (float)amountOfPixelsInPaths)){
                    stopThePresses = true;
                    break;
                }

                // if "corner", add as vertex
                if (assetPixels[currentY][currentX].r == 255){
                    newVertex.x = currentX;
                    newVertex.y = currentY;
                    vertices[startingPixelindex].Add(newVertex);
                }
                
                canVisit_L = true;
                canVisit_TL = true;
                canVisit_T = true;
                canVisit_TR = true;
                canVisit_R = true;
                canVisit_BR = true;
                canVisit_B = true;
                canVisit_BL = true;

                // are we heading outside asset-boundaries?
                if(currentX == 0){
                    canVisit_L = false;
                    canVisit_TL = false;
                    canVisit_BL = false;
                }
                if(currentX == maxIndexX){
                    canVisit_R = false;
                    canVisit_TR = false;
                    canVisit_BR = false;
                }
                if(currentY == 0){
                    canVisit_B = false;
                    canVisit_BR = false;
                    canVisit_BL = false;
                }
                if(currentY == maxIndexY){
                    canVisit_T = false;
                    canVisit_TR = false;
                    canVisit_TL = false;
                }

                // note: deviating from standard order of direction to prevent cutting corners

                // are we backtracking?
                if(prevX - currentX == -1 && prevY - currentY == 0)
                    canVisit_L = false;
                if(prevX - currentX == 1 && prevY - currentY == 0)
                    canVisit_R = false;
                if(prevX - currentX == 0 && prevY - currentY == 1)
                    canVisit_T = false;
                if(prevX - currentX == 0 && prevY - currentY == -1)
                    canVisit_B = false;
                if(prevX - currentX == -1 && prevY - currentY == 1)
                    canVisit_TL = false;
                if(prevX - currentX == 1 && prevY - currentY == 1)
                    canVisit_TR = false;
                if(prevX - currentX == 1 && prevY - currentY == -1)
                    canVisit_BR = false;
                if(prevX - currentX == -1 && prevY - currentY == 1)
                    canVisit_BL = false;

                // does the direction lead to nowhere?
                if(canVisit_L){
                    pixel_L = assetPixels[currentY][currentX - 1];
                    if(pixel_L.r == 0 || pixel_L.g != pixel.Key)
                        canVisit_L = false;
                    else if(prevX - currentX == 1 && prevY - currentY == 0){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        //Debug.Log("L");
                        continue;
                    }
                }
                if(canVisit_R){
                    pixel_R = assetPixels[currentY][currentX + 1];
                    if(pixel_R.r == 0 || pixel_R.g != pixel.Key)
                        canVisit_R = false;
                    else if(prevX - currentX == -1 && prevY - currentY == 0){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        //Debug.Log("R");
                        continue;
                    }
                }
                if(canVisit_T){
                    pixel_T = assetPixels[currentY + 1][currentX];
                    if(pixel_T.r == 0 || pixel_T.g != pixel.Key)
                        canVisit_T = false;
                    else if(prevX - currentX == 0 && prevY - currentY == -1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentY += 1;
                        //Debug.Log("T");
                        continue;
                    }
                }
                if(canVisit_B){
                    pixel_B = assetPixels[currentY - 1][currentX];
                    if(pixel_B.r == 0 || pixel_B.g != pixel.Key)
                        canVisit_B = false;
                    else if(prevX - currentX == 0 && prevY - currentY == 1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentY -= 1;
                        //Debug.Log("B");
                        continue;
                    }
                }
                if(canVisit_TL){
                    pixel_TL = assetPixels[currentY + 1][currentX - 1];
                    if(pixel_TL.r == 0 || pixel_TL.g != pixel.Key)
                        canVisit_TL = false;
                    else if(prevX - currentX == 1 && prevY - currentY == -1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        currentY += 1;
                        //Debug.Log("TL");
                        continue;
                    }
                }
                if(canVisit_TR){
                    pixel_TR = assetPixels[currentY + 1][currentX + 1];
                    if(pixel_TR.r == 0 || pixel_TR.g != pixel.Key)
                        canVisit_TR = false;
                    else if(prevX - currentX == -1 && prevY - currentY == -1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        currentY += 1;
                        //Debug.Log("TR");
                        continue;
                    }
                }
                if(canVisit_BR){
                    pixel_BR = assetPixels[currentY - 1][currentX + 1];
                    if(pixel_BR.r == 0 || pixel_BR.g != pixel.Key)
                        canVisit_BR = false;
                    else if(prevX - currentX == -1 && prevY - currentY == 1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        currentY -= 1;
                        //Debug.Log("BR");
                        continue;
                    }
                }
                if(canVisit_BL){
                    pixel_BL = assetPixels[currentY - 1][currentX - 1];
                    if(pixel_BL.r == 0 || pixel_BL.g != pixel.Key)
                        canVisit_BL = false;
                    else if(prevX - currentX == 1 && prevY - currentY == 1){ // keep going same direction!
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        currentY -= 1;
                        //Debug.Log("BL");
                        continue;
                    }
                }

                // go somewhere!
                prevX = currentX;
                prevY = currentY;
                if(canVisit_L){
                    currentX -= 1;
                    //Debug.Log("L");
                    continue;
                }
                if(canVisit_R){
                    currentX += 1;
                    //Debug.Log("R");
                    continue;
                }
                if(canVisit_T){
                    currentY += 1;
                    //Debug.Log("T");
                    continue;
                }
                if(canVisit_B){
                    currentY -= 1;
                    //Debug.Log("B");
                    continue;
                }
                if(canVisit_TL){
                    currentX -= 1;
                    currentY += 1;
                    //Debug.Log("TL");
                    continue;
                }
                if(canVisit_TR){
                    currentX += 1;
                    currentY += 1;
                    //Debug.Log("TR");
                    continue;
                }
                if(canVisit_BL){
                    currentX -= 1;
                    currentY -= 1;
                    //Debug.Log("BL");
                    continue;
                }
                if(canVisit_BR){
                    currentX += 1;
                    currentY -= 1;
                    //Debug.Log("BR");
                    continue;
                }

                stopThePresses = true;
                Debug.LogErrorFormat("Failed to find path in shadowmap! | Collider: {0} | Pixel: ({1}, {2})", _colliderDefinition, currentX, currentY);
                break;
            }
            while (pixelsIterated < 1000 && (currentX != pixel.Value.x || currentY != pixel.Value.y));

            startingPixelindex++;
            EditorUtility.ClearProgressBar();

            if(stopThePresses)
                break;
        }

        if(stopThePresses)
            return;

        for (int i = 0; i < vertices.Length; i++) {
            for (int j = 0; j < vertices[i].Count; j++) {
                newVertex = vertices[i][j];
                if(newVertex.x >= Tile.RESOLUTION * 0.5f)
                    newVertex.x += 1;
                if(newVertex.y >= Tile.RESOLUTION * 0.5f)
                    newVertex.y += 1;
                newVertex.x = newVertex.x / Tile.RESOLUTION;
                newVertex.y = newVertex.y / Tile.RESOLUTION;
                newVertex.x -= Tile.RADIUS;
                newVertex.y -= Tile.RADIUS;
                vertices[i][j] = newVertex;
            }
        }

        if(_colliderPrefab == null){
            string _myPath = SAVEPATH_SHADOWCOLLIDER + _colliderDefinition.ToString() + ".prefab";
            if(File.Exists(_myPath)){
                Debug.LogError("A prefab called " + _myPath + " already exists!");
                stopThePresses = true;
                return;
            }

            GameObject _blueprint = AssetDatabase.LoadAssetAtPath<GameObject>(LOADPATH_SHADOWCOLLIDER_BLUEPRINT);
            _colliderPrefab = PrefabUtility.CreatePrefab(_myPath, _blueprint as GameObject).GetComponent<PolygonCollider2D>();
        }

        ObjectPooler _pooler = FindObjectOfType<ObjectPooler>();
        if (!_pooler.HasPoolForID(_colliderDefinition))
            _pooler.AddPool(_colliderDefinition, _colliderPrefab.GetComponent<PoolerObject>(), _additionalIDs);

        _colliderPrefab.pathCount = vertices.Length;
        for (int i = 0; i < _colliderPrefab.pathCount; i++)
            _colliderPrefab.SetPath(i, vertices[i].ToArray());

        EditorUtility.SetDirty(_colliderPrefab);
    }
}