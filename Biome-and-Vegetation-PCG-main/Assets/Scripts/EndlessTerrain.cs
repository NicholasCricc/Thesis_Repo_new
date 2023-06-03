using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour{

    public static float scale = 1f;

    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDist;

    public Transform viewer;

    public Material mapMaterial;


    public static UnityEngine.Vector2 viewerPosition;
    UnityEngine.Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleViewDist; 

    Dictionary<UnityEngine.Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<UnityEngine.Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
        UpdateVisibleChunks();
    }

    void Update() {
        viewerPosition = new UnityEngine.Vector2(viewer.position.x, viewer.position.z) / scale;
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
        
    }

    void UpdateVisibleChunks() {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleViewDist; yOffset <= chunksVisibleViewDist; yOffset++) {
            for (int xOffset = -chunksVisibleViewDist; xOffset <= chunksVisibleViewDist; xOffset++) {

                UnityEngine.Vector2 viewedChunkCoord = new UnityEngine.Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                } else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial)); //9 chunck
                }

            }
        }

    }

    public class TerrainChunk {

        UnityEngine.Vector2 position;

        GameObject meshObject;
        GameObject vegetationContainer;

        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        // Vegetation gameobjects
        List<GameObject> vegetations;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {

            this.detailLevels = detailLevels;

            position = coord * size;

            bounds = new Bounds(position, Vector2.one * size);

            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            
            meshRenderer.material = material;
            
            meshObject.transform.position = positionV3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);


            vegetationContainer = new GameObject("Vegetation Container");
            vegetationContainer.transform.parent = meshObject.transform;
            vegetationContainer.transform.position = vegetationContainer.transform.position + new Vector3(0, 0, 0);


            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDateReceived);
        
        }

        void OnMapDateReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = new Texture2D(MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);

            texture = Resources.Load("Boxed_OutputTIF_Colour") as Texture2D;
            
            //TextureGenerator.TextureFromColorMap(mapData.biomeMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;
            meshRenderer.material.mainTextureScale = new Vector2(1f, -1f);
           
            this.vegetations = new List<GameObject>(mapData.poissonDiskSamples.Count);
            CreateVegetation();

            UpdateTerrainChunk();
        }

        void CreateVegetation() {

            int width = mapData.heightMap.GetLength(0);
            int height = mapData.heightMap.GetLength(1);
            Debug.Log(width);
            float topLeftX = (width - 1) / -2f;
            float topLeftZ = (height - 1) / 2f;

            foreach (PoissonSampleData sample in mapData.poissonDiskSamples) {
                float posX = topLeftX + sample.position.x;
                float treeHeight = mapData.heightCurve.Evaluate(mapData.heightMap[(int)sample.position.x, (int)sample.position.y]) * mapData.heightMultiplier;
                float posZ = topLeftZ - sample.position.y;

                //vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));

                //Part of Rabat for demo
                if ((posX) > 16 && (posX) < 21 && (posZ) > -88 && (posZ) < -85)
                {
                    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                }

                //1km
                //if ((posX) > 93 && (posX) < 103 && (posZ) > -62 && (posZ) < -50)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //2km
                //if ((posX) > 93 && (posX) < 112 && (posZ) > -70 && (posZ) < -50)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //3km
                //if ((posX) > 102 && (posX) < 129 && (posZ) > -80 && (posZ) < -54)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //4km
                //if ((posX) > 102 && (posX) < 124 && (posZ) > -85 && (posZ) < -40)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //5km
                //if ((posX) > 118 && (posX) < 134 && (posZ) > -101 && (posZ) < -33)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //200m
                //if ((posX) > 119.43 && (posX) < 120.636 && (posZ) > -70.259 && (posZ) < -68.88)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //600m
                //if ((posX) > 118.9 && (posX) < 124.18 && (posZ) > -76 && (posZ) < -70.72)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //1000m
                //if ((posX) > 119.43 && (posX) < 124.315 && (posZ) > -80.24 && (posZ) < -68.88)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}

                //1400m
                //if ((posX) > 119.43 && (posX) < 128.51 && (posZ) > -81.97 && (posZ) < -68.88)
                //{
                //    vegetations.Add(Instantiate(sample.vegetationPrefab[UnityEngine.Random.Range(0, sample.vegetationPrefab.Count)], meshObject.transform.position + new Vector3(posX, treeHeight, posZ), Quaternion.identity, vegetationContainer.transform));
                //}
            }
        }

        public void UpdateTerrainChunk() {

            // Guard
            if (!mapDataReceived) {
                return;
            }

            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistFromNearestEdge <= maxViewDist;

            if (visible) {

                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++) {
                    if (viewerDistFromNearestEdge > detailLevels[i].visibleDistanceThreshold) {
                        lodIndex = i + 1;
                    } else {
                        break;
                    }
                }


                if (lodIndex != previousLODIndex) {

                    LODMesh lodMesh = lodMeshes[lodIndex];

                    if (lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(mapData);
                    }
                }

                terrainChunksVisibleLastUpdate.Add(this);

                SetVegetationVisible(detailLevels[lodIndex].vegetationsVisible);

            }

            SetVisible(visible);
        }

        public void SetVegetationVisible(bool vegetationsVisible) {
            vegetationContainer.SetActive(vegetationsVisible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObject.activeSelf;
        }

    }

    class LODMesh {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback) {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo {
        [Range(1, 6)]
        public int lod;
        public float visibleDistanceThreshold;
        public bool vegetationsVisible;
    }

}
