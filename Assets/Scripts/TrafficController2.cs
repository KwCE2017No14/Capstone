using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficController2 : TrafficLight
{
//    public Light[] lights = new Light[3];//red / yellow / green 순
//    public int time = 0;
//    public int inten_light = 2;
//    public int period = 600;

    // Use this for initialization
    void Start(){
        lights[0].intensity = 0;//red
        lights[1].intensity = 0;//yellow
        lights[2].intensity = inten_light;//green
    }

    // Update is called once per frame
    void FixedUpdate(){
        time += 1;
        ChangeColor();
    }

    void ChangeColor(){
        if (time == 0.42*period){//green to yellow
            lights[2].intensity = 0;
            lights[1].intensity = inten_light;
        }
        else if (time == 0.5*period){//yellow to red
            lights[1].intensity = 0;
            lights[0].intensity = inten_light;
        }
        else if (time == period){//red to green
            lights[0].intensity = 0;
            lights[2].intensity = inten_light;
            time = 0;
        }
    }
}
