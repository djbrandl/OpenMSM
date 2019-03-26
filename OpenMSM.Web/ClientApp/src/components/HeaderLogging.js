import React, { Component } from 'react';
import { connect } from 'react-redux';
import { bindActionCreators } from 'redux';
import { actionCreators } from '../store/HeaderLogging';
import { TabContent, TabPane, Nav, NavItem, NavLink, Row, Col, ListGroup, ListGroupItem, Card, CardTitle } from 'reactstrap';
import classnames from 'classnames';

class HeaderLogging extends Component {
    render() {
        return (
            <div>
                <Nav tabs>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeHeaderTab === 'LastCall' })} onClick={() => { this.props.setActiveTab('LastCall'); }} href='#'>Last API Call</NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink className={classnames({ active: this.props.activeHeaderTab === 'Messages' })} onClick={() => { this.props.setActiveTab('Messages') }} href='#'>Server response messages</NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.props.activeHeaderTab}>
                    <TabPane tabId="LastCall">
                        <Row>
                            <Col sm="6">
                                <Card body>
                                    <CardTitle>{this.props.lastApiCall.url}</CardTitle>
                                    <pre>
                                        {JSON.stringify(this.props.lastApiCall.details, undefined, 2)}
                                    </pre>
                                </Card>
                            </Col>
                            <Col sm="6">
                                <Card body>
                                    <CardTitle>Response</CardTitle>
                                    <pre>
                                        {JSON.stringify(this.props.lastApiCall.response, undefined, 2)}
                                    </pre>
                                </Card>
                            </Col>
                        </Row>
                    </TabPane>
                    <TabPane tabId="Messages">
                        <Row>
                            <Col sm="12">
                                <br />
                                <ListGroup>
                                    {this.props.messages.map((message, index) => (
                                        <ListGroupItem key={index}>{message}</ListGroupItem>
                                    ))}
                                </ListGroup>
                            </Col>
                        </Row>
                    </TabPane>
                </TabContent>
            </div >
        );
    }
}

export default connect(
    state => state.headerLogging,
    dispatch => bindActionCreators(actionCreators, dispatch)
)(HeaderLogging);
