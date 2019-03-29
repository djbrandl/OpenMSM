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
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace OpenMSM.ServiceDefinitions
{
    public interface IChannelManagementServiceSoap
    {
        void CreateChannel(string ChannelURI, ChannelType ChannelType, string ChannelDescription, [System.Xml.Serialization.XmlElementAttribute("SecurityToken")] System.Xml.XmlElement[] SecurityToken);
        void AddSecurityTokens(string ChannelURI, [System.Xml.Serialization.XmlElementAttribute("SecurityToken")] System.Xml.XmlElement[] SecurityToken);
        void RemoveSecurityTokens(string ChannelURI, [System.Xml.Serialization.XmlElementAttribute("SecurityToken")] System.Xml.XmlElement[] SecurityToken);
        void DeleteChannel(string ChannelURI);
        Channel GetChannel(string ChannelURI);
        Channel[] GetChannels();
    }

    public partial class GetChannelsRequest
    {

        public GetChannelsRequest()
        {
        }
    }

    [DataContract(Name = "ChannelType", Namespace = "http://www.openoandm.org/ws-isbm/")]
    public enum ChannelType
    {
        [EnumMember]
        Publication,
        [EnumMember]
        Request,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.openoandm.org/ws-isbm/")]
    public partial class Channel
    {

        private string channelURIField;

        private ChannelType channelTypeField;

        private string channelDescriptionField;
        
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string ChannelURI
        {
            get
            {
                return this.channelURIField;
            }
            set
            {
                this.channelURIField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public ChannelType ChannelType
        {
            get
            {
                return this.channelTypeField;
            }
            set
            {
                this.channelTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public string ChannelDescription
        {
            get
            {
                return this.channelDescriptionField;
            }
            set
            {
                this.channelDescriptionField = value;
            }
        }
    }

}
