using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    public Terrain terrain;
    public Transform water;
    public Material material;

    public Transform objectOnTerrain;

    //TerrainData terrainData;

    public int depth = 20;

    public int width = 256;
    public int height = 256;

    public float scale = 20.0f;

    public float offsetX = 100.0f;
    public float offsetY = 100.0f;

    public Sprite sprite = null;

    public bool mapIsGenerate = false;

    public bool generateHeightMap = false;

    public bool generateObject = false;

    public GameObject[] tree;
    public GameObject[] detail;
    public GameObject[] grass;

    Sprite spriteOld = null;

    List<GameObject> treeOnTerrain = new List<GameObject>();
    List<GameObject> rockOnTerrain = new List<GameObject>();

    [Range(0.0f, 100.0f)]
    public int grassDensity = 95;
    [Range(1.0f, 100.0f)]
    public int treeDensity = 25;

    public bool useSpecialTextureMap = false;
    public Sprite textureMap;
    Sprite oldTextureMap = null;

    public string terrainId = "0";

    //[SerializeField]
    // public TerrainData dataTerrain;

    // Use this for initialization
    void Awake()
    {
        spriteOld = sprite;
        oldTextureMap = textureMap;

        offsetX = Random.Range(0.0f, 9999.0f);
        offsetY = Random.Range(0.0f, 9999.0f);

        terrain = GetComponent<Terrain>();

        if(objectOnTerrain == null)
        {
            objectOnTerrain = transform;
        }
        if(terrain == null)
        {
            terrain = GetComponent<Terrain>();
        }

        //terrain.terrainData.alphamapResolution = resolution;
        //terrain.terrainData.baseMapResolution = resolution;

        //Generate();

        //TreePrototype treePrototype = new TreePrototype();
        //treePrototype.prefab()

        /*TreeInstance treeInstance = new TreeInstance();
        treeInstance.prototypeIndex = 0;
        treeInstance.position = Vector3.zero;
        terrain.AddTreeInstance(treeInstance);*/
    }
	
	// Update is called once per frame
	void Update ()
    {
#if UNITY_EDITOR
        if(spriteOld == null && sprite != null)
        {
            spriteOld = sprite;
        }

        if (sprite != null && spriteOld != null)
        {
            if (sprite.name != spriteOld.name)
            {
                spriteOld = sprite;

                for (int i = 0; i < treeOnTerrain.Count; i++)
                {
                    DestroyImmediate(treeOnTerrain[i]);
                }
                for (int i = 0; i < rockOnTerrain.Count; i++)
                {
                    DestroyImmediate(rockOnTerrain[i]);
                }

                rockOnTerrain.Clear();
                treeOnTerrain.Clear();
                Generate();
            }
        }

        if (useSpecialTextureMap && textureMap != null && textureMap.name != oldTextureMap.name)
        {
            oldTextureMap = textureMap;

            Texture2D tex = NormalMap(textureMap.texture, 1.0f);

            material.SetTexture("_BumpMap", tex);
            material.SetTexture("_HeightMap ", textureMap.texture);
            material.SetTexture("_DetailAlbedoMap", textureMap.texture);

            terrain.materialType = Terrain.MaterialType.Custom;
            terrain.materialTemplate = material;
        }
#endif
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        if (!generateHeightMap)
        {
            terrainData.heightmapResolution = width + 1;

            terrainData.size = new Vector3(width, scale, height);
            terrainData.SetHeights(0, 0, GenerateHeights());
        }
        else
        {
            terrainData.SetHeights(0, 0, CreateHeightmapToImage());
        }

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[height, width];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                heights[y, x] = CalculateHeightToImage(x, y);
            }
        }

        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / width * scale;
        float yCoord = (float)y / height * scale;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    float CalculateHeightToImage(int x, int y)
    {
        float xCoord = 0.0f;
        float yCoord = 0.0f;

        Color color = sprite.texture.GetPixel(x, y);
        xCoord = ((color.r * 20.0f) + (color.g * 90.0f) + (color.b * 110.0f)) / width;
        yCoord = ((color.r) + (color.g * 90.0f) + (-color.b * 150.0f)) / height;
        
        return Mathf.PerlinNoise(xCoord, yCoord);
    }


    private Texture2D NormalMap(Texture2D source, float strength)
    {
        strength = Mathf.Clamp(strength, 0.0F, 1.0F);

        Texture2D normalTexture;
        float xLeft;
        float xRight;
        float yUp;
        float yDown;
        float yDelta;
        float xDelta;

        normalTexture = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);

        for (int y = 0; y < normalTexture.height; y++)
        {
            for (int x = 0; x < normalTexture.width; x++)
            {
                xLeft = source.GetPixel(x - 1, y).grayscale * strength;
                xRight = source.GetPixel(x + 1, y).grayscale * strength;
                yUp = source.GetPixel(x, y - 1).grayscale * strength;
                yDown = source.GetPixel(x, y + 1).grayscale * strength;
                xDelta = ((xLeft - xRight) + 1) * 0.5f;
                yDelta = ((yUp - yDown) + 1) * 0.5f;
                normalTexture.SetPixel(x, y, new Color(xDelta, yDelta, 1.0f, yDelta));
            }
        }
        normalTexture.Apply();

        //Code for exporting the image to assets folder
        System.IO.File.WriteAllBytes("Assets/NormalMap.png", normalTexture.EncodeToPNG());

        return normalTexture;
    }

    float[,] CreateHeightmapToImage()
    {
        Texture2D heightmap = sprite.texture;

        var terrain = Terrain.activeTerrain.terrainData;
        int w = heightmap.width;
        int h = heightmap.height;
        int w2 = terrain.heightmapWidth;
        float[,] heightmapData = terrain.GetHeights(0, 0, w2, w2);
        Color[] mapColors = heightmap.GetPixels();
        Color[] map = new Color[w2 * w2];
        if (w2 != w || h != w)
        {
            // Resize using nearest-neighbor scaling if texture has no filtering
            if (heightmap.filterMode == FilterMode.Point)
            {
                float dx = (float)w / (float)w2;
                float dy = (float)h / (float)w2;
                for (int y = 0; y < w2; y++)
                {
                    int thisY = Mathf.FloorToInt(dy * y) * w;
                    int yw = y * w2;
                    for (int x = 0; x < w2; x++)
                    {
                        map[yw + x] = mapColors[Mathf.FloorToInt(thisY + dx * x)];
                    }
                }
            }
            // Otherwise resize using bilinear filtering
            else
            {
                float ratioX = (1.0f / ((float)w2 / (w - 1)));
                float ratioY = (1.0f / ((float)w2 / (h - 1)));
                for (int y = 0; y < w2; y++)
                {
                    int yy = Mathf.FloorToInt(y * ratioY);
                    int y1 = yy * w;
                    int y2 = (yy + 1) * w;
                    int yw = y * w2;
                    for (int x = 0; x < w2; x++)
                    {
                        int xx = Mathf.FloorToInt(x * ratioX);
                        Color bl = mapColors[y1 + xx];
                        Color br = mapColors[y1 + xx + 1];
                        Color tl = mapColors[y2 + xx];
                        Color tr = mapColors[y2 + xx + 1];
                        float xLerp = x * ratioX - xx;
                        map[yw + x] = Color.Lerp(Color.Lerp(bl, br, xLerp), Color.Lerp(tl, tr, xLerp), y * ratioY - (float)yy);
                    }
                }
            }
        }
        else
        {
            // Use original if no resize is needed
            map = mapColors;
        }
        // Assign texture data to heightmap
        for(int y = 0; y < w2; y++)
        {
            for (int x = 0; x < w2; x++)
            {
                heightmapData[y, x] = map[y * w2 + x].grayscale;
            }
        }

        return heightmapData;
        //terrain.SetHeights(0, 0, heightmapData);
    }

    void GenerateDetail()
    {
        int[,] map = terrain.terrainData.GetDetailLayer(0, 0, (int)terrain.terrainData.detailWidth, (int)terrain.terrainData.detailHeight, 0);
        
        for (int i = 0; i < terrain.terrainData.detailWidth; i++)
        {
            for (int j = 0; j < terrain.terrainData.detailHeight; j++)
            {
                if (sprite.texture.GetPixel(i, j).grayscale > 0.37f && sprite.texture.GetPixel(i, j).grayscale < 0.6f)
                //if (sprite.texture.GetPixel(i, j).r < 0.5f && sprite.texture.GetPixel(i, j).g > 0.5f && sprite.texture.GetPixel(i, j).b < 0.5f)
                {
                    Vector3 vect = new Vector3((float)i, 0.0f, (float)j);
                    Vector3 place = new Vector3(i, Terrain.activeTerrain.SampleHeight(vect), j);
                    if (Random.Range(0, 100) < grassDensity)
                    {
                        try
                        {
                            map[(int)(j * (terrain.terrainData.detailHeight / terrain.terrainData.size.z)), (int)(i * (terrain.terrainData.detailWidth / terrain.terrainData.size.x))] = 6;
                        }
                        catch
                        {

                        }
                    }

                    switch (Random.Range(0, 3))
                    {
                        case 0: // Tree
                            if (Random.Range(0, treeDensity) == (int)(treeDensity / 2.0f))
                            {
                                //Quaternion rot = Quaternion.FromToRotation(Vector3.up, terrain.terrainData.GetInterpolatedNormal(i, j));

                                /*TreeInstance treeInstance = new TreeInstance();
                                treeInstance.prototypeIndex = Random.Range(0, tree.Length);
                                treeInstance.position = place;
                                terrain.AddTreeInstance(treeInstance);*/

                                //map[i, j] = Random.Range(0, 6);
                                //mapTree[i, j] = 1;
                                GameObject obj = Instantiate(tree[Random.Range(0, tree.Length)], place, new Quaternion(0, Random.Range(0, 360), 0, 0), objectOnTerrain);
                                treeOnTerrain.Add(obj);
                            }
                            break;
                        case 1: // Rock
                            if (Random.Range(0, 100) == 2)
                            {
                                //Quaternion rot = Quaternion.FromToRotation(Vector3.up, terrain.terrainData.GetInterpolatedNormal(i, j));
                                //Instantiate(rock[Random.Range(0, rock.Length)], place, new Quaternion(0, Random.Range(0, 360), 0, 0), objectOnTerrain);

                                GameObject obj = Instantiate(detail[Random.Range(0, detail.Length)], place, new Quaternion(0, Random.Range(0, 360), 0, 0), objectOnTerrain);
                                rockOnTerrain.Add(obj);
                            }
                            break;
                        case 2: // Grass / Flower
                            if (Random.Range(0, 100) < 95)
                            {
                                //map[j, i] = 1;

                                //Quaternion rot = Quaternion.FromToRotation(Vector3.up, terrain.terrainData.GetInterpolatedNormal(i, j));
                                //Instantiate(flower[Random.Range(0, flower.Length)], place, new Quaternion(0, Random.Range(0, 360), 0, 0), objectOnTerrain);
                            }
                            break;
                    }
                }
            }
        }
        terrain.terrainData.SetDetailLayer(0, 0, 0, map);
    }

    void Generate()
    {
        width = sprite.texture.width;
        height = sprite.texture.height;

        int resolution = Mathf.Max(width, height);

        if (resolution % 16 != 0)
        {
            resolution += (16 - resolution % 16);
        }

        terrain.terrainData.SetDetailResolution(resolution, 16);

        terrain.terrainData.size = new Vector3(width, depth, height);
        
        if (water != null)
        {
            water.position = new Vector3(width/2, scale / 2.0f, height/2);
            water.localScale = new Vector3(width, 0.0f, height);
        }

        terrain.terrainData = GenerateTerrain(terrain.terrainData);

        material.EnableKeyword("_NORMALMAP");

        if(useSpecialTextureMap)
        {
            material.mainTexture = textureMap.texture;
            Texture2D tex = NormalMap(textureMap.texture, 1.0f);

            material.SetTexture("_BumpMap", tex);
            material.SetTexture("_HeightMap ", textureMap.texture);
            material.SetTexture("_DetailAlbedoMap", textureMap.texture);
        }
        else
        {
            material.mainTexture = sprite.texture;
            Texture2D tex = NormalMap(sprite.texture, 1.0f);

            material.SetTexture("_BumpMap", tex);
            material.SetTexture("_HeightMap ", sprite.texture);
            material.SetTexture("_DetailAlbedoMap", sprite.texture);
        }

        terrain.materialType = Terrain.MaterialType.Custom;
        terrain.materialTemplate = material;

        mapIsGenerate = true;

        if (generateObject)
        {
            int[,] map = terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0);

            for (int y = 0; y < terrain.terrainData.detailHeight; y++)
            {
                for (int x = 0; x < terrain.terrainData.detailWidth; x++)
                {
                    map[x, y] = 0;
                }
            }
            terrain.terrainData.SetDetailLayer(0, 0, 0, map);

            GenerateDetail();

            /*List<TreePrototype> treePrototype = new List<TreePrototype>();

            for(int i = 0; i < tree.Length; i++)
            {
                TreePrototype treeprot = new TreePrototype();
                treeprot.prefab = tree[i];
                treePrototype.Add(treeprot);

                //Debug.Log("i: " + i + ", treePrototype[i].prefab: " + treePrototype[i].prefab.name);
            }*/
        }
    }

    public void SaveTerrain()
    {
        if (AssetDatabase.Contains(terrain.terrainData))
        {
            Debug.Log("save");
            AssetDatabase.CopyAsset("Assets/New Terrain.asset", "Assets/Resources/Terrain/" + terrainId + ".asset");
        }
        else
        {
            Debug.Log("create");
            AssetDatabase.CreateAsset(terrain.terrainData, "Assets/Resources/Terrain/" + terrainId + ".asset");
        }
    }

    public void LoadTerrain(TerrainData data)
    {
        if (data != null)
        {
            terrain.terrainData = data;
        }
    }
}
