using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PopulateWorld : MonoBehaviour
{

    [SerializeField] private GameObject Particle;

    public int x,y,z;


    public static double StiffnessConstant;

    public bool randomColor;
    //public Color[] colorChoices;

    public void Start()
    {

        for (int a = 0; a < x; a++)
        {
            for (int b = 0; b < z; b++)
            {
                for (int c = 0; c < y; c++)
                {
                    
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = new Vector3(a,  c , b);
                    String name = String.Format("Particle {0},{1},{2}", a, c, b);
                    sphere.name = name;
                    sphere.AddComponent<Sphere>();
                    sphere.AddComponent<SphereCollider>();
                    sphere.AddComponent<Rigidbody>();
                }
                

            }
        }
        
        
        GameObject FarGuy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        FarGuy.transform.position = new Vector3(-1,  0 , 2);
        String fname = "FarGuy";
        FarGuy.name = fname;
        FarGuy.AddComponent<Sphere>();
        FarGuy.AddComponent<SphereCollider>();
        FarGuy.AddComponent<Rigidbody>();
        
        
        
        
        
        
    }

    // Update is called once per frame
    void Update()
    {

    }


}

        
    

