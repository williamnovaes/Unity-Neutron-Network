using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public class NeutronServerDatabase : NeutronServerFunctions
{
    const string @USER = "@user";
    const string @PASS = "@pass";
    public IEnumerator Login(Player mSocket, string username, string password)
    {
        WWWForm formData = new WWWForm();
        { formData.AddField(@USER, username); formData.AddField(@PASS, password); };

        using (UnityWebRequest request = UnityWebRequest.Post(NeutronServerConstants.URL_LOGIN, formData))
        {
            yield return request.SendWebRequest();
            //================================================//
            string response = request.downloadHandler.text;
            //================================================//
            try
            {
                int ID = int.Parse(response);
                if (ID != 0)
                {
                    if (!IDS.ContainsKey(mSocket.tcpClient))
                    {
                        if (IDS.TryAdd(mSocket.tcpClient, ID))
                        {
                            Response(mSocket, Packet.Login, SendTo.Only, new object[] { 1, ID }); // Correct user and pass is 1;
                        }
                    }
                }
                else if (ID == 0) Response(mSocket, Packet.Login, SendTo.Only, new object[] { 0, ID }); // Wrong User And Pass is 0
            }
            catch
            {
                Response(mSocket, Packet.Login, SendTo.Only, new object[] { 0, 0 });
            }
        }
    }

    void Response(Player mSocket, Packet packetToResponse, SendTo send, object[] response)
    {
        using (NeutronWriter writer = new NeutronWriter())
        {
            writer.WritePacket(Packet.Database);
            writer.WritePacket(packetToResponse);
            writer.Write(response.Serialize());
            //========================================================================================\\
            mSocket.Send(send, writer.GetBuffer(), Broadcast.None, null, ProtocolType.Tcp, null, null);
        }
    }
}
