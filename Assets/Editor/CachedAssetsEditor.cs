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
    }

    private int currentWallSetIndex;
    private const int AMOUNT_OF_SHIT = 93;
    private int shitCount = 0;
    public void GenerateShadowColliders() {

        for (int i = 0; i < thisCA.WallSets.Length; i++) {
            currentWallSetIndex = i;

            thisCA.WallSets[i].floor_Single_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Single);
            thisCA.WallSets[i].floor_FourWay_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_FourWay);
            thisCA.WallSets[i].floor_Vertical_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Vertical_T);
            thisCA.WallSets[i].floor_Vertical_M_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Vertical_M);
            thisCA.WallSets[i].floor_Vertical_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Vertical_B);
            thisCA.WallSets[i].floor_Horizontal_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Horizontal_L);
            thisCA.WallSets[i].floor_Horizontal_M_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Horizontal_M);
            thisCA.WallSets[i].floor_Horizontal_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Horizontal_R);
            thisCA.WallSets[i].floor_Corner_TopRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Corner_TopRight);
            thisCA.WallSets[i].floor_Corner_TopLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Corner_TopLeft);
            thisCA.WallSets[i].floor_Corner_BottomRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Corner_BottomRight);
            thisCA.WallSets[i].floor_Corner_BottomLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Corner_BottomLeft);
            thisCA.WallSets[i].floor_Tee_Right_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Tee_Right);
            thisCA.WallSets[i].floor_Tee_Left_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Tee_Left);
            thisCA.WallSets[i].floor_Tee_Top_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Tee_Top);
            thisCA.WallSets[i].floor_Tee_Bottom_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Tee_Bottom);

            thisCA.WallSets[i].floor_Diagonal_TopRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopRight);
            thisCA.WallSets[i].floor_Diagonal_TopRight_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopRight_T);
            thisCA.WallSets[i].floor_Diagonal_TopRight_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopRight_R);
            thisCA.WallSets[i].floor_Diagonal_TopRight_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopRight_TR);
            thisCA.WallSets[i].floor_Diagonal_TopLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopLeft);
            thisCA.WallSets[i].floor_Diagonal_TopLeft_T_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopLeft_T);
            thisCA.WallSets[i].floor_Diagonal_TopLeft_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopLeft_L);
            thisCA.WallSets[i].floor_Diagonal_TopLeft_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_TopLeft_TL);
            thisCA.WallSets[i].floor_Diagonal_BottomRight_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomRight);
            thisCA.WallSets[i].floor_Diagonal_BottomRight_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomRight_B);
            thisCA.WallSets[i].floor_Diagonal_BottomRight_R_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomRight_R);
            thisCA.WallSets[i].floor_Diagonal_BottomRight_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomRight_BR);
            thisCA.WallSets[i].floor_Diagonal_BottomLeft_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomLeft);
            thisCA.WallSets[i].floor_Diagonal_BottomLeft_B_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomLeft_B);
            thisCA.WallSets[i].floor_Diagonal_BottomLeft_L_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomLeft_L);
            thisCA.WallSets[i].floor_Diagonal_BottomLeft_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floor_Diagonal_BottomLeft_BL);

            thisCA.WallSets[i].floorCornerHider_All_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_All);
            thisCA.WallSets[i].floorCornerHider_TL_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TL_BR);
            thisCA.WallSets[i].floorCornerHider_TR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TR_BL);
            thisCA.WallSets[i].floorCornerHider_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TL);
            thisCA.WallSets[i].floorCornerHider_TL_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TL_TR);
            thisCA.WallSets[i].floorCornerHider_TL_TR_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TL_TR_BR);
            thisCA.WallSets[i].floorCornerHider_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TR);
            thisCA.WallSets[i].floorCornerHider_TR_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TR_BR);
            thisCA.WallSets[i].floorCornerHider_TR_BR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_TR_BR_BL);
            thisCA.WallSets[i].floorCornerHider_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BR);
            thisCA.WallSets[i].floorCornerHider_BR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BR_BL);
            thisCA.WallSets[i].floorCornerHider_BR_BL_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BR_BL_TL);
            thisCA.WallSets[i].floorCornerHider_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BL);
            thisCA.WallSets[i].floorCornerHider_BL_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BL_TL);
            thisCA.WallSets[i].floorCornerHider_BL_TL_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.floorCornerHider_BL_TL_TR);

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

            thisCA.WallSets[i].wallCornerHider_All_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_All);
            thisCA.WallSets[i].wallCornerHider_TL_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TL_BR);
            thisCA.WallSets[i].wallCornerHider_TR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TR_BL);
            thisCA.WallSets[i].wallCornerHider_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TL);
            thisCA.WallSets[i].wallCornerHider_TL_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TL_TR);
            thisCA.WallSets[i].wallCornerHider_TL_TR_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TL_TR_BR);
            thisCA.WallSets[i].wallCornerHider_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TR);
            thisCA.WallSets[i].wallCornerHider_TR_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TR_BR);
            thisCA.WallSets[i].wallCornerHider_TR_BR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_TR_BR_BL);
            thisCA.WallSets[i].wallCornerHider_BR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BR);
            thisCA.WallSets[i].wallCornerHider_BR_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BR_BL);
            thisCA.WallSets[i].wallCornerHider_BR_BL_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BR_BL_TL);
            thisCA.WallSets[i].wallCornerHider_BL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BL);
            thisCA.WallSets[i].wallCornerHider_BL_TL_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BL_TL);
            thisCA.WallSets[i].wallCornerHider_BL_TL_TR_shadow = GenerateShadowCollider(CachedAssets.WallSet.wallCornerHider_BL_TL_TR);
        }

        EditorUtility.ClearProgressBar();
    }
    private List<Color32> shadowPixels;
    private Color32[][] assetPixels = new Color32[Grid.TILE_RESOLUTION * 2][];
    private Dictionary<int, Vector2> startingPixels = new Dictionary<int, Vector2>();
    private int currentX = -1;
    private int currentY = -1;
    private int prevX = -1;
    private int prevY = -1;
    private List<List<Vector2>> vertices;
    private int startingPixelindex;
    Vector2[][] GenerateShadowCollider(CachedAssets.DoubleInt _texturePos) {
        shitCount++;
        EditorUtility.DisplayProgressBar("Generating Collider Paths", "Collider #" + shitCount.ToString(), shitCount / AMOUNT_OF_SHIT);

        if (shadowPixels == null)
            shadowPixels = new List<Color32>(thisCA.WallSets[currentWallSetIndex].ShadowMap.GetPixels32());
        for (int y = 0; y < assetPixels.Length; y++)
            assetPixels[y] = new Color32[Grid.TILE_RESOLUTION];

        for (int y = 0; y < assetPixels.Length; y++) {
            for (int x = 0; x < assetPixels[y].Length; x++) {
                if (y < assetPixels.Length * 0.5f)
                    assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((Grid.TILE_RESOLUTION * (_texturePos.Y + y)) + (_texturePos.X + x))];
                else
                    assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((Grid.TILE_RESOLUTION * (_texturePos.Y + y + 1)) + (_texturePos.X + x))];
            }
        }

        startingPixels.Clear();
        for (int y = 0; y < assetPixels.Length; y++) {
            for (int x = 0; x < assetPixels[y].Length; x++) {

                // find a pixel for each path and cache as starting points
                if (assetPixels[y][x].g > 0 && !startingPixels.ContainsKey(assetPixels[y][x].g))
                    startingPixels.Add(assetPixels[y][x].g, new Vector2(x, y));
            }
        }

        startingPixelindex = 0;
        vertices = new List<List<Vector2>>(startingPixels.Count);
        foreach (KeyValuePair<int, Vector2> pixel in startingPixels) {
            EditorUtility.DisplayProgressBar("Generating Collider Paths", "Following paths...", startingPixelindex / startingPixels.Count);

            vertices[startingPixelindex] = new List<Vector2>();
            currentX = (int)pixel.Value.x;
            currentY = (int)pixel.Value.y;
            prevX = currentX;
            prevY = currentY;

            dwodn // This is gonna crash because I don't check if I'm still within the array's boundaries when navigating below!

            while (currentX != pixel.Value.x && currentY != pixel.Value.y) {
                // if corner, add as vertex
                if (assetPixels[currentY][currentX].r == 255)
                    vertices[startingPixelindex].Add(new Vector2(currentX, currentY));

                if (currentX - prevX != -1 || currentY - prevY != 0) { // if not from L, try L
                    if (assetPixels[currentY][currentX - 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        continue;
                    }
                }
                if (currentX - prevX != -1 || currentY - prevY != 1) { // if not from TL, try TL
                    if (assetPixels[currentY + 1][currentX - 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        currentY += 1;
                        continue;
                    }
                }
                if (currentX - prevX != 0 || currentY - prevY != 1) { // if not from T, try T
                    if (assetPixels[currentY + 1][currentX].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentY += 1;
                        continue;
                    }
                }
                if (currentX - prevX != 1 || currentY - prevY != 1) { // if not from TR, try TR
                    if (assetPixels[currentY + 1][currentX + 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        currentY += 1;
                        continue;
                    }
                }
                if (currentX - prevX != 1 || currentY - prevY != 0) { // if not from R, try R
                    if (assetPixels[currentY][currentX + 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        continue;
                    }
                }
                if (currentX - prevX != 1 || currentY - prevY != -1) { // if not from BR, try BR
                    if (assetPixels[currentY - 1][currentX + 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX += 1;
                        currentY -= 1;
                        continue;
                    }
                }
                if (currentX - prevX != 0 || currentY - prevY != -1) { // if not from B, try B
                    if (assetPixels[currentY - 1][currentX].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentY -= 1;
                        continue;
                    }
                }
                if (currentX - prevX != -1 || currentY - prevY != -1) { // // if not from BL, try BL
                    if (assetPixels[currentY - 1][currentX - 1].g == pixel.Key) {
                        prevX = currentX;
                        prevY = currentY;
                        currentX -= 1;
                        currentY -= 1;
                        continue;
                    }
                }
            }

            startingPixelindex++;
            EditorUtility.ClearProgressBar();
        }

        Vector2[][] array = new Vector2[vertices.Count][];
        for (int i = 0; i < array.Length; i++)
            array[i] = vertices[i].ToArray();
        return array;
    }
}
