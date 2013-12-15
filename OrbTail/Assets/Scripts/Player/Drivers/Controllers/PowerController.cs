﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PowerController : MonoBehaviour
{

    public delegate void DelegatePowerAttached(object sender, GameObject ship, Power power);

    /// <summary>
    /// Fired when a power has been attached
    /// </summary>
    public event DelegatePowerAttached EventPowerAttached;

    private EventLogger event_logger;
    private Dictionary<int, Power> powers;
    private InputProxy input;

    public void Start()
    {

        input = GetComponent<InputProxy>();
        event_logger = GameObject.FindGameObjectWithTag(Tags.Game).GetComponent<EventLogger>();

    }

    public void Awake(){

        powers = new Dictionary<int, Power>();

    }

    public void AddPower(Power power)
    {

        //Deactivate any previous power

        Power old_power = null;
        
        if( powers.TryGetValue(power.Group, out old_power))
        {

            old_power.Deactivate();
            powers.Remove(old_power.Group);

        }

        //Activate and add the power to the bag

        power.Activate(gameObject);

        powers.Add(power.Group, power);

        power.EventDestroyed += RemovePower;

        //Relay the call to others

        if (Network.isServer)
        {

            networkView.RPC("RPCAddPower", RPCMode.Others, power.Name);

        }

        //Fires the attach event
        if (EventPowerAttached != null)
        {

            EventPowerAttached(this, gameObject, power);

        }


    }

    void RemovePower(object sender, int group)
    {

        //Removes the power
        var power = powers[group];
        powers.Remove(power.Group);

        Debug.Log(power.Name + "Removed!");
        
    }

    [RPC]
    public void RPCAddPower(string power_name)
    {

        var power = PowerFactory.Instance.PowerFromName(power_name);

        AddPower(power);

    }

    [RPC]
    public void RPCFirePower(string power_name)
    {

        var group = PowerFactory.Instance.GroupFromName(power_name);

        var power = powers[group];

        power.Fire();

        Debug.Log(power.Name + " Fired! (RPC)");
        
    }

    public void Update()
    {

        //Shared update for each power
        foreach (Power power in new List<Power>( powers.Values ))
        {

            power.Update();

        }

        //Only the owner of the power can shoot it
        if (NetworkHelper.IsOwnerSide(networkView))
        {

            Power power;

            foreach (int group in input.FiredPowerUps.Where((int g) => { return powers.ContainsKey(g); }))
            {

                power = powers[group];

                if (power.Fire()){
                    
                    Debug.Log(power.Name + " Fired!");

                    if (Network.peerType != NetworkPeerType.Disconnected)
                    {

                        networkView.RPC("RPCFirePower", RPCMode.Others, power.Name);

                    }

                }

            }

        }

    }

    public PowerView GetPowerView(int group)
    {

        return powers[group];

    }
}
