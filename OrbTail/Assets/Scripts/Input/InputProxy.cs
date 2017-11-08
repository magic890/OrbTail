﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used to exchange and read input data over the network
/// </summary>
public class InputProxy : MonoBehaviour, IInputBroker{

    // Use this for initialization
	public void Start () {
        
        if (NetworkHelper.IsOwnerSide(GetComponent<NetworkView>()))
        {

            var identity = GetComponent<PlayerIdentity>();

            if (!identity.IsHuman)
            {

                //AI
                InputBroker = GetComponent<PlayerAI>().GetInputBroker();
				
            }
            else
            {

                //Human
                if (SystemInfo.supportsAccelerometer)
                {

                    //Mobile platform
                    InputBroker = new MobileInputBroker();

                }
                else
                {

                    //Desktop platform
                    InputBroker = new DesktopInputBroker();

                }

            }

        }
        else
        {

            //Remote, don't care
            InputBroker = null;

        }

	}
	
	// Update is called once per frame
	public void Update () {

        if (InputBroker != null)
        {

            InputBroker.Update();

            Acceleration = InputBroker.Acceleration;
            Steering = InputBroker.Steering;
            fired_powers_ = InputBroker.FiredPowerUps;

        }

	}

    /// <summary>
    /// Called when the script needs to be serialized
    /// </summary>
    public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        
        //Stream: | Acceleration | Steering | #Powers | Group(1) | Group(2) | ...

        if (stream.isWriting &&
            GetComponent<NetworkView>().isMine)
        {

            Serialize(stream);

        }else if( stream.isReading &&
                  !GetComponent<NetworkView>().isMine)
        {

            Deserialize(stream);

        }

    }

    /// <summary>
    /// Returns the acceleration command's status. 0 no acceleration, 1 maximum acceleration.
    /// </summary>
    public float Acceleration { get; private set; }

    /// <summary>
    /// Returns the steering command's status. -1 steer left, 0 no steering, 1 steer right
    /// </summary>
    public float Steering { get; private set; }

    /// <summary>
    /// Returns a collection which indicates all the power ups the user wants to fire. The elements indicates just the group of the proper power
    /// </summary>
    public ICollection<int> FiredPowerUps
    {

        get
        {
            return new List<int>(fired_powers_);
        }

    }

    /// <summary>
    /// Serializes the proxy over the net
    /// </summary>
    /// <param name="stream">The stream to write into</param>
    private void Serialize(BitStream stream){

        float acceleration = Acceleration;
        float steering = Steering;
        float powers_count = fired_powers_.Count;
        int group;

        stream.Serialize(ref acceleration);
        stream.Serialize(ref steering);
        stream.Serialize(ref powers_count);

        
        foreach (int power_group in fired_powers_)
        {

            group = power_group;
            stream.Serialize(ref group);

        }

    }

    /// <summary>
    /// Deserializes the proxy from the net
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    private void Deserialize(BitStream stream){

        float acceleration = 0.0f;
        float steering = 0.0f;
        float powers_count = 0.0f;
        int group = 0;

        stream.Serialize(ref acceleration);
        stream.Serialize(ref steering);
        stream.Serialize(ref powers_count);

        Acceleration = acceleration;
        Steering = steering;
        fired_powers_ = new List<int>();

        
        for (; powers_count > 0; powers_count--)
        {

            stream.Serialize(ref group);

            fired_powers_.Add(group);

        }

    }

    /// <summary>
    /// The input broker used to read user's input or to exechange data
    /// </summary>
    private IInputBroker InputBroker { get; set; }

    /// <summary>
    /// The list of the powerups to be fired
    /// </summary>
    private ICollection<int> fired_powers_ = new List<int>();

}
