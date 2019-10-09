﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("http://www.openoandm.org/ws-isbm/", ClrNamespace="www.openoandm.org.wsisbm")]

namespace OpenMSM.Web.ServiceDefinitions
{

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", ConfigurationName = "ProviderPublicationService")]
    public interface IProviderPublicationService
    {

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/OpenPublicationSession", ReplyAction = "*")]
        [System.ServiceModel.FaultContractAttribute(typeof(www.openoandm.org.wsisbm.ChannelFault), Action = "http://www.openoandm.org/ws-isbm/OpenPublicationSession", Name = "ChannelFault")]
        [System.ServiceModel.FaultContractAttribute(typeof(www.openoandm.org.wsisbm.OperationFault), Action = "http://www.openoandm.org/ws-isbm/OpenPublicationSession", Name = "OperationFault")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "SessionID")]
        string OpenPublicationSession(string ChannelURI);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/OpenPublicationSession", ReplyAction = "*")]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "SessionID")]
        System.Threading.Tasks.Task<string> OpenPublicationSessionAsync(string ChannelURI);

        // CODEGEN: Parameter 'Topic' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'System.Xml.Serialization.XmlElementAttribute'.
        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/PostPublication", ReplyAction = "*")]
        [System.ServiceModel.FaultContractAttribute(typeof(www.openoandm.org.wsisbm.SessionFault), Action = "http://www.openoandm.org/ws-isbm/PostPublication", Name = "SessionFault")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        [return: System.ServiceModel.MessageParameterAttribute(Name = "MessageID")]
        PostPublicationResponse PostPublication(PostPublicationRequest request);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/PostPublication", ReplyAction = "*")]
        System.Threading.Tasks.Task<PostPublicationResponse> PostPublicationAsync(PostPublicationRequest request);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/ExpirePublication", ReplyAction = "*")]
        [System.ServiceModel.FaultContractAttribute(typeof(www.openoandm.org.wsisbm.SessionFault), Action = "http://www.openoandm.org/ws-isbm/ExpirePublication", Name = "SessionFault")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        void ExpirePublication(string SessionID, string MessageID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/ExpirePublication", ReplyAction = "*")]
        System.Threading.Tasks.Task ExpirePublicationAsync(string SessionID, string MessageID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/ClosePublicationSession", ReplyAction = "*")]
        [System.ServiceModel.FaultContractAttribute(typeof(www.openoandm.org.wsisbm.SessionFault), Action = "http://www.openoandm.org/ws-isbm/ClosePublicationSession", Name = "SessionFault")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        void ClosePublicationSession(string SessionID);

        [System.ServiceModel.OperationContractAttribute(Action = "http://www.openoandm.org/ws-isbm/ClosePublicationSession", ReplyAction = "*")]
        System.Threading.Tasks.Task ClosePublicationSessionAsync(string SessionID);
    }


    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(XMLContent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(StringContent))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BinaryContent))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.openoandm.org/ws-isbm/")]
    public abstract partial class MessageContent
    {
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.openoandm.org/ws-isbm/")]
    public partial class XMLContent : MessageContent
    {

        private System.Xml.XmlElement anyField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAnyElementAttribute(Order = 0)]
        public System.Xml.XmlElement Any
        {
            get
            {
                return this.anyField;
            }
            set
            {
                this.anyField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.openoandm.org/ws-isbm/")]
    public partial class StringContent : MessageContent
    {

        private string contentField;

        private string mediaTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string Content
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mediaType
        {
            get
            {
                return this.mediaTypeField;
            }
            set
            {
                this.mediaTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.7.3081.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.openoandm.org/ws-isbm/")]
    public partial class BinaryContent : MessageContent
    {

        private byte[] contentField;

        private string mediaTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType = "base64Binary", Order = 0)]
        public byte[] Content
        {
            get
            {
                return this.contentField;
            }
            set
            {
                this.contentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string mediaType
        {
            get
            {
                return this.mediaTypeField;
            }
            set
            {
                this.mediaTypeField = value;
            }
        }
    }
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName = "PostPublication", WrapperNamespace = "http://www.openoandm.org/ws-isbm/", IsWrapped = true)]
    public partial class PostPublicationRequest
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", Order = 0)]
        public string SessionID;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", Order = 1)]
        public MessageContent MessageContent;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("Topic")]
        public string[] Topic;

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", Order = 3)]
        [System.Xml.Serialization.XmlElementAttribute(DataType = "duration")]
        public string Expiry;

        public PostPublicationRequest()
        {
        }

        public PostPublicationRequest(string SessionID, MessageContent MessageContent, string[] Topic, string Expiry)
        {
            this.SessionID = SessionID;
            this.MessageContent = MessageContent;
            this.Topic = Topic;
            this.Expiry = Expiry;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName = "PostPublicationResponse", WrapperNamespace = "http://www.openoandm.org/ws-isbm/", IsWrapped = true)]
    public partial class PostPublicationResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://www.openoandm.org/ws-isbm/", Order = 0)]
        public string MessageID;

        public PostPublicationResponse()
        {
        }

        public PostPublicationResponse(string MessageID)
        {
            this.MessageID = MessageID;
        }
    }
}
