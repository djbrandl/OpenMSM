﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by wsdl, Version=4.6.1055.0.
// 
namespace OpenMSM.ServiceDefinitions
{
    public interface INotificationServiceSoap
    {
        void NotifyListener(string SessionID, string MessageID, [System.Xml.Serialization.XmlElementAttribute("Topic")] string[] Topic, string RequestMessageID);
    }
}