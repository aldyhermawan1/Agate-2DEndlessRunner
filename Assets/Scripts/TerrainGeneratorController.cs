using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneratorController : MonoBehaviour
{
    [Header("Force Early Template")]
    public List<TerrainTemplateController> earlyTerrainTemplates;

    [Header("Templates")]
    public List<TerrainTemplateController> terrainTemplates;
    public float terrainTemplateWidth;

    [Header("Generator Area")]
    public Camera gameCamera;
    public float areaStartOffset;
    public float areaEndOffset;

    private List<GameObject> spawnedTerrain;
    private Dictionary<string, List<GameObject>> pool; //pool

    private float lastGeneratedPositionX;
    private float lastRemovedPositionX;
    
    private const float debugLineHeight = 10.0f;

    void Start()
    {
        pool = new Dictionary<string, List<GameObject>>(); //init pool
        spawnedTerrain = new List<GameObject>();

        lastGeneratedPositionX = GetHorizontalPositionStart();
        lastRemovedPositionX = lastGeneratedPositionX - terrainTemplateWidth;

        //Generate Starting Terrain
        foreach(TerrainTemplateController terrain in earlyTerrainTemplates){
            GenerateTerrain(lastGeneratedPositionX, terrain);
            lastGeneratedPositionX += terrainTemplateWidth;
        }

        //Generate New Terrain
        while(lastGeneratedPositionX < GetHorizontalPositionEnd()){
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
    }

    void Update()
    {
        //Generate New Terrain
        while(lastGeneratedPositionX < GetHorizontalPositionEnd()){
            GenerateTerrain(lastGeneratedPositionX);
            lastGeneratedPositionX += terrainTemplateWidth;
        }
        //Deleting Old Terrain
        while(lastRemovedPositionX + terrainTemplateWidth < GetHorizontalPositionStart()){
            lastRemovedPositionX += terrainTemplateWidth;
            RemoveTerrain(lastRemovedPositionX);
        }
    }

    private void GenerateTerrain(float posX, TerrainTemplateController forceTerrain = null){
        GameObject newTerrain = null;
        if(forceTerrain == null){
            //newTerrain = Instantiate(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
            newTerrain = GenerateFromPool(terrainTemplates[Random.Range(0, terrainTemplates.Count)].gameObject, transform);
        } else {
            // newTerrain = Instantiate(forceTerrain.gameObject, transform);
            newTerrain = GenerateFromPool(forceTerrain.gameObject, transform);
        }
        newTerrain.transform.position = new Vector2(posX, 0f);
        spawnedTerrain.Add(newTerrain);
    }

    private void RemoveTerrain(float posX){
        GameObject terrainToRemove = null;

        foreach(GameObject item in spawnedTerrain){
            if(item.transform.position.x == posX){
                terrainToRemove = item;
                break;
            }
            
        }

        if(terrainToRemove != null){
            spawnedTerrain.Remove(terrainToRemove);
            ReturnToPool(terrainToRemove);
        }
    }

    //Pool
    private GameObject GenerateFromPool(GameObject item, Transform parent){
        if(pool.ContainsKey(item.name)){
            //if found
            if(pool[item.name].Count > 0){
                GameObject newItemFromPool = pool[item.name][0];
                pool[item.name].Remove(newItemFromPool);
                newItemFromPool.SetActive(true);
                return newItemFromPool;
            }
        } else {
            pool.Add(item.name, new List<GameObject>()); //If list not defined, create new one
        }
        //Create new terrain if no item available in pool
        GameObject newItem = Instantiate(item, parent);
        newItem.name = item.name;
        return newItem;
    }

    private void ReturnToPool(GameObject item){
        if(!pool.ContainsKey(item.name)){
            Debug.LogError(("INVALID POOL ITEM!!"));
        }

        pool[item.name].Add(item);
        item.SetActive(false);
    }

    private float GetHorizontalPositionStart(){
        return gameCamera.ViewportToWorldPoint(new Vector2(0f, 0f)).x + areaStartOffset;
    }

    private float GetHorizontalPositionEnd(){
        return gameCamera.ViewportToWorldPoint(new Vector2(1f, 0f)).x + areaEndOffset;
    }

    //Debug
    private void OnDrawGizmos() {
        Vector3 areaStartPosition = transform.position;
        Vector3 areaEndPosition = transform.position;

        areaStartPosition.x = GetHorizontalPositionStart();
        areaEndPosition.x = GetHorizontalPositionEnd();

        Debug.DrawLine(areaStartPosition + Vector3.up * debugLineHeight / 2, areaStartPosition + Vector3.down * debugLineHeight / 2, Color.red);
        Debug.DrawLine(areaEndPosition + Vector3.up * debugLineHeight / 2, areaEndPosition + Vector3.down * debugLineHeight / 2, Color.red);
    }
}
