using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPixel : MonoBehaviour
{
    //public Texture2D source;
    //public Texture2D destination;
    //public Texture2D image;
    public Texture2D heightmap;
    public Vector3 size = new Vector3(100, 10, 100);


    // Start is called before the first frame update
    void Start()
    {
        //Getimage();
    }

    // Update is called once per frame
    void Update()
    {
        Getimage();
    }

    public void Getpixel()
    {
        // Get a copy of the color data from the source Texture2D, in high-precision float format.
        // Each element in the array represents the color data for an individual pixel.
        //int sourceMipLevel = 0;
        //Color[] pixels = source.GetPixels(sourceMipLevel);

        // If required, manipulate the pixels before applying them to the destination Texture2D.
        // This example code reverses the array, which rotates the image 180 degrees.
        //System.Array.Reverse(pixels, 0, pixels.Length);

        // Set the pixels of the destination Texture2D.
        //int destinationMipLevel = 0;
        //destination.SetPixels(pixels, destinationMipLevel);

        // Apply changes to the destination Texture2D, which uploads its data to the GPU.
        //destination.Apply();
    }

    public void Getimage()
    {
        //var x = 0;
        //var y = 0;
        //for (int i = 0; i < image.width; i++)
        //    for (int j = 0; j < image.height; j++)
        //    {
        //        Color pixel = image.GetPixel(i, j);

        //        // if it's a white color then just debug...
        //        if (pixel == Color.white)
        //            whitePixels++;
        //        else
        //            blackPixels++;
        //    }
        
        //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //cube.transform.position = new Vector3(0, 0.5f, 0);
        //Debug.Log(image.GetPixel(1, 5));

        int x = Mathf.FloorToInt(transform.position.x / size.x * heightmap.width);
        int z = Mathf.FloorToInt(transform.position.z / size.z * heightmap.height);
        Vector3 pos = transform.position;
        pos.y = heightmap.GetPixel(x, z).grayscale * size.y;
        transform.position = pos;
    }
}
