﻿using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NeutronServerEvents : MonoBehaviour
{
    void OnEnable()
    {
        NeutronServerFunctions.onPlayerDisconnected += OnPlayerDisconnected;
        NeutronServerFunctions.onPlayerInstantiated += OnPlayerInstantiated;
        NeutronServerFunctions.onPlayerDestroyed += OnPlayerDestroyed;
        NeutronServerFunctions.onPlayerJoinedChannel += OnPlayerJoinedChannel;
        NeutronServerFunctions.onPlayerLeaveChannel += OnPlayerLeaveChannel;
        NeutronServerFunctions.onPlayerJoinedRoom += OnPlayerJoinedRoom;
        NeutronServerFunctions.onPlayerLeaveRoom += OnPlayerLeaveRoom;
        NeutronServerFunctions.onChanged += OnPlayerPropertiesChanged;
        NeutronServerFunctions.onCheatDetected += OnCheatDetected;
        ServerOnCollisionEvents.onPlayerCollision += OnPlayerCollision;
        ServerOnCollisionEvents.onPlayerTrigger += OnPlayerTrigger;
    }
    void OnDisable()
    {
        NeutronServerFunctions.onPlayerDisconnected -= OnPlayerDisconnected;
        NeutronServerFunctions.onPlayerInstantiated -= OnPlayerInstantiated;
        NeutronServerFunctions.onPlayerDestroyed -= OnPlayerDestroyed;
        NeutronServerFunctions.onPlayerJoinedChannel -= OnPlayerJoinedChannel;
        NeutronServerFunctions.onPlayerLeaveChannel -= OnPlayerLeaveChannel;
        NeutronServerFunctions.onPlayerJoinedRoom -= OnPlayerJoinedRoom;
        NeutronServerFunctions.onPlayerLeaveRoom -= OnPlayerLeaveRoom;
        NeutronServerFunctions.onChanged -= OnPlayerPropertiesChanged;
        NeutronServerFunctions.onCheatDetected -= OnCheatDetected;
        ServerOnCollisionEvents.onPlayerCollision -= OnPlayerCollision;
        ServerOnCollisionEvents.onPlayerTrigger -= OnPlayerTrigger;
    }

    private void Awake()
    {
        //DontDestroyOnLoad(gameObject);
    }

    private void OnPlayerTrigger(Player player, Collider coll, string type)
    {

    }

    private void OnPlayerCollision(Player mPlayer, Collision coll, string type)
    {
       
    }

    private void OnCheatDetected(Player playerDetected, System.String cheatName)
    {
        
    }

    private void OnPlayerPropertiesChanged(Player mPlayer, NeutronSyncBehaviour properties, System.String propertieName, Broadcast broadcast)
    {
        
    }

    private void OnPlayerLeaveRoom(Player playerLeave)
    {

    }

    private void OnPlayerJoinedRoom(Player playerJoined)
    {

    }

    private void OnPlayerLeaveChannel(Player playerLeave)
    {

    }

    private void OnPlayerJoinedChannel(Player playerJoined)
    {

    }

    private void OnPlayerDestroyed(Player playerDestroyed)
    {

    }

    private void OnPlayerInstantiated(Player playerInstantiated)
    {

    }

    private void OnPlayerDisconnected(Player playerDisconnected)
    {
        Debug.Log($"The player [{playerDisconnected.Nickname}] have disconnected from server :D");
    }
}
