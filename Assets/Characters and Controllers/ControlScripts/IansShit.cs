using UnityEngine;
using System.Collections;

// Use this for initialization

/*enum ClimbState {
	none = 0,
	canclimb = 1,
	climbing = 2,
	topOff = 3
};*/

/*public class ParkourController : MonoBehaviour {
	public float CrouchSpeed = 0.5f;
	public float dt = 0.0f;
	
	public int zone   = 0;
	public int top    = 0;
	public int ground = 0;
	public int key    = 0;
	public int crouchKey = 0;
	public int crouchKeep = 0;
	public int crouching = 0;

    public bool canVault = false;
    private int vaulting = 0;
    public float vaultScoot = 1.0f;
    public float vaultSlowDown = 0.1f;
    private Vector3 vaultEndVelocity = new Vector3(0.0f, 0.0f, 0.0f);
	
	public float dist = 1.0f;
	
	public int state  = (int)ClimbState.none;
	
	private CharacterMotor motor;
	void Awake () {
		motor = GetComponent<CharacterMotor>();
	}
	
	void UpdateCrouchState() {
		if (crouchKey>0 || crouchKeep>0) {
			if (crouching==0) {
				transform.localScale -= new Vector3(0.0f,0.5f,0.0f);
				transform.localPosition -= new Vector3(0.0f,0.5f,0.0f);
				dt = 0.0f;
			}
			crouching = 1;
		} else {
			if (crouching>0) {
				transform.localScale += new Vector3(0.0f,0.5f,0.0f);
				transform.localPosition += new Vector3(0.0f,0.5f,0.0f);
			}
			crouching = 0;
		}
	}
	// Update is called once per frame (sort of)
	void Update () {
        // current velocity from the character motor
        Vector3 velocity = motor.movement.velocity;

		RaycastHit hitinfo = new RaycastHit();
		int layermask = 1+2+4+8+16; // Player
		Physics.Raycast(Camera.main.transform.localPosition, Vector3.down, out hitinfo, Mathf.Infinity, layermask);
		dist = hitinfo.distance;
		
		if (crouching>0) {
			dt += Time.deltaTime;
			if (dt>1.0f) {
				dt = 1.0f;
			}
		}
		
		//if (Input.GetKeyDown(KeyCode.Space)) {}
		if (state==(int)ClimbState.canclimb && Input.GetKeyDown(KeyCode.Space)) {
			key++;
			TriggerStateUpdate();
		} else if (state==(int)ClimbState.climbing && Input.GetKeyDown(KeyCode.Space)) {
			key--;
			TriggerStateUpdate();
		} else if (canVault && Input.GetKeyDown(KeyCode.Space)) {
            vaultEndVelocity = velocity - motor.inputMoveDirection * vaultSlowDown;
            vaulting = 1;
        }

        // vault
        if (vaulting > 0) {
            // vault state 1 is start
            if (motor.IsGrounded()) {
                motor.SetVelocity(velocity + motor.inputMoveDirection * vaultScoot);
                ++vaulting;
            }
            // vault state 2 is ending
            else if (vaulting > 1 && velocity.sqrMagnitude > vaultEndVelocity.sqrMagnitude) {
                motor.SetVelocity(Vector3.Lerp(velocity, vaultEndVelocity, Time.time));
            }
        }

		if (Input.GetKeyDown(KeyCode.LeftShift)) {
			crouchKey++;
			UpdateCrouchState();
		} else if (Input.GetKeyUp(KeyCode.LeftShift)) {
			crouchKey--;
			UpdateCrouchState();
		}
		
		Vector3 directionVector;
		if (state==(int)ClimbState.topOff) {
			directionVector = new Vector3(Input.GetAxis("Horizontal"),1.5f,3.0f);
			motor.SetVelocity(directionVector);
			motor.inputJump = false;
		} else if (state==(int)ClimbState.climbing) {
			//motor.grounded = false;
			directionVector = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"),0);
			motor.SetVelocity(directionVector);
			motor.inputJump = false;
		} else {
			//motor.grounded = true;
			directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			
			if (directionVector != Vector3.zero) {
				// Get the length of the directon vector and then normalize it
				// Dividing by the length is cheaper than normalizing when we already have the length anyway
				var directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;
				
				// Make sure the length is no bigger than 1
				directionLength = Mathf.Min(1, directionLength);
				
				// Make the input vector more sensitive towards the extremes and less sensitive in the middle
				// This makes it easier to control slow speeds when using analog sticks
				directionLength = directionLength * directionLength;
				
				// modify the speed for crouching
				directionLength*= (1.0f - dt*CrouchSpeed*crouching);
				
				// Multiply the normalized direction vector by the modified length
				directionVector = directionVector * directionLength;
			}
			motor.inputJump = Input.GetButton("Jump");
		}
		// Apply the direction to the CharacterMotor
		motor.inputMoveDirection = transform.rotation * directionVector;
	}
	
	// Use this for initialization
	void Start () {
	
	}
	void TriggerStateUpdate() {
		if (zone>0 && ground>0 && key == 0) {
			state = (int)ClimbState.canclimb;
		} else if (zone>0 && top==0 && key>0) {
			state = (int)ClimbState.climbing;
		} else if (top>0 && key>0) {
			state = (int)ClimbState.topOff;
		} else {
			state = (int)ClimbState.none;
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.tag == "Climbable Zone") {
			zone++;
		}
		if (other.tag == "Climbable Top") {
			top++;
		}
		if (other.tag == "Climbable Ground") {
			ground++;
		}
		if (other.tag == "Crouch Keep") {
			crouchKeep++;
		}
        if (other.tag == "Vault Ground") {
            canVault = true;
        }
		TriggerStateUpdate();
		UpdateCrouchState();
	}
	void OnTriggerExit(Collider other) {
		if (other.tag == "Climbable Zone") {
			zone--;
			key=0;
		}
		if (other.tag == "Climbable Top") {
			top--;
		}
		if (other.tag == "Climbable Ground") {
			ground--;
		}
		if (other.tag == "Crouch Keep") {
			crouchKeep--;
		}
        if (other.tag == "Vault Ground") {
            canVault = false;
        }
		TriggerStateUpdate();
		UpdateCrouchState();
	}
}*/