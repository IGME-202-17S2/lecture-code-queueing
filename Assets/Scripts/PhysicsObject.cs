using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * PhsyicsObject 
 * This component tracks position and movement information for a GameObject
 * It knows how to ApplyForce (but some other script will call that)
 * It runs its own BounceCheck method (to keep the object in-bounds)
 * Movement happens in LateUpdate, so that some other script can call
 *   ApplyForce in its Update, and we can be sure all Forces have been
 *   applied before moving the PhysicsObject.
 */
public class PhysicsObject : MonoBehaviour {

	public Vector3 velocity;

	// there are two seek targets
	public Vector3 target; // is the first, on screen, that we start heading to
	public Vector3 off; // is the second, off screen, that we head to after reaching target

	public float radius = 0.25f;
	public float maxSpeed = 0.125f;
	public List<PhysicsObject> neighbors;

	float dangerDistance = 0.85f;

	float brakes = 1f;

	void Start () {
		Respawn ();
		this.transform.position += new Vector3(0, Random.Range(0, -1.5f), 0);
	}

	void Respawn() {

		// no forces to start off
		velocity = Vector3.zero;
		this.transform.position = new Vector3 (Random.Range (-5f, 5f), 5f, 0);
		target = new Vector3 (0, -4f, 0);
		off = new Vector3 (0, -6f, 0);
	}

	void ClampVelocity() {
		float mag = velocity.magnitude;
		if (mag > maxSpeed) {
			float scale = maxSpeed / mag;
			velocity *= scale;
		}
	}

	public Vector3 Seek(Vector3 target) {
		Vector3 desiredVelocity = target - transform.position;
		Vector3 steering = (desiredVelocity.normalized * maxSpeed) - velocity;
		return steering;
	}
		
	public void SeparateHard(PhysicsObject a, PhysicsObject b, Vector3 a2b) {
		// get a unit vector representing the axis of collision
		Vector3 axis = a2b.normalized;

		// find half of the overlap amount
		float offset = ((2 * radius) - a2b.magnitude) / 2f;

		// move a that distance one way along the collision axis
		a.transform.position = a.transform.position + (-1 * axis) * offset;
		// move be the other way along the collision axis
		b.transform.position = b.transform.position + axis * offset;
	}

	public bool ShouldBrake() {
		Vector3 forward = velocity.normalized;
		foreach (PhysicsObject other in neighbors) {
			if (other != this) {
				Vector3 toOther = other.transform.position - transform.position;

				if (toOther.magnitude < 2 * radius) {
					// force separation if they are too close!
					SeparateHard (this, other, toOther);
				}

				// they are in front if the Dot product of the vector between us
				// is greater than my radius
				// (less than radius means beside or behind)
				bool isInFront = Vector3.Dot (forward, toOther) > radius;

				// they are dangerous if they are in front and closer than the dangerDistance
				bool isDangerous = isInFront && toOther.magnitude < dangerDistance;

				// they are moving slower if their velocity is less than mine
				bool isSlower = other.velocity.sqrMagnitude < this.velocity.sqrMagnitude;

				if ( isDangerous && isSlower) {
					return true;
				}
			}
		}
		return false;
	}
		
	void LateUpdate () {

		Vector3 seekForce = Seek (target);

		if (ShouldBrake ()) {
			// cut the brake multiplier in half
			// to a minimum of 0.3f
			brakes = Mathf.Max (0.3f, brakes * 0.6f);
		} else {
			// increase the brake multiplier by a factor
			// up to a maximum of 1f
			brakes = Mathf.Min (1f, brakes * 1.02f);
		}

		velocity += seekForce;
		velocity *= brakes;

		// obey the speed limit
		ClampVelocity ();

		// update position by velocity (respecting velocity as units per second)
		transform.position += velocity * Time.deltaTime;

		Vector3 distance = target - transform.position;
		if (distance.sqrMagnitude < 0.5f) {
			if (target != off) {
				target = off;
			} else {
				Respawn ();
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere (transform.position, radius);
	}
}
