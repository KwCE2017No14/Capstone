using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TrafficLight : MonoBehaviour
{
    public Light[] lights = new Light[3];//red / yellow / green 순
    public int time = 0;
    public int inten_light = 2;
    public int period = 600;
}

public class TrafficController : TrafficLight
{
//    public Light[] lights=new Light[3];//red / yellow / green 순
//    public int time = 0;
//    public int inten_light = 2;
//    public int period = 600;

	// Use this for initialization
	void Start () {
        lights[0].intensity = inten_light;//red
        lights[1].intensity = 0;//yellow
        lights[2].intensity = 0;//green
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        time += 1;
        ChangeColor();
	}

    void ChangeColor() {
        if (time == 0.5*period){//red to green
            lights[0].intensity = 0;
            lights[2].intensity = inten_light;
        }
        else if (time == 0.92*period){//green to yellow
            lights[2].intensity = 0;
            lights[1].intensity = inten_light;
        }
        else if (time == period) {//yellow to red
            lights[1].intensity = 0;
            lights[0].intensity = inten_light;
            time = 0;
        }
    } 
}
