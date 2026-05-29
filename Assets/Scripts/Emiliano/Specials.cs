using UnityEngine;
using UnityEngine.InputSystem;

public class Specials : MonoBehaviour
{
    public GameObject player;
    
    //funcion de dash invencible comun y super
    public void Armadillo(bool super)
    {
        //special
        if (!super)
        {
            //dash invencible corto
        }
        //super
        else
        {
            //dash invencible largo
        }
    }

    //especiales de araña
    public void Spider(bool super)
    {
        //super
        if (!super)
        {
            //Dispara una bala en 8 direcciones
        }
        //super
        else
        {
            //Dispara muchas balas en 8 direcciones
        }
    }
}
