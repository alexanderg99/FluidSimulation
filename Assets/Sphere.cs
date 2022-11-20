using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using DefaultNamespace;
using UnityEngine.VFX;


public class Sphere : MonoBehaviour
{ 
    
    public List<GameObject> nearestNeighbours = new List<GameObject>() {};

    public double mass = 1;
    public double Density;
    public double Pressure;
    public Vector3 velocity;
    public Vector3 accel;

    public double prevDensity = 0.3;
    public double prevPressure;
    public Vector3 prevPosition;
    public Vector3 prevVelocity;
    public Vector3 prevAccel;
    
    
    public Vector3 surfaceNormal;
    public int StiffnessConstant = 1;
    public int RestDensity = 1;
    public double viscosity = Math.Pow(10, -6);
    public Cell CellBelongTo = new Cell(Vector3.zero);

    public Vector3 prev;
    
    double density(List<GameObject> Neighbours)
    {

        double d = 0;
        for (int i = 0; i < Neighbours.Count; i++)
        {
            d += this.mass * utils.kernel(prevPosition, Neighbours[i].GetComponent<Sphere>().prevPosition, 1, 3);
        }

        return d;

    }

    double pressure(int stiffnessConstant, int restDensity, List<GameObject> Neighbours)
    {
        return stiffnessConstant * (Math.Pow((density(Neighbours) / restDensity), 7) - 1);
    }

    private Vector3 NablaWij(Sphere neighbour, double q)
    {
        //set h = 1
            
        //start nablaW calculation
        Vector3 nablaW = Vector3.zero;
            
        if (q <= 0.5)
        {
            double partialX =
                (36 * Math.Pow(q, 2) - 12 * q)*(prevPosition.x - neighbour.GetComponent<Sphere>().prevPosition.x);
            double partialY =
                (36 * Math.Pow(q, 2) - 12 * q)*(prevPosition.y - neighbour.GetComponent<Sphere>().prevPosition.y);
            double partialZ =
                (36 * Math.Pow(q, 2) - 12 * q)*(prevPosition.z - neighbour.GetComponent<Sphere>().prevPosition.z);
            nablaW = new Vector3((float) partialX, (float) partialY, (float) partialZ);
        }
        else if (q < 1)
        {
            double partialX =
                (-6*Math.Pow((1-q),2)) * (prevPosition.x - neighbour.GetComponent<Sphere>().prevPosition.x);
            double partialY =
                (-6*Math.Pow((1-q),2)) * (prevPosition.y - neighbour.GetComponent<Sphere>().prevPosition.y);
            double partialZ =
                (-6*Math.Pow((1-q),2)) * (prevPosition.z - neighbour.GetComponent<Sphere>().prevPosition.z);
            nablaW = new Vector3((float) partialX, (float) partialY, (float) partialZ);
        }

        else
        {
            nablaW = Vector3.zero;
        }

        return nablaW;

    }


    private Vector3 viscosityForce(double h = 1)
    {
        Vector3 force = Vector3.zero;

        for (int i = 0; i < nearestNeighbours.Count; i++)
        {
            Sphere neighbour = nearestNeighbours[i].GetComponent<Sphere>();
            double coefficient1 = neighbour.mass / neighbour.prevDensity;
            Vector3 A_ij = prevVelocity - neighbour.prevVelocity;
            double q = (prevPosition - neighbour.prevPosition).magnitude / 1;
            Vector3 xij = prevPosition - neighbour.prevPosition;
            Vector3 nablaWij = NablaWij(neighbour, q);
            double numerator = Vector3.Dot(xij, nablaWij);
            double denominator = Vector3.Dot(xij, xij) + 0.01 * Math.Pow(h, 2);


            Vector3 sum = (float)(coefficient1 * numerator / denominator) * A_ij;
            force += sum;

        }
        return (2 *(float)mass * (float)viscosity) * force;
    }

    private Vector3 otherForce()
    {
        Vector3 g = new Vector3(0, -9.8f, 0);
        return (float)mass * g;
    }
    
    
    
    private Vector3 pressureForce()
    {
        Vector3 force = Vector3.zero;

        for (int i = 0; i < nearestNeighbours.Count; i++)
        {
            Sphere neighbour = nearestNeighbours[i].GetComponent<Sphere>();

            double coefficient = neighbour.mass *
                                 (prevPressure / Math.Pow(prevDensity, 2) +
                                  neighbour.prevPressure / Math.Pow(neighbour.prevDensity, 2));
            
            double q = (prevPosition - neighbour.prevPosition).magnitude / 1;
            //set h = 1
            
            //start nablaW calculation
            Vector3 nablaW = Vector3.zero;

            nablaW = NablaWij(neighbour, q);
            
            
            //end nablaW calculation

            force = force + (float) coefficient * nablaW;
        }
        
        return -(float)mass*force;
    }
    



    public void FindNearestNeighbours()
    {
        List<GameObject> Neighbours = new List<GameObject>() { };
        
        for (int i = 0; i < CellBelongTo.Neighbours.Count; i++)
        {
            for (int j = 0; j < CellBelongTo.Neighbours[i].Contains.Count; j++)
            {
                //for every neighbouring cell, add all the spheres that belong to it into the Neighbours list
                //of the sphere.
                
                //this is not runnign
                
                
                Neighbours.Add(CellBelongTo.Neighbours[i].Contains[j].gameObject);
            }
        }

        nearestNeighbours = Neighbours;
    }

    public void UpdateBelongingCell()
    {
        
        //Find Hash Number: 
        double hashPos = utils.hash(new Vector3((float)Math.Floor(this.transform.position.x / UniformGrid.VoxelSize), (float)Math.Floor(this.transform.position.y/ UniformGrid.VoxelSize),
            (float)Math.Floor(this.transform.position.z/ UniformGrid.VoxelSize)));
        
        //What I thnk it does: the hash function maps the center of the sphere to the next lower-left corner.
        CellBelongTo = UniformGrid.hashTable[hashPos];
        
    }
    
    
    public List<GameObject> FindNearestNeighboursColliderVersion(Vector3 from, List<GameObject> nearestNeighbours, float radius)
    {

        //use a neighbour detection algorithmxw
        Vector3 location = Vector3.zero;

        Collider[] neighbourCollider = Physics.OverlapSphere(transform.position, radius);
        //find all colliders touching the sphere.
        for (int i = 0; i < neighbourCollider.Length; i++)
        {
            nearestNeighbours.Add(neighbourCollider[i].transform.gameObject);
        }
        

        List<GameObject> neighbours = nearestNeighbours.Where(n => n && n != this.transform.gameObject &&
                                                           (n.transform.position - transform.position).sqrMagnitude <
                                                           radius)
            .OrderBy(n => (n.transform.position - transform.position).sqrMagnitude)
            .ToList();

        return neighbours;

    }

    

    private void OnMouseDown()
    {
        
        //Debug.Log(CellBelongTo.Neighbours.Count.ToString() + " " + nearestNeighbours.Count.ToString());
        for (int i = 0; i < nearestNeighbours.Count; i++)
        {
            var cubeRenderer = nearestNeighbours[i].GetComponent<Renderer>();
            // Call SetColor using the shader property name "_Color" and setting the color to red
            cubeRenderer.material.SetColor("_Color", Color.red);
        }
    }

    private void OnMouseUp()
    {
        //Debug.Log("MouseUppie");
        for (int i = 0; i < nearestNeighbours.Count; i++)
        {
            var cubeRenderer = nearestNeighbours[i].GetComponent<Renderer>();
            // Call SetColor using the shader property name "_Color" and setting the color to red
            cubeRenderer.material.SetColor("_Color", Color.white);
        }
    }

    
    
    IEnumerator waiter(){
        
        yield return new WaitForSeconds(1);

    //Rotate 20 deg
    //transform.Rotate(new Vector3(20, 0, 0), Space.World);
}

    // Start is called before the first frame update
    void Start()
    {
        
        // what doI need? sometimes I get NaNs.
        StartCoroutine(waiter());
        
        
        prevPosition = transform.position;
        velocity = new Vector3(0,0,0);
        accel = new Vector3(0,0,0);
        prevVelocity = new Vector3(0,0,0);
        prevAccel = new Vector3(0,0,0);
        
        Density = density(nearestNeighbours);
        Pressure = pressure(StiffnessConstant, RestDensity, nearestNeighbours);
       
        prevDensity = Density;
        prevPressure = Pressure;
        
        
        
        
        
        //
        
        
        
        
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
        
        prevDensity = Density;
        prevPressure = Pressure;
        prevVelocity = velocity + Vector3.zero;
        prevAccel = accel+ Vector3.zero;
        
        //velocity = (transform.position - prev) / Time.deltaTime;
        //prev = transform.position;
        
        Density = density(nearestNeighbours);
        Pressure = pressure(StiffnessConstant, RestDensity, nearestNeighbours);
        Vector3 totalForce = pressureForce() + viscosityForce() + otherForce();
        accel = totalForce / (float)mass;

        velocity += accel * Time.deltaTime;







        prevPosition = this.transform.position;





    }
}
