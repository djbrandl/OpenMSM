import React, { Component } from 'react';
import { bindActionCreators } from 'redux';
import { connect } from 'react-redux';
import { actionCreators } from '../store/Admin';
import { Jumbotron, TabContent, TabPane, Nav, NavItem, NavLink, Row, Col } from 'reactstrap';
import LogMessage from './LogMessage'
import classnames from 'classnames';

class Admin extends Component {
    componentDidMount() {
        this.props.startSignalR();
    }
    render() {
        return (
            <div>
                <Jumbotron>
                    <h1 className="display-3">Admin</h1>
                    <p className="lead">This page encompasses some Admin functionality for debugging and maintenance.</p>
                </Jumbotron>
                <Nav tabs>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Log' })} onClick={() => { this.props.setActiveTab('Log'); }} href='#'>Live Message Logging</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Orphan' })} onClick={() => { this.props.setActiveTab('Orphan') }} href='#'>Orphaned Sessions</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Dead' })} onClick={() => { this.props.setActiveTab('Dead') }} href='#'>Identify Dead Apps</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeTab === 'Options' })} onClick={() => { this.props.setActiveTab('Options') }} href='#'>Security Options</NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.props.activeTab}>
                    <TabPane tabId="Log">
                        <Row>
                            <Col sm="12">
                                <br />
                                {[...this.props.logMessages].reverse().map((message, index) => (
                                    <LogMessage key={index} data={message} index={index} />
                                ))}
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Orphan">
                        <Row>
                            <Col sm="12">
                                <br />
                                Kill orphaned sessions (show sessions ordered by most recent activity descending)
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Dead">
                        <Row>
                            <Col sm="12">
                                <br />
                                Identify dead apps - those who have messages being created and no subscribers/responders OR those who have loads of old messages that are unread
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Options">
                        <Row>
                            <Col sm="12">
                                <br />
                                Control security options
                                <ul>
                                    <li>Disable remote channel creation</li>
                                    <li>Enable or disable storing of log messages</li>
                                    <li>Enable/disable white listing of clients by IP or potentially MAC address (if available)</li>
                                </ul>
                            </Col>
                        </Row>
                    </TabPane>
                </TabContent>

            </div>
        );
    }
}


export default connect(
    state => state.admin,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(Admin);
