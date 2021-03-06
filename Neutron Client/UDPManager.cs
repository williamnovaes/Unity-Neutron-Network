﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPManager : NeutronDatabase
{
    protected static Events.OnUDPData onUDPData { get; set; }

    public static void OnUDPReceive(IAsyncResult ia)
    {
        try
        {
            byte[] data = _UDPSocket.EndReceive(ia, ref _IEPRef);
            //==============================================================================================\\
            _UDPSocket.BeginReceive(OnUDPReceive, null);
            //==============================================================================================\\
            if (data.Length > 0)
            {
                byte[] decompressedBuffer = data.Decompress(COMPRESSION_MODE, data.Length);
                //==================================================================================\\
                using (NeutronReader mReader = new NeutronReader(decompressedBuffer))
                {
                    Packet mCommand = mReader.ReadPacket<Packet>();
                    switch (mCommand)
                    {
                        case Packet.SendInput:
                            SerializableInput nInput = mReader.ReadBytes(2048).DeserializeObject<SerializableInput>();
                            //================================================================================================================================
                            SerializableVector3 nVelocity = nInput.Vector;
                            //================================================================================================================================
                            Vector3 velocity = new Vector3(nVelocity.x, nVelocity.y, nVelocity.z);
                            //================================================================================================================================
                            //Neutron.Enqueue(() => playerRB.velocity = velocity);
                            break;
                        case Packet.RPC:
                            HandleRPC(mReader.ReadInt32(), mReader.ReadBytes(4096));
                            break;
                        case Packet.VoiceChat:
                            HandleVoiceChat(mReader.ReadInt32(), mReader.ReadBytes(4096));
                            break;
                    }
                }
            }
            else
            {
                LoggerError("UDP Error");
            }
        }
        catch (SocketException ex) { LoggerError(ex.Message + ":" + ex.ErrorCode); }
    }
}
