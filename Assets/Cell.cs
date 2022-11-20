using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector3 Position;
    public List<Sphere> Contains = new List<Sphere>();
    public List<Cell> Neighbours = new List<Cell>();
    private int Length;

    public Cell(Vector3 position)
    {
        Position = position;
    }

    public override string ToString()
    {
        return "I am the Cell at Position x= " + Position.x.ToString() + "y=" + Position.y.ToString()
               + "z=" + Position.z.ToString();
    }


    // Start is called before the first frame update
    void Start()
    {
        //Initialization of Neighbours is done by Grid.
        //T
        //
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
