﻿using UnityEngine;
using System.Collections;

public class MissileBehavior : MonoBehaviour {


    public GameObject Target { get; set; }
    public GameObject Owner { get; set; }
    public const string explosion_prefab_path = "Prefabs/Power/Explosion";
    private const float maxMissileSteering = 6.0f;
    private const float maxMissileSpeed = 8.0f;
    private const float explosionForce = 60.0f;
    private const float timeToLive = 2.5f;
	private const float smoothCurve = 10f;
	private AudioClip explosionSound;

    public void SetTarget(GameObject target, GameObject owner)
    {

        Target = target;
        Owner = owner;

        if (Network.peerType != NetworkPeerType.Disconnected &&
            GetComponent<NetworkView>().isMine)
        {

            GetComponent<NetworkView>().RPC("RPCSetTarget", RPCMode.Others, target.GetComponent<NetworkView>().viewID, owner.GetComponent<NetworkView>().viewID);

        }

    }

    [RPC]
    private void RPCSetTarget(NetworkViewID target_id, NetworkViewID owner_id)
    {

        SetTarget(NetworkView.Find(target_id).gameObject,
                  NetworkView.Find(owner_id).gameObject);

    }

    void Start()
    {

		if(NetworkHelper.IsServerSide())
		{

			StartCoroutine("DestroyMissileTTL");

		}

		explosionSound = Resources.Load<AudioClip>("Sounds/Powers/Explosion");

    }

    void Update()
    {

        // Movements
        FloatingObject floating = GetComponent<FloatingObject>();
        if (Target != null)
        {
            Vector3 direction = Target.transform.position - this.transform.position;
            direction.Normalize();
            Vector3 new_forward = Vector3.RotateTowards(transform.forward, direction, Time.deltaTime * maxMissileSteering, 0);
            new_forward.Normalize();
            
            this.transform.rotation = Quaternion.LookRotation(new_forward, -floating.ArenaDown);
        }
        Vector3 forwardProjected = Vector3.Cross(floating.ArenaDown,
                                                    Vector3.Cross(-floating.ArenaDown, this.transform.forward)
                                                    ).normalized;
        
		forwardProjected = Vector3.Lerp(this.transform.forward, forwardProjected, Time.deltaTime * smoothCurve);

        this.GetComponent<Rigidbody>().AddForce(forwardProjected * maxMissileSpeed, ForceMode.VelocityChange);
    }

    void OnCollisionEnter(Collision collision)
    {

		if(NetworkHelper.IsServerSide())
        {
			GameObject objectCollided = collision.gameObject;

			if (objectCollided.tag == Tags.Ship && objectCollided != Owner)
            {

                if (Network.peerType == NetworkPeerType.Disconnected)
                {

                    OnImpact(collision.gameObject);

                }
                else
                {

                    GetComponent<NetworkView>().RPC("RPCOnImpact", RPCMode.All, collision.gameObject.GetComponent<NetworkView>().viewID);

                }

                collision.gameObject.GetComponent<TailController>().GetDetacherDriverStack().GetHead().DetachOrbs(int.MaxValue, collision.gameObject.GetComponent<Tail>());

            }

        }

    }

    private IEnumerator DestroyMissileTTL()
    {
        
        //Delayed destrution
        yield return new WaitForSeconds(timeToLive);

        if (Network.peerType == NetworkPeerType.Disconnected)
        {

            RPCDestroyMissile();

        }
        else
        {

            GetComponent<NetworkView>().RPC("RPCDestroyMissile", RPCMode.All);

        }

    }

    private void OnImpact(GameObject target)
    {

        target.GetComponent<Rigidbody>().AddForce(transform.forward * explosionForce, ForceMode.Impulse);

        StartCoroutine("DestroyMissile");

    }

    [RPC]
    private void RPCOnImpact(NetworkViewID target_id)
    {

        //Apply the force
        OnImpact(NetworkView.Find(target_id).gameObject);

    }

    [RPC]
    private void RPCDestroyMissile()
    {

        StartCoroutine("DestroyMissile");

    }

    private IEnumerator DestroyMissile()
    {
        var explosion_resource = Resources.Load(explosion_prefab_path);
        GameObject explosion = GameObject.Instantiate(explosion_resource, this.gameObject.transform.position, Quaternion.identity) as GameObject;
        
		AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        Target = null;
        GetComponent<Collider>().enabled = false;
        GetComponent<ParticleSystem>().enableEmission = false;
        GetComponent<MeshFilter>().mesh = null;

        // Delayed for GFX
        yield return new WaitForSeconds(1.0f);

        Destroy(explosion);

        if (Network.peerType == NetworkPeerType.Disconnected)
        {

            Destroy(this.gameObject);

        }
        else if (Network.peerType == NetworkPeerType.Server)
        {

            Network.Destroy(this.gameObject);

        }

    }

}
