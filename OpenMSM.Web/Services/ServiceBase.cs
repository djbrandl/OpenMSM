using AutoMapper;
using OpenMSM.Data;
using OpenMSM.Data.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.XPath;
using System.ServiceModel;
using www.openoandm.org.wsisbm;

namespace OpenMSM.Web.Services
{
    public abstract class ServiceBase
    {
        public static string TokenSaltEV = "OPENMSM_TOKEN_SALT";
        protected readonly IMapper mapper;
        protected readonly AppDbContext appDbContext;
        private string _accessToken { get; set; }
        private string _tokenSalt { get; }

        public ServiceBase(AppDbContext dbContext, IMapper mapper)
        {
            this.mapper = mapper;
            this.appDbContext = dbContext;
            _tokenSalt = Environment.GetEnvironmentVariable(TokenSaltEV, EnvironmentVariableTarget.Machine);
        }

        protected string GetHashedToken(string token)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(token, Convert.FromBase64String(_tokenSalt), KeyDerivationPrf.HMACSHA256, 10000, 256 / 8));
        }

        public void SetAccessToken(string token)
        {
            this._accessToken = GetHashedToken(token);
        }

        protected string GetAccessToken()
        {
            return _accessToken;
        }

        protected OpenMSM.Data.Models.Channel GetChannelByUri(string channelUri)
        {
            return this.appDbContext.Set<OpenMSM.Data.Models.Channel>()
                            .Include(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.URI.ToLower() == channelUri.ToLower());
        }

        protected bool DoPermissionsMatchChannel(OpenMSM.Data.Models.Channel channel)
        {
            var permissionsToken = this.GetAccessToken();
            return !(channel.ChannelsSecurityTokens.Any() && !channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        protected OpenMSM.Data.Models.Session GetSessionById(Guid sessionId)
        {
            return this.appDbContext.Set<OpenMSM.Data.Models.Session>().Include(m => m.SessionNamespaces).Include(m => m.SessionTopics).Include(m => m.Channel)
                .ThenInclude(m => m.ChannelsSecurityTokens).ThenInclude(cst => cst.SecurityToken).FirstOrDefault(m => m.Id == sessionId);
        }

        protected bool DoPermissionsMatchSession(OpenMSM.Data.Models.Session session)
        {
            var permissionsToken = this.GetAccessToken();
            return !(session.Channel.ChannelsSecurityTokens.Any() && !session.Channel.ChannelsSecurityTokens.Select(m => m.SecurityToken?.Token).Contains(permissionsToken));
        }

        protected OpenMSM.Data.Models.Session CheckSession(string sessionID, SessionType requiredType)
        {
            if (string.IsNullOrWhiteSpace(sessionID))
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("SessionID cannot be null or empty."), new FaultCode("Sender"), string.Empty);
            }
            var session = GetSessionById(new Guid(sessionID));
            if (session == null)
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("A session with the specified ID does not exist."), new FaultCode("Sender"), string.Empty);
            }
            if (session.Type != requiredType)
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("The session specified is not of the correct type for this action."), new FaultCode("Sender"), string.Empty);
            }
            if (session.IsClosed)
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("The session specified is closed."), new FaultCode("Sender"), string.Empty);
            }
            if (!DoPermissionsMatchSession(session))
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("Provided header security token does not match the token assigned to the session's channel."), new FaultCode("Sender"), string.Empty);
            }
            return session;
        }

        protected void ValidateXPath(string xPathExpression)
        {
            if (string.IsNullOrWhiteSpace(xPathExpression))
            {
                return;
            }

            try
            {
                var expr = XPathExpression.Compile(xPathExpression);
                if (new XPathResultType[] { XPathResultType.Any, XPathResultType.Error, XPathResultType.Navigator }.Contains(expr.ReturnType))
                {
                    throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("Current implementation does not handle XPathExpressions which return XPathResultType.Any/Error/Navigator."), new FaultCode("Receiver"), string.Empty);
                }
            }
            catch (XPathException)
            {
                throw new FaultException<SessionFault>(new SessionFault(), new FaultReason("Provided XPathExpression does not compile or is not a valid XPathExpression."), new FaultCode("Sender"), string.Empty);
            }
        }

        protected OpenMSM.Data.Models.MessagesSession GetNextMessageSession(Guid sessionId, Expression<Func<MessagesSession, bool>> optionalPredicate = null)
        {
            Expression<Func<MessagesSession, bool>> predicate = m => m.SessionId == sessionId // for the session
                   && !m.Message.ExpiredByCreatorOn.HasValue // that is not expired by the publisher 
                   && (!m.Message.ExpiresOn.HasValue || m.Message.ExpiresOn.Value >= DateTime.UtcNow); // and has not expired by by the date that it was set on creation

            var conditional = appDbContext.Set<MessagesSession>().Include(m => m.Message).ThenInclude(m => m.MessageTopics)
                   .OrderBy(m => m.Message.CreatedOn) // with the lowest created on date
                   .Where(predicate);

            if (optionalPredicate == null)
            {
                return conditional.FirstOrDefault(); 
            }
            else
            {
                return conditional.FirstOrDefault(optionalPredicate);
            }
        }

        protected XmlElement EvalateFilter(XmlElement messageContent, IList<OpenMSM.Data.Models.SessionNamespace> namespaces, string xPathExpression)
        {
            // If there is no filtering of message content
            if (string.IsNullOrWhiteSpace(xPathExpression) || namespaces == null || !namespaces.Any())
            {
                return messageContent;
            }

            var expr = System.Xml.XPath.XPathExpression.Compile(xPathExpression);
            var navigator = messageContent.OwnerDocument.CreateNavigator();
            foreach (var ns in namespaces)
            {
                var nsManager = new XmlNamespaceManager(navigator.NameTable);
                nsManager.AddNamespace(ns.Prefix, ns.Name);
            }
            switch (expr.ReturnType)
            {
                // If the expression evaluates to a boolean, return full message content if the boolean evaluted to true, otherwise return null
                case XPathResultType.Boolean:
                    return (bool)navigator.Evaluate(expr) ? messageContent : null;
                // If the expression evaluates to a number, return full message content if the number is greater than 0, otherwise return null
                // I am imagining a scenario where you are counting the number of child elements to filter as this scenario
                case XPathResultType.Number:
                    return (int)navigator.Evaluate(expr) > 0 ? messageContent : null;
                // If the expression evaluates to a string, return full message content if the string is not empty, otherwise return null
                case XPathResultType.String:
                    return string.IsNullOrWhiteSpace(navigator.Evaluate(expr).ToString()) ? null : messageContent;

                // If the expression evaluates to a node set, return full message content if the node set count is greater than 0, otherwise return null
                case XPathResultType.NodeSet:
                    var nodes = navigator.Select(expr);
                    return nodes.Count > 0 ? messageContent : null;
                default:
                    return null;
            }
        }
    }
}
