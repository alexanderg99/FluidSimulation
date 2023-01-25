using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class UniformGrid : MonoBehaviour
{

    private Vector3 lowerLeft = new Vector3(0,0,0);
    private Vector3 upperRight = new Vector3(10,10,10);
    
    public static int VoxelSize = 1;
    public static Cell[,,] CellArray;


    public static Dictionary<double, Cell> hashTable = new Dictionary<double, Cell>();
    public static int VoxelsPerSide;
    
    
    public static int XVoxelsPerSide;
    public static int YVoxelsPerSide;
    public static int ZVoxelsPerSide;

    
    // this is the GPU implementation
    private ComputeBuffer posBuffer;
    void OnEnable () {
        posBuffer = new ComputeBuffer(8 * 8, 12);
        //ComputeShader.Dispatch(0, 2, 4, 1);


    }
    
    void OnDisable () {
        posBuffer.Release();
        posBuffer = null;
    }
    
    //this is the end of the GPU implementation.
    
    
    
    

    [SerializeField] Material transparentMaterial;


    

    private void NewInitializeNeighbours()
    {

        for (int a = 0; a < XVoxelsPerSide; a ++)
        {
            for (int b = 0; b < YVoxelsPerSide; b ++)
            {
                for (int c = 0; c < ZVoxelsPerSide; c ++)
                {
                    for (int aa = a-1; aa<a+2; aa++)
                    {
                        if (aa >= 0 && aa < XVoxelsPerSide)
                        {
                            for (int bb = b - 1; bb < b + 2; bb++)
                            {
                                if (bb >= 0 && bb < YVoxelsPerSide)
                                {
                                    for (int cc = c - 1; cc < c + 2; cc++)
                                    {

                                        if (cc >= 0 && cc < ZVoxelsPerSide)
                                        {
                                            //Debug.Log(aa.ToString() + " "+ bb.ToString() + " "+ cc.ToString());
                                            //if (!(aa==a && bb==b && cc==c))
                                            //{
                                                    
                                            CellArray[a,b,c].Neighbours.Add(CellArray[aa,bb,cc]);

                                            //}
                                                
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                            }
                            
                            
                            
                        }
                        
                    }
                    
                    
                        
                    
                }
            }
        }
        
    }

    

    private void NewInitializeBoundingBox(int VoxelSize, int[] minMaxes)
    {

        int minX = minMaxes[0];
        int maxX = minMaxes[1];
        int minY = minMaxes[2];
        int maxY = minMaxes[3];
        int minZ = minMaxes[4];
        int maxZ = minMaxes[5];
            
        
        hashTable.Clear();
        
        
        float x = (maxX - minX);
        float y = (maxY - minY);
        float z = (maxZ - minZ);
        
        //setting the length of cube.
        Vector3 scale = new Vector3(VoxelSize, VoxelSize, VoxelSize);
        
        // number of Voxels per side.
        XVoxelsPerSide = (int) Math.Ceiling((x / VoxelSize))+1;
        YVoxelsPerSide = (int) Math.Ceiling((y / VoxelSize))+1;
        ZVoxelsPerSide = (int) Math.Ceiling((z / VoxelSize))+1;
        CellArray = new Cell[XVoxelsPerSide, YVoxelsPerSide, ZVoxelsPerSide];
        
        
        
        //size of array
        
        
        for (int a = 0; a < XVoxelsPerSide; a++)
        {
            for (int b = 0; b < YVoxelsPerSide; b++)
            {
                for (int c = 0; c < ZVoxelsPerSide; c++)
                {
                    //CellArray stores the coordinates of the center of the cube. 0.5 is the radius of the particle. we offset the bounding box by the radius to mimimize wasted space.
                    
                    Vector3 CellPos = new Vector3((float) a * VoxelSize + minX + (float)(0.5*(VoxelSize-1)), (float) b*VoxelSize + minY + (float)(0.5*(VoxelSize-1)), (float) c*VoxelSize + minZ +(float)(0.5*(VoxelSize-1)));
                    Cell cell = new Cell(CellPos); 
                    CellArray[a, b, c] = cell;
                    //hashTable.Add(CellPos,cell)
                    
                    //below is the representation of the cube.
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = CellPos;
                    cube.transform.localScale = scale;
                    String name = String.Format("Cube {0},{1},{2}", a, b, c);
                    cube.GetComponent<MeshRenderer>().material = transparentMaterial;
                    cube.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
                    cube.GetComponent<Collider>().enabled = false;
                    cube.AddComponent<BoundingBox>();
                    cube.name = name;
                    
                    //Hashing The Center

                    Vector3 hashVec = new Vector3((float) Math.Floor(a + (minX+0.5) / VoxelSize),
                        (float) Math.Floor(b + (minY+0.5f) / VoxelSize),
                        (float) Math.Floor(c + (minZ+0.5f) / VoxelSize));

                    double hashPos = utils.hash(hashVec);
                    hashTable.Add(hashPos, cell);
                    
                    Debug.Log(XVoxelsPerSide + " " + YVoxelsPerSide +" " + ZVoxelsPerSide);

                }
                

            }
        }
        
        NewInitializeNeighbours();
        
        
        
        
        
        
        
    }
    

    public void ClearCells(Cell[,,] cellArray)
    {

        for (int a = 0; a < XVoxelsPerSide; a++)
        {
            for (int b = 0; b < YVoxelsPerSide; b++)
            {
                for (int c = 0; c < ZVoxelsPerSide; c++)
                {

                    //empty the "contains" list for every cell
                    cellArray[a, b, c].Contains.Clear();
                }
            }
        }

    }
    
    public void UpdateCells(Cell[,,] cellArray) {


        ClearCells(cellArray);
        Sphere[] allSpheres = FindObjectsOfType<Sphere>();
        
        
        for (int i = 0; i < allSpheres.Length; i++)
        {
            //loop over all spheres. Adds the sphere to the "contains" list of the cell that the
            //sphere belongs to
            
            //updates the cell taht the sphere belongs to based on its position.
            
            allSpheres[i].UpdateBelongingCell();
            //Debug.Log(allSpheres[i].CellBelongTo.Position.ToString() +" " + i.ToString());
            
            //adds the current sphere to the contains list.
            allSpheres[i].CellBelongTo.Contains.Add(allSpheres[i]);
     
            
        }
        
        for (int i = 0; i < allSpheres.Length; i++)
        {
            allSpheres[i].FindNearestNeighbours();
            
        }

    }
    
    
    private int[] FindMinMax()
    {
        Sphere[] all = FindObjectsOfType<Sphere>();
        int l = all.Length - 1;
        Sphere[] a = all.OrderBy(x => x.transform.position.x).ToArray();
        int minX = (int)Math.Floor(a[0].transform.position.x);
        int maxX = (int)Math.Ceiling(a[l].transform.position.x);
        
        a = all.OrderBy(x => x.transform.position.y).ToArray();
        int minY = (int)Math.Floor(a[0].transform.position.y);
        int maxY = (int)Math.Ceiling(a[l].transform.position.y);
        
        a = all.OrderBy(x => x.transform.position.z).ToArray();
        int minZ = (int)Math.Floor(a[0].transform.position.z);
        int maxZ = (int)Math.Ceiling(a[l].transform.position.z);

        int[] ans = new int[] {minX, maxX, minY, maxY, minZ, maxZ};
        
        


        return ans;



    }

    void ClearBoundingBox()
    {
        BoundingBox[] all = FindObjectsOfType<BoundingBox>();
        Debug.Log(all.Length);
        
        for (int i = 0; i < all.Length; i++)
        {
            
            Destroy(all[i].transform.gameObject);
            Destroy(all[i].transform.root.gameObject);
           
        }
        
    }
    
    
    
    // Start is called before the first frame update
    void Start()
    {
        int[] minMaxes = FindMinMax();
        
        NewInitializeBoundingBox(VoxelSize, minMaxes); 
        ClearBoundingBox();
        UpdateCells(CellArray);
    }

    

    // Update is called once per frame
    void Update()
    {
        int[] minMaxes = FindMinMax();
        ClearBoundingBox();
        NewInitializeBoundingBox(VoxelSize, minMaxes); 
        UpdateCells(CellArray);
        





    }
    
    
    private void InitializeBoundingBox(int VoxelSize, Vector3 lowerLeft, Vector3 upperRight)
    {
        float x = (upperRight - lowerLeft).x;
        float y = (upperRight - lowerLeft).y;
        float z = (upperRight - lowerLeft).z;
        
        //setting the length of cube.
        Vector3 scale = new Vector3(VoxelSize, VoxelSize, VoxelSize);
        
        // number of Voxels per side.
        VoxelsPerSide = (int) Math.Ceiling((x / VoxelSize));
        
        //size of array
        CellArray = new Cell[VoxelsPerSide, VoxelsPerSide, VoxelsPerSide];
        //creates array of cells

        for (int a = 0; a < VoxelsPerSide; a++)
        {
            for (int b = 0; b < VoxelsPerSide; b++)
            {
                for (int c = 0; c < VoxelsPerSide; c++)
                {
                    //CellArray stores the coordinates of the center of the cube. 0.5 is the radius of the particle. we offset the bounding box by the radius to mimimize wasted space.
                    
                    Vector3 CellPos = new Vector3((float) a * VoxelSize + this.lowerLeft.x + (float)(0.5*(VoxelSize-1)), (float) b*VoxelSize + this.lowerLeft.y + (float)(0.5*(VoxelSize-1)), (float) c*VoxelSize + this.lowerLeft.z +(float)(0.5*(VoxelSize-1)));
                    Cell cell = new Cell(CellPos); 
                    CellArray[a, c, b] = cell;
                    //hashTable.Add(CellPos,cell)
                    
                    //below is the representation of the cube.
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    
                    cube.transform.position = CellPos;
                    cube.transform.localScale = scale;
                    String name = String.Format("Cell {0},{1},{2}", a, c, b);
                    cube.GetComponent<MeshRenderer>().material = transparentMaterial;
                    cube.GetComponent<MeshRenderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
                    cube.GetComponent<Collider>().enabled = false;
                    
                    cube.name = name;
                    
                    //Hashing The Center

                    Vector3 hashVec = new Vector3((float) Math.Floor(a + (this.lowerLeft.x+0.5) / VoxelSize),
                        (float) Math.Floor(b + (this.lowerLeft.y+0.5f) / VoxelSize),
                        (float) Math.Floor(c + (this.lowerLeft.z+0.5f) / VoxelSize));

                    double hashPos = utils.hash(hashVec);
                   
                    hashTable.Add(hashPos, cell);

                }
                

            }
        }
        
        InitializeNeighbours(this.lowerLeft);
        

    }
    
    
    private void InitializeNeighbours(Vector3 lowerLeft)
    {

        for (int a = 0; a < VoxelsPerSide; a ++)
        {
            for (int b = 0; b < VoxelsPerSide; b ++)
            {
                for (int c = 0; c < VoxelsPerSide; c ++)
                {
                    for (int aa = a-1; aa<a+2; aa++)
                    {
                        if (aa >= 0 && aa < VoxelsPerSide)
                        {
                            for (int bb = b - 1; bb < b + 2; bb++)
                            {
                                if (bb >= 0 && bb < VoxelsPerSide)
                                {
                                    for (int cc = c - 1; cc < c + 2; cc++)
                                    {

                                        if (cc >= 0 && cc < VoxelsPerSide)
                                        {
                                            //Debug.Log(aa.ToString() + " "+ bb.ToString() + " "+ cc.ToString());
                                            //if (!(aa==a && bb==b && cc==c))
                                            //{
                                                    
                                            CellArray[a,c,b].Neighbours.Add(CellArray[aa,cc,bb]);

                                            //}
                                                
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                            }
                            
                            
                            
                        }
                        
                    }
                    
                    
                        
                    
                }
            }
        }
        
    }
    
    
}
