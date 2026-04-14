using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class persobouge : MonoBehaviour
{
    public float vitesse;
    Vector3 direction;
    Rigidbody rb;
    bool ausol;
    


   
    private float rotationY = 0f;
    public float sensibiliteX;
    public float forcesaut=1000f;
    public float fakegravity;
    public Animator animator;
    public Transform corps;
    
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();

    }       

    // Update is called once per frame
    void Update()
    {
        Vector3 vertical = transform.forward * Input.GetAxisRaw("Vertical");
        Vector3 horizontal = transform.right * Input.GetAxisRaw("Horizontal");
        
        
        

       
                 
        
        direction = vertical + horizontal;

       
    

       
        

       

        TourneavecCamera();

        if(Input.GetButtonDown("Jump")){

            sauter();
           
        }
        if(ausol==false){

            rb.AddForce(new Vector3(0, fakegravity, 0), ForceMode.Force);
           



        }

       

        Animate();

        
    } 


    private void Animate(){

        float speed=rb.linearVelocity.magnitude;

        animator.SetFloat("speed", speed);

        bool jump= !ausol;
        animator.SetBool("jump", jump);

        corps.localRotation = Quaternion.Euler(direction);




    }

    private void FixedUpdate()
    {

        
        rb.AddForce(direction.normalized * vitesse, ForceMode.Force);
    }

    void TourneavecCamera()
    {

        float mouseX = Input.GetAxis("Mouse X");
        rotationY += mouseX * sensibiliteX;
        //Debug.Log(rotationY);
      
        if(Mathf.Abs(rotationY)>10f){
            transform.localRotation = Quaternion.Euler(0, rotationY, 0);
        }
            
    }   

    void sauter(){


        if (ausol==true){

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);



             rb.AddForce(transform.up * forcesaut, ForceMode.Impulse);




        
        }


       // Physics.gravity = new Vector3(0, -20.0F, 0);


    }    

    private void OnCollisionEnter(Collision touche){

        if (touche.transform.tag=="sol"){

            ausol=true;





        }

    }

    private void OnCollisionExit(Collision touche){

        if (touche.transform.tag=="sol"){

            ausol=false;





        }




    }

   



}