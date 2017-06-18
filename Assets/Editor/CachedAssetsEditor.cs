using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(CachedAssets))]
public class CachedAssetsEditor : Editor {

    private CachedAssets thisCA;
    public override void OnInspectorGUI() {
        thisCA = (CachedAssets)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Collider Paths"))
            GenerateShadowColliders();
        if (GUILayout.Button("Test Apply Collider"))
            TestApplyCollider();
    }

    public void TestApplyCollider(){
        // //Debug.Log(thisCA.WallSets);
        // //Debug.Log(thisCA.WallSets[0].wall_Single_shadow);
        thisCA.ShadowCollider = new CachedAssets.MovableCollider(thisCA.WallSets[0].wall_Single_shadow.Paths.Length);
        for (int i = 0; i < thisCA.WallSets[0].wall_Single_shadow.Paths.Length; i++) {
            // //Debug.Log("hello? " + thisCA.WallSets[0].wall_Single_shadow[i].Length);
            // for (int j = 0; j < thisCA.WallSets[0].wall_Single_shadow[i].Length; i++)
            //     //Debug.Log(thisCA.WallSets[0].wall_Single_shadow[i][j]);
            thisCA.ShadowCollider.SetPath(i, thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices);
            for (int j = 0; j < thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices.Length; j++) {
                Debug.Log(thisCA.WallSets[0].wall_Single_shadow.Paths[i].Vertices[j]);
            }
        }
    }

    private int currentWallSetIndex;
    private const int AMOUNT_OF_SHIT = 93;
    private int shitCount = 0;
    private bool stopThePresses = false;
    public void GenerateShadowColliders() {
        shitCount = 0;
        shadowPixels = null;
        stopThePresses = false;

        for (int i = 0; i < thisCA.WallSets.Length; i++) {
            currentWallSetIndex = i;

            thisCA.WallSets[i].wall_Single_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Single);
            thisCA.WallSets[i].wall_FourWay_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_FourWay);
            thisCA.WallSets[i].wall_Vertical_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Vertical_T);
            thisCA.WallSets[i].wall_Vertical_M_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Vertical_M);
            thisCA.WallSets[i].wall_Vertical_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Vertical_B);
            thisCA.WallSets[i].wall_Horizontal_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Horizontal_L);
            thisCA.WallSets[i].wall_Horizontal_M_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Horizontal_M);
            thisCA.WallSets[i].wall_Horizontal_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Horizontal_R);

            thisCA.WallSets[i].wall_Corner_TopRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Corner_TopRight);
            thisCA.WallSets[i].wall_Corner_TopLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Corner_TopLeft);
            thisCA.WallSets[i].wall_Corner_BottomRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Corner_BottomRight);
            thisCA.WallSets[i].wall_Corner_BottomLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Corner_BottomLeft);
            thisCA.WallSets[i].wall_Tee_Right_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Tee_Right);
            thisCA.WallSets[i].wall_Tee_Left_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Tee_Left);
            thisCA.WallSets[i].wall_Tee_Top_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Tee_Top);
            thisCA.WallSets[i].wall_Tee_Bottom_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Tee_Bottom);

            thisCA.WallSets[i].wall_Diagonal_TopRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopRight);
            thisCA.WallSets[i].wall_Diagonal_TopRight_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopRight_T);
            thisCA.WallSets[i].wall_Diagonal_TopRight_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopRight_R);
            thisCA.WallSets[i].wall_Diagonal_TopRight_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopRight_TR);
            thisCA.WallSets[i].wall_Diagonal_TopLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopLeft);
            thisCA.WallSets[i].wall_Diagonal_TopLeft_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopLeft_T);
            thisCA.WallSets[i].wall_Diagonal_TopLeft_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopLeft_L);
            thisCA.WallSets[i].wall_Diagonal_TopLeft_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_TopLeft_TL);
            thisCA.WallSets[i].wall_Diagonal_BottomRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomRight);
            thisCA.WallSets[i].wall_Diagonal_BottomRight_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomRight_B);
            thisCA.WallSets[i].wall_Diagonal_BottomRight_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomRight_R);
            thisCA.WallSets[i].wall_Diagonal_BottomRight_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomRight_BR);
            thisCA.WallSets[i].wall_Diagonal_BottomLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomLeft);
            thisCA.WallSets[i].wall_Diagonal_BottomLeft_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomLeft_B);
            thisCA.WallSets[i].wall_Diagonal_BottomLeft_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomLeft_L);
            thisCA.WallSets[i].wall_Diagonal_BottomLeft_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wall_Diagonal_BottomLeft_BL);

            thisCA.WallSets[i].anim_AirlockHorizontal_Close_B_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockHorizontal_Close_B_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockHorizontal_Close_T_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockHorizontal_Close_T_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockHorizontal_Close_T_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockHorizontal_Close_T_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockHorizontal_Close_T_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockHorizontal_Open_B_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockHorizontal_Open_B_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockHorizontal_Close_B_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockHorizontal_Open_B_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockHorizontal_Open_B_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockHorizontal_Open_T_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockHorizontal_Open_T_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockHorizontal_Open_T_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockHorizontal_Open_T_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockHorizontal_Open_T_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockHorizontal_Wait_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockHorizontal_Wait_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockHorizontal_Wait_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockHorizontal_Wait_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockHorizontal_Wait_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockVertical_Close_L_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockVertical_Close_L_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockVertical_Close_L_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockVertical_Close_L_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockVertical_Close_L_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockVertical_Close_R_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockVertical_Close_R_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockVertical_Close_R_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockVertical_Close_R_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockVertical_Close_R_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockVertical_Open_L_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockVertical_Open_L_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockVertical_Open_L_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockVertical_Open_L_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockVertical_Open_L_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockVertical_Open_R_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockVertical_Open_R_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockVertical_Open_R_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockVertical_Open_R_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockVertical_Open_R_Bottom.Frames[j]);

            thisCA.WallSets[i].anim_AirlockVertical_Wait_shadow = new CachedAssets.MovableCollider[CachedAssets.WallSet.anim_AirlockVertical_Wait_Bottom.Frames.Length];
            for (int j = 0; j < CachedAssets.WallSet.anim_AirlockVertical_Wait_Bottom.Frames.Length; j++)
               thisCA.WallSets[i].anim_AirlockVertical_Wait_shadow[j] = GenerateShadowCollider(CachedAssets.WallSet.anim_AirlockVertical_Wait_Bottom.Frames[j]);

            if (stopThePresses){
                Debug.Log("Collider generation was cancelled!".Color(Color.red));
                break;
            }
            else
                Debug.Log(("Finished Wallset #" + i + "!").Color(Color.green));
        }

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
    CachedAssets.MovableCollider GenerateShadowCollider(CachedAssets.DoubleInt _texturePos) { 
        if(stopThePresses)
            return null;

        shitCount++;
        EditorUtility.DisplayProgressBar("Generating Collider Paths", "Collider #" + shitCount.ToString(), (float)shitCount / (float)AMOUNT_OF_SHIT);

        if (shadowPixels == null)
            shadowPixels = new List<Color32>(thisCA.WallSets[currentWallSetIndex].ShadowMap.GetPixels32());
        
        assetPixels = new Color32[TILE_PIXEL_RES * 2][];
        for (int y = 0; y < assetPixels.Length; y++)
            assetPixels[y] = new Color32[TILE_PIXEL_RES];
        
        for (int y = 0; y < assetPixels.Length; y++) {
            for (int x = 0; x < assetPixels[y].Length; x++) {
                assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((CachedAssets.WallSet.TEXTURE_SIZE_X * ((_texturePos.Y * TILE_PIXEL_RES) + y)) + ((_texturePos.X * TILE_PIXEL_RES) + x))];
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
                //Debug.LogError("Well, I found nothing. (" + currentX + ", " + currentY + ")");
                break;
            }
            while (pixelsIterated < 1000 && (currentX != pixel.Value.x || currentY != pixel.Value.y));

            startingPixelindex++;
            EditorUtility.ClearProgressBar();

            if(stopThePresses)
                break;
        }

        if(stopThePresses)
            return null;

        for (int i = 0; i < vertices.Length; i++) {
            for (int j = 0; j < vertices[i].Count; j++) {
                newVertex = vertices[i][j];
                newVertex.x += 1;
                newVertex.y += 1;
                newVertex.x = Mathf.RoundToInt(newVertex.x / Tile.RESOLUTION);
                newVertex.y = Mathf.RoundToInt(newVertex.y / Tile.RESOLUTION);
                newVertex.x -= Tile.RADIUS;
                newVertex.y -= Tile.RADIUS;
                vertices[i][j] = newVertex;
            }
        }
        CachedAssets.MovableCollider movColl = new CachedAssets.MovableCollider(vertices.Length);
        for (int i = 0; i < movColl.Paths.Length; i++){
            movColl.Paths[i] = new CachedAssets.ColliderVertices();
            movColl.Paths[i].Vertices = vertices[i].ToArray();
        }
        return movColl;
    }
}
